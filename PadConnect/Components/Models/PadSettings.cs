using Microsoft.Maui.Graphics;

namespace PadConnect.Components.Models
{
    public class PadSettings
    {
        public PadAction Action { get; set; } = PadAction.None;
        public string? SceneUUID { get; set; }
        public string? SceneName { get; set; }
        public LightMode LightMode { get; set; } = LightMode.Static;
        public Color LightColor { get; set; } = Colors.White;
        public byte RgbRed { get; set; } = 255;
        public byte RgbGreen { get; set; } = 255;
        public byte RgbBlue { get; set; } = 255;
        public SliderOrientation SliderOrientation { get; set; } = SliderOrientation.Vertical;
        public float SliderValue { get; set; } = 50.0f;
    }
}