using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace VSUL
{
    public class EmbeddedDllClass
    {
        public static ICoreAPI api;

        private static string tempFolder;

        public static void ExtractEmbeddedDlls()
        {
            if (RuntimeEnv.OS != OS.Windows) return;
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (var dllName in resourceNames)
                ExtractEmbeddedDll(dllName);
        }

        public static void ExtractEmbeddedDll(string resourceName)
        {
            if (RuntimeEnv.OS != OS.Windows)
            {
                api.Logger.Error("OS is not Windows.");
                return;
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = assembly.GetName();
            tempFolder ??= $"{assemblyName.Name}.{assemblyName.Version}";

            byte[] resourceBytes;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                resourceBytes = memoryStream.ToArray();
            }

            string dirName = Path.Combine(Path.GetTempPath(), tempFolder);
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);

            string[] resourceNameParts = resourceName.Split(".");
            string dllName = $"{resourceNameParts[resourceNameParts.Length - 2]}.{resourceNameParts[resourceNameParts.Length - 1]}";
            string dllPath = Path.Combine(dirName, dllName);
            bool alreadyExtracted = false;
            if (File.Exists(dllPath))
            {
                byte[] existingResource = File.ReadAllBytes(dllPath);
                alreadyExtracted = resourceBytes.SequenceEqual(existingResource);
            }
            if (alreadyExtracted) return;
            File.WriteAllBytes(dllPath, resourceBytes);

            api.Logger.Debug($"Extracting {dllPath}");
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string fileName);

        public static IntPtr LoadDll(string dllName)
        {
            if (RuntimeEnv.OS != OS.Windows)
            {
                api.Logger.Error("OS is not Windows.");
                return IntPtr.Zero;
            }

            if (tempFolder == null)
            {
                api.Logger.Error("Cannot load embedded dlls before extracting them.");
                return IntPtr.Zero;
            }

            string dllPath = Path.Combine(Path.GetTempPath(), tempFolder, dllName);
            IntPtr handle = LoadLibrary(dllPath);

            if (handle == IntPtr.Zero)
                api.Logger.Error($"Failed to load embedded DLL {dllPath}");
            else
                api.Logger.Debug($"Loaded {dllPath}");

            return handle;
        }
    }
}