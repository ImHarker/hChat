using System.Text.Json.Serialization;

namespace hChatAPI.Models;

public class UserBackupCode {
    public int Id { get; set; }
    
    public int User2FAId { get; set; }
    [JsonIgnore]
    public User2FA User2FA { get; set; } = new User2FA();
    
    public string HashedCode { get; set; } = "";
    public bool IsUsed { get; set; } = false;
}