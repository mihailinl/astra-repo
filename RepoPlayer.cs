using Astra.Sdk;
using UnityEngine;

namespace AstraRepo
{
    /// <summary>
    /// R.E.P.O.'s local player, as the Astra SDK sees a player. Reached by NAME
    /// (<see cref="Astra.Sdk.GameType"/>): this plugin never compiles against the game's own
    /// assemblies and no longer needs the BepInEx community's publicizer (verified against the
    /// installed game with <c>tools/check-game-members</c> in astra-bepinex).
    /// </summary>
    static class RepoPlayer
    {
        static readonly GameType PlayerControllerType = GameType.Find("PlayerController");
        static readonly GameType PlayerAvatarType = GameType.Find("PlayerAvatar");
        static readonly GameType PlayerCollisionControllerType = GameType.Find("PlayerCollisionController");

        static readonly Member<object> PlayerAvatarScript = PlayerControllerType.Member<object>("playerAvatarScript");
        static readonly Member<object> CollisionController = PlayerControllerType.Member<object>("CollisionController");

        /// <summary>Raw facts <see cref="AstraRepo.Plugin"/> reads every frame.</summary>
        public static readonly Member<bool> Crouching = PlayerControllerType.Member<bool>("Crouching");
        public static readonly Member<bool> Crawling = PlayerControllerType.Member<bool>("Crawling");

        static readonly Member<bool> DeadSet = PlayerAvatarType.Member<bool>("deadSet");
        static readonly Member<bool> IsDisabled = PlayerAvatarType.Member<bool>("isDisabled");
        public static readonly Member<bool> IsSliding = PlayerAvatarType.Member<bool>("isSliding");
        public static readonly Member<bool> IsTumbling = PlayerAvatarType.Member<bool>("isTumbling");
        public static readonly Member<bool> IsSprinting = PlayerAvatarType.Member<bool>("isSprinting");

        static readonly Member<bool> Grounded = PlayerCollisionControllerType.Member<bool>("Grounded");

        static object cachedPc;
        static CapsuleCollider body;

        /// <summary>YOUR player (never another one), or null in the menu, while loading, dead or
        /// disabled (she then stays where she is).</summary>
        public static object Local
        {
            get
            {
                var pc = PlayerControllerType.Static<object>("instance");
                if (pc == null) return null;
                var avatar = PlayerAvatarScript.Get(pc);
                return avatar != null && !DeadSet.Get(avatar) && !IsDisabled.Get(avatar) ? pc : null;
            }
        }

        /// <summary>The avatar script (a game-specific type, read only through further
        /// <see cref="GameType"/> members) behind <paramref name="player"/>.</summary>
        public static object Avatar(object player) => PlayerAvatarScript.Get(player);

        public static PlayerInfo? Locate()
        {
            var pc = Local;
            var root = pc as Component;
            if (root == null) return null;
            if (pc != cachedPc)
            {
                cachedPc = pc;
                body = root.GetComponentInChildren<CapsuleCollider>();
            }
            var cc = CollisionController.Get(pc);
            bool grounded = cc == null || Grounded.Get(cc);
            if (body == null)
                return new PlayerInfo { Feet = root.transform.position, Forward = root.transform.forward, Root = root.gameObject, Grounded = grounded };
            var b = body.bounds;
            return new PlayerInfo
            {
                Feet = new Vector3(b.center.x, b.min.y, b.center.z),
                Forward = root.transform.forward,
                Root = root.gameObject,
                Grounded = grounded,
                // Her size follows the player's STANDING height only (a crouch or a crawl shrinks the capsule).
                Height = !Crouching.Get(pc) && !Crawling.Get(pc) ? b.size.y : 0,
            };
        }
    }
}
