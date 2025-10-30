using Microsoft.UI.Dispatching;
using NetStats.Models;
using NetStats.UI;
using System.Runtime.InteropServices.Marshalling;
using WindowSill.API;

namespace NetStats;

/// <summary>
/// Vista UI minimalista che mostra l'uso della rete in tempo reale.
/// Visualizza velocità di download e upload in Mb/s con layout orizzontale responsivo.
/// </summary>
internal sealed class NetStatsView : Button
{
    private readonly NetworkMonitorViewModel _viewModel;
    private readonly SillView _sillView;

    public NetStatsView(SillView sillView, NetworkMonitorViewModel viewModel, NetworkUsageStorage storage, DispatcherQueue dispatcherQueue)
    {
        _sillView = sillView;

        this.DataContext(
            viewModel,
            (view, vm) => view
            .Style(x => x.StaticResource("SillButtonStyle"))
            .Height(double.NaN)
            .Content(
                new SillOrientedStackPanel()
                    .VerticalAlignment(VerticalAlignment.Center)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Spacing(4)
                    .Children(
                        new StackPanel()
                            .Orientation(Orientation.Horizontal)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Spacing(4)
                            .Children(
                                new TextBlock()
                                    .Text("↓"),
                                new TextBlock()
                                    .MinWidth(60)
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
                                    .MinWidth(60)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .Text(x => x.Binding(() => vm.UploadMbps).OneWay())
                            )
            )
        ));

        Click += NetStatsView_Click;

        DetailsPopup = new();
        DetailsPopup.Content = NetworkStatsPopup.CreateView(storage, dispatcherQueue);

        _viewModel = viewModel;

    }

    internal SillView View => _sillView;
    private SillPopup DetailsPopup { get; set; }

    private async void NetStatsView_Click(object sender, RoutedEventArgs e)
    {
        await DetailsPopup.ShowAsync(View);
    }

}
