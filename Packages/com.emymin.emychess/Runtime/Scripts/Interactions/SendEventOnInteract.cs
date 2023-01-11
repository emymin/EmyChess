
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Emychess.Interactions
{
    public class SendEventOnInteract : UdonSharpBehaviour
    {
        public UdonBehaviour targetBehaviour;
        public string eventName;
        public override void Interact()
        {
            targetBehaviour.SendCustomEvent(eventName);
        }
    }

}
