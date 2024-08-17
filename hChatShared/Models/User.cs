using System.Security.Cryptography;

namespace hChatShared.Models;

public static class User {
    public static string Username { get; set; } = "";
    public static byte[] Secret { get; set; } = new byte[64];

    //TODO: Handle username and password
    static User() {
        User.Username = "Test";
        var pwd = "Test";
        var salt = new byte[32];
        var file = Path.Combine(Utils.GetLocalAppDataPath(), "key");
        if (!File.Exists(file)) {
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            File.WriteAllBytes(file, salt);
            File.AppendAllText(file, "\n");
        }
        else {
            File.OpenRead(file).Read(salt);
        }

        Rfc2898DeriveBytes.Pbkdf2(pwd, salt, User.Secret, 150_000, HashAlgorithmName.SHA256);
    }

    public static void DeleteSecretFromMemory() {
        Array.Clear(Secret, 0, Secret.Length);
    }
}