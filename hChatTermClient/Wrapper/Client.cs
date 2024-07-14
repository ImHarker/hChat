using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using hChatAPI.Net;

namespace hChatTermClient.Wrapper;

public class Client {
    private HttpClientHandler? _handler;
    private HttpClient? _client;
    private readonly string _url = "https://localhost:7168/";
    private ClientWebSocket? _socket;
    private readonly string _serverUri = "wss://localhost:7168/ws?token=";


    public async Task Run() {
        _handler = new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        _client = new HttpClient(_handler);
        var response = await _client.GetAsync(_url + "status");
        Console.WriteLine(response.Content.ReadAsStringAsync().Result);

        response = await _client.PostAsync(_url + "auth/login",
            new StringContent("{\"username\":\"admin\",\"password\":\"admin123!\"}", Encoding.UTF8,
                "application/json"));
        Console.WriteLine(response.Content.ReadAsStringAsync().Result);

        var token = response.Content.ReadAsStringAsync().Result;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        response = await _client.GetAsync(_url + "Auth/protected");
        Console.WriteLine(response.Content.ReadAsStringAsync().Result);


        _socket = new ClientWebSocket();
        _socket.Options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
        try {
            Console.WriteLine("Connecting to the server...");
            await _socket.ConnectAsync(new Uri(_serverUri + token), CancellationToken.None);
            Console.WriteLine("Connected to the server");

            _ = Task.Run(ReceiveMessages);
        }
        catch (WebSocketException e) {
            Console.WriteLine($"WebSocket error: {e.Message}");
            Console.WriteLine($"WebSocket error: {e.InnerException?.Message}");
        }
        catch (Exception e) {
            Console.WriteLine($"Unexpected error: {e.Message}");
        }
    }

    public async Task Send(PacketBuilder packet) {
        var messageBytes = packet.GetPacketBytes();
        if (_socket != null)
            await _socket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true,
                CancellationToken.None);
        Console.WriteLine($"Sent: {Encoding.UTF8.GetString(packet.GetPacketBytes())}");
    }

    private async Task ReceiveMessages() {
        try {

            var buffer = new byte[1024 * 4];
            while (_socket is { State: WebSocketState.Open }) {
                WebSocketReceiveResult result =
                    await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    Console.WriteLine("Closed WebSocket connection");
                }
                else {
                    MessageHandler.HandleMessage(_socket, buffer);
                }
            }
        }
        catch (Exception e) {
            Console.WriteLine($"Unexpected error: {e.Message}");
        }
    }
    
}