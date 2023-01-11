
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Emychess.Misc
{
    /// <summary>
    /// Behaviour that shows or hides the game over message above the board
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GameOverMessage : UdonSharpBehaviour
    {
        [UdonSynced][HideInInspector]
        public byte gameOverState; //0: not over, 1 checkmate, 2 stalemate, 3 time over
        [UdonSynced][HideInInspector]
        public bool winningSide;
        public ChessManager chessManager;
        public Text statusMessage;
        public GameObject uiParent;

        public void _Refresh()
        {
            bool isGameOver = gameOverState != 0;
            uiParent.SetActive(isGameOver);
            if (isGameOver)
            {
                string winningPlayerName = chessManager.GetPlayer(winningSide).displayName;
                string message = "";
                switch (gameOverState)
                {
                    case 1: 
                        message = "CHECKMATE! Winner: " + winningPlayerName;
                        break;
                    case 2: 
                        message = "STALEMATE, nobody wins";
                        break;
                    case 3:
                        message = "TIME'S UP! Winner: " + winningPlayerName;
                        break;
                    default:
                        message = "ERROR, invalid game over state";
                        break;
                }
                statusMessage.text = message;
                chessManager.board._AllPiecesUngrabbable();
            }
        }
        public override void OnDeserialization()
        {
            _Refresh();
        }

        /// <summary>
        /// Sets a game over message
        /// </summary>
        /// <param name="type">1 if checkmate, 2 if stalemate, 3 if time is up</param>
        /// <param name="winningSide"></param>
        public void _SetGameOverMessage(int type,bool winningSide)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            gameOverState = (byte)type;
            this.winningSide = winningSide;
            _Refresh();
            RequestSerialization();
        }
        /// <summary>
        /// Hides the game over message
        /// </summary>
        public void _Reset()
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            gameOverState = 0;
            _Refresh();
            RequestSerialization();
        }
        public void Start()
        {
            if (Networking.IsMaster) { _Reset(); }
        }
    }

}
