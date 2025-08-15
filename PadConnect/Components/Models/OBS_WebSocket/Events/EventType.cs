using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PadConnect.Components.Models.OBS_WebSocket.Events
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EventType
    {
        ExitStarted,
        VendorEvent,
        CustomEvent,
        CurrentSceneCollectionChanging,
        CurrentSceneCollectionChanged,
        SceneCollectionListChanged,
        CurrentProfileChanging,
        CurrentProfileChanged,
        ProfileListChanged,
        SceneCreated,
        SceneRemoved,
        SceneNameChanged,
        CurrentProgramSceneChanged,
        CurrentPreviewSceneChanged,
        SceneListChanged,
        InputCreated,
        InputRemoved,
        InputNameChanged,
        InputSettingsChanged,
        InputActiveStateChanged,
        InputShowStateChanged,
        InputMuteStateChanged,
        InputVolumeChanged,
        InputAudioBalanceChanged,
        InputAudioSyncOffsetChanged,
        InputAudioTracksChanged,
        InputAudioMonitorTypeChanged,
        InputVolumeMeters,
        CurrentSceneTransitionChanged,
        CurrentSceneTransitionDurationChanged,
        SceneTransitionStarted,
        SceneTransitionEnded,
        SceneTransitionVideoEnded,
        SourceFilterListReindexed,
        SourceFilterCreated,
        SourceFilterRemoved,
        SourceFilterNameChanged,
        SourceFilterSettingsChanged,
        SourceFilterEnableStateChanged,
        SceneItemCreated,
        SceneItemRemoved,
        SceneItemListReindexed,
        SceneItemEnableStateChanged,
        SceneItemLockStateChanged,
        SceneItemSelected,
        SceneItemTransformChanged,
        StreamStateChanged,
        RecordStateChanged,
        RecordFileChanged,
        ReplayBufferStateChanged,
        VirtualcamStateChanged,
        ReplayBufferSaved,
        MediaInputPlaybackStarted,
        MediaInputPlaybackEnded,
        MediaInputActionTriggered,
        StudioModeStateChanged,
        ScreenshotSaved
    }
}
