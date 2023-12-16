using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using hChat.Models;
using HLogger;

namespace hChat.Utils
{
    public static class ClientWrapper
    {
		private static NetworkStream? stream;

		//Listener
		private static TcpListener? listener = new TcpListener(IPAddress.Any, 1337);
		private static CancellationTokenSource? cancellationToken;
		private static Thread? listenerThread;

		//Client
		private static TcpClient client = new TcpClient();
		private static EncryptedCommunicationHandler encHandler = new EncryptedCommunicationHandler();
		public static User User { get; set; } = new User("", encHandler.ExportPublicKey());
		public static User Peer { get; set; } = new User("", "");
		

		public static void Init() {
			Console.Write("Username: ");
			string? username;
			while (String.IsNullOrWhiteSpace(username = Console.ReadLine())) {
				Console.WriteLine("Username cannot be empty!");
				Console.Write("Username: ");
			}
			User.Username = username;
			StartListenening();
		}
		
		public static void Connect(string hostname) {
			StopListening();
			try {
				client.Connect(hostname, 1337);
				HandleClient(client);
			} catch (Exception e) {
				hLogger.Exception(e);
				//StartListenening();
			}
		}

		public static void Disconnect() {
			client.Close();
			StopListening();
		}




		private static void StartListenening() {
			if (listenerThread != null) return;
			
			cancellationToken = new CancellationTokenSource();
			listenerThread = new Thread(() => {
				listener.Start();
				while (!cancellationToken.Token.IsCancellationRequested) {
					var tcpClient = listener.AcceptTcpClient();
					HandleClient(tcpClient);
				}

				cancellationToken.Dispose();
				listenerThread = null;
			});
			listenerThread.Start();
		}
		
		private static void StopListening() {
			if (listenerThread != null) {
				cancellationToken.Cancel();
				listenerThread.Join();
				listener.Stop();
			}
		}

		private static void HandleClient(TcpClient tcpClient) {
			stream = tcpClient.GetStream();
			Thread handleMessageThread = new Thread(HandleMessageReceived);
			Console.WriteLine($"Connected to {tcpClient.Client.RemoteEndPoint}.\n\n");
			SendHello();
			Console.WriteLine(ClientWrapper.Receive());
			handleMessageThread.Start();

			while (IsConnected()) {
				if (cancellationToken is { Token.IsCancellationRequested: true }) break;
				string? msg;
				if(String.IsNullOrWhiteSpace(msg = Console.ReadLine())) continue;
				ClientWrapper.Send(new Message(msg));
			}
		}

		private static void HandleMessageReceived() {
			while (IsConnected()) {
				if (cancellationToken is { Token.IsCancellationRequested: true }) break;
				var msg = ClientWrapper.Receive();
				Console.WriteLine(msg);
			}
		}

	
		private static void SendHello() {
			try {
				Message hello = new Message("");
				byte[] msg = Encoding.ASCII.GetBytes(hello.Serialize());
				int length = msg.Length;
				byte[] lengthBytes = BitConverter.GetBytes(length);
				byte[] dataToSend = lengthBytes.Concat(msg).ToArray();
				stream.Write(dataToSend, 0, dataToSend.Length);
			} catch (Exception e) {
				hLogger.Exception(e);
			}
		}

		private static void Send(Message message)
        {
            try
            { 
	            Aes aes = encHandler.InitializeAes(Convert.FromBase64String(Peer.PublicKey));
				message.Data = Convert.ToBase64String(EncryptedCommunicationHandler.EncryptData(Encoding.ASCII.GetBytes(message.Data), aes));
				message.Signature = Convert.ToBase64String(EncryptedCommunicationHandler.SignData(Convert.FromBase64String(message.Data), encHandler.myECDH));
				byte[] msg = Encoding.ASCII.GetBytes(message.Serialize());
				int length = msg.Length;
				byte[] lengthBytes = BitConverter.GetBytes(length);
				byte[] dataToSend = lengthBytes.Concat(msg).ToArray();
				stream.Write(dataToSend, 0, dataToSend.Length);
			}
            catch (Exception e)
            {
				hLogger.Exception(e);
            }
		}

		private static string? Receive() {

			try {
				byte[] lengthBytes = new byte[sizeof(int)];
				stream.Read(lengthBytes, 0, lengthBytes.Length);

				int length = BitConverter.ToInt16(lengthBytes, 0);

				byte[] msgBytes = new byte[length];
				int totalBytesRead = 0;
				int bytesRead = 0;

				while (totalBytesRead < length) {
					bytesRead = stream.Read(msgBytes, totalBytesRead, length - totalBytesRead);

					if (bytesRead == 0) {
						break;
					}

					totalBytesRead += bytesRead;
				}
				if (totalBytesRead < length) {
					return null;
				}
				
				Message receivedMessage = Message.Deserialize(Encoding.ASCII.GetString(msgBytes));

				if (receivedMessage.Data == "") {
					Peer = receivedMessage.Sender;
					return $"{receivedMessage.Timestamp:HH:mm:ss} - {receivedMessage.Sender.Username} PubKey(b64): {receivedMessage.Sender.PublicKey}";
				}

				Aes aes = encHandler.InitializeAes(Convert.FromBase64String(receivedMessage.Sender.PublicKey));
				bool verified = EncryptedCommunicationHandler.VerifySignature(Convert.FromBase64String(receivedMessage.Data), Convert.FromBase64String(receivedMessage.Signature), Convert.FromBase64String(receivedMessage.Sender.PublicKey));
				receivedMessage.Data = Encoding.ASCII.GetString(EncryptedCommunicationHandler.DecryptData(Convert.FromBase64String(receivedMessage.Data), aes));
				if (!verified) {
					Console.WriteLine("Message signature not verified!");
					return null;
				}
				return $"{receivedMessage.Timestamp:HH:mm:ss} - {receivedMessage.Sender.Username}: {receivedMessage.Data}";
			} catch (Exception e) {
				hLogger.Exception(e);
				return null;
			}
		}

		private static bool IsConnected() {
			try {
				if (stream == null) return false;
				stream.Write(Array.Empty<byte>(), 0, Array.Empty<byte>().Length);
				return true;

			}
			catch (Exception e) {
				return false;
			}
		}

    }
}
