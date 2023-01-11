
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using VRC.Udon.Common;

namespace Emychess
{
    /// <summary>
    /// Behaviour for a generic chess piece
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Piece : UdonSharpBehaviour
    {
        #region Piece state
        /// <summary>
        /// Synced state of the piece, fit in a single byte to improve network usage, do NOT access and set directly
        /// </summary>
        /// <remarks>
        /// first 3 bits represent the x position, next 3 bits the y, 1 bit white, 1 bit hasMoved;
        /// </remarks>
        [UdonSynced]
        private byte _state;
        /// <summary>
        /// X position (3 bits maximum)
        /// </summary>
        public byte x
        {
            get { return (byte)(_state >> 5); }
            set { _state &= 0b00011111; _state |= (byte)(value << 5); }
        }
        /// <summary>
        /// Y position (3 bits maximum)
        /// </summary>
        public byte y
        {
            get { return (byte)((_state & 0b00011100) >> 2); }
            set { _state &= 0b11100011; _state |= (byte)(value << 2); }
        }
        /// <summary>
        /// Wheter the piece is white
        /// </summary>
        public bool white
        {
            get { return ((_state & 0b00000010) >> 1) == 1; }
            set { _state &= 0b11111101; _state |= (byte)((value ? 1 : 0) << 1); }
        }
        /// <summary>
        /// Wheter the piece has moved once after the start of the game
        /// </summary>
        public bool hasMoved
        {
            get { return (_state & 0b00000001) == 1; }
            set { _state &= 0b11111110; _state |= (byte)(value ? 1 : 0); }
        }

        public void _DebugState()
        {
            Debug.Log("State: " + _state +" Properties "+ x + " " + y + " " + white + " " + hasMoved+" "+type,this);
        }

        #endregion

        private int previousX;
        private int previousY;
        private bool justMoved;


        private Renderer pieceRenderer;
        /// <summary>
        /// The currently legal moves that the piece can make, the DefaultRules behaviour generates them
        /// </summary>
        private Vector2[] legalMoves;
        private VRC_Pickup pickup;


        //set by board's inspector script, make sure to generate pools
        [HideInInspector]
        public VRCObjectPool pool;
        [HideInInspector]
        public Board board;
        [HideInInspector]
        public string type;


        public void _UpdateState()
        {
            if (transform.parent != board.pieces_parent) { transform.SetParent(board.pieces_parent); }
            if (pickup == null) { pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup)); }
            if (board.chessManager.inProgress)
            {
                if(Networking.LocalPlayer==board.chessManager.GetPlayer(board.chessManager.currentSide) && board.chessManager.currentSide==white)
                {
                    SetGrabbable(true);
                }
                else
                {
                    SetGrabbable(false);
                }
            }
            else
            {
                SetGrabbable(false);
            }

            if (type == "pawn")
            {
                if (Mathf.Abs(previousY - y) == 2) //NOTE, for new joiners this is gonna be incorrect, but it's fine
                {
                    board.PawnThatDidADoublePushLastRound = this;
                }
            }

            //set the correct transform for the piece on the board
            transform.localPosition = new Vector3(-(x + .5f), 0, -(y + .5f));
            transform.localRotation = white ? Quaternion.identity : Quaternion.identity * Quaternion.Euler(0, 180f, 0);
            transform.localScale = Vector3.one;
            previousX = x;
            previousY = y;

            if (pieceRenderer == null) { pieceRenderer = GetComponent<Renderer>(); }
            pieceRenderer.material = white ? board.whitePieceMaterial : board.blackPieceMaterial;

            board._RegenerateGrid();
            board.chessManager.RefreshPiecePlacerPieceCount();

