using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models.OBS_WebSocket.Events.Scenes
{
    internal class SceneListChanged
    {
        public List<SceneListChangedItem>? scenes { get; set; }
    }

    internal class SceneListChangedItem
    {
        public int? sceneIndex { get; set; }
        public string? sceneName { get; set; }
        public string? sceneUuid { get; set; }
    }
}
