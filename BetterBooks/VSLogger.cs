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

        public VSLogger(ICoreClientAPI api)
        {
            this.api = api;
        }

        public void Dispose()
        {
        }
        public ULLogger? GetNativeStruct()
        {
            unsafe
            {
                return new ULLogger
                {
                    LogMessage = (delegate* unmanaged[Cdecl]<ULLogLevel, ULString*, void>)
                        Marshal.GetFunctionPointerForDelegate((ULLogLevel logLevel, ULString* message) => LogMessage(logLevel, message->ToString()))
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
    }
}
