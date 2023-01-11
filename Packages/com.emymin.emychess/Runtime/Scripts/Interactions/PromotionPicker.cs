
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Emychess.Interactions
{
    /// <summary>
    /// Behaviour for the promotion interface, show with <see cref="Setup(Piece)"/>, hide with <see cref="_Hide"/>
    /// </summary>
    public class PromotionPicker : UdonSharpBehaviour
    {
        public Board board;
        public GameObject piecesButtonsParent;
        [HideInInspector]
        public Piece targetPiece;
        private PromotionButton[] promotionButtons;

        public void Start()
        {
            promotionButtons = new PromotionButton[piecesButtonsParent.transform.childCount];
            for(int i = 0; i < piecesButtonsParent.transform.childCount; i++)
            {
                GameObject child = piecesButtonsParent.transform.GetChild(i).gameObject;
                promotionButtons[i] = (PromotionButton)(UdonSharpBehaviour)child.GetComponent(typeof(UdonBehaviour));
            }
        }
        /// <summary>
        /// Function that shows the promotion interface above the specified piece
        /// </summary>
        /// <param name="piece"></param>
        public void Setup(Piece piece)
        {
            targetPiece = piece;
            board._AllPiecesUngrabbable();
            transform.localPosition = new Vector3(-(targetPiece.x + .5f), 0, -(targetPiece.y+.5f));
            transform.localRotation = targetPiece.white ? Quaternion.identity : Quaternion.identity * Quaternion.Euler(0, 180f, 0);
            piecesButtonsParent.SetActive(true);
            foreach(PromotionButton button in promotionButtons)
            {
                button._RefreshCounter();
            }
        }
        public void _PromotionDone()
        {
            _Hide();
            board.chessManager._EndTurn();
        }
        public void _Hide()
        {
            piecesButtonsParent.SetActive(false);
        }
        public void PromoteTo(string type)
        {
            if (type == "pawn")
            {
                _PromotionDone();
                return;
            }
            if (board.GetPiecesAvailableCount(type) > 0 )
            {
                int x = targetPiece.x;
                int y = targetPiece.y;
                bool white = targetPiece.white;
                targetPiece._ReturnToPool();
                board._SpawnPiece(x, y, white, type);
                _PromotionDone();
            }
        }
        
    }

}
