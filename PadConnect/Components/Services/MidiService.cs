using PadConnect.Components.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using Windows.Storage.Streams;

namespace PadConnect.Components.Services
{
    /// <summary>
    /// This service is intended to handle MIDI operations with Launchpad.
    /// </summary>
    internal class MidiService
    {
        public MidiService() {
            test();
        }

        MidiInPort midiInPort;
        MidiOutPort midiOutPort;

        public async void test()
        {
            var inD = await GetMidiInDevicesAsync();
            var outD = await GetMidiOutDevicesAsync();

            Debug.WriteLine("MIDI IN Devices:");
            foreach (var device in inD)
            {
                Debug.WriteLine($"- {device.Name} ({device.Id})");
            }

            Debug.WriteLine("MIDI OUT Devices:");
            foreach (var device in outD)
            {
                Debug.WriteLine($"- {device.Name} ({device.Id})");
            }

            // Example of how to use the devices

            midiInPort = await MidiInPort.FromIdAsync(inD[0].Id);
            midiOutPort = (MidiOutPort)await MidiOutPort.FromIdAsync(outD[1].Id);

            midiOutPort.SendBuffer(LaunchpadSysExModel.SetProgrammerMode);
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetColor(0, 0, 2, 2, LaunchpadSysExModel.LightMode.STATIC, 21));
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetColor(0, 3, LaunchpadSysExModel.LightMode.STATIC, 4));
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetColor(1, 3, LaunchpadSysExModel.LightMode.STATIC, 5));
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetColor(2, 3, LaunchpadSysExModel.LightMode.STATIC, 6));
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetColor(3, 3, LaunchpadSysExModel.LightMode.STATIC, 7));

            midiOutPort.SendBuffer(LaunchpadSysExModel.SetColor(0, 4, LaunchpadSysExModel.LightMode.STATIC, 20));
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetColor(1, 4, LaunchpadSysExModel.LightMode.STATIC, 21));
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetColor(2, 4, LaunchpadSysExModel.LightMode.STATIC, 22));
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetColor(3, 4, LaunchpadSysExModel.LightMode.STATIC, 23));

            midiOutPort.SendBuffer(LaunchpadSysExModel.SetColor(0, 5, LaunchpadSysExModel.LightMode.CUSTOM, 127, 0, 0));
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetColor(1, 5, LaunchpadSysExModel.LightMode.CUSTOM, 63, 0, 0));
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetColor(2, 5, LaunchpadSysExModel.LightMode.CUSTOM, 31, 0, 0));
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetColor(3, 5, LaunchpadSysExModel.LightMode.CUSTOM, 15, 0, 0));

            midiOutPort.SendBuffer(LaunchpadSysExModel.SetSlider(7, LaunchpadSysExModel.SliderType.VERTICAL, 75.0f, 73, 56, 109));
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetSlider(6, LaunchpadSysExModel.SliderType.VERTICAL, 71.7f, 73, 56, 109));
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetSlider(5, LaunchpadSysExModel.SliderType.VERTICAL, 65.7f, 73, 56, 109));
            midiOutPort.SendBuffer(LaunchpadSysExModel.SetSlider(4, LaunchpadSysExModel.SliderType.VERTICAL, 62.6f, 73, 56, 109));

            midiOutPort.SendBuffer(LaunchpadSysExModel.SetSlider(7, LaunchpadSysExModel.SliderType.HORIZONTAL, 65.7f, 37, 0, 65));
        }

        public async Task<IReadOnlyList<DeviceInformation>> GetMidiInDevicesAsync()
        {
            // Identyfikator klasy urządzenia MIDI IN
            string midiInSelector = MidiInPort.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(midiInSelector);
            return devices;
        }

        public async Task<IReadOnlyList<DeviceInformation>> GetMidiOutDevicesAsync()
        {
            // Identyfikator klasy urządzenia MIDI OUT
            string midiOutSelector = MidiOutPort.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(midiOutSelector);
            return devices;
        }

        public void Clear()
        {
            midiOutPort?.SendBuffer(LaunchpadSysExModel.SetColor(0, 0, 8, 8, LaunchpadSysExModel.LightMode.STATIC, 0));
        }
    }
}
