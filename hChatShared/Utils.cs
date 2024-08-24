using System.Reflection;
using System.Text;

namespace hChatShared;

public static class Utils {

    public static string GetLocalAppDataPath() {
        try {
            var assemblyName = Assembly.GetEntryAssembly()!.GetName().Name;
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (assemblyName == null) throw new Exception("Unexpected Error - Assembly Name is null");
            return Path.Combine(localAppData, assemblyName);
        }
        catch (Exception e){
            Console.WriteLine(e.Message);
            throw;
        }
    }
    
    public static void CreateAppDataFolder() {
        try {
            Directory.CreateDirectory(GetLocalAppDataPath());
        }
        catch (Exception e){
            Console.WriteLine(e.Message);
            throw;
        }
    }
}

public static class Base32Encoder {
    private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static string Encode(byte[] data) {
        StringBuilder result = new StringBuilder();

        int bitCount = 0;
        int currentByte = 0;

        foreach (byte b in data) {
            currentByte = (currentByte << 8) | b;
            bitCount += 8;

            while (bitCount >= 5) {
                bitCount -= 5;
                int index = (currentByte >> bitCount) & 0x1F;
                result.Append(Base32Chars[index]);
            }
        }

        if (bitCount > 0) {
            int index = (currentByte << (5 - bitCount)) & 0x1F;
            result.Append(Base32Chars[index]);
        }

        return result.ToString();
    }

    public static byte[] Decode(string base32String) {
        base32String = base32String.ToUpper();

        int bitCount = 0;
        int currentByte = 0;

        using (MemoryStream stream = new MemoryStream()) {
            foreach (char c in base32String) {
                int index = Base32Chars.IndexOf(c);
                if (index == -1) {
                    throw new ArgumentException("Invalid character in Base32 string");
                }

                currentByte = (currentByte << 5) | index;
                bitCount += 5;

                if (bitCount >= 8) {
                    bitCount -= 8;
                    stream.WriteByte((byte)(currentByte >> bitCount));
                }
            }

            return stream.ToArray();
        }
    }
}