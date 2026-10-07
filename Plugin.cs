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
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(AstraSdk.Guid, AstraSdk.Dependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            var astra = AstraSdk.Register(MyPluginInfo.PLUGIN_GUID, "R.E.P.O.");
            astra.Defaults.MatchPlayerHeight = 0.95f;
            astra.Defaults.TeleportDistance = 20f;
            astra.UseCamera(() => CameraUtils.Instance != null ? CameraUtils.Instance.MainCamera : null);
            astra.UsePlayer(_ => RepoPlayer.Locate());
            astra.OnFrame(f =>
            {
                var pc = RepoPlayer.Local;
                if (pc == null) return;
                var avatar = pc.playerAvatarScript;
                f.Params.Set("crouching", pc.Crouching)
                    .Set("crawling", pc.Crawling)
                    .Set("sliding", avatar.isSliding)
                    .Set("tumbling", avatar.isTumbling)
                    .Set("sprinting", avatar.isSprinting);
            });
            Logger.LogInfo("Astra is coming along on the extraction");
        }
    }
}
