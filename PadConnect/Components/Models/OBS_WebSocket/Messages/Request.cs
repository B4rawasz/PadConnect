using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models.OBS_WebSocket.Messages
{
    internal class Request : Message
    {
        public new WebSocketOpCode op { get; set; } = WebSocketOpCode.Request;
        public new required MessageRequestData d { get; set; } // Data payload specific to Request message
    }

    internal class MessageRequestData
    {
        public required string requestType { get; set; }
        public required string requestId { get; set; }
        public object? requestData { get; set; }
    }
}
