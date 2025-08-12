using PadConnect.Components.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace PadConnect.Components.Services
{
    public class MidiMessage(int pad, bool active) : EventArgs
    {
        public int Pad { get; } = pad;
        public bool Active { get; } = active;
    }

    /// <summary>
    /// This service is intended to handle MIDI operations with Launchpad.
    /// </summary>
    internal class MidiService
    {
        public event EventHandler<MidiMessage>? Message;
        public event EventHandler<bool>? StatusUpdate;

        private TypedEventHandler<MidiInPort, MidiMessageReceivedEventArgs>? _midiMessageReceivedHandler;

        private MidiInPort? _midiInPort;
        private MidiOutPort? _midiOutPort;

        private DeviceWatcher? _midiDeviceWatcher;

        private bool _connectionStatus = false;
        public bool ConnectionStatus { get => _connectionStatus; }

        /*public async void test()
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

            midiInPort = await MidiInPort.FromIdAsync(inD[1].Id);
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

            midiInPort.MessageReceived += MidiInPort_MessageReceived;
        }*/

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

        public async void SetMidiDevice(string deviceInId, string deviceOutId)
        {
            if (_midiMessageReceivedHandler != null && _midiInPort != null)
            {
                _midiInPort.MessageReceived -= _midiMessageReceivedHandler;
                _midiMessageReceivedHandler = null;
            }

            _midiInPort?.Dispose();

            _midiOutPort?.Dispose();

            try
            {
                _midiInPort = await MidiInPort.FromIdAsync(deviceInId);
                _midiOutPort = (MidiOutPort)await MidiOutPort.FromIdAsync(deviceOutId);

                _midiMessageReceivedHandler = MidiInPort_MessageReceived;
                _midiInPort.MessageReceived += _midiMessageReceivedHandler;

                if (_midiDeviceWatcher != null)
                {
                    _midiDeviceWatcher.Removed -= MidiDevice_Removed;
                }

                _midiOutPort.SendBuffer(LaunchpadSysExModel.SetProgrammerMode);

                StartMidiDeviceWatcher();
                StatusUpdate?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting MIDI device: {ex.Message}");
                _connectionStatus = false;
                StatusUpdate?.Invoke(this, false);
            }
        }

        private void StartMidiDeviceWatcher()
        {
            string midiSelector = MidiInPort.GetDeviceSelector() + " OR " + MidiOutPort.GetDeviceSelector();
            _midiDeviceWatcher = DeviceInformation.CreateWatcher(midiSelector);

            _midiDeviceWatcher.Removed += MidiDevice_Removed;
            _midiDeviceWatcher.Start();
        }

        private void MidiDevice_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            // Check if the removed device matches your current MIDI device IDs
            if (_midiInPort != null && args.Id == _midiInPort.DeviceId)
            {
                _midiInPort.MessageReceived -= _midiMessageReceivedHandler;
                _midiInPort.Dispose();
                _midiInPort = null;
                _connectionStatus = false;
            }
            if (_midiOutPort != null && args.Id == _midiOutPort.DeviceId)
            {
                _midiOutPort.Dispose();
                _midiOutPort = null;
                _connectionStatus = false;
            }

            StatusUpdate?.Invoke(this, false);
        }

        private void MidiInPort_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            if (args.Message.Type == MidiMessageType.NoteOn)
            {
                var data = args.Message as MidiNoteOnMessage;
                if (data != null && Message != null)
                {
                    Message.Invoke(this, new MidiMessage(data.Note, data.Velocity == 127));
                }
            }

            if (args.Message.Type == MidiMessageType.ControlChange)
            {
                var data = args.Message as MidiControlChangeMessage;
                if (data != null && Message != null)
                {
                    Message.Invoke(this, new MidiMessage(data.Controller, data.ControlValue == 127));
                }
            }
        }

        public void Clear()
        {
            _midiOutPort?.SendBuffer(LaunchpadSysExModel.SetColor(0, 0, 8, 8, LaunchpadSysExModel.LightMode.STATIC, 0));
        }
    }
}
