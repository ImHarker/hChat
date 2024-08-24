using System.Text.Json.Serialization;

namespace hChatAPI.Models;

public class User2FA {
    public int Id { get; set; }
    
    public int UserId { get; set; }
    [JsonIgnore]
    public User User { get; set; } = new User();
    
    public bool Is2FAEnabled { get; set; } = false;
    public string? SecretKey { get; set; }
    public ICollection<UserBackupCode> BackupCodes { get; set; } = new List<UserBackupCode>();
}