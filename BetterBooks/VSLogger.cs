using System;
using System.Runtime.InteropServices;
using UltralightNet;
using UltralightNet.Platform.HighPerformance;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace BetterBooks
{
    public class VSLogger : UltralightNet.Platform.ILogger
    {
        public ICoreAPI api;

        readonly GCHandle[]? handles;

        public bool IsDisposed { get; private set; }

        public VSLogger(ICoreClientAPI api)
        {
            this.api = api;

            handles = new GCHandle[1];
        }

        public static nint AllocateDelegate<TDelegate>(TDelegate d, out GCHandle handle) where TDelegate : Delegate
        {
            handle = GCHandle.Alloc(d);
            return Marshal.GetFunctionPointerForDelegate(d);
        }

        public ULLogger? GetNativeStruct()
        {
            unsafe
            {
                return new ULLogger
                {
                    LogMessage = (delegate* unmanaged[Cdecl]<ULLogLevel, ULString*, void>)
                        AllocateDelegate((ULLogLevel logLevel, ULString* message) => LogMessage(logLevel, message->ToString()), out handles[0])
                };
            }
        }

        public void LogMessage(ULLogLevel logLevel, string message)
        {
            switch (logLevel)
            {
                case ULLogLevel.Error:
                    api.Logger.Error(message);
                    break;
                case ULLogLevel.Warning:
                    api.Logger.Warning(message);
                    break;
                case ULLogLevel.Info:
                    api.Logger.Notification(message);
                    break;
                default:
                    api.Logger.Notification(message);
                    break;
            }
            
        }

        public void Dispose()
        {
            api.Logger.Notification("VSLogger.Dispose()");
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
