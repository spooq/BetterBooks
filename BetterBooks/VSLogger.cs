using System;
using System.Runtime.InteropServices;
using UltralightNet;
using UltralightNet.Platform.HighPerformance;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VSUL
{
    public class VSLogger : UltralightNet.Platform.ILogger
    {
        public ICoreAPI api;

        readonly GCHandle[] handles;

        public bool IsDisposed { get; private set; }

        public VSLogger(ICoreClientAPI api)
        {
            this.api = api;

            handles = new GCHandle[1];
        }

        public ULLogger? GetNativeStruct()
        {
            unsafe
            {
                return new ULLogger
                {
                    LogMessage = (delegate* unmanaged[Cdecl]<ULLogLevel, ULString*, void>)Utility.AllocateDelegate((ULLogLevel logLevel, ULString* message) => LogMessage(logLevel, message->ToString()), out handles[0])
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
            api.Logger.Debug("VSLogger.Dispose()");
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
