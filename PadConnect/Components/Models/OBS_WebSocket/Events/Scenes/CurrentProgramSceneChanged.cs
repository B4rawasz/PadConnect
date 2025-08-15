using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models.OBS_WebSocket.Events.Scenes
{
    internal class CurrentProgramSceneChanged
    {
        public string? sceneName { get; set; }
        public string? sceneUuid { get; set; }
    }
}
