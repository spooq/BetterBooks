using System;
using System.Runtime.InteropServices;
using UltralightNet;
using UltralightNet.Platform;
using UltralightNet.Platform.HighPerformance;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VSUL
{
    public class VSFilesystem : IFileSystem
    {
        public ICoreAPI api;

        private readonly GCHandle[] handles;

        public bool IsDisposed { get; private set; }

        public VSFilesystem(ICoreClientAPI api)
        {
            this.api = api;

            handles = new GCHandle[4];
        }


        public ULFileSystem? GetNativeStruct()
        {
            unsafe
            {
                return new()
                {
                    FileExists = (delegate* unmanaged[Cdecl]<ULString*, bool>)Utility.AllocateDelegate((ULString* path) => FileExists(path->ToString()), out handles[0]),
                    GetFileMimeType = (delegate* unmanaged[Cdecl]<ULString*, ULString*>)Utility.AllocateDelegate((ULString* path) => new ULString(GetFileMimeType(path->ToString())).Allocate(), out handles[1]),
                    GetFileCharset = (delegate* unmanaged[Cdecl]<ULString*, ULString*>)Utility.AllocateDelegate((ULString* path) => new ULString(GetFileCharset(path->ToString())).Allocate(), out handles[2]),
                    OpenFile = (delegate* unmanaged[Cdecl]<ULString*, ULBuffer>)Utility.AllocateDelegate((ULString* path) => OpenFile(path->ToString()), out handles[3])
                };
            }
        }

        public bool FileExists(string path)
        {
            var asset = Utility.UrlToAssetLocation(path);
            bool exists = api.Assets.Exists(asset);
            api.Logger.Debug($"FileExists: {path} => {asset.Path} => {exists}");
            return exists;
        }

        public string GetFileCharset(string path)
        {
            string charset = "utf-8";
            api.Logger.Debug($"GetFileCharset: {path} => charset {charset}");
            return charset;
        }

        public string GetFileMimeType(string path)
        {
            string mimeType = MimeMapping.MimeUtility.GetMimeMapping(path);
            api.Logger.Debug($"GetFileMimeType: {path} => mimetype {mimeType}");
            return mimeType;
        }

        public ULBuffer OpenFile(string path)
        {
            var asset = Utility.UrlToAssetLocation(path);
            var data = api.Assets.Get(asset);
            api.Logger.Debug($"FileExists: {path} => {asset.Path} => size {data.Data.Length}");

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
            api.Logger.Debug("VSFilesystem.Dispose()");
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
