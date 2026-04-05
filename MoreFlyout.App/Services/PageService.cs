using MoreFlyout.App.Contracts.Services;

namespace MoreFlyout.App.Services;

// Thanks to Jay for providing the code to update the related functions of the parent page

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = [];
    private readonly Dictionary<string, Type> _pageParents = [];

    public PageService()
    {
        Configure<HomeViewModel, HomePage>();
        Configure<ConditionViewModel,ConditionPage>();
        Configure<AboutViewModel, AboutPage>();
        Configure<SettingsViewModel, SettingsPage>();

        Configure<KeyIndicatorViewModel, KeyIndicatorPage>();
        Configure<MediaViewModel, MediaPage>();
        Configure<DarkModeViewModel, DarkModePage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    public Type? GetPageParents(string key)
    {
        Type? pageType;
        lock (_pageParents)
        {
            _pageParents.TryGetValue(key, out pageType);
        }

        return pageType;
    }

    // Lists the parent pages of the specified page
    public List<Type> GetPageParentChain(string key)
    {
        var parentChain = new List<Type>();
        var currentPageType = GetPageType(key);

        while (true)
        {
            var parentType = GetPageParents(currentPageType.FullName!);
            if (parentType is not null)
            {
                parentChain.Insert(0, parentType);
                currentPageType = parentType;
            }
            else
            {
                // No more parents found, exit the loop
                break;
            }
        }

        return parentChain;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (_pages.ContainsValue(type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }

    private void Matching<CV, PV>()
        where CV : Page
        where PV : Page
    {
        // CV: Child View, PV: Parent View
        lock (_pageParents)
        {
            var key = typeof(CV).FullName!;
            if (_pageParents.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already matched in PageService");
            }

            var type = typeof(PV);

            _pageParents.Add(key, type);
        }
    }
}
