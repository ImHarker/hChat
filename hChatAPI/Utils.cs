using System.Reflection;

namespace hChatAPI;

public static class Utils {

    public static string GetLocalAppDataPath() {
        try {
            var assemblyName = Assembly.GetCallingAssembly().GetName().Name;
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