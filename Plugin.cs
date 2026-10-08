using Astra.Sdk;
using BepInEx;

namespace AstraRepo
{
    /// <summary>
    /// Astra for R.E.P.O.: she comes along on the extraction.
    /// <list type="bullet">
    /// <item>she accompanies YOUR player and is drawn into the camera you look through;</item>
    /// <item>she is sized to your player's standing height and follows you through the level;</item>
    /// <item>raw facts for her animation set every frame — crouching, crawling, sliding, tumbling,
    /// sprinting. What they look like is the set's business.</item>
    /// </list>
    /// Every game type (<c>PlayerController</c>, <c>PlayerAvatar</c>, <c>CameraUtils</c>) is reached
    /// by NAME through <see cref="Astra.Sdk.GameType"/>: this plugin compiles against the Astra SDK,
    /// BepInEx and Unity only — never the game's own assemblies, and no longer needs the publicizer.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(AstraSdk.Guid, AstraSdk.Dependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        static readonly GameType CameraUtilsType = GameType.Find("CameraUtils");
        static readonly Member<UnityEngine.Camera> MainCamera = CameraUtilsType.Member<UnityEngine.Camera>("MainCamera");

        void Awake()
        {
            var astra = AstraSdk.Register(MyPluginInfo.PLUGIN_GUID, "R.E.P.O.");
            astra.Defaults.MatchPlayerHeight = 0.95f;
            astra.Defaults.TeleportDistance = 20f;
            astra.UseCamera(() => MainCamera.Get(CameraUtilsType.Static<object>("Instance")));
            astra.UsePlayer(_ => RepoPlayer.Locate());
            astra.OnFrame(f =>
            {
                var pc = RepoPlayer.Local;
                if (pc == null) return;
                var avatar = RepoPlayer.Avatar(pc);
                f.Params.Set("crouching", RepoPlayer.Crouching.Get(pc))
                    .Set("crawling", RepoPlayer.Crawling.Get(pc))
                    .Set("sliding", RepoPlayer.IsSliding.Get(avatar))
                    .Set("tumbling", RepoPlayer.IsTumbling.Get(avatar))
                    .Set("sprinting", RepoPlayer.IsSprinting.Get(avatar));
            });
            Logger.LogInfo("Astra is coming along on the extraction");
        }
    }
}
