
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3;
using UnityEngine.UI;

namespace Emychess.Interactions
{
    /// <summary>
    /// Behaviour that lets users spawn pieces while in anarchy mode
    /// </summary>
    public class PiecePlacer : UdonSharpBehaviour
    {
        public Board board;
        public bool white;
        [HideInInspector]
        public string currentType;
        [HideInInspector]
        public VRC_Pickup pickup;
        [Tooltip("The parent that holds the pieces that are part of the PiecePlacer interface")]
        public Transform pieceMeshParent;
        public Text availableText;
        public Text pieceCount;

        private string[] types = { "pawn", "knight", "rook", "bishop", "queen", "king" };
        //TODO should index be [UdonSynced] to sync the interface for everyone?
        private byte index = 0;

        public void _Setup()
        {
            index = 0;
            currentType = types[index];
            SetEnabled(true);
            _Refresh();
        }

        public void SetEnabled(bool enabled)
        {
            if (pickup == null) { pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup)); }
            pickup.pickupable = enabled;
        }
        public void _NextType()
        {
            index = (byte)((index + 1) % types.Length);
            currentType = types[index];
            _Refresh();
        }
        public void _PreviousType()
        {
            index = (byte)(((index - 1) + types.Length) % types.Length);
            currentType = types[index];
            _Refresh();
        }
        public void _Refresh()
        {
            if (currentType == "" || currentType == null) { _Setup();return; }
            foreach(Transform child in pieceMeshParent)
            {
                child.gameObject.SetActive(child.name == currentType);
            }
            availableText.text = "Available " + currentType + "s";
            pieceCount.text = board.GetPiecesAvailableCount(currentType).ToString();
        }
        public override void OnDrop()
        {
            Vector3 pos = board.pieces_parent.InverseTransformPoint(transform.position); //TODO board should have methods to get coordinates from position and viceversa, also for Piece placement
            int x = (int)pos.x*-1-1;
            int y = (int)pos.z*-1-1;
            if (board.currentRules.anarchy && (board.GetPiecesAvailableCount(currentType)>0) )
            {
                Piece captured = board.GetPiece(x, y);
                if (captured != null) captured._Capture();
                Piece spawned = board._SpawnPiece(x, y, white, currentType);
                if (spawned != null)
                {
                    spawned.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PieceMovedAudio");
                }
            }
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            _Refresh();
        }
        public void OnEnable()
        {
            _Setup();
        }
    }

}
