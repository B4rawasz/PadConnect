using PadConnect.Components.Models.OBS_WebSocket.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models.OBS_WebSocket.Messages
{
    internal class Event : Message
    {
        public new MessageEventData? d { get; set; } // Data payload specific to Event message
    }

    internal class MessageEventData
    {
        public EventType? eventType { get; set; }
        public int? eventIntent { get; set; }
        public object? eventData { get; set; }
    }
}
