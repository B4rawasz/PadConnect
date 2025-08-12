using PadConnect.Components.Models.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
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
                    // Handle the received message (e.g., update UI, log, etc.)
                    Debug.WriteLine($"Received: {message}");

                    Message? msg = JsonSerializer.Deserialize<Message>(message);
                    if (msg != null)
                    {
                        Debug.WriteLine($"Message OpCode: {msg.op}");

                        if(msg.op == WebSocketOpCode.Hello)
                        {
                            var helloMessage = JsonSerializer.Deserialize<MessageHello>(message);
                            if (helloMessage != null && helloMessage.d != null)
                            {
                                Debug.WriteLine($"Hello message received: {helloMessage.d.obsStudioVersion}");
                                Debug.WriteLine($"Salt: {helloMessage.d.authentication?.salt}");
                            }
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
                        Debug.WriteLine("WebSocket closed by server");
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
