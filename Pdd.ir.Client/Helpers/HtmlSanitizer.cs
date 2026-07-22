using System.Text.RegularExpressions;

namespace Pdd.ir.Client.Helpers
{
    /// <summary>
    /// Simple HTML sanitizer to prevent XSS attacks.
    /// Allows safe HTML tags while removing script/event handlers.
    /// </summary>
    public static partial class HtmlSanitizer
    {
        // Allowed HTML tags
        private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "p", "br", "hr", "h1", "h2", "h3", "h4", "h5", "h6",
            "strong", "b", "em", "i", "u", "s", "del", "ins",
            "ul", "ol", "li", "a", "img", "blockquote", "pre", "code",
            "table", "thead", "tbody", "tr", "th", "td",
            "div", "span", "section", "article", "aside", "header", "footer",
            "figure", "figcaption", "details", "summary"
        };

        // Allowed attributes per tag
        private static readonly Dictionary<string, HashSet<string>> AllowedAttributes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["a"] = new() { "href", "title", "target", "rel" },
            ["img"] = new() { "src", "alt", "title", "width", "height" },
            ["div"] = new() { "class", "id", "style" },
            ["span"] = new() { "class", "id", "style" },
            ["table"] = new() { "class", "id" },
            ["td"] = new() { "class", "colspan", "rowspan" },
            ["th"] = new() { "class", "colspan", "rowspan" }
        };

        // Blocked patterns (script, event handlers, etc.)
        private static readonly string[] BlockedPatterns = new[]
        {
            @"<script[^>]*>.*?</script>",
            @"javascript:",
            @"on\w+\s*=",
            @"data:text/html",
            @"vbscript:",
            @"expression\(",
            @"<iframe[^>]*>",
            @"<object[^>]*>",
            @"<embed[^>]*>",
            @"<applet[^>]*>",
            @"<form[^>]*>",
            @"<input[^>]*>",
            @"<textarea[^>]*>",
            @"<select[^>]*>"
        };

        /// <summary>
        /// Sanitize HTML content to prevent XSS attacks.
        /// </summary>
        public static string Sanitize(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // Remove blocked patterns first
            var sanitized = html;
            foreach (var pattern in BlockedPatterns)
            {
                sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }

            // Remove event handlers from all tags
            sanitized = Regex.Replace(sanitized, @"on(\w+)=""[^""]*""", "", RegexOptions.IgnoreCase);

            // Remove style expressions
            sanitized = Regex.Replace(sanitized, @"expression\([^)]*\)", "", RegexOptions.IgnoreCase);

            // Remove javascript: URLs
            sanitized = Regex.Replace(sanitized, @"href=""javascript:[^""]*""", "href=\"#\"", RegexOptions.IgnoreCase);

            return sanitized.Trim();
        }

        /// <summary>
        /// Check if a tag is allowed.
        /// </summary>
        private static bool IsTagAllowed(string tag)
        {
            return AllowedTags.Contains(tag);
        }

        /// <summary>
        /// Check if an attribute is allowed for a tag.
        /// </summary>
        private static bool IsAttributeAllowed(string tag, string attribute)
        {
            if (AllowedAttributes.TryGetValue(tag, out var allowed))
            {
                return allowed.Contains(attribute);
            }
            return false;
        }
    }
}
