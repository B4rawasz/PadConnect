using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PadConnect.Components.Models
{
    /// <summary>  
    /// SysEx comunication model for Launchpad devices.  
    /// </summary>  
    /// <remarks>Tested only for Launchpad Mini MK3.</remarks>  
    internal class LaunchpadSysExModel
    {
        private static readonly byte[] _header = { 0xF0, 0x00, 0x20, 0x29, 0x02, 0x0D };
        private static readonly byte[] _footer = { 0xF7 };

        private static readonly byte[] _setProgrammerMode = { 0x00, 0x7F };
        private static readonly byte[] _setNnormalMode = { 0x00, 0x06 };

        private static readonly byte[] _setColor = { 0x03 };

        public enum LightMode
        {
            STATIC = 0x00,
            FLASHING = 0x01,
            PULSING = 0x02,
            CUSTOM = 0x03
        }

        public enum SliderType
        {
            VERTICAL = 0x00,
            HORIZONTAL = 0x01
        }

        /// <summary>  
        /// SysEx message to set the Launchpad device into programmer mode.  
        /// </summary>  
        public static readonly IBuffer SetProgrammerMode = BuildSysExMessage(_setProgrammerMode);

        /// <summary>  
        /// SysEx message to set the Launchpad device into normal mode.  
        /// </summary>  
        public static readonly IBuffer SetNormalMode = BuildSysExMessage(_setNnormalMode);

        /// <summary>  
        /// Translates the X and Y coordinates of a Launchpad button to its corresponding byte value.  
        /// </summary>  
        /// <param name="x">X position.</param>  
        /// <param name="y">Y position.</param>  
        /// <returns>Layout value as byte.</returns>  
        /// <exception cref="ArgumentOutOfRangeException">X and Y must be between 0 and 8.</exception>  
        public static byte TranslatePosition(byte x, byte y)
        {
            if (x < 0 || x > 8 || y < 0 || y > 8)
                throw new ArgumentOutOfRangeException("X and Y must be between 0 and 8.");

            return (byte)((y + 1) * 10 + x + 1);
        }

        /// <summary>  
        /// Translates a Launchpad layout value (11-99) to its X and Y coordinates.  
        /// </summary>  
        /// <param name="layoutValue">Layout value as byte.</param>  
        /// <returns>X and Y cordinates</returns>  
        /// <exception cref="ArgumentOutOfRangeException">Layout value must be between 11 and 99.</exception>  
        public static Tuple<byte, byte> TranslatePosition(byte layoutValue)
        {
            if (layoutValue < 11 || layoutValue > 99)
                throw new ArgumentOutOfRangeException("Layout value must be between 11 and 99.");
            byte x = (byte)((layoutValue - 1) % 10);
            byte y = (byte)((layoutValue - 1) / 10 - 1);

            if (x < 0 || x > 8 || y < 0 || y > 8)
                throw new ArgumentOutOfRangeException("Incorect layout value.");

            return new Tuple<byte, byte>(x, y);
        }

        /// <summary>  
        /// Sets the color of a Launchpad button at the specified X and Y coordinates with the given light mode and color(s).  
        /// </summary>  
        /// <param name="x">X cordinate</param>  
        /// <param name="y">Y cordinate</param>  
        /// <param name="mode">Light style</param>  
        /// <param name="color">Light color</param>  
        /// <returns>SysEx command</returns>  
        /// <exception cref="ArgumentException">Color checking for difrent modes</exception>  
        public static IBuffer SetColor(byte x, byte y, LightMode mode, params byte[] color)
        {
            if (mode == LightMode.FLASHING)
            {
                if (color.Length != 2)
                    throw new ArgumentException("For FLASHING mode, you must provide exactly 2 colors.");
            }
            else if (mode == LightMode.CUSTOM)
            {
                if (color.Length != 3)
                    throw new ArgumentException("For CUSTOM mode, you must provide exactly 3 colors (R, G, B).");
            }
            else
            {
                if (color.Length != 1)
                    throw new ArgumentException("For STATIC or PULSING mode, you must provide exactly 1 color.");
            }

            var payload = new List<byte> { (byte)mode, TranslatePosition(x, y) };
            payload.AddRange(color);

            return BuildSysExMessage(_setColor, [.. payload]);
        }

        /// <summary>
        /// Sets the color of a Launchpad button defined by its layout value with the given light mode and color(s).
        /// </summary>
        /// <param name="layoutValue">Layout value as byte.></param>
        /// <param name="mode">Light style</param>  
        /// <param name="color">Light color</param>  
        /// <returns>SysEx command</returns>
        /// <exception cref="ArgumentException">Color checking for difrent modes</exception>
        public static IBuffer SetColor(byte layoutValue, LightMode mode, params byte[] color)
        {
            if (mode == LightMode.FLASHING)
            {
                if (color.Length != 2)
                    throw new ArgumentException("For FLASHING mode, you must provide exactly 2 colors.");
            }
            else if (mode == LightMode.CUSTOM)
            {
                if (color.Length != 3)
                    throw new ArgumentException("For CUSTOM mode, you must provide exactly 3 colors (R, G, B).");
            }
            else
            {
                if (color.Length != 1)
                    throw new ArgumentException("For STATIC or PULSING mode, you must provide exactly 1 color.");
            }

            var payload = new List<byte> { (byte)mode, layoutValue };
            payload.AddRange(color);

            return BuildSysExMessage(_setColor, [.. payload]);
        }

        /// <summary>  
        /// Sets the color of a range of Launchpad buttons defined by their X and Y coordinates with the given light mode and color(s).  
        /// </summary>  
        /// <param name="x1">Bottom left X cordinate</param>  
        /// <param name="y1">Bottom left Y cordinate</param>  
        /// <param name="x2">Top left X cordinate</param>  
        /// <param name="y2">Top left Y cordinate</param>  
        /// <param name="mode">Light style</param>  
        /// <param name="color">Light color</param>  
        /// <returns>SysEx command</returns>  
        /// <exception cref="ArgumentException">Color checking for difrent modes.</exception>  
        /// <exception cref="ArgumentException">X1 and Y1 must be lowwer than X2 and Y2</exception>  
        public static IBuffer SetColor(byte x1, byte y1, byte x2, byte y2, LightMode mode, params byte[] color)
        {
            if (mode == LightMode.FLASHING)
            {
                if (color.Length != 2)
                    throw new ArgumentException("For FLASHING mode, you must provide exactly 2 colors.");
            }
            else
            {
                if (color.Length != 1)
                    throw new ArgumentException("For STATIC or PULSING mode, you must provide exactly 1 color.");
            }

            if (x1 > x2 || y1 > y2)
                throw new ArgumentException("Invalid range: x1 must be less than or equal to x2, and y1 must be less than or equal to y2.");

            var payload = new List<byte>();

            for (byte x = x1; x <= x2; x++)
            {
                for (byte y = y1; y <= y2; y++)
                {
                    payload.Add((byte)mode);
                    payload.Add(TranslatePosition(x, y));
                    payload.AddRange(color);
                }
            }

            return BuildSysExMessage(_setColor, [.. payload]);
        }

        /// <summary>
        /// Sets the color of a range of Launchpad buttons defined by their percentage values.
        /// </summary>
        /// <param name="position">Position on X or Y axis</param>
        /// <param name="type">Direction</param>
        /// <param name="percentage">Slider percentage</param>
        /// <param name="color">Slider color (RGB)</param>
        /// <returns>SysEx command</returns>
        /// <exception cref="ArgumentOutOfRangeException">Position and percentage check</exception>
        /// <exception cref="ArgumentException">RGB check</exception>
        public static IBuffer SetSlider(byte position, SliderType type, float percentage, params byte[] color)
        {
            if (position < 0 || position > 8)
                throw new ArgumentOutOfRangeException("Position must be between 0 and 8.");

            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException("Percentage must be between 0 and 100.");

            if (color.Length != 3)
                throw new ArgumentException("For Slider, you must provide exactly 3 colors (R, G, B).");

            var payload = new List<byte>();

            byte length = (byte)Math.Ceiling(percentage * 8.0f / 100f);

            for(byte i = 0; i < length - 1; i++)
            {
                payload.Add((byte)LightMode.CUSTOM);
                if (type == SliderType.VERTICAL)
                {
                    payload.Add(TranslatePosition(position, i));
                }
                else
                {
                    payload.Add(TranslatePosition(i, position));
                }
                payload.AddRange(color);
            }

            for (byte i = length; i < 8; i++)
            {
                payload.Add((byte)LightMode.STATIC);
                if (type == SliderType.VERTICAL)
                {
                    payload.Add(TranslatePosition(position, i));
                }
                else
                {
                    payload.Add(TranslatePosition(i, position));
                }
                payload.Add(0);
            }

            float lastDiodPercentage = percentage - ((length - 1) * 12.5f);

            byte lastDiodPower = lastDiodPercentage >= 12.5f || percentage == 100f
                ? (byte)4
                : (byte)Math.Clamp(Math.Ceiling(lastDiodPercentage / 3.125f), 1, 4);

            payload.Add((byte)LightMode.CUSTOM);
            if (type == SliderType.VERTICAL)
            {
                payload.Add(TranslatePosition(position, (byte)Math.Clamp(length - 1, 0, 7)));
            }
            else
            {
                payload.Add(TranslatePosition((byte)Math.Clamp(length - 1, 0, 7), position));
            }
            payload.Add((byte)(color[0] / (1 << (4 - lastDiodPower))));
            payload.Add((byte)(color[1] / (1 << (4 - lastDiodPower))));
            payload.Add((byte)(color[2] / (1 << (4 - lastDiodPower))));

            return BuildSysExMessage(_setColor, [.. payload]);
        }

        /// <summary>  
        /// Builds a SysEx message for the Launchpad device from multiple payload arrays.  
        /// </summary>  
        /// <param name="payloads">One or more byte arrays to be concatenated as payload.</param>  
        /// <returns>Completed SysEx message.</returns>  
        private static IBuffer BuildSysExMessage(params byte[][] payloads)
        {
            int payloadLength = payloads.Sum(arr => arr.Length);

            var result = new byte[_header.Length + payloadLength + _footer.Length];
            int offset = 0;

            System.Buffer.BlockCopy(_header, 0, result, offset, _header.Length);
            offset += _header.Length;

            foreach (var arr in payloads)
            {
                System.Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }

            System.Buffer.BlockCopy(_footer, 0, result, offset, _footer.Length);

            Debug.WriteLine($"SysEx Message: {BitConverter.ToString(result)}");

            using (var writer = new DataWriter())
            {
                writer.WriteBytes(result);
                return writer.DetachBuffer();
            }
        }
    }
}
