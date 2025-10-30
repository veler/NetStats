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
    private string _downloadMbps = "0.00";
    private string _uploadMbps = "0.00";

    public event PropertyChangedEventHandler? PropertyChanged;

    public string DownloadMbps
    {
        get => _downloadMbps + " Mb/s";
        set => SetProperty(ref _downloadMbps, value);
    }

    public string UploadMbps
    {
        get => _uploadMbps + " Mb/s";
        set => SetProperty(ref _uploadMbps, value);
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (!Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static SillView CreateView(NetworkMonitorViewModel vm, NetworkUsageStorage storage, DispatcherQueue dispatcherQueue)
    {
        SillView view = new();
        view.Content = new NetStatsView(view, vm, storage, dispatcherQueue);
        return view;
    }

}
