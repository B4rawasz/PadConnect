using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models
{
    public class PanelModel : INotifyPropertyChanged
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Label => $"{X}, {Y}";
        public Color? BackgroundColor { get; set; }
        public string? SubscribedUUID { get; set; }
        public ControlEvents? SubscribedEvent { get; set; }
        
        public PadSettings Settings { get; set; } = new PadSettings();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
