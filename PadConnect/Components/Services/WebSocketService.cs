using PadConnect.Components.Models.OBS_WebSocket.Messages;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PadConnect.Components.Services
{
    internal partial class WebSocketService
    {
        private string _webSocketUrl = "ws://localhost:4455";
        private string _webSocketPassword = "123456";
        private bool _autoReconnect = true;

        public event EventHandler<bool>? StatusUpdate;
        private bool _connectionStatus = false;
        public bool ConnectionStatus { get => _connectionStatus; }

        private ClientWebSocket? _ws;

        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private JsonSerializerOptions _jsonEventOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        public async void SetWebsocket(string address, string password, bool autoReconnect)
        {
            _webSocketUrl = address;
            _webSocketPassword = password;
            _autoReconnect = autoReconnect;

            await CloseExistingConnection();

            try
            {
                await ConnectAndStartReceiving(address);
                UpdateConnectionStatus(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WebSocket connection error: {ex.Message}");
                CleanupConnection();
                UpdateConnectionStatus(false);
            }
        }

        private async Task CloseExistingConnection()
        {
            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
            }
        }

        private async Task ConnectAndStartReceiving(string address)
        {
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(address), CancellationToken.None);
            _ = StartReceivingAsync(HandleWebSocketMessage);
        }

        private void HandleWebSocketMessage(string message)
        {
            Debug.WriteLine($"---\nReceived:\n{message}");

            var msg = JsonSerializer.Deserialize<Message>(message);
            if (msg == null) return;

            Debug.WriteLine($"Message OpCode: {msg.op}");

            switch (msg.op)
            {
                case MessageOpCode.Hello:
                    HandleHelloMessage(message);
                    break;
                case MessageOpCode.Identified:
                    HandleIdentifiedMessage(message);
                    break;
                case MessageOpCode.Event:
                    HandleEventMessage(message);
                    break;
                default:
                    Debug.WriteLine($"Unhandled OpCode: {msg.op}");
                    break;
            }
        }

        private void HandleHelloMessage(string message)
        {
            var helloMessage = JsonSerializer.Deserialize<Hello>(message);
            if (helloMessage?.d == null) return;

            Debug.WriteLine($"Hello message received: {helloMessage.d.obsStudioVersion}");

            var identifyJson = JsonSerializer.Serialize(Auth(helloMessage), _jsonOptions);
            Debug.WriteLine(identifyJson);

            var bytes = Encoding.UTF8.GetBytes(identifyJson);
            _ws?.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private void HandleIdentifiedMessage(string message)
        {
            var identifiedMessage = JsonSerializer.Deserialize<Identified>(message);
            if (identifiedMessage?.d == null) return;

            Debug.WriteLine($"Identified message received: {identifiedMessage.d.negotiatedRpcVersion}");
        }

        private void HandleEventMessage(string message)
        {
            var eventMessage = JsonSerializer.Deserialize<Event>(message);
            if (eventMessage?.d?.eventType == null) return;

            if (EventHandlers.TryGetValue(eventMessage.d.eventType.Value, out var handler))
            {
                Debug.WriteLine($"Handling EventType: {eventMessage.d.eventType}");
                handler(eventMessage.d.eventData);
            }
            else
            {
                Debug.WriteLine($"Unhandled EventType: {eventMessage.d.eventType}");
            }
        }

        private void UpdateConnectionStatus(bool status)
        {
            _connectionStatus = status;
            StatusUpdate?.Invoke(this, _connectionStatus);
        }

        private void CleanupConnection()
        {
            _ws = null;
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
                await Task.Delay(1000);
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
