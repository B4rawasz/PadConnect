using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models
{
    public class PanelModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsActive { get; set; }
        public string Label => $"{X}, {Y}";
        public Color BackgroundColor { get; set; }
    }
}
