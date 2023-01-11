
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Emychess.Interactions
{
    /// <summary>
    /// Behaviour for the timer buttons
    /// </summary>
    public class TimerButton : UdonSharpBehaviour
    {
        public bool allowPush=false;
        public bool isRaised;
        public float raiseAngle;
        public Timer timer;

    
        /// <summary>
        /// Make the button raised or pressed
        /// </summary>
        /// <param name="raised"></param>
        public void SetRaised(bool raised)
        {
            isRaised = raised;
            Quaternion currentRot = transform.localRotation;
            transform.localEulerAngles = new Vector3(raised ? raiseAngle : 0, currentRot.eulerAngles.y, currentRot.eulerAngles.z);
            DisableInteractive = !(isRaised&&(!timer.chessManager.automatedTimer)); //TODO should it only be clicked by players? or is it better to have everyone be able to click it?
        }
        
        public override void Interact()
        {
            if (!allowPush && !timer.chessManager.automatedTimer)
            {
                timer._SwitchSide();
            }
            
        }
    
    }

}
