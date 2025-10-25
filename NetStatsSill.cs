using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using WindowSill.API;
using NetStats.UI;
using NetStats.Models;

namespace NetStats;

[Export(typeof(ISill))]                                     // Marks this class as a Sill to be discovered by MEF.
[Name("NetStats")]                              // A unique, internal name of the sill.
[Priority(Priority.Lowest)]                                 // Optional. The priority of this sill relative to others. Lowest means it will be after all other sills.
public sealed class NetStatsSill
    : ISillActivatedByDefault,  // Indicates that this sill is always visible and active.
    ISillListView               // List view to support popup functionality
{
    private readonly NetworkMonitorViewModel _viewModel;
    private readonly NetworkUsageStorage _storage;
    private readonly NetworkStatsService _networkStatsService;
    private readonly IPluginInfo _pluginInfo;
    private readonly ObservableCollection<SillListViewItem> _viewList;

    [ImportingConstructor]
    public NetStatsSill(IPluginInfo pluginInfo)
    {
        _viewModel = new NetworkMonitorViewModel();
        _storage = new NetworkUsageStorage();
        _networkStatsService = new NetworkStatsService(_viewModel, _storage, DispatcherQueue.GetForCurrentThread());
        _pluginInfo = pluginInfo;
        
        // Crea la lista con un singolo item cliccabile
        _viewList = new ObservableCollection<SillListViewItem>
        {
            NetworkStatsPopupItem.CreateView(_viewModel, _storage, DispatcherQueue.GetForCurrentThread())
        };
    }

    public string DisplayName => "NetStats"; // Temporaneamente hardcoded per evitare problemi di risorse PRI

    public IconElement CreateIcon()
        => new ImageIcon
        {
            Source = new SvgImageSource(new Uri(System.IO.Path.Combine(_pluginInfo.GetPluginContentDirectory(), "Assets", "netstats.svg")))
        };

    public ObservableCollection<SillListViewItem> ViewList => _viewList;

    // PlaceholderView nasconde l'icona del sill quando la lista non è vuota
    public SillView? PlaceholderView => null;

    public SillSettingsView[]? SettingsViews => null;

    public ValueTask OnActivatedAsync()
    {
        _networkStatsService.Start();
        return ValueTask.CompletedTask;
    }

    public ValueTask OnDeactivatedAsync()
    {
        _networkStatsService.Stop();
        return ValueTask.CompletedTask;
    }
}
