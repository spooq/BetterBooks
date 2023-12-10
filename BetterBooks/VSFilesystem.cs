using System.Runtime.InteropServices;
using UltralightNet;
using UltralightNet.Platform;
using UltralightNet.Platform.HighPerformance;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace BetterBooks
{
    public class VSFilesystem : IFileSystem
    {
        public ICoreAPI api;
        public string domain;
        private string modID;

        public VSFilesystem(ICoreClientAPI api, string modID)
        {
            this.api = api;
            this.modID = modID;
        }

        public void Dispose()
        {
            api.Logger.Notification("dispose");
        }

        public ULFileSystem? GetNativeStruct()
        {
            api.Logger.Notification("GetNativeStruct");

            unsafe
            {
                return new ULFileSystem
                {
                    FileExists = (delegate* unmanaged[Cdecl]<ULString*, bool>)
                        Marshal.GetFunctionPointerForDelegate((ULString* path) => FileExists(path->ToString())),
                    GetFileMimeType = (delegate* unmanaged[Cdecl]<ULString*, ULString*>)
                        Marshal.GetFunctionPointerForDelegate((ULString* path) => new ULString(GetFileMimeType(path->ToString())).Allocate()),
                    GetFileCharset = (delegate* unmanaged[Cdecl]<ULString*, ULString*>)
                        Marshal.GetFunctionPointerForDelegate((ULString* path) => new ULString(GetFileCharset(path->ToString())).Allocate()),
                    OpenFile = (delegate* unmanaged[Cdecl]<ULString*, ULBuffer>)
                        Marshal.GetFunctionPointerForDelegate((ULString* path) => OpenFile(path->ToString()))
                };
            }
        }

        public bool FileExists(string path)
        {
            bool result = api.Assets.Exists(new AssetLocation(modID, "config/" + path.ToLower()));
            api.Logger.Notification("FileExists: " + path + " - " + (result ? "yes" : "no"));
            return result;
        }

        public string GetFileCharset(string path)
        {
            api.Logger.Notification("GetFileCharset: " + path);
            return "utf-8";
        }

        public string GetFileMimeType(string path)
        {
            api.Logger.Notification("GetFileMimeType: " + path);
            return "text/html";
        }

        public ULBuffer OpenFile(string path)
        {
            var data = api.Assets.Get(new AssetLocation(modID, "config/" + path.ToLower()));

            api.Logger.Notification("OpenFile: " + path + " length=" + data.Data.Length);

            unsafe
            {
                fixed (void* ptr = &data.Data[0])
                {
                    return ULBuffer.CreateFromDataCopy(ptr, (nuint)data.Data.Length);
                }
            }
        }
    }
}
