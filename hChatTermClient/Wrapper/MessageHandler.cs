using System.Globalization;
using System.Net.WebSockets;
using hChatAPI.Net;

namespace hChatTermClient.Wrapper;

/*
 * OPCODE Definitions
 * +--------+----------------------+--------------------------------------------------------------------------+
 * | Opcode | Name                 | Description                                                              |
 * |--------|----------------------|--------------------------------------------------------------------------|
 * | 0x01   | REQUEST_PUBKEY       | Request public key of an user                                            |
 * | 0x02   | SEND_PUBKEY          | Send public key to an user                                               |
 * | 0x03   | SEND_MESSAGE         | Send a chat message                                                      |
 * | 0x04   | RECEIVE_MESSAGE      | Receive a chat message                                                   |
 * | 0x05   | REQUEST_MESSAGES     | Request undelivered messages (for when a user comes online)              |
 * | 0x06   | SEND_MESSAGES        | Send undelivered messages                                                |
 * | 0x07   | REQUEST_TIMESTAMP    | Request current timestamp from server                                    |
 * | 0x08   | SEND_TIMESTAMP       | Send current timestamp from server                                       |
 * +--------+----------------------+--------------------------------------------------------------------------+
 *
 */
public static class MessageHandler {
    public static void HandleMessage(WebSocket socket, byte[] buffer) {
        using var stream = new MemoryStream(buffer);
        using var packetReader = new PacketReader(stream);
        var opCode = packetReader.ReadOpCode();


        switch (opCode) {
            case 0x01: {
                Console.WriteLine("REQUEST_PUBKEY - Invalid Client Operation");
                break;
            }

            case 0x02: {
                Console.WriteLine("SEND_PUBKEY");
                break;
            }

            case 0x03: {
                Console.WriteLine("SEND_MESSAGE - Invalid Client Operation");
                break;
            }

            case 0x04: {
                Console.WriteLine("RECEIVE_MESSAGE");
                var msg = packetReader.ReadMessage();
                Console.WriteLine(msg);
                break;
            }

            case 0x05: {
                Console.WriteLine("REQUEST_MESSAGES - Invalid Client Operation");
                break;
            }

            case 0x06: {
                Console.WriteLine("SEND_MESSAGES");
                break;
            }

            case 0x07: {
                Console.WriteLine("REQUEST_TIMESTAMP - Invalid Client Operation");
                break;
            }

            case 0x08: {
                Console.WriteLine("SEND_TIMESTAMP");
                var timestamp = packetReader.ReadMessage();
                Console.WriteLine(timestamp);
                var dateTime = DateTime.Parse(timestamp, CultureInfo.InvariantCulture);
                var clockskew = DateTime.UtcNow - dateTime;
                Console.WriteLine(clockskew);
                break;
            }

            default:
                Console.WriteLine($"Invalid opcode received. {opCode}");
                break;
        }
    }
}