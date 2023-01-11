
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Emychess.Misc
{
    /// <summary>
    /// Stores a player reference using object ownership, to keep track of who's playing
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerHolder : UdonSharpBehaviour
    {
        /// <summary>
        /// Can't set owner on an object without any networked data on it, so I'm just gonna put a smol synced bool that doesn't do anything
        /// </summary>
        [UdonSynced]
        private bool _donotuse;
        public void _SetOwner()
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }
        public VRCPlayerApi GetOwner()
        {
            return Networking.GetOwner(this.gameObject);
        }
    }
}
