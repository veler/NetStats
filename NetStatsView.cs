using WindowSill.API;

namespace NetStats;

/// <summary>
/// Vista UI minimalista che mostra l'uso della rete in tempo reale.
/// Visualizza velocità di download e upload in Mb/s con layout orizzontale responsivo.
/// </summary>
internal sealed class NetStatsView
{
    private readonly NetworkMonitorViewModel _viewModel;
    private readonly SillView _sillView;

    public NetStatsView(NetworkMonitorViewModel viewModel)
    {
        _viewModel = viewModel;
        _sillView = new SillView();
        _sillView.DataContext = viewModel;
        InitializeComponent();
    }

    internal SillView View => _sillView;

    private void InitializeComponent()
    {
        _sillView.Content(
            new SillOrientedStackPanel()
                .VerticalAlignment(VerticalAlignment.Center)
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
                                .MinWidth(32)
                                .VerticalAlignment(VerticalAlignment.Center)
                                .Text(x => x.Binding(() => _viewModel.DownloadMbps).OneWay())
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
                                .Text(x => x.Binding(() => _viewModel.UploadMbps).OneWay())
                        )
                )
        );
    }
}
