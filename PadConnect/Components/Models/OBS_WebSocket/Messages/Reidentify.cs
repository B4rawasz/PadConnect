using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Models.OBS_WebSocket.Messages
{
    internal class Reidentify : Message
    {
        public new WebSocketOpCode op { get; set; } = WebSocketOpCode.Reidentify;
        public new required MessageReidentifyData d { get; set; }
    }

    internal class MessageReidentifyData
    {
        public int? eventSubscriptions { get; set; }
    }
}
