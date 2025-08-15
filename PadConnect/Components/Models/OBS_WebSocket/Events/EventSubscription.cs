using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models.OBS_WebSocket.Events
{
    public enum EventSubscription
    {
        None = 0,
        General = 1 << 0,
        Config = 1 << 1,
        Scenes = 1 << 2,
        Inputs = 1 << 3,
        Transitions = 1 << 4,
        Filters = 1 << 5,
        Outputs = 1 << 6,
        SceneItems = 1 << 7,
        MediaInputs = 1 << 8,
        Vendors = 1 << 9,
        Ui = 1 << 10,
        InputVolumeMeters = 1 << 16,
        InputActiveStateChanged = 1 << 17,
        InputShowStateChanged = 1 << 18,
        SceneItemTransformChanged = 1 << 19,
        All = General | Config | Scenes | Inputs | Transitions | Filters | Outputs | SceneItems | MediaInputs | Vendors | Ui,
        AllHighVolume = All | InputVolumeMeters | InputActiveStateChanged | InputShowStateChanged | SceneItemTransformChanged
    }
}
