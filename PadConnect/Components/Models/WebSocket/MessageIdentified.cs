using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models.WebSocket
{
    internal class MessageIdentified : Message
    {
        public new MessageIdentifiedData? d { get; set; } // Data payload specific to Identified message
    }

    internal class MessageIdentifiedData
    {
        public int? negotiatedRpcVersion { get; set; } // RPC version
    }
}
