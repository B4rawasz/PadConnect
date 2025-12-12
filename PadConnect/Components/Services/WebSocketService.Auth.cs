using PadConnect.Components.Models.OBS_WebSocket.Events;
using PadConnect.Components.Models.OBS_WebSocket.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PadConnect.Components.Services
{
    internal partial class WebSocketService
    {
        private Identify Auth(Hello message)
        {
            if (message.d == null)
            {
                throw new ArgumentException("Hello message data is null");
            }

            if (message.d.authentication == null)
            {
                return new Identify
                {
                    d = new MessageIdentifyData
                    {
                        rpcVersion = message.d?.rpcVersion ?? 1,
                        eventSubscriptions = EventSubscription.All
                    }
                };
            }

            var salt = message.d.authentication.salt ?? "";
            var challenge = message.d.authentication.challenge ?? "";

            var passwordSalt = _webSocketPassword + salt;

            using var sha256 = SHA256.Create();
            var passwordSaltBytes = Encoding.UTF8.GetBytes(passwordSalt);
            var hash1 = sha256.ComputeHash(passwordSaltBytes);
            var base64Secret = Convert.ToBase64String(hash1);

            var secretChallenge = base64Secret + challenge;

            var secretChallengeBytes = Encoding.UTF8.GetBytes(secretChallenge);
            var hash2 = sha256.ComputeHash(secretChallengeBytes);
            var authenticationString = Convert.ToBase64String(hash2);


            return new Identify
            {
                d = new MessageIdentifyData
                {
                    rpcVersion = message.d?.rpcVersion ?? 1,
                    authentication = authenticationString,
                    eventSubscriptions = EventSubscription.All
                }
            };
        }
    }
}
