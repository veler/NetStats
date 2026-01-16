using Microsoft.UI.Dispatching;
using NetStats.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WindowSill.API;

namespace NetStats;

/// <summary>
/// ViewModel che espone proprietà bindabili per il monitoraggio della rete in tempo reale.
/// Implementa INotifyPropertyChanged per aggiornare la UI quando i dati cambiano.
/// </summary>
public sealed class NetworkMonitorViewModel : INotifyPropertyChanged
{
    private string _downloadSpeed = "0.00";
    private string _uploadSpeed = "0.00";
    private string _speedUnit = "Mb/s";

    public event PropertyChangedEventHandler? PropertyChanged;

    public string DownloadSpeed
    {
        get => _downloadSpeed;
        set => SetProperty(ref _downloadSpeed, value);
    }

    public string UploadSpeed
    {
        get => _uploadSpeed;
        set => SetProperty(ref _uploadSpeed, value);
    }

    public string SpeedUnit
    {
        get => _speedUnit;
        set => SetProperty(ref _speedUnit, value);
    }

    public string DownloadSpeedFormatted => $"{_downloadSpeed} {_speedUnit}";
    public string UploadSpeedFormatted => $"{_uploadSpeed} {_speedUnit}";

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (!Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            
            // Notifica le proprietà calcolate quando cambiano i loro componenti
            if (propertyName == nameof(DownloadSpeed) || propertyName == nameof(SpeedUnit))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadSpeedFormatted)));
            }
            if (propertyName == nameof(UploadSpeed) || propertyName == nameof(SpeedUnit))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UploadSpeedFormatted)));
            }
        }
    }

    public static SillView CreateView(NetworkMonitorViewModel vm, NetworkUsageStorage storage, DispatcherQueue dispatcherQueue)
    {
        SillView view = new();
        view.Content = new NetStatsView(view, vm, storage, dispatcherQueue);
        return view;
    }

}
