using System.Text.Json;
using WindowSill.API;
using Path = System.IO.Path;

namespace NetStats.Models;

/// <summary>
/// Gestisce la persistenza dei dati di utilizzo della rete.
/// Salva i dati giornalieri, mensili e annuali in formato JSON.
/// </summary>
public sealed class NetworkUsageStorage
{
    private readonly string _storageFilePath;
    private NetworkUsagePersistentData _data = new();

    public NetworkUsageStorage(IPluginInfo pluginInfo)
    {
        string _pluginDataFolder = pluginInfo.GetPluginDataFolder();
        _storageFilePath = Path.Combine(_pluginDataFolder, "network_usage_data.json");

        LoadData();
    }

    /// <summary>
    /// Ottiene i dati di utilizzo per il giorno corrente.
    /// </summary>
    public NetworkUsageData GetDailyData()
    {
        var today = DateTime.UtcNow.Date;
        
        // Se i dati sono di un giorno diverso, reset
        if (_data.DailyData.LastUpdated.Date != today)
        {
            _data.DailyData = new NetworkUsageData();
            SaveData();
        }

        return _data.DailyData;
    }

    /// <summary>
    /// Ottiene i dati di utilizzo per il mese corrente.
    /// </summary>
    public NetworkUsageData GetMonthlyData()
    {
        var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        
        // Se i dati sono di un mese diverso, reset
        if (new DateTime(_data.MonthlyData.LastUpdated.Year, _data.MonthlyData.LastUpdated.Month, 1) != currentMonth)
        {
            _data.MonthlyData = new NetworkUsageData();
            SaveData();
        }

        return _data.MonthlyData;
    }

    /// <summary>
    /// Ottiene i dati di utilizzo per l'anno corrente.
    /// </summary>
    public NetworkUsageData GetYearlyData()
    {
        var currentYear = DateTime.UtcNow.Year;
        
        // Se i dati sono di un anno diverso, reset
        if (_data.YearlyData.LastUpdated.Year != currentYear)
        {
            _data.YearlyData = new NetworkUsageData();
            SaveData();
        }

        return _data.YearlyData;
    }

    /// <summary>
    /// Aggiorna tutti i periodi con i nuovi byte trasferiti.
    /// </summary>
    public void UpdateUsage(long receivedBytes, long sentBytes)
    {
        GetDailyData().AddBytes(receivedBytes, sentBytes);
        GetMonthlyData().AddBytes(receivedBytes, sentBytes);
        GetYearlyData().AddBytes(receivedBytes, sentBytes);
        
        SaveData();
    }

    private void LoadData()
    {
        try
        {
            if (File.Exists(_storageFilePath))
            {
                var json = File.ReadAllText(_storageFilePath);
                _data = JsonSerializer.Deserialize<NetworkUsagePersistentData>(json) 
                    ?? new NetworkUsagePersistentData();
            }
            else
            {
                _data = new NetworkUsagePersistentData();
            }
        }
        catch
        {
            // Se c'è un errore nel caricamento, inizializza dati vuoti
            _data = new NetworkUsagePersistentData();
        }
    }

    private void SaveData()
    {
        try
        {
            var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(_storageFilePath, json);
        }
        catch
        {
            // Ignora errori di salvataggio
        }
    }
}

/// <summary>
/// Struttura dati per la persistenza.
/// </summary>
internal sealed class NetworkUsagePersistentData
{
    public NetworkUsageData DailyData { get; set; } = new();
    public NetworkUsageData MonthlyData { get; set; } = new();
    public NetworkUsageData YearlyData { get; set; } = new();
}
