using System.Net.NetworkInformation;
using Microsoft.UI.Dispatching;
using NetStats.Models;
using NetStats.UI;
using System.Collections.ObjectModel;
using WindowSill.API;

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
    private readonly ISettingsProvider _settingsProvider;
    private DispatcherQueueTimer? _updateTimer;

    private long _lastReceivedBytes;
    private long _lastSentBytes;
    private DateTime _lastUpdateTime;

    private HashSet<string> _disabledInterfaceIds = new();
    
    // Mappa per tracciare i dati precedenti per ogni interfaccia (per calcolare la velocità)
    private Dictionary<string, (long Rx, long Tx)> _previousInterfaceStats = new();

    public ObservableCollection<NetworkInterfaceViewModel> Interfaces { get; } = new();

    public NetworkStatsService(NetworkMonitorViewModel viewModel, NetworkUsageStorage storage, DispatcherQueue dispatcherQueue, ISettingsProvider settingsProvider)
    {
        _viewModel = viewModel;
        _storage = storage;
        _dispatcherQueue = dispatcherQueue;
        _settingsProvider = settingsProvider;

        var disabled = _settingsProvider.GetSetting(NetStatsSill.DisabledInterfacesSetting);
        if (disabled != null)
        {
            _disabledInterfaceIds = new HashSet<string>(disabled);
        }

        _settingsProvider.SettingChanged += OnSettingChanged;

        _lastUpdateTime = DateTime.UtcNow;
        InitializeNetworkStats();
    }

    private void OnSettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (e.SettingName == NetStatsSill.DisabledInterfacesSetting.Name)
        {
            var disabled = _settingsProvider.GetSetting(NetStatsSill.DisabledInterfacesSetting);
            _disabledInterfaceIds = new HashSet<string>(disabled ?? Array.Empty<string>());
        }
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
        try
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var networkInterface in interfaces)
            {
                if (IsPhysicalInterface(networkInterface))
                {
                    var ipv4Stats = networkInterface.GetIPv4Statistics();
                    _previousInterfaceStats[networkInterface.Id] = (ipv4Stats.BytesReceived, ipv4Stats.BytesSent);
                }
            }
        }
        catch { }
    }

    private void UpdateNetworkStats()
    {
        var now = DateTime.UtcNow;
        var timeDeltaSeconds = (now - _lastUpdateTime).TotalSeconds;

        if (timeDeltaSeconds <= 0)
            return;

        long totalReceivedBytesDelta = 0;
        long totalSentBytesDelta = 0;

        try
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var networkInterface in interfaces)
            {
                // Scarta loopback
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                // Verifica se è una interfaccia hardware
                if (!IsPhysicalInterface(networkInterface))
                    continue;

                var id = networkInterface.Id;
                var ipv4Stats = networkInterface.GetIPv4Statistics();
                long currentRx = ipv4Stats.BytesReceived;
                long currentTx = ipv4Stats.BytesSent;

                // Calculate delta for this interface
                long deltaRx = 0;
                long deltaTx = 0;

                if (_previousInterfaceStats.TryGetValue(id, out var prevStats))
                {
                    deltaRx = currentRx - prevStats.Rx;
                    deltaTx = currentTx - prevStats.Tx;
                    // Handle reset/overflow
                    if (deltaRx < 0) deltaRx = currentRx;
                    if (deltaTx < 0) deltaTx = currentTx;
                }
                _previousInterfaceStats[id] = (currentRx, currentTx);

                // Update ViewModel
                var vm = Interfaces.FirstOrDefault(x => x.Id == id);
                if (vm == null)
                {
                    vm = new NetworkInterfaceViewModel
                    {
                        Id = id,
                        Name = networkInterface.Name,
                        Description = networkInterface.Description,
                        IsSelected = !_disabledInterfaceIds.Contains(id)
                    };
                    vm.SelectionChanged += OnInterfaceSelectionChanged;
                    Interfaces.Add(vm);
                }
                else
                {
                    bool shouldBeSelected = !_disabledInterfaceIds.Contains(id);
                    if (vm.IsSelected != shouldBeSelected)
                    {
                        vm.SelectionChanged -= OnInterfaceSelectionChanged;
                        vm.IsSelected = shouldBeSelected;
                        vm.SelectionChanged += OnInterfaceSelectionChanged;
                    }
                }

                // Update stats in VM
                vm.RxTotal = $"Rx: {FormatBytes(currentRx)}";
                vm.TxTotal = $"Tx: {FormatBytes(currentTx)}";

                double speedRxMbps = (deltaRx / timeDeltaSeconds) / 125_000d;
                double speedTxMbps = (deltaTx / timeDeltaSeconds) / 125_000d;
                vm.CurrentSpeed = $"↓ {speedRxMbps:F2} Mb/s  ↑ {speedTxMbps:F2} Mb/s";

                // Add to global total if selected
                if (vm.IsSelected)
                {
                    totalReceivedBytesDelta += deltaRx;
                    totalSentBytesDelta += deltaTx;
                }
            }
        }
        catch
        {
            // Ignore errors
        }

        // Persisti i dati solo se sono positivi
        if (totalReceivedBytesDelta >= 0 && totalSentBytesDelta >= 0)
        {
            _storage.UpdateUsage(totalReceivedBytesDelta, totalSentBytesDelta);
        }

        // Conversione da byte/s a Mb/s
        var downloadMbps = Math.Max(0, (totalReceivedBytesDelta / timeDeltaSeconds) / 125_000d);
        var uploadMbps = Math.Max(0, (totalSentBytesDelta / timeDeltaSeconds) / 125_000d);

        // Aggiorna il ViewModel con i valori formattati a 2 decimali
        _viewModel.DownloadMbps = downloadMbps.ToString("F2");
        _viewModel.UploadMbps = uploadMbps.ToString("F2");

        _lastUpdateTime = now;
    }

    private void OnInterfaceSelectionChanged(object? sender, EventArgs e)
    {
        if (sender is NetworkInterfaceViewModel vm)
        {
            if (vm.IsSelected)
                _disabledInterfaceIds.Remove(vm.Id);
            else
                _disabledInterfaceIds.Add(vm.Id);

            _settingsProvider.SetSetting(NetStatsSill.DisabledInterfacesSetting, _disabledInterfaceIds.ToArray());
        }
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
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
