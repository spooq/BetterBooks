using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace VSUL
{
    public static class Utility
    {
        public static nint AllocateDelegate<TDelegate>(TDelegate d, out GCHandle handle) where TDelegate : Delegate
        {
            handle = GCHandle.Alloc(d);
            return Marshal.GetFunctionPointerForDelegate(d);
        }

        public static AssetLocation UrlToAssetLocation(string url)
        {
            var dirs = url.Split(Path.DirectorySeparatorChar).ToList();
            var modId = dirs.PopOne().ToLower();
            return new AssetLocation(modId, "config/" + string.Join(Path.DirectorySeparatorChar, dirs));
        }
    }
}
