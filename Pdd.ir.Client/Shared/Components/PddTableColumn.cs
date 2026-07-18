using Microsoft.AspNetCore.Components;

namespace Pdd.ir.Client.Shared.Components;

public class PddTableColumn<TItem>
{
    public string Title { get; set; } = "";
    public Func<TItem, string>? GetValue { get; set; }
    public RenderFragment<TItem>? Template { get; set; }
    public string HeaderStyle { get; set; } = "";
    public string Style { get; set; } = "";
}
