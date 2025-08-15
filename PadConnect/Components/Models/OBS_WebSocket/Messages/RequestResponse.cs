using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models.OBS_WebSocket.Messages
{
    internal class RequestResponse : Message
    {
        public new MessageRequestResponseData? d { get; set; } // Data payload specific to RequestResponse message
    }

    internal class MessageRequestResponseData
    {
        public string? requestType { get; set; }
        public string? requestId { get; set; }
        public object? requestStatus { get; set; }
        public object? responseData { get; set; }
    }
}
