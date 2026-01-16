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
    ISillSingleView             // Indicates that this sill provides a custom single view (not a list of commands).
{
    private NetStatsView? _view;
    private readonly NetworkMonitorViewModel _viewModel;
    private readonly NetworkUsageStorage _storage;
    private readonly NetworkStatsService _networkStatsService;
    private readonly ISettingsProvider _settingsProvider;

    private readonly IPluginInfo _pluginInfo;

    internal static readonly SettingDefinition<string[]> DisabledInterfacesSetting = new(Array.Empty<string>(), typeof(NetStatsSill).Assembly);
    internal static readonly SettingDefinition<string> SpeedUnitSetting = new("Mbps", typeof(NetStatsSill).Assembly);

    [ImportingConstructor]
    public NetStatsSill(IPluginInfo pluginInfo, ISettingsProvider settingsProvider)
    {
        _viewModel = new NetworkMonitorViewModel();
        _storage = new NetworkUsageStorage(pluginInfo);
        _networkStatsService = new NetworkStatsService(_viewModel, _storage, DispatcherQueue.GetForCurrentThread(), settingsProvider);
        _pluginInfo = pluginInfo;
        _settingsProvider = settingsProvider;
        View = NetworkMonitorViewModel.CreateView(_viewModel, _storage, DispatcherQueue.GetForCurrentThread());
    }

    public string DisplayName => "NetStats"; // Temporaneamente hardcoded per evitare problemi di risorse PRI

    public IconElement CreateIcon()
        => new ImageIcon
        {
            Source = new SvgImageSource(new Uri(System.IO.Path.Combine(_pluginInfo.GetPluginContentDirectory(), "Assets", "netstats.svg")))
        };

    public SillView View { get; private set; }

    public SillView? PlaceholderView => null;

    public SillSettingsView[]? SettingsViews => new[] { NetStatsSettingsView.Create(_networkStatsService, _settingsProvider) };

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
