using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models.OBS_WebSocket.Messages
{
    internal class RequestBatch : Message
    {
        public new MessageOpCode op { get; set; } = MessageOpCode.RequestBatch;
        public new required MessageRequestBatchData d { get; set; }
    }

    internal class MessageRequestBatchData
    {
        public required string requestId { get; set; }
        public bool? haltOnFailure { get; set; }
        public int? executionType { get; set; }
        public required List<object> requests { get; set; } // ???
    }
}