            if ((board.whiteKing == null || !board.whiteKing.gameObject.activeInHierarchy) && type == "king" && white) { board.whiteKing = this; }
            if ((board.blackKing == null || !board.blackKing.gameObject.activeInHierarchy) && type == "king" && !white) { board.blackKing = this; }

        }
        public override void OnDeserialization() //TODO late joiners sometimes can't see all pieces, some limit on manual syncing?
        {
            _UpdateState();
        }
        public void _RequestSerialization()
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            RequestSerialization();
        }
        public void _SetPosition(int posx, int posy)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            x = (byte)Mathf.Clamp(posx, 0, 7);
            y = (byte)Mathf.Clamp(posy, 0, 7);
            _UpdateState();
            RequestSerialization();
        }
        public void _Setup(int posx, int posy, bool isWhite)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            x = (byte)Mathf.Clamp(posx, 0, 7);
            y = (byte)Mathf.Clamp(posy, 0, 7);
            white = isWhite;
            hasMoved = false;
            _UpdateState();
            //RequestSerialization();
            SendCustomEventDelayedFrames("_RequestSerialization", 20); //is this a bug? race condition? I honestly have no clue, but if I don't delay this, it just doesn't sync correctly when spawning a lot of pieces. EVEN FOR LATER JOINERS, HOW????
        }

        /// <summary>
        /// Get the piece value based on the chess piece relative value system
        /// </summary>
        /// <returns></returns>
        public int GetValue()
        {
            if (type == "pawn") return 1;
            if (type == "bishop" || type == "knight") return 3;
            if (type == "rook") return 5;
            if (type == "queen") return 9;
            return 0;
        }

        public void _Capture()
        {
            if (pickup.IsHeld) { pickup.Drop(); }
            _ReturnToPool();
            board.chessManager.AddScore(GetValue(), !white);
        }
        /// <summary>
        /// Remove piece from board and return to the VRC object pool
        /// </summary>
        public void _ReturnToPool()
        {
            Networking.SetOwner(Networking.LocalPlayer, pool.gameObject);
            pool.Return(this.gameObject);
        }
        /// <summary>
        /// Get the piece's position as a Vec2 (vec2int is not supported in Udon yet)
        /// </summary>
        /// <returns></returns>
        public Vector2 GetVec()
        {
            return new Vector2(x, y);
        }
        /// <summary>
        /// Play the piece move sound
        /// </summary>
        public void PieceMovedAudio()
        {
            GetComponent<AudioSource>().PlayOneShot(board.pieceMovedClip);
        }
        /// <summary>
        /// Play the piece capture sound
        /// </summary>
        public void PieceCapturedAudio()
        {
            GetComponent<AudioSource>().PlayOneShot(board.pieceCapturedClip);
        }
        public void SetGrabbable(bool grabbable)
        {
            pickup.pickupable = grabbable;
        }

        //TODO there should be a desktop mode that gets a nicer view of the board and can move with mouse

        /// <summary>
        /// Function to be called when the piece prepared for a move (for example, when it's picked up). Finds all legal moves, stores them in <see cref="legalMoves"/> and shows them on the board with <see cref="Board.SetIndicator(int, int, float)"/>
        /// </summary>
        public void PiecePicked()
        {
            if (!board.currentRules.anarchy)
            {
                board._AllPiecesUngrabbable();
                legalMoves = board.currentRules.GetAllLegalMoves(this, board);
                foreach (Vector2 legalMove in legalMoves)
                {
                    if (legalMove != board.currentRules.legalMovesIgnoreMarker)
                    {
                        if (legalMove == board.currentRules.legalMovesEndMarker) { break; }
                        board.SetIndicator((int)legalMove.x, (int)legalMove.y, 1);
                    }
                }
            }
        }


        public override void OnPickup() //TODO there should be some system so others can see your picked up piece
        {
            PiecePicked();

        }

        /// <summary>
        /// Function to be called at the end of a move attempt. Will pass <see cref="legalMoves"/> to <see cref="GameRules.DefaultRules.MoveLegalCheck(Piece, int, int, Board, Vector2[])"/> to perform the move
        /// </summary>
        /// <param name="newx"></param>
        /// <param name="newy"></param>
        public void PieceDropped(int newx,int newy)
        {
            int moveResult = 0;
            if (board.currentRules.anarchy) { moveResult = board.currentRules.Move(this, newx, newy, board); }
            else { moveResult = board.currentRules.MoveLegalCheck(this, newx, newy, board, legalMoves); }

            if (moveResult != 0)
            {
                if (moveResult == 1)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PieceMovedAudio");
                }
                else if (moveResult == 2)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PieceCapturedAudio");
                }

                if (!board.currentRules.anarchy)
                {
                    if (type == "pawn" && (white ? y == 7 : y == 0))
                    {
                        board.chessManager.picker.Setup(this);
                    }
                    else
                    {
                        board.chessManager._EndTurn(); //TODO legal moves are not always up to date when picking up a piece at the start of a turn
                    }
                }

            }
            else
            {
                board._SetPiecesGrabbable(Networking.LocalPlayer, white);
            }

            board._ClearIndicators();
        }

        public override void OnDrop() //TODO when dropping piece it could animate into place
        {
            if (this.gameObject.activeSelf)
            {
                float movex = transform.localPosition.x * -1f - .5f;
                float movey = transform.localPosition.z * -1f - .5f;
                if (board.currentRules.anarchy) //TODO maybe have pieces turn red when they reach deletion distance, to make it clearer
                {
                    if (movex<-3||movex>11||movey<-3||movey>11){
                        _Capture();
                        return;
                    }
                }
                int newx = Mathf.RoundToInt(Mathf.Clamp(movex, 0, 7));
                int newy = Mathf.RoundToInt(Mathf.Clamp(movey, 0, 7));
                PieceDropped(newx, newy);
                
            }

        }

    }

}