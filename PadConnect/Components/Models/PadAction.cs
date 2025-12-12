namespace PadConnect.Components.Models
{
    public enum PadAction
    {
        None,
        // Basic actions
        MuteMicrophone,
        UnmuteMicrophone,
        ToggleMicrophone,
        SwitchScene,
        StartStreaming,
        StopStreaming,
        StartRecording,
        StopRecording,
        // Slider actions (only for x=0 or y=0)
        VolumeControl,
        MicrophoneGainControl,
        DesktopAudioControl,
        // Corner specific actions (only for x=0, y=0)
        SliderOrientation
    }
}