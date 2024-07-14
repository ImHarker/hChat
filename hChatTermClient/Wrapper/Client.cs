using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using hChatAPI.Net;

namespace hChatTermClient.Wrapper;

public class Client {
    private HttpClientHandler handler;
    private HttpClient client;
    private string url = "https://localhost:7168/";
    private ClientWebSocket socket;
    private string serverUri = "wss://localhost:7168/ws?token=";


    public async Task Run() {
        byte[] buffer = new byte[1024];
        handler = new HttpClientHandler {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };
        client = new HttpClient(handler);
        var response = await client.GetAsync(url + "status");
        Console.WriteLine(response.Content.ReadAsStringAsync().Result);

        response = await client.PostAsync(url + "auth/login",
            new StringContent("{\"username\":\"admin\",\"password\":\"admin123!\"}", Encoding.UTF8,
                "application/json"));
        Console.WriteLine(response.Content.ReadAsStringAsync().Result);

        var token = response.Content.ReadAsStringAsync().Result;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        response = await client.GetAsync(url + "Auth/protected");
        Console.WriteLine(response.Content.ReadAsStringAsync().Result);


        socket = new ClientWebSocket();
        socket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
        try {
            Console.WriteLine("Connecting to the server...");
            await socket.ConnectAsync(new Uri(serverUri + token), CancellationToken.None);
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
        byte[] messageBytes = packet.GetPacketBytes();
        await socket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true,
            CancellationToken.None);
        Console.WriteLine($"Sent: {Encoding.UTF8.GetString(packet.GetPacketBytes())}");
    }

    private async Task ReceiveMessages() {
        try {

            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open) {
                WebSocketReceiveResult result =
                    await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    Console.WriteLine("Closed WebSocket connection");
                }
                else {
                    MessageHandler.HandleMessage(socket, buffer);
                }
            }
        }
        catch (Exception e) {
            Console.WriteLine($"Unexpected error: {e.Message}");
        }
    }
    
}