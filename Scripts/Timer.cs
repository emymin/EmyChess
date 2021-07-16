
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using System;
using Emychess.Interactions;

namespace Emychess
{
    /// <summary>
    /// Timer behaviour
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class Timer : UdonSharpBehaviour
    {
        [UdonSynced(UdonSyncMode.Linear)][HideInInspector]
        public float timeBlack;
        [UdonSynced(UdonSyncMode.Linear)][HideInInspector]
        public float timeWhite;
        [UdonSynced][HideInInspector]
        public bool isStarted;
        [UdonSynced][HideInInspector]
        public bool isCurrentSideWhite;
        [UdonSynced][HideInInspector]
        public byte moves;

        public ChessManager chessManager;
        public Text displayBlack;
        public Text millisecondsDisplayBlack;
        public Text displayWhite;
        public Text millisecondsDisplayWhite;
        public Text displayMoves;
        public TimerButton whiteButton;
        public TimerButton blackButton;
        

        private float startingTime;
        private float startedCountingDownTime;
        
        /// <summary>
        /// Formats the time to hours:minutes:seconds milliseconds
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public string GetTimeString(float time)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            string timeText = string.Format("{0:D2}:{1:D2}:{2:D2} {3:D3}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds,timeSpan.Milliseconds);
            return timeText;

        }

        public void _RefreshDisplay()
        {
            string[] timeBlackStrings = GetTimeString(timeBlack).Split(' ');
            string[] timeWhiteStrings = GetTimeString(timeWhite).Split(' ');
            displayBlack.text = timeBlackStrings[0];
            displayWhite.text = timeWhiteStrings[0];
            millisecondsDisplayBlack.text = timeBlackStrings[1];
            millisecondsDisplayWhite.text = timeWhiteStrings[1];
            displayMoves.text = moves.ToString("D3");
            whiteButton.SetRaised(isCurrentSideWhite);
            blackButton.SetRaised(!isCurrentSideWhite);
        }
        public override void OnDeserialization()
        {
            _RefreshDisplay();
        }

        /// <summary>
        /// Sets the time for a specific side
        /// </summary>
        /// <param name="time">Time in seconds, maximum is 99 hours</param>
        /// <param name="white">Specified side</param>
        /// <returns></returns>
        public bool _SetTime(float time,bool white)
        {
            if (time <= 60 * 60 * 99 && time>=0)
            {
                if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer)
                {
                    Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                }

                if (white) { timeWhite = time; } else { timeBlack = time; }
                _RefreshDisplay();
                return true;
            }
            else { return false; }
        }
        /// <summary>
        /// Sets the time for both sides, see <see cref="_SetTime(float, bool)"/>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool _SetTimeBothSides(float time)
        {
            bool successwhite = _SetTime(time, true);
            bool successblack = _SetTime(time, false);
            return successwhite && successblack;
        }

        public void _AddTime(float time)
        {
            if (!isStarted)
            {
                _SetTimeBothSides(timeWhite + time);
            }
        }
        public void _Add30s()
        {
            _AddTime(30);
        }
        public void _Add1m()
        {
            _AddTime(60);
        }
        public void _Add15m()
        {
            _AddTime(60 * 15);
        }
        /// <summary>
        /// Start the timer for the specified side
        /// </summary>
        /// <param name="white"></param>
        public void _StartCountDown(bool white)
        {
            if(timeBlack>0 && timeWhite > 0)
            {

                Networking.SetOwner(Networking.LocalPlayer, this.gameObject); //TODO the side currently counting down should have ownership of the timer
                isCurrentSideWhite = white;
                startedCountingDownTime = Time.time;
                startingTime = white ? timeWhite : timeBlack;
                if (moves < 255) { moves++; }
                isStarted = true;
            }
            _RefreshDisplay();
        }
        public void _ResetTimer(float time)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            _SetTimeBothSides(time);
            isStarted = false;
            moves = 0;
            isCurrentSideWhite = true;
            _RefreshDisplay();
        }
        public void _ResetTimerEvent()
        {
            _ResetTimer(0);
        }
        /// <summary>
        /// Switches the currently counting down side
        /// </summary>
        public void _SwitchSide()
        {
            _StartCountDown(!isCurrentSideWhite);
        }
        public void Update()
        {
            
            if (Networking.GetOwner(this.gameObject)==Networking.LocalPlayer)
            {
                if (isStarted && (!chessManager.automatedTimer || (chessManager.gameOverMessage.gameOverState==0) ))
                {
                    
                    float newTime = startingTime - (Time.time - startedCountingDownTime);
                    bool success = _SetTime(newTime, isCurrentSideWhite);
                    if (!success)
                    {
                        if (newTime <= 0 && chessManager.automatedTimer && chessManager.inProgress) { chessManager.gameOverMessage._SetGameOverMessage(3, !isCurrentSideWhite); }
                        //_ResetTimer(0); 
                    }
                }
                _RefreshDisplay();
            }
        }
        public void Start()
        {
            if (Networking.IsMaster) { _ResetTimer(0); }
        }

    }

}