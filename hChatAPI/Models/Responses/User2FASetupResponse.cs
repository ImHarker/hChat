namespace hChatAPI.Models.Responses;

public class User2FASetupResponse {
    public string Uri { get; set; }
    public string[] BackupCodes { get; set; }
    public string ChallengeToken { get; set; }
}