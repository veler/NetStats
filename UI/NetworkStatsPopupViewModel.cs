using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Dispatching;
using NetStats.Models;

namespace NetStats.UI;

/// <summary>
/// ViewModel per il popup delle statistiche cumulate.
/// Mostra i dati di utilizzo giornalieri, mensili e annuali.
/// Aggiorna automaticamente i dati ogni secondo quando il popup è aperto.
/// </summary>
public sealed class NetworkStatsPopupViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly NetworkUsageStorage _storage;
    private readonly DispatcherQueue _dispatcherQueue;
    private DispatcherQueueTimer? _updateTimer;
    
    private string _dailyDownload = "0 B";
    private string _dailyUpload = "0 B";
    private string _monthlyDownload = "0 B";
    private string _monthlyUpload = "0 B";
    private string _yearlyDownload = "0 B";
    private string _yearlyUpload = "0 B";

    public event PropertyChangedEventHandler? PropertyChanged;

    public NetworkStatsPopupViewModel(NetworkUsageStorage storage, DispatcherQueue dispatcherQueue)
    {
        _storage = storage;
        _dispatcherQueue = dispatcherQueue;
        UpdateData();
    }

    public string DailyDownload
    {
        get => _dailyDownload;
        set => SetProperty(ref _dailyDownload, value);
    }

    public string DailyUpload
    {
        get => _dailyUpload;
        set => SetProperty(ref _dailyUpload, value);
    }

    public string MonthlyDownload
    {
        get => _monthlyDownload;
        set => SetProperty(ref _monthlyDownload, value);
    }

    public string MonthlyUpload
    {
        get => _monthlyUpload;
        set => SetProperty(ref _monthlyUpload, value);
    }

    public string YearlyDownload
    {
        get => _yearlyDownload;
        set => SetProperty(ref _yearlyDownload, value);
    }

    public string YearlyUpload
    {
        get => _yearlyUpload;
        set => SetProperty(ref _yearlyUpload, value);
    }

    /// <summary>
    /// Inizia l'aggiornamento periodico dei dati ogni secondo.
    /// </summary>
    public void StartPeriodicUpdates()
    {
        if (_updateTimer != null)
            return;

        _updateTimer = _dispatcherQueue.CreateTimer();
        _updateTimer.Interval = TimeSpan.FromSeconds(1);
        _updateTimer.Tick += (s, e) => UpdateData();
        _updateTimer.IsRepeating = true;
        _updateTimer.Start();
    }

    /// <summary>
    /// Ferma l'aggiornamento periodico dei dati.
    /// </summary>
    public void StopPeriodicUpdates()
    {
        _updateTimer?.Stop();
    }

    /// <summary>
    /// Aggiorna i dati dal storage.
    /// </summary>
    public void UpdateData()
    {
        var dailyData = _storage.GetDailyData();
        DailyDownload = NetworkUsageData.FormatBytes(dailyData.TotalBytesReceived);
        DailyUpload = NetworkUsageData.FormatBytes(dailyData.TotalBytesSent);

        var monthlyData = _storage.GetMonthlyData();
        MonthlyDownload = NetworkUsageData.FormatBytes(monthlyData.TotalBytesReceived);
        MonthlyUpload = NetworkUsageData.FormatBytes(monthlyData.TotalBytesSent);

        var yearlyData = _storage.GetYearlyData();
        YearlyDownload = NetworkUsageData.FormatBytes(yearlyData.TotalBytesReceived);
        YearlyUpload = NetworkUsageData.FormatBytes(yearlyData.TotalBytesSent);
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (!Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public void Dispose()
    {
        StopPeriodicUpdates();
    }
}
