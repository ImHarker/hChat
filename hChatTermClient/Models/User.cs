namespace hChatTermClient.Models;

public class User {
    public string Username { get; set; } = "";
    public byte[] Secret { get; set; } = new byte[64];

    public void DeleteSecretFromMemory() {
        Array.Clear(Secret, 0, Secret.Length);
    }
    
}