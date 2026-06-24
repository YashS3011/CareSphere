namespace CareSphere.Infrastructure;

/// <summary>
/// Scoped service that allows pages to push a dynamic breadcrumb/title
/// into the module layout's top navbar without any prop drilling.
/// Usage in a page: inject PageTitleService, call SetTitle("Module / Page Name")
/// Usage in a layout: subscribe to OnTitleChanged, render Title property.
/// </summary>
public class PageTitleService
{
    private string _title = "CareSphere";

    /// <summary>Current breadcrumb text shown in the top navbar.</summary>
    public string Title => _title;

    /// <summary>Fires whenever SetTitle is called, so the layout can re-render.</summary>
    public event Action? OnTitleChanged;

    /// <summary>Called by pages to update the navbar breadcrumb.</summary>
    public void SetTitle(string title)
    {
        _title = title;
        OnTitleChanged?.Invoke();
    }
}
