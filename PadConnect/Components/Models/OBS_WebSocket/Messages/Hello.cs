using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models.OBS_WebSocket.Messages
{
    internal class Hello : Message
    {
        public new MessageHelloData? d { get; set; } // Data payload specific to Hello message
    }

    public class MessageHelloData
    {
        public string? obsStudioVersion { get; set; } // Version of OBS Studio
        public string? obsWebSocketVersion { get; set; } // Version of OBS WebSocket
        public int? rpcVersion { get; set; } // Version of the RPC protocol
        public MessageHelloDataAuth? authentication { get; set; } // Optional authentication data
    }

    public class MessageHelloDataAuth
    {
        public string? challenge { get; set; } // Challenge string for authentication
        public string? salt { get; set; } // Salt for authentication
    }
}
