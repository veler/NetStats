using WindowSill.API;
using Microsoft.UI.Dispatching;
using NetStats.Models;

namespace NetStats.UI;

/// <summary>
/// Item della lista che mostra le statistiche di rete in tempo reale
/// e apre un popup con i dati cumulati quando viene cliccato.
/// </summary>
internal sealed class NetworkStatsPopupItem
{
    private readonly SillListViewPopupItem _view;
    private readonly NetworkMonitorViewModel _viewModel;
    private readonly NetworkUsageStorage _storage;
    private readonly DispatcherQueue _dispatcherQueue;

    private NetworkStatsPopupItem(NetworkMonitorViewModel viewModel, NetworkUsageStorage storage, DispatcherQueue dispatcherQueue)
    {
        _viewModel = viewModel;
        _storage = storage;
        _dispatcherQueue = dispatcherQueue;

        _view = new SillListViewPopupItem();
        
        // Imposta il contenuto con binding al viewModel
        _view.Content(
            new SillOrientedStackPanel()
                .VerticalAlignment(VerticalAlignment.Center)
                .Spacing(4)
                .Margin(x => x.ThemeResource("SillCommandContentMargin"))
                .DataContext(
                    _viewModel,
                    (panel, vm) => panel
                    .Children(
                        new StackPanel()
                            .Orientation(Orientation.Horizontal)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Spacing(4)
                            .Children(
                                new TextBlock()
                                    .Text("↓"),
                                new TextBlock()
                                    .MinWidth(32)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .Text(x => x.Binding(() => vm.DownloadMbps).OneWay())
                            ),
                        new StackPanel()
                            .Orientation(Orientation.Horizontal)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Spacing(4)
                            .Children(
                                new TextBlock()
                                    .Text("↑"),
                                new TextBlock()
                                    .MinWidth(32)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .Text(x => x.Binding(() => vm.UploadMbps).OneWay())
                            )
                    )
                )
        );

        // Imposta il contenuto del popup
        _view.PopupContent = NetworkStatsPopup.CreateView(_storage, _dispatcherQueue);
    }

    internal static SillListViewPopupItem CreateView(NetworkMonitorViewModel viewModel, NetworkUsageStorage storage, DispatcherQueue dispatcherQueue)
    {
        var item = new NetworkStatsPopupItem(viewModel, storage, dispatcherQueue);
        return item._view;
    }
}
