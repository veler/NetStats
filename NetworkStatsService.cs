using System.Net.NetworkInformation;
using Microsoft.UI.Dispatching;
using NetStats.Models;

namespace NetStats;

/// <summary>
/// Servizio che monitora le statistiche di rete in tempo reale.
/// Calcola i Mb/s cumulativi da tutte le interfacce di rete attive (escludendo loopback)
/// e aggiorna il ViewModel ogni secondo.
/// Persiste i dati di utilizzo cumulativo.
/// </summary>
public sealed class NetworkStatsService : IDisposable
{
    private readonly NetworkMonitorViewModel _viewModel;
    private readonly NetworkUsageStorage _storage;
    private readonly DispatcherQueue _dispatcherQueue;
    private DispatcherQueueTimer? _updateTimer;

    private long _lastReceivedBytes;
    private long _lastSentBytes;
    private DateTime _lastUpdateTime;

    public NetworkStatsService(NetworkMonitorViewModel viewModel, NetworkUsageStorage storage, DispatcherQueue dispatcherQueue)
    {
        _viewModel = viewModel;
        _storage = storage;
        _dispatcherQueue = dispatcherQueue;

        _lastUpdateTime = DateTime.UtcNow;
        InitializeNetworkStats();
    }

    /// <summary>
    /// Avvia il monitoraggio della rete con aggiornamenti ogni secondo.
    /// </summary>
    public void Start()
    {
        _updateTimer = _dispatcherQueue.CreateTimer();
        _updateTimer.Interval = TimeSpan.FromSeconds(1);
        _updateTimer.Tick += (s, e) => UpdateNetworkStats();
        _updateTimer.IsRepeating = true;
        _updateTimer.Start();
    }

    /// <summary>
    /// Ferma il monitoraggio della rete.
    /// </summary>
    public void Stop()
    {
        _updateTimer?.Stop();
    }

    private void InitializeNetworkStats()
    {
        var (receivedBytes, sentBytes) = GetCumulativeNetworkStats();
        _lastReceivedBytes = receivedBytes;
        _lastSentBytes = sentBytes;
    }

    private void UpdateNetworkStats()
    {
        var now = DateTime.UtcNow;
        var timeDeltaSeconds = (now - _lastUpdateTime).TotalSeconds;

        if (timeDeltaSeconds <= 0)
            return;

        var (receivedBytes, sentBytes) = GetCumulativeNetworkStats();

        var receivedBytesDelta = receivedBytes - _lastReceivedBytes;
        var sentBytesDelta = sentBytes - _lastSentBytes;

        // Persisti i dati solo se sono positivi (evita dati negativi da reset di interfacce)
        if (receivedBytesDelta >= 0 && sentBytesDelta >= 0)
        {
            _storage.UpdateUsage(receivedBytesDelta, sentBytesDelta);
        }

        // Conversione da byte/s a Mb/s (1 Mb = 1,000,000 bit = 125,000 byte)
        var downloadMbps = (receivedBytesDelta / timeDeltaSeconds) / 125_000d;
        var uploadMbps = (sentBytesDelta / timeDeltaSeconds) / 125_000d;

        // Aggiorna il ViewModel con i valori formattati a 2 decimali
        _viewModel.DownloadMbps = downloadMbps.ToString("F2");
        _viewModel.UploadMbps = uploadMbps.ToString("F2");

        _lastReceivedBytes = receivedBytes;
        _lastSentBytes = sentBytes;
        _lastUpdateTime = now;
    }

    /// <summary>
    /// Calcola i byte cumulativi ricevuti e inviati da tutte le interfacce di rete attive.
    /// Considera solo le interfacce hardware fisiche (con MAC address valido e velocità positiva).
    /// </summary>
    private static (long ReceivedBytes, long SentBytes) GetCumulativeNetworkStats()
    {
        long totalReceivedBytes = 0;
        long totalSentBytes = 0;

        try
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var networkInterface in interfaces)
            {
                // Scarta loopback
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                // Scarta interfacce non operative
                if (networkInterface.OperationalStatus != OperationalStatus.Up)
                    continue;

                // Verifica se è una interfaccia hardware: deve avere MAC address valido e velocità > 0
                if (!IsPhysicalInterface(networkInterface))
                    continue;

                var ipv4Stats = networkInterface.GetIPv4Statistics();
                totalReceivedBytes += ipv4Stats.BytesReceived;
                totalSentBytes += ipv4Stats.BytesSent;
            }
        }
        catch
        {
            // Se si verifica un errore nel recupero delle statistiche, ritorna 0,0
            return (0, 0);
        }

        return (totalReceivedBytes, totalSentBytes);
    }

    /// <summary>
    /// Determina se un'interfaccia di rete è hardware (non virtuale/software).
    /// Controlla: MAC address valido, velocità positiva, e filtra nomi di interfacce virtuali.
    /// </summary>
    private static bool IsPhysicalInterface(NetworkInterface networkInterface)
    {
        // Solo Ethernet e Wireless80211 sono considerate hardware
        if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
            networkInterface.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
            return false;

        // Verifica che abbia un MAC address valido (non tutti zeri)
        PhysicalAddress macAddress = networkInterface.GetPhysicalAddress();
        byte[] addressBytes = macAddress.GetAddressBytes();
        if (addressBytes.Length == 0 || addressBytes.All(b => b == 0))
            return false;

        // Verifica che abbia una velocità fisica positiva
        // Le interfacce software hanno Speed = 0 o -1
        if (networkInterface.Speed <= 0)
            return false;

        // Filtra ulteriormente per nomi di interfacce note come software
        string name = networkInterface.Name.ToLower();
        string desc = networkInterface.Description.ToLower();

        // Esclude nomi noti di interfacce virtuali/software
        string[] exclusionPatterns = new[]
        {
            "wfp", "qos", "filter",              // Driver Windows
            "virtual", "pseudo",                  // Virtual adapters
            "wan miniport", "miniport",          // WAN adapters
            "bluetooth",                          // Bluetooth (non è rete primaria)
            "teredo", "6to4",                     // Tunnel adapters
            "vpn", "tap", "hamachi",             // VPN adapters
            "loopback"                            // Loopback
        };

        foreach (var pattern in exclusionPatterns)
        {
            if (name.Contains(pattern) || desc.Contains(pattern))
                return false;
        }

        return true;
    }

    public void Dispose()
    {
        Stop();
    }
}
