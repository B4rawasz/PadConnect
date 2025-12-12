using PadConnect.Components.Models;
using PadConnect.Components.Models.OBS_WebSocket.Events.Scenes;
using PadConnect.Components.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Devices.Enumeration;
using static Microsoft.IO.RecyclableMemoryStreamManager;

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

    private PanelModel? _selectedPanel;
    public PanelModel? SelectedPanel
    {
        get => _selectedPanel;
        set { _selectedPanel = value; OnPropertyChanged(); }
    }

    public bool PopupVisible => CurrentPopupType != PopupType.None;

    public ObservableCollection<PanelModel> GridPanels { get; } = new();
    public ObservableCollection<DeviceInformation> MidiInDevices { get; } = new();
    public ObservableCollection<DeviceInformation> MidiOutDevices { get; } = new();
    
    // Available scenes for scene switching
    public Dictionary<string, string> AvailableScenes { get; } = new()
    {
        { "Scene 1", "scene-uuid-1" },
        { "Scene 2", "scene-uuid-2" },
        { "Scene 3", "scene-uuid-3" },
        { "Gaming Scene", "scene-uuid-gaming" },
        { "Just Chatting", "scene-uuid-chatting" }
    };

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
        _webSocketService.CurrentProgramSceneChanged += HandleCurrentProgramSceneChanged;
        LoadMidiDevices();
    }

    private void HandleCurrentProgramSceneChanged(object? sender, CurrentProgramSceneChanged e)
    {
        Debug.WriteLine($"Current program scene changed to: {e.sceneName}");
        _midiService.SetPanel(1, 1, 19);
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

    public void ShowPadSettingsPopup(PanelModel panel)
    {
        SelectedPanel = panel;
        CurrentPopupType = PopupType.PadSettings;
    }

    public void ClosePopup()
    {
        CurrentPopupType = PopupType.None;
        SelectedPanel = null;
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

    public void ApplyPadSettings()
    {
        if (SelectedPanel != null)
        {
            Debug.WriteLine($"Applying pad settings for panel ({SelectedPanel.X}, {SelectedPanel.Y})");
            Debug.WriteLine($"Action: {SelectedPanel.Settings.Action}");
            Debug.WriteLine($"Light Mode: {SelectedPanel.Settings.LightMode}");
            Debug.WriteLine($"Light Color: {SelectedPanel.Settings.LightColor}");
            
            // Here you would implement the actual pad configuration logic
            // For example, set the light color on the physical device
            // _midiService.SetPadLight(SelectedPanel.X, SelectedPanel.Y, SelectedPanel.Settings);
        }
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

    public void AssignActionToPanel(PanelModel p, int v)
    {
        Debug.WriteLine($"{p.X} {p.Y} {v}");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}