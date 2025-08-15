using PadConnect.Components.Models.OBS_WebSocket.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace PadConnect.Components.Models.OBS_WebSocket.Messages
{
    internal class Identify : Message
    {
        public new MessageOpCode op { get; set; } = MessageOpCode.Identify;
        public new required MessageIdentifyData d { get; set; } // Data payload specific to Identify message
    }

    internal class MessageIdentifyData
    {
        public required int rpcVersion { get; set; } // RPC version
        public string? authentication { get; set; } // Authentication token or method
        public EventSubscription? eventSubscriptions { get; set; } // Subscribed events
    }
}
