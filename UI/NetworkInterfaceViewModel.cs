using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetStats.UI;

public class NetworkInterfaceViewModel : INotifyPropertyChanged
{
    private bool _isSelected;
    private string _rxTotal = "";
    private string _txTotal = "";
    private string _currentSpeed = "";

    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public string RxTotal
    {
        get => _rxTotal;
        set { if (_rxTotal != value) { _rxTotal = value; OnPropertyChanged(); } }
    }

    public string TxTotal
    {
        get => _txTotal;
        set { if (_txTotal != value) { _txTotal = value; OnPropertyChanged(); } }
    }

    public string CurrentSpeed
    {
        get => _currentSpeed;
        set { if (_currentSpeed != value) { _currentSpeed = value; OnPropertyChanged(); } }
    }

    public event EventHandler? SelectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
