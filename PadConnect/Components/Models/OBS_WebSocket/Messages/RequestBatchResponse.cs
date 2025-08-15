using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models.OBS_WebSocket.Messages
{
    internal class RequestBatchResponse : Message
    {
        public new MessageRequestBatchResponseData? d { get; set; }
    }

    internal class MessageRequestBatchResponseData
    {
        public string? requestId { get; set; }
        public List<object>? responses { get; set; }
    }
}
