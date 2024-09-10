namespace hChatAPI.Models.Requests;

public class Recovery2FARequest {
    public UserAuthRequest UserAuthRequest { get; set; }
    public string Code { get; set; }
}