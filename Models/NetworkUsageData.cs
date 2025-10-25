namespace NetStats.Models;

/// <summary>
/// Rappresenta i dati di utilizzo della rete per un periodo specifico.
/// </summary>
public sealed class NetworkUsageData
{
    public long TotalBytesReceived { get; set; }
    public long TotalBytesSent { get; set; }
    public DateTime LastUpdated { get; set; }

    public NetworkUsageData()
    {
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Aggiunge i byte ricevuti e inviati al totale.
    /// </summary>
    public void AddBytes(long receivedBytes, long sentBytes)
    {
        TotalBytesReceived += receivedBytes;
        TotalBytesSent += sentBytes;
        LastUpdated = DateTime.UtcNow;
    }

    /// <summary>
    /// Formatta i byte in una stringa leggibile (GB, MB, KB).
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        const long GB = 1024L * 1024L * 1024L;
        const long MB = 1024L * 1024L;
        const long KB = 1024L;

        if (bytes >= GB)
            return $"{bytes / (double)GB:F2} GB";
        if (bytes >= MB)
            return $"{bytes / (double)MB:F2} MB";
        if (bytes >= KB)
            return $"{bytes / (double)KB:F2} KB";
        
        return $"{bytes} B";
    }
}
