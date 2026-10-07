using Astra.Sdk;
using UnityEngine;

namespace AstraRepo
{
    /// <summary>R.E.P.O.'s local player, as the Astra SDK sees a player.</summary>
    static class RepoPlayer
    {
        static PlayerController cached;
        static CapsuleCollider body;

        /// <summary>YOUR player (never another one), or null in the menu, while loading, dead or
        /// disabled (she then stays where she is).</summary>
        public static PlayerController Local
        {
            get
            {
                var pc = PlayerController.instance;
                var avatar = pc != null ? pc.playerAvatarScript : null;
                if (pc == null || avatar == null || avatar.deadSet || avatar.isDisabled) return null;
                return pc;
            }
        }

        public static PlayerInfo? Locate()
        {
            var pc = Local;
            if (pc == null) return null;
            if (pc != cached)
            {
                cached = pc;
                body = pc.GetComponentInChildren<CapsuleCollider>();
            }
            var at = pc.transform.position;
            bool grounded = pc.CollisionController == null || pc.CollisionController.Grounded;
            if (body == null) return new PlayerInfo { Feet = at, Forward = pc.transform.forward, Root = pc.gameObject, Grounded = grounded };
            var b = body.bounds;
            return new PlayerInfo
            {
                Feet = new Vector3(b.center.x, b.min.y, b.center.z),
                Forward = pc.transform.forward,
                Root = pc.gameObject,
                Grounded = grounded,
                // Her size follows the player's STANDING height only (a crouch or a crawl shrinks the capsule).
                Height = !pc.Crouching && !pc.Crawling ? b.size.y : 0,
            };
        }
    }
}
