using PadConnect.Components.Models;
using PadConnect.Components.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Devices.Enumeration;

public class HomeViewModel : INotifyPropertyChanged
{
    private string _selectedInDeviceId = "";
    public string SelectedInDeviceId
    {
        get => _selectedInDeviceId;
        set { _selectedInDeviceId = value; OnPropertyChanged(); }
    }

    private string _selectedOutDeviceId = "";
    public string SelectedOutDeviceId
    {
        get => _selectedOutDeviceId;
        set { _selectedOutDeviceId = value; OnPropertyChanged(); }
    }

    private string _webSocketUrl = "ws://localhost:4455";
    public string WebSocketUrl
    {
        get => _webSocketUrl;
        set { _webSocketUrl = value; OnPropertyChanged(); }
    }

    private string _webSocketPassword = "123456";
    public string WebSocketPassword
    {
        get => _webSocketPassword;
        set { _webSocketPassword = value; OnPropertyChanged(); }
    }

    private bool _autoReconnect = true;
    public bool AutoReconnect
    {
        get => _autoReconnect;
        set { _autoReconnect = value; OnPropertyChanged(); }
    }

    private PopupType _currentPopupType = PopupType.None;
    public PopupType CurrentPopupType
    {
        get => _currentPopupType;
        set { _currentPopupType = value; OnPropertyChanged(); OnPropertyChanged(nameof(PopupVisible)); }
    }

    private bool _midiConnected = false;
    public bool MidiConnected
    {
        get => _midiConnected;
        set { _midiConnected = value; OnPropertyChanged(); }
    }

    private bool _webSocketConnected = false;
    public bool WebSocketConnected
    {
        get => _webSocketConnected;
        set { _webSocketConnected = value; OnPropertyChanged(); }
    }

    public bool PopupVisible => CurrentPopupType != PopupType.None;

    public ObservableCollection<PanelModel> GridPanels { get; } = new();
    public ObservableCollection<DeviceInformation> MidiInDevices { get; } = new();
    public ObservableCollection<DeviceInformation> MidiOutDevices { get; } = new();

    private MidiService _midiService = new();
    private WebSocketService _webSocketService = new();

    public HomeViewModel()
    {
        for (byte y = 8; y >= 0 && y < 10; y--)
        {
            for (byte x = 0; x < 9; x++)
            {
                var panel = new PanelModel { X = x, Y = y };
                panel.PropertyChanged += (_, _) => OnPropertyChanged(nameof(GridPanels));
                GridPanels.Add(panel);
            }
        }

        _midiService.Message += OnMidiMessage;
        _midiService.StatusUpdate += (s, e) => MidiConnected = e;
        _webSocketService.StatusUpdate += (s, e) => WebSocketConnected = e;
        LoadMidiDevices();
    }

    private async void LoadMidiDevices()
    {
        var inDevices = await _midiService.GetMidiInDevicesAsync();
        var outDevices = await _midiService.GetMidiOutDevicesAsync();

        MidiInDevices.Clear();
        MidiOutDevices.Clear();

        foreach (var device in inDevices)
        {
            MidiInDevices.Add(device);
        }

        foreach (var device in outDevices)
        {
            MidiOutDevices.Add(device);
        }
    }

    public void ShowDevicePopup()
    {
        CurrentPopupType = PopupType.DeviceSettings;
    }

    public void ShowWebSocketPopup()
    {
        CurrentPopupType = PopupType.WebSocketSettings;
    }

    public void ClosePopup()
    {
        CurrentPopupType = PopupType.None;
    }

    public void ApplyDeviceSettings()
    {
        Debug.WriteLine($"Applying devices - In: {SelectedInDeviceId}, Out: {SelectedOutDeviceId}");
        _midiService.SetMidiDevice(SelectedInDeviceId, SelectedOutDeviceId);
        ClosePopup();
    }

    public void ApplyWebSocketSettings()
    {
        Debug.WriteLine($"Applying WebSocket settings - URL: {WebSocketUrl}, Password: {WebSocketPassword}, AutoReconnect: {AutoReconnect}");
        _webSocketService.SetWebsocket(WebSocketUrl, WebSocketPassword, AutoReconnect);
        ClosePopup();
    }

    private void OnMidiMessage(object? sender, MidiMessage e)
    {
        Tuple<byte, byte> position = LaunchpadSysExModel.TranslatePosition(e.Pad);

        var panel = GridPanels.FirstOrDefault(p => p.X == position.Item1 && p.Y == position.Item2);
        if(panel != null)
        {
            Debug.WriteLine($"{position.Item1} {position.Item2} {e.Active}");
            if (e.Active)
            {
                panel.IsActive = !panel.IsActive;
            }
        }
    }

    public void PanelPressed(PanelModel panel)
    {
        Debug.WriteLine($"Button pressed at ({panel.Label})");
        _midiService.Clear();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}