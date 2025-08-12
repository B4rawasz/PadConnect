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
    public class MidiMessage(byte pad, bool active) : EventArgs
    {
        public byte Pad { get; } = pad;
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
