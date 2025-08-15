using PadConnect.Components.Models.OBS_WebSocket.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PadConnect.Components.Services
{
    internal class WebSocketService
    {
        private string _webSocketUrl = "ws://localhost:4455";
        private string _webSocketPassword = "";
        private bool _autoReconnect = true;

        public event EventHandler<bool>? StatusUpdate;
        private bool _connectionStatus = false;
        public bool ConnectionStatus { get => _connectionStatus; }

        private ClientWebSocket? _ws;

        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public async void SetWebsocket(string address, string password, bool autoReconnect)
        {
            _webSocketUrl = address;
            _webSocketPassword = password;
            _autoReconnect = autoReconnect;

            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
            }

            try
            {
                _ws = new ClientWebSocket();
                await _ws.ConnectAsync(new Uri(address), CancellationToken.None);
                _ = StartReceivingAsync(message =>
                {
                    Debug.WriteLine($"Received: {message}");

                    Message? msg = JsonSerializer.Deserialize<Message>(message);
                    if (msg != null)
                    {
                        Debug.WriteLine($"Message OpCode: {msg.op}");

                        switch (msg.op)
                        {
                            case WebSocketOpCode.Hello:
                                var helloMessage = JsonSerializer.Deserialize<Hello>(message);
                                if (helloMessage != null && helloMessage.d != null)
                                {
                                    Debug.WriteLine($"Hello message received: {helloMessage.d.obsStudioVersion}");
                                    Debug.WriteLine($"Salt: {helloMessage.d.authentication?.salt}");

                                    if(helloMessage.d.authentication != null)
                                    {
                                        var salt = helloMessage.d.authentication.salt ?? "";
                                        var challenge = helloMessage.d.authentication.challenge ?? "";

                                        var passwordSalt = _webSocketPassword + salt;

                                        using var sha256 = SHA256.Create();
                                        var passwordSaltBytes = Encoding.UTF8.GetBytes(passwordSalt);
                                        var hash1 = sha256.ComputeHash(passwordSaltBytes);
                                        var base64Secret = Convert.ToBase64String(hash1);

                                        var secretChallenge = base64Secret + challenge;

                                        var secretChallengeBytes = Encoding.UTF8.GetBytes(secretChallenge);
                                        var hash2 = sha256.ComputeHash(secretChallengeBytes);
                                        var authenticationString = Convert.ToBase64String(hash2);


                                        var identifyMessage = new Identify
                                        {
                                            d = new MessageIdentifyData
                                            {
                                                rpcVersion = helloMessage.d?.rpcVersion ?? 1,
                                                authentication = authenticationString,
                                            }
                                        };
                                        var identifyJson = JsonSerializer.Serialize(identifyMessage, _jsonOptions);
                                        _ws?.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(identifyJson)), WebSocketMessageType.Text, true, CancellationToken.None);
                                    }
                                }
                                break;
                            case WebSocketOpCode.Identified:
                                var identifiedMessage = JsonSerializer.Deserialize<Identified>(message);
                                if (identifiedMessage != null && identifiedMessage.d != null)
                                {
                                    Debug.WriteLine($"Identified message received: {identifiedMessage.d.negotiatedRpcVersion}");
                                }
                                break;
                            case WebSocketOpCode.Event:
                                var eventMessage = JsonSerializer.Deserialize<Event>(message);
                                if (eventMessage != null && eventMessage.d != null)
                                {
                                    Debug.WriteLine($"Event message received: {eventMessage.d.eventType}");
                                    // Handle the event as needed
                                }
                                break;
                            default:
                                Debug.WriteLine($"Unhandled OpCode: {msg.op}");
                                break;
                        }
                    }
                });
                _connectionStatus = true;
                StatusUpdate?.Invoke(this, _connectionStatus);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WebSocket connection error: {ex.Message}");
                _ws = null;
                _connectionStatus = false;
                StatusUpdate?.Invoke(this, _connectionStatus);
            }
        }

        private async Task StartReceivingAsync(Action<string> onMessageReceived)
        {
            if (_ws == null || _ws.State != WebSocketState.Open)
                return;

            var buffer = new byte[4096];
            try
            {
                while (_ws.State == WebSocketState.Open)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.WriteLine($"WebSocket closed by server. CloseStatus: {result.CloseStatus}, Reason: {result.CloseStatusDescription}");
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                        break;
                    }
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    onMessageReceived?.Invoke(message);
                }
            }
            catch (WebSocketException ex)
            {
                Debug.WriteLine($"WebSocket closed: {ex.Message}");
            }

            _ws?.Dispose();
            _ws = null;
            _connectionStatus = false;
            StatusUpdate?.Invoke(this, _connectionStatus);

            if (_autoReconnect)
            {
                Debug.WriteLine("Attempting to reconnect...");
                await Task.Delay(2000); // Wait before trying to reconnect
                SetWebsocket(_webSocketUrl, _webSocketPassword, _autoReconnect);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
                _ws.Dispose();
                _ws = null;
                _connectionStatus = false;
                StatusUpdate?.Invoke(this, _connectionStatus);
            }
        }
    }
}
