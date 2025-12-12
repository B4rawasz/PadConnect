using PadConnect.Components.Models.OBS_WebSocket.Events;
using PadConnect.Components.Models.OBS_WebSocket.Events.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PadConnect.Components.Services
{
    internal partial class WebSocketService
    {
        //Scenes
        public event EventHandler<CurrentPreviewSceneChanged>? CurrentPreviewSceneChanged;
        public event EventHandler<CurrentProgramSceneChanged>? CurrentProgramSceneChanged;
        public event EventHandler<SceneCreated>? SceneCreated;
        public event EventHandler<SceneListChanged>? SceneListChanged;
        public event EventHandler<SceneNameChanged>? SceneNameChanged;
        public event EventHandler<SceneRemoved>? SceneRemoved;

        private Dictionary<EventType, Action<JsonElement?>>? _eventHandlers;
        private Dictionary<EventType, Action<JsonElement?>> EventHandlers => _eventHandlers ??= new()
        {
            { EventType.SceneCreated, data => HandleEvent(data, SceneCreated) },
            { EventType.SceneRemoved, data => HandleEvent(data, SceneRemoved) },
            { EventType.SceneNameChanged, data => HandleEvent(data, SceneNameChanged) },
            { EventType.CurrentProgramSceneChanged, data => HandleEvent(data, CurrentProgramSceneChanged) },
            { EventType.CurrentPreviewSceneChanged, data => HandleEvent(data, CurrentPreviewSceneChanged) },
            { EventType.SceneListChanged, data => HandleEvent(data, SceneListChanged) }
        };

        private void HandleEvent<T>(JsonElement? eventData, EventHandler<T>? handler) where T : class
        {
            if (eventData == null || handler == null) return;

            var data = eventData.Value.Deserialize<T>();
            if (data != null)
            {
                handler.Invoke(this, data);
            }
        }
    }
}
