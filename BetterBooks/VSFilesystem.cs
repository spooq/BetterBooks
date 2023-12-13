using System;
using System.Reflection.Metadata;
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

        private string modID;

        readonly GCHandle[]? handles;

        public bool IsDisposed { get; private set; }

        public VSFilesystem(ICoreClientAPI api, string modID)
        {
            this.api = api;
            this.modID = modID;

            handles = new GCHandle[4];
        }

        public static nint AllocateDelegate<TDelegate>(TDelegate d, out GCHandle handle) where TDelegate : Delegate
        {
            handle = GCHandle.Alloc(d);
            return Marshal.GetFunctionPointerForDelegate(d);
        }

        public ULFileSystem? GetNativeStruct()
        {
            unsafe
            {
                return new()
                {
                    FileExists = (delegate* unmanaged[Cdecl]<ULString*, bool>)AllocateDelegate((ULString* path) => FileExists(path->ToString()), out handles[0]),
                    GetFileMimeType = (delegate* unmanaged[Cdecl]<ULString*, ULString*>)AllocateDelegate((ULString* path) => new ULString(GetFileMimeType(path->ToString())).Allocate(), out handles[1]),
                    GetFileCharset = (delegate* unmanaged[Cdecl]<ULString*, ULString*>)AllocateDelegate((ULString* path) => new ULString(GetFileCharset(path->ToString())).Allocate(), out handles[2]),
                    OpenFile = (delegate* unmanaged[Cdecl]<ULString*, ULBuffer>)AllocateDelegate((ULString* path) => OpenFile(path->ToString()), out handles[3])
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
            api.Logger.Notification("OpenFile: " + path);
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

        public void Dispose()
        {
            api.Logger.Notification("VSFilesystem.Dispose()");
            if (IsDisposed) return;
            if (handles is not null)
            {
                foreach (var handle in handles) if (handle.IsAllocated) handle.Free();
            }

            try { Dispose(); }
            finally
            {
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }
    }
}
