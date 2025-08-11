using PadConnect.Components.Models;
using PadConnect.Components.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class HomeViewModel : INotifyPropertyChanged
{
    private string _message;
    public string Message
    {
        get => _message;
        set { _message = value; OnPropertyChanged(); }
    }

    public ObservableCollection<PanelModel> GridPanels { get; } = new();

    private MidiService _midiService = new();

    public HomeViewModel()
    {
        _message = "";

        for (int y = 8; y >= 0; y--)
        {
            for (int x = 0; x < 9; x++)
            {
                GridPanels.Add(new PanelModel { X = x, Y = y });
            }
        }
    }

    public void PanelPressed(PanelModel panel)
    {
        // Handle the button press logic here
        // For example, log or update state
        Debug.WriteLine($"Button pressed at ({panel.Label})");
        OnPropertyChanged(nameof(GridPanels));
        _midiService.Clear();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}