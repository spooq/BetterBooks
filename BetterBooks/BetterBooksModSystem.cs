using System;
using System.IO;
using UltralightNet;
using UltralightNet.AppCore;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace BetterBooks
{
    public class BetterBooksModSystem : ModSystem
    {
        public enum LoadingState
        {
            Start,
            Starting,
            Loaded,
            Waiting,
            Done,
            Error
        };

        public Renderer renderer;
        public View view;
        public VSFilesystem fs;
        public VSLogger logger;

        LoadingState state = LoadingState.Starting;

        public ICoreClientAPI capi;

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            capi = api;
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            base.AssetsFinalize(api);

            if (api.Side == EnumAppSide.Server)
                return;

            api.Assets.Reload(AssetCategory.config);

            // Sneak in native dlls
            EmbeddedDllClass.ExtractEmbeddedDlls();
            EmbeddedDllClass.LoadDll("UltralightCore.dll", api);
            EmbeddedDllClass.LoadDll("WebCore.dll", api);
            EmbeddedDllClass.LoadDll("Ultralight.dll", api);
            EmbeddedDllClass.LoadDll("AppCore.dll", api);

            AppCoreMethods.SetPlatformFontLoader();

            logger = new VSLogger(capi);
            ULPlatform.Logger = logger;
            fs = new VSFilesystem(capi, Mod.Info.ModID);
            ULPlatform.FileSystem = fs;

            var cfg = new ULConfig();
            renderer = ULPlatform.CreateRenderer(cfg);

            var viewCfg = new ULViewConfig()
            {
                IsTransparent = true,
                EnableImages = true,
                EnableJavaScript = true,
            };
            view = renderer.CreateView(1200, 1000);

            view.OnFinishLoading += (_, _, _) =>
            {
                api.Logger.Notification($"OnFinishLoading");
                state = LoadingState.Waiting;
            };

            view.OnFailLoading +=
                (ulong frameId,
                bool isMainFrame,
                string url,
                string description,
                string errorDomain,
                int errorCode) =>
                {
                    api.Logger.Error($"{errorDomain} : {errorCode} : {description}");
                };

            api.Event.RegisterGameTickListener(ClientOnGameTick, 50);

            try
            {
                view.URL = "file:///epub.html";
                //view.HTML = "<html>hello world</html>";
                renderer.Update();
                renderer.Render();
                writeBitmap();
                state = LoadingState.Done;
            }
            catch (Exception ex)
            {
                api.Logger.Error(ex.ToString());
            }
        }

        public void ClientOnGameTick(float dt)
        {
            capi.Logger.Notification("ClientOnGameTick");
            string ex = null;
            string result = null;

            switch (state)
            {
                case LoadingState.Start:
                    state = LoadingState.Starting;
                    break;

                case LoadingState.Starting:
                    renderer.Update();
                    break;

                case LoadingState.Waiting:
                    renderer.Update();
                    result = view.EvaluateScript("ready", out ex);
                    if (result == "loaded")
                        state = LoadingState.Loaded;
                    break;

                case LoadingState.Loaded:
                    renderer.Render();
                    writeBitmap();
                    state = LoadingState.Done;
                    break;

                case LoadingState.Done:
                    break;

                case LoadingState.Error:
                    break;
            };
        }

        public void writeBitmap()
        {
            ULSurface surface = view.Surface ?? throw new Exception("Surface not found, did you perhaps set ViewConfig.IsAccelerated to true?");
            ULBitmap bitmap = surface.Bitmap;
            var path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            bitmap.WritePng(Path.Combine(path, $"OUTPUT.png"));
        }
    }
}
