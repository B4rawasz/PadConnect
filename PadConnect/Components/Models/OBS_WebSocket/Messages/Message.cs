using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models.OBS_WebSocket.Messages
{
    internal class Message
    {
        public MessageOpCode op { get; set; } // Operation code
        public object? d { get; set; } // Data payload
    }
}
