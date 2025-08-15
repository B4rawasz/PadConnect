using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace PadConnect.Components.Models.WebSocket
{
    internal class MessageIdentify
    {
        public new WebSocketOpCode op { get; set; } = WebSocketOpCode.Identify;
        public new MessageIdentifyData? d { get; set; } // Data payload specific to Identify message
    }

    internal class MessageIdentifyData
    {
        public int? rpcVersion { get; set; } // RPC version
        public string? authentication { get; set; } // Authentication token or method
        public int? eventSubscriptions { get; set; } // Subscribed events
    }
}
