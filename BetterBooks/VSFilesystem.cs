﻿using System;
using System.Runtime.InteropServices;
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

        public ULFileSystem? GetNativeStruct() => null;

        public bool FileExists(string path)
        {
            bool result = api.Assets.Exists(new AssetLocation(modID, "config/" + path.ToLower()));
            api.Logger.Notification("FileExists: " + path + " - " + (result ? "yes" : "no"));
            return result;
        }

        public string GetFileCharset(string path)
        {
            api.Logger.Notification("GetFileCharset: " + path);
            throw new NotImplementedException();
        }

        public string GetFileMimeType(string path)
        {
            api.Logger.Notification("GetFileMimeType: " + path);
            // no.
            return "text/utf8";
        }

        public ULBuffer OpenFile(string path)
        {
            api.Logger.Notification("OpenFile: " + path);

            var data = api.Assets.Get(new AssetLocation(modID, "config/" + path.ToLower()));

            ULBuffer buffer;
            nint ptr = 0;

            try
            {
                ptr = Marshal.AllocHGlobal(data.Data.Length);
                Marshal.Copy(data.Data, 0, ptr, data.Data.Length);

                unsafe
                {
                    buffer = ULBuffer.CreateFromDataCopy((void*)ptr, (uint)data.Data.Length);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return buffer;
        }

        public ULBuffer OpenFile3(string path)
        {
            var data = api.Assets.Get(new AssetLocation(modID, "config/" + path.ToLower()));

            unsafe
            {
                fixed (void* ptr = &data.Data[0])
                {
                    return ULBuffer.CreateFromDataCopy(ptr, (uint)data.Data.Length);
                }
            }
        }

        public ULBuffer OpenFile2(string path)
        {
            var data = api.Assets.Get(new AssetLocation(modID, "config/" + path.ToLower()));

            ULBuffer buffer;
            nint ptr = 0;

            try
            {
                ptr = Marshal.AllocHGlobal(data.Data.Length);
                Marshal.Copy(data.Data, 0, ptr, data.Data.Length);

                unsafe
                {
                    buffer = ULBuffer.CreateFromDataCopy((void*)ptr, (uint)data.Data.Length);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return buffer;
        }
    }
}
