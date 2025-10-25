using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using System.ComponentModel.Composition;
using WindowSill.API;

namespace NetStats;

[Export(typeof(ISill))]                                     // Marks this class as a Sill to be discovered by MEF.
[Name("NetStats")]                              // A unique, internal name of the sill.
[Priority(Priority.Lowest)]                                 // Optional. The priority of this sill relative to others. Lowest means it will be after all other sills.
public sealed class NetStatsSill
    : ISillActivatedByDefault,  // Indicates that this sill is always visible and active.
    ISillSingleView             // Indicates that this sill provides a custom single view (not a list of commands).
{
    private readonly NetworkMonitorViewModel _viewModel;
    private readonly NetworkStatsService _networkStatsService;
    private NetStatsView? _view;

    private readonly IPluginInfo _pluginInfo;

    [ImportingConstructor]
    public NetStatsSill(IPluginInfo pluginInfo)
    {
        _viewModel = new NetworkMonitorViewModel();
        _networkStatsService = new NetworkStatsService(_viewModel, DispatcherQueue.GetForCurrentThread());
        _pluginInfo = pluginInfo;
    }

    public string DisplayName => "NetStats"; // Temporaneamente hardcoded per evitare problemi di risorse PRI

    public IconElement CreateIcon()
        => new ImageIcon
        {
            Source = new SvgImageSource(new Uri(System.IO.Path.Combine(_pluginInfo.GetPluginContentDirectory(), "Assets", "netstats.svg")))
        };

    public SillView View
    {
        get
        {
            if (_view == null)
            {
                _view = new NetStatsView(_viewModel);
            }
            return _view.View;
        }
    }

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
