using WindowSill.API;
using Microsoft.UI.Dispatching;
using NetStats.Models;

namespace NetStats.UI;

/// <summary>
/// Popup che mostra statistiche di rete cumulate per periodi diversi.
/// Visualizza dati giornalieri, mensili e annuali dal storage persistente.
/// </summary>
internal sealed class NetworkStatsPopup : IDisposable
{
    private readonly SillPopupContent _view;
    private readonly NetworkStatsPopupViewModel _viewModel;

    private NetworkStatsPopup(NetworkUsageStorage storage, DispatcherQueue dispatcherQueue)
    {
        _viewModel = new NetworkStatsPopupViewModel(storage, dispatcherQueue);

        _view = new SillPopupContent(OnOpening)
            .Width(400)
            .Content(
                new Grid()
                    .Padding(24)
                    .RowSpacing(16)
                    .RowDefinitions(
                        new GridLength(1, GridUnitType.Auto),  // Title
                        new GridLength(1, GridUnitType.Auto),  // Daily stats
                        new GridLength(1, GridUnitType.Auto),  // Monthly stats
                        new GridLength(1, GridUnitType.Auto)   // Yearly stats
                    )
                    .DataContext(
                        _viewModel,
                        (grid, vm) => grid
                        .Children(
                            // Title
                            new TextBlock()
                                .Grid(row: 0)
                                .Style(x => x.ThemeResource("TitleTextBlockStyle"))
                                .Text("/NetStats/Misc/PopupTitle".GetLocalizedString()),
                            
                            // Daily section
                            CreateStatsSection("/NetStats/Misc/PopupDailyTitle".GetLocalizedString(), 
                                nameof(vm.DailyDownload), nameof(vm.DailyUpload))
                                .Grid(row: 1),
                            
                            // Monthly section
                            CreateStatsSection("/NetStats/Misc/PopupMonthlyTitle".GetLocalizedString(), 
                                nameof(vm.MonthlyDownload), nameof(vm.MonthlyUpload))
                                .Grid(row: 2),
                            
                            // Yearly section
                            CreateStatsSection("/NetStats/Misc/PopupYearlyTitle".GetLocalizedString(), 
                                nameof(vm.YearlyDownload), nameof(vm.YearlyUpload))
                                .Grid(row: 3)
                        )
                    )
            );
    }

    public static SillPopupContent CreateView(NetworkUsageStorage storage, DispatcherQueue dispatcherQueue)
    {
        var popup = new NetworkStatsPopup(storage, dispatcherQueue);
        return popup._view;
    }

    private void OnOpening()
    {
        // Avvia l'aggiornamento periodico dei dati quando il popup viene aperto
        _viewModel.StartPeriodicUpdates();
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    private static StackPanel CreateStatsSection(string title, string downloadPropertyName, string uploadPropertyName)
    {
        return new StackPanel()
            .Spacing(8)
            .Children(
                new TextBlock()
                    .Style(x => x.ThemeResource("BodyStrongTextBlockStyle"))
                    .Text(title),
                
                new Grid()
                    .ColumnSpacing(16)
                    .ColumnDefinitions(
                        new GridLength(1, GridUnitType.Star),
                        new GridLength(1, GridUnitType.Star)
                    )
                    .Children(
                        // Download column
                        new StackPanel()
                            .Grid(column: 0)
                            .Spacing(4)
                            .Children(
                                new TextBlock()
                                    .Foreground(x => x.ThemeResource("TextFillColorSecondaryBrush"))
                                    .Text("/NetStats/Misc/PopupDownloadLabel".GetLocalizedString()),
                                new TextBlock()
                                    .Style(x => x.ThemeResource("SubtitleTextBlockStyle"))
                                    .Text(x => x.Binding(downloadPropertyName).Mode(BindingMode.OneWay))
                            ),
                        
                        // Upload column
                        new StackPanel()
                            .Grid(column: 1)
                            .Spacing(4)
                            .Children(
                                new TextBlock()
                                    .Foreground(x => x.ThemeResource("TextFillColorSecondaryBrush"))
                                    .Text("/NetStats/Misc/PopupUploadLabel".GetLocalizedString()),
                                new TextBlock()
                                    .Style(x => x.ThemeResource("SubtitleTextBlockStyle"))
                                    .Text(x => x.Binding(uploadPropertyName).Mode(BindingMode.OneWay))
                            )
                    )
            );
    }
}
