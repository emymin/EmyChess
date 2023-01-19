
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Emychess.Misc
{
    /// <summary>
    /// Simple behaviour to make an object SPEEN (used in the gameover text)
    /// </summary>
    public class Spin : UdonSharpBehaviour
    {
        public float speed;
        public void Update()
        {
            transform.Rotate(Vector3.up, speed);
        }
        public void OnEnable()
        {
            transform.rotation = Quaternion.identity;
        }
    }

}
