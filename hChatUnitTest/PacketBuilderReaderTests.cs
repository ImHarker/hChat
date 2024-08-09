using hChatTermClient.Net;

namespace hChatUnitTest;

[TestFixture]
public class PacketBuilderTests {
    [Test]
    public void WriteOpCode_ShouldWriteOpCodeCorrectly() {
        // Arrange
        var packetBuilder = new PacketBuilder();
        byte expectedOpCode = 42;

        // Act
        packetBuilder.WriteOpCode(expectedOpCode);
        byte[] packetBytes = packetBuilder.GetPacketBytes();

        // Assert
        Assert.That(expectedOpCode, Is.EqualTo(packetBytes[0]));
    }

    [Test]
    public void WriteMessage_ShouldWriteMessageCorrectly() {
        // Arrange
        var packetBuilder = new PacketBuilder();
        string message = "Hello, World!";

        // Act
        packetBuilder.WriteMessage(message);
        byte[] packetBytes = packetBuilder.GetPacketBytes();

        // Assert
        using (var reader = new PacketReader(new MemoryStream(packetBytes))) {
            string readMessage = reader.ReadMessage();
            Assert.That(message, Is.EqualTo(readMessage));
        }
    }

    [Test]
    public void Clear_ShouldResetStream() {
        // Arrange
        var packetBuilder = new PacketBuilder();
        packetBuilder.WriteOpCode(1);
        packetBuilder.WriteMessage("Test");

        // Act
        packetBuilder.Clear();
        byte[] packetBytes = packetBuilder.GetPacketBytes();

        // Assert
        Assert.That(0, Is.EqualTo(packetBytes.Length));
    }
}

[TestFixture]
    public class PacketReaderTests {
        [Test]
        public void ReadMessage_ShouldReadMessageCorrectly() {
            // Arrange
            var packetBuilder = new PacketBuilder();
            string expectedMessage = "Hello, World!";
            packetBuilder.WriteMessage(expectedMessage);
            byte[] packetBytes = packetBuilder.GetPacketBytes();

            using (var stream = new MemoryStream(packetBytes))
            using (var reader = new PacketReader(stream)) {
                // Act
                string message = reader.ReadMessage();

                // Assert
                Assert.That(expectedMessage, Is.EqualTo(message));
            }
        }

        [Test]
        public void ReadOpCode_ShouldReadOpCodeCorrectly() {
            // Arrange
            var packetBuilder = new PacketBuilder();
            byte expectedOpCode = 42;
            packetBuilder.WriteOpCode(expectedOpCode);
            byte[] packetBytes = packetBuilder.GetPacketBytes();

            using (var stream = new MemoryStream(packetBytes))
            using (var reader = new PacketReader(stream)) {
                // Act
                byte opCode = reader.ReadOpCode();

                // Assert
                Assert.That(expectedOpCode, Is.EqualTo(opCode));
            }
        }
    }
