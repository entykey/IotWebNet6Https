namespace IOTWeb.Services
{
    using System.Net.WebSockets;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using IOTWeb.Models;
    using System.Threading.Tasks;

    public class WebSocketService : IDisposable
    {
        private ClientWebSocket? _client;
        // private readonly string _webSocketUrl = "wss://113.161.84.132:8081/FMA";
        private readonly string _webSocketUrl = "wss://113.161.84.132:8081/DATA";
        // private readonly string _webSocketUrl = "ws://127.0.0.1:1880/FMA";
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly byte[] _buffer = new byte[1024 * 8]; 
        private CancellationTokenSource? _cancellationTokenSource;

        public event Action<AccelAndForce> OnAccelAndForceReceived;

        public WebSocketService()
        {
            _client = new ClientWebSocket();
            OnAccelAndForceReceived = delegate { };
            _jsonOptions = new JsonSerializerOptions
            {
                Converters = { new AccelAndForceConverter() }
            };
        }

        public async Task StartAsync()
        {
            if (_client != null && _client.State == WebSocketState.Open)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _client = new ClientWebSocket();
        
            try
            {   
                await _client.ConnectAsync(new Uri(_webSocketUrl), _cancellationTokenSource.Token);

                // _ = Task.Run(() => ReceiveMessagesAsync(_cancellationTokenSource.Token));
                await ReceiveMessagesAsync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebSocketService] WebSocket connection failed: {ex.Message}");
            }
        }

        private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (_client.State == WebSocketState.Open)
                {
                    var result = await _client.ReceiveAsync(new ArraySegment<byte>(_buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                        break;
                    }

                    var message = Encoding.UTF8.GetString(_buffer, 0, result.Count);

                    //* Debug: (print out packetNumber of each received message)
                    // Console.WriteLine(message);
                    using (JsonDocument doc = JsonDocument.Parse(message))
                    {
                        if (doc.RootElement.TryGetProperty("packetNumber", out var extractedPacketNumber))
                        {
                            Console.WriteLine("packetNumber: " + extractedPacketNumber);
                        }
                    }
                    //*/
                    
                    // _ = Task.Run(() => ProcessMessageAsync(message));  // async task scheduled but not awaited
                    await ProcessMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebSocketService] Error receiving message: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (_client != null && _client.State == WebSocketState.Open)
            {
                _cancellationTokenSource?.Cancel();
                _client.Dispose();
                _client = null;
            }
        }

        private async Task ProcessMessageAsync(string message)
        {
            try
            {
                // Console.WriteLine("Message:" + message);
                using (JsonDocument doc = JsonDocument.Parse(message))
                {
                    if (doc.RootElement.TryGetProperty("data", out var dataArray))
                    {
                        foreach (var item in dataArray.EnumerateArray())
                        {
                            var accelAndForce = JsonSerializer.Deserialize<AccelAndForce>(item.GetRawText(), _jsonOptions);

                            if (accelAndForce != null)
                            {
                                OnAccelAndForceReceived?.Invoke(accelAndForce);
                            }
                        }
                    }
                }
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"[WebSocketService] Error deserializing JSON: {jsonEx.Message}");
            }
        }


        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }

        private class AccelAndForceConverter : JsonConverter<AccelAndForce>
        {
            public override AccelAndForce Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                float timestamp = 0;
                float accX = 0;
                float accY = 0;
                float accZ = 0;
                float force = 0;

                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected StartObject token");
                }

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        var propertyName = reader.GetString();
                        reader.Read(); 

                        switch (propertyName)
                        {
                            case "timestamp":
                                timestamp = reader.GetSingle();
                                break;

                            case "accX":
                                accX = reader.GetSingle();
                                break;

                            case "accY":
                                accY = reader.GetSingle();
                                break;

                            case "accZ":
                                accZ = reader.GetSingle();
                                break;

                            case "force":
                                force = reader.GetSingle();
                                break;

                            default:
                                reader.Skip(); 
                                break;
                        }
                    }
                    else if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return new AccelAndForce
                        {
                            Timestamp = timestamp,
                            AccX = accX,
                            AccY = accY,
                            AccZ = accZ,
                            Force = force
                        };
                    }
                }

                throw new JsonException("Incomplete JSON data");
            }

            public override void Write(Utf8JsonWriter writer, AccelAndForce value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WriteNumber("accX", value.AccX);
                writer.WriteNumber("accY", value.AccY);
                writer.WriteNumber("accZ", value.AccZ);
                writer.WriteNumber("force", value.Force);
                writer.WriteEndObject();
            }
        }
    }


}