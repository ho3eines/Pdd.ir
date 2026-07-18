using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace Pdd.ir.Client.Services;

public class TranslateService : ITranslateService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private Dictionary<string, string> _translations = new();
    private string _currentLanguage = "fa";

    public string CurrentLanguage => _currentLanguage;
    public event Action? OnLanguageChanged;

    public TranslateService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
        // Load default translations synchronously
        _translations = GetFallbackTranslations("fa");
    }

    public async Task InitializeAsync()
    {
        try
        {
            var saved = await _js.InvokeAsync<string?>("localStorage.getItem", "app_lang");
            if (!string.IsNullOrEmpty(saved) && saved != _currentLanguage)
            {
                await LoadLanguageAsync(saved);
            }
        }
        catch { }
    }

    public async Task LoadLanguageAsync(string culture)
    {
        _currentLanguage = culture;

        // Try HTTP first, fallback to inline
        try
        {
            var json = await _http.GetStringAsync($"lang/{culture}.json");
            var loaded = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (loaded != null && loaded.Count > 0)
                _translations = loaded;
            else
                _translations = GetFallbackTranslations(culture);
        }
        catch
        {
            _translations = GetFallbackTranslations(culture);
        }

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "app_lang", culture);
            await _js.InvokeVoidAsync("setDocDirection", culture);
        }
        catch { }

        OnLanguageChanged?.Invoke();
    }

    public string Text(string key)
    {
        return _translations.TryGetValue(key, out var value) ? value : key;
    }

    // ═══════════════════════════════════════════════════════════
    // INLINE FALLBACK TRANSLATIONS
    // ═══════════════════════════════════════════════════════════

    private Dictionary<string, string> GetFallbackTranslations(string culture)
    {
        if (culture == "fa") return Fa();
        return En();
    }

    private Dictionary<string, string> Fa() => new()
    {
        ["home"] = "خانه", ["products"] = "محصولات", ["blog"] = "بلاگ", ["portfolio"] = "نمونه کارها",
        ["about"] = "درباره ما", ["contact"] = "تماس با ما", ["login"] = "ورود", ["logout"] = "خروج",
        ["dashboard"] = "داشبورد", ["settings"] = "تنظیمات", ["save"] = "ذخیره", ["cancel"] = "لغو",
        ["delete"] = "حذف", ["edit"] = "ویرایش", ["search"] = "جستجو", ["loading"] = "در حال بارگذاری...",
        ["no_data"] = "داده‌ای موجود نیست", ["active"] = "فعال", ["inactive"] = "غیرفعال",
        ["status"] = "وضعیت", ["actions"] = "عملیات", ["records"] = "رکورد", ["of"] = "از",
        ["success"] = "موفق", ["error"] = "خطا", ["warning"] = "هشدار", ["info"] = "اطلاعات",
        ["saved_successfully"] = "با موفقیت ذخیره شد", ["deleted_successfully"] = "با موفقیت حذف شد",
        ["are_you_sure_delete"] = "آیا از حذف مطمئن هستید؟", ["required_field"] = "فیلد الزامی است",
        ["username_required"] = "نام کاربری الزامی است", ["role_required"] = "نام نقش الزامی است",
        ["password_required"] = "رمز عبور را وارد کنید", ["password_changed"] = "رمز عبور تغییر کرد",
        ["login_failed"] = "نام کاربری یا رمز عبور اشتباه است", ["login_empty"] = "نام کاربری و رمز عبور را وارد کنید",
        ["login_title"] = "ورود به پنل مدیریت", ["login_subtitle"] = "نام کاربری و رمز عبور خود را وارد کنید",
        ["username"] = "نام کاربری", ["password"] = "رمز عبور", ["full_name"] = "نام کامل",
        ["role"] = "نقش", ["category"] = "دسته‌بندی", ["title"] = "عنوان", ["subtitle"] = "زیرعنوان",
        ["description"] = "توضیحات", ["content"] = "محتوا", ["slug"] = "Slug", ["summary"] = "خلاصه",
        ["author"] = "نویسنده", ["published"] = "منتشر شده", ["draft"] = "پیش‌نویس",
        ["view_count"] = "بازدید", ["sort_order"] = "ترتیب نمایش", ["image_url"] = "تصویر URL",
        ["project_url"] = "آدرس پروژه", ["email_field"] = "ایمیل", ["phone_field"] = "تلفن",
        ["date"] = "تاریخ", ["name"] = "نام", ["subject_field"] = "موضوع", ["message"] = "پیام",
        ["read"] = "خوانده شده", ["unread"] = "جدید",
        ["product_saved"] = "محصول ذخیره شد", ["blog_saved"] = "مقاله ذخیره شد",
        ["portfolio_saved"] = "نمونه کار ذخیره شد", ["user_saved"] = "کاربر ذخیره شد", ["role_saved"] = "نقش ذخیره شد",
        ["product_new"] = "محصول جدید", ["product_edit"] = "ویرایش محصول",
        ["blog_new"] = "مقاله جدید", ["blog_edit"] = "ویرایش مقاله",
        ["portfolio_new"] = "نمونه کار جدید", ["portfolio_edit"] = "ویرایش نمونه کار",
        ["user_new"] = "کاربر جدید", ["user_edit"] = "ویرایش کاربر",
        ["role_new"] = "نقش جدید", ["role_edit"] = "ویرایش نقش",
        ["password_change"] = "تغییر رمز عبور", ["password_new"] = "رمز عبور جدید",
        ["permissions"] = "دسترسی‌ها", ["select_all"] = "انتخاب همه", ["deselect_all"] = "لغو انتخاب",
        ["guide"] = "راهنما", ["admin_panel"] = "پنل مدیریت", ["back_to_site"] = "بازگشت به سایت",
        ["admin_products"] = "مدیریت محصولات", ["admin_blog"] = "مدیریت بلاگ",
        ["admin_portfolio"] = "مدیریت نمونه کارها", ["admin_messages"] = "پیام‌ها",
        ["admin_users"] = "کاربران", ["admin_settings"] = "تنظیمات",
        ["new_item"] = "جدید", ["confirm"] = "تأیید", ["yes"] = "بله", ["no"] = "خیر",
        ["send_message"] = "ارسال پیام", ["contact_us"] = "تماس با ما",
        ["years_experience"] = "سال تجربه", ["medical_centers"] = "مرکز درمانی",
        ["subsystems"] = "زیرسیستم", ["support_24_7"] = "پشتیبانی ۲۴/۷",
        ["address"] = "آدرس", ["phone"] = "تلفن", ["email"] = "ایمیل",
        ["company_name"] = "شرکت مهندسی طراح داده پیشرو", ["all_rights_reserved"] = "تمامی حقوق محفوظ است",
        ["hero_title_1"] = "راهکارهای هوشمند نرم‌افزاری", ["hero_subtitle_1"] = "طراحی سیستم‌های مدرن برای آینده",
        ["hero_title_2"] = "مدیریت هوشمند بیمارستان", ["hero_subtitle_2"] = "نرم‌افزار یکپارچه درمانی برای بیمارستان‌های مدرن",
        ["hero_title_3"] = "محصولات ما", ["hero_subtitle_3"] = "شش سیستم یکپارچه برای مدیریت جامع مراکز درمانی",
        ["view_products"] = "مشاهده محصولات", ["ready_to_start"] = "آماده شروع هستید؟",
        ["contact_description"] = "با ما تماس بگیرید و بهترین راهکار را دریافت کنید",
        ["latest_projects"] = "آخرین پروژه‌های شرکت", ["company_goal"] = "هدف اصلی شرکت",
        ["customer_growth"] = "سیر صعودی مشتریان شرکت",
        ["welcome"] = "خوش آمدید", ["new_product"] = "محصول جدید", ["new_article"] = "مقاله جدید",
        ["new_portfolio"] = "نمونه کار جدید", ["new_user"] = "کاربر جدید", ["new_role"] = "نقش جدید",
        ["view_all"] = "مشاهده همه", ["no_products"] = "محصولی وجود ندارد", ["no_messages"] = "پیامی وجود ندارد",
        ["no_articles"] = "مقاله‌ای وجود ندارد", ["no_permissions"] = "هیچ دسترسی‌ای یافت نشد",
        ["recent_products"] = "آخرین محصولات", ["recent_messages"] = "آخرین پیام‌ها", ["recent_articles"] = "آخرین مقالات",
        ["product_guide"] = "اطلاعات محصول را وارد کنید.", ["portfolio_guide"] = "اطلاعات نمونه کار را وارد کنید.",
        ["blog_guide"] = "اطلاعات مقاله را وارد کنید. Slug باید منحصربفرد باشد.",
        ["user_guide"] = "اطلاعات کاربر را وارد کنید.", ["role_guide"] = "نام نقش و دسترسی‌های آن را تنظیم کنید.",
        ["select_role"] = "انتخاب نقش...", ["user_role"] = "کاربر", ["admin_role"] = "ادمین", ["manager_role"] = "مدیر",
        ["pdd_engineering"] = "طراح داده پیشرو", ["toggle_theme"] = "تغییر تم", ["logout_title"] = "خروج",
        ["contact_messages_guide"] = "پیام‌های دریافتی از فرم تماس", ["users_guide"] = "تعریف، ویرایش و مدیریت دسترسی کاربران",
        ["roles_title"] = "مدیریت نقش‌ها", ["roles_subtitle"] = "مدیریت نقش‌ها و دسترسی‌ها",
        ["roles_guide"] = "تعریف نقش‌ها و مدیریت دسترسی‌های هر نقش",
        ["site_settings"] = "تنظیمات سایت", ["site_settings_guide"] = "مدیریت تنظیمات کلی سایت",
        ["company_info"] = "اطلاعات شرکت", ["company_name"] = "نام شرکت",
        ["social_media"] = "شبکه‌های اجتماعی", ["settings_saved"] = "تنظیمات با موفقیت ذخیره شد.",
        ["change_password"] = "تغییر رمز عبور", ["new_password_guide"] = "رمز عبور جدید را وارد کنید.",
        ["new_password"] = "رمز عبور جدید", ["password_changed_msg"] = "رمز عبور تغییر کرد",
        ["name_label"] = "نام", ["close"] = "بستن", ["no_data_found"] = "داده‌ای موجود نیست",
        ["confirm_delete"] = "آیا از حذف مطمئن هستید؟"
    };

    private Dictionary<string, string> En() => new()
    {
        ["home"] = "Home", ["products"] = "Products", ["blog"] = "Blog", ["portfolio"] = "Portfolio",
        ["about"] = "About Us", ["contact"] = "Contact Us", ["login"] = "Login", ["logout"] = "Logout",
        ["dashboard"] = "Dashboard", ["settings"] = "Settings", ["save"] = "Save", ["cancel"] = "Cancel",
        ["delete"] = "Delete", ["edit"] = "Edit", ["search"] = "Search", ["loading"] = "Loading...",
        ["no_data"] = "No data available", ["active"] = "Active", ["inactive"] = "Inactive",
        ["status"] = "Status", ["actions"] = "Actions", ["records"] = "records", ["of"] = "of",
        ["success"] = "Success", ["error"] = "Error", ["warning"] = "Warning", ["info"] = "Info",
        ["saved_successfully"] = "Saved successfully", ["deleted_successfully"] = "Deleted successfully",
        ["are_you_sure_delete"] = "Are you sure you want to delete?", ["required_field"] = "This field is required",
        ["username_required"] = "Username is required", ["role_required"] = "Role name is required",
        ["password_required"] = "Please enter password", ["password_changed"] = "Password has been changed",
        ["login_failed"] = "Invalid username or password", ["login_empty"] = "Please enter username and password",
        ["login_title"] = "Admin Panel Login", ["login_subtitle"] = "Enter your username and password",
        ["username"] = "Username", ["password"] = "Password", ["full_name"] = "Full Name",
        ["role"] = "Role", ["category"] = "Category", ["title"] = "Title", ["subtitle"] = "Subtitle",
        ["description"] = "Description", ["content"] = "Content", ["slug"] = "Slug", ["summary"] = "Summary",
        ["author"] = "Author", ["published"] = "Published", ["draft"] = "Draft",
        ["view_count"] = "Views", ["sort_order"] = "Sort Order", ["image_url"] = "Image URL",
        ["project_url"] = "Project URL", ["email_field"] = "Email", ["phone_field"] = "Phone",
        ["date"] = "Date", ["name"] = "Name", ["subject_field"] = "Subject", ["message"] = "Message",
        ["read"] = "Read", ["unread"] = "Unread",
        ["product_saved"] = "Product saved", ["blog_saved"] = "Article saved",
        ["portfolio_saved"] = "Portfolio saved", ["user_saved"] = "User saved", ["role_saved"] = "Role saved",
        ["product_new"] = "New Product", ["product_edit"] = "Edit Product",
        ["blog_new"] = "New Article", ["blog_edit"] = "Edit Article",
        ["portfolio_new"] = "New Portfolio", ["portfolio_edit"] = "Edit Portfolio",
        ["user_new"] = "New User", ["user_edit"] = "Edit User",
        ["role_new"] = "New Role", ["role_edit"] = "Edit Role",
        ["password_change"] = "Change Password", ["password_new"] = "New Password",
        ["permissions"] = "Permissions", ["select_all"] = "Select All", ["deselect_all"] = "Deselect All",
        ["guide"] = "Guide", ["admin_panel"] = "Admin Panel", ["back_to_site"] = "Back to Site",
        ["admin_products"] = "Product Management", ["admin_blog"] = "Blog Management",
        ["admin_portfolio"] = "Portfolio Management", ["admin_messages"] = "Messages",
        ["admin_users"] = "Users", ["admin_settings"] = "Settings",
        ["new_item"] = "New", ["confirm"] = "Confirm", ["yes"] = "Yes", ["no"] = "No",
        ["send_message"] = "Send Message", ["contact_us"] = "Contact Us",
        ["years_experience"] = "Years Experience", ["medical_centers"] = "Medical Centers",
        ["subsystems"] = "Subsystems", ["support_24_7"] = "24/7 Support",
        ["address"] = "Address", ["phone"] = "Phone", ["email"] = "Email",
        ["company_name"] = "PDD Engineering Company", ["all_rights_reserved"] = "All Rights Reserved",
        ["hero_title_1"] = "Smart Software Solutions", ["hero_subtitle_1"] = "Designing modern systems for the future",
        ["hero_title_2"] = "Smart Hospital Management", ["hero_subtitle_2"] = "Integrated healthcare software for modern hospitals",
        ["hero_title_3"] = "Our Products", ["hero_subtitle_3"] = "Six integrated systems for comprehensive healthcare management",
        ["view_products"] = "View Products", ["ready_to_start"] = "Ready to Get Started?",
        ["contact_description"] = "Contact us and get the best solution",
        ["latest_projects"] = "Latest Company Projects", ["company_goal"] = "Company Main Goal",
        ["customer_growth"] = "Company Customer Growth",
        ["welcome"] = "Welcome", ["new_product"] = "New Product", ["new_article"] = "New Article",
        ["new_portfolio"] = "New Portfolio", ["new_user"] = "New User", ["new_role"] = "New Role",
        ["view_all"] = "View All", ["no_products"] = "No products yet", ["no_messages"] = "No messages yet",
        ["no_articles"] = "No articles yet", ["no_permissions"] = "No permissions found",
        ["recent_products"] = "Recent Products", ["recent_messages"] = "Recent Messages", ["recent_articles"] = "Recent Articles",
        ["product_guide"] = "Enter product information.", ["portfolio_guide"] = "Enter portfolio information.",
        ["blog_guide"] = "Enter article information. Slug must be unique.",
        ["user_guide"] = "Enter user information.", ["role_guide"] = "Set role name and its permissions.",
        ["select_role"] = "Select role...", ["user_role"] = "User", ["admin_role"] = "Admin", ["manager_role"] = "Manager",
        ["pdd_engineering"] = "PDD Engineering", ["toggle_theme"] = "Toggle Theme", ["logout_title"] = "Logout",
        ["contact_messages_guide"] = "Messages from contact form", ["users_guide"] = "Create, edit and manage user access",
        ["roles_title"] = "Roles Management", ["roles_subtitle"] = "Roles & Permissions",
        ["roles_guide"] = "Define roles and manage their permissions",
        ["site_settings"] = "Site Settings", ["site_settings_guide"] = "Manage general site settings",
        ["company_info"] = "Company Info", ["company_name_label"] = "Company Name",
        ["social_media"] = "Social Media", ["settings_saved"] = "Settings saved successfully.",
        ["change_password"] = "Change Password", ["new_password_guide"] = "Enter the new password.",
        ["new_password"] = "New Password", ["password_changed_msg"] = "Password has been changed",
        ["name_label"] = "Name", ["close"] = "Close", ["no_data_found"] = "No data available",
        ["confirm_delete"] = "Are you sure you want to delete?"
    };
}
