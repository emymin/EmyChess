
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

namespace Emychess.GameRules
{
    /// <summary>
    /// Behaviour that specifies the chess rules to follow
    /// </summary>
    /// <remarks>
    /// Rule checking can be turned on and off with <see cref="SetAnarchy(bool)"/>
    /// </remarks>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class DefaultRules : UdonSharpBehaviour
    {
        //this is where Udon(Sharp)'s limitations make the code a bit hard to design in a sane way
        // legal moves are passed around as Vector2 arrays, which emulate list functionality by using a legalMovesIgnoreMarker to indicate elements to be skipped (for quick removal)
        // and legalMovesEndMarker to indicate the end of the list
        // this also means that functions receiving it need to always check what kind of move it was (regular, castling, en passant, double push) and find which object was supposed to be captured
        // a workaround could be using Object[] arrays to work as structs, to put all move information inside it, but that comes with its own annoyances
        //TODO decouple rules more from rest of the scripts

        /// <summary>
        /// Wheter anarchy mode is enabled, set with <see cref="SetAnarchy(bool)"/>
        /// </summary>
        [UdonSynced]
        public bool anarchy=false;

        /// <summary>
        /// Value that indicates the end of a move list
        /// </summary>
        [HideInInspector]
        public Vector2 legalMovesEndMarker;
        /// <summary>
        /// Value that can be skipped when iterating over a move list
        /// </summary>
        [HideInInspector]
        public Vector2 legalMovesIgnoreMarker;
        /// <summary>
        /// Directions that a generic sliding piece can move in. Also the queen's legal directions.
        /// </summary>
        private Vector2[] slideDirections;
        /// <summary>
        /// Directions that a rook can move in
        /// </summary>
        private Vector2[] rookDirections;
        /// <summary>
        /// Directions that a bishop can move in
        /// </summary>
        private Vector2[] bishopDirections;
        private int[] rookColumns;
        public void Start()
        {
            legalMovesEndMarker = new Vector2(-1, -1);
            legalMovesIgnoreMarker = new Vector2(-2, -2);
            rookColumns = new[] { 0, 7 };
            slideDirections = new [] { new Vector2(-1,0),new Vector2(1,0),new Vector2(0,-1),new Vector2(0,1),new Vector2(-1,1),new Vector2(-1,-1),new Vector2(1,1),new Vector2(1,-1)};
            rookDirections = new Vector2[4];
            for(int i = 0; i < 4; i++) { rookDirections[i] = slideDirections[i]; }
            bishopDirections = new Vector2[4];
            for(int i = 0; i < 4; i++) { bishopDirections[i] = slideDirections[7 - i]; }
        }

        /// <summary>
        /// Used when generating move lists, "appends" a move at the specified index, returns the incremented index, also sets the legalMovesEndMarker if the last element in the array is reached
        /// </summary>
        /// <param name="index"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="legalMoves"></param>
        /// <returns>If the append was succesful, index incremented by 1</returns>
        private int AppendMove(int index, int x,int y,Vector2[] legalMoves)
        {
            int newindex = index;
            if (x < 0 || x > 7 || y < 0 || y > 7) { return newindex; }
            if (index < legalMoves.Length)
            {     
                legalMoves[index] = new Vector2(x, y);
                if(index < legalMoves.Length - 1)
                {
                    legalMoves[index + 1] = legalMovesEndMarker;//used to mark the end of the """"list""""
                    newindex++;
                }
            }
            return newindex;

        }

        /// <summary>
        /// Gets all the pseudolegal moves (legal moves that don't take in account king check) that a piece can make, for fully legal moves, check <see cref="GetAllLegalMoves(Piece, Board)"/>
        /// </summary>
        /// <param name="movedPiece">Piece of which to find the legal moves</param>
        /// <param name="grid">Board's state in grid form, see <see cref="Board.GetBoardGrid"/></param>
        /// <param name="PawnThatDidADoublePushLastRound">A reference to a pawn that did a double push in the previous turn (for en passant), null if none</param>
        /// <param name="board">A reference to the board behaviour (UdonSharp doesn't support static methods)</param>
        /// <returns>List of pseudolegalmoves</returns>
        public Vector2[] GetAllPseudoLegalMovesGrid(Piece movedPiece,Piece[] grid,Piece PawnThatDidADoublePushLastRound,Board board)
        {
            Vector2[] legalMoves = new Vector2[64]; // SADLY still no lists, so gotta do this the cursed way
            int index = 0;
            legalMoves[index] = legalMovesEndMarker;
            string type = movedPiece.type;
            int x = movedPiece.x;
            int y = movedPiece.y;
            bool white = movedPiece.white;
            bool hasMoved = movedPiece.hasMoved;
            //AppendMove will check if the move is out of the board, so no need to check that here
            if (type == "pawn")
            {
                int dir = white ? 1 : -1;
                if (board.GetGridPiece(x,y+dir,grid)==null){
                    index = AppendMove(index, x, y + dir, legalMoves);
                    if (!hasMoved)
                    {
                        if (board.GetPiece(x, y + dir* 2) == null) {
                            index = AppendMove(index, x, y + dir * 2, legalMoves); 
                        }
                    }
                }
                Piece pieceCaptureLeft = board.GetGridPiece(x - 1, y + dir,grid);
                Piece pieceCaptureRight = board.GetGridPiece(x + 1, y + dir,grid);
                
                if (pieceCaptureLeft != null && pieceCaptureLeft.white!=white)
                {
                    index = AppendMove(index, x - 1, y + dir, legalMoves);
                }
                if (pieceCaptureRight!= null && pieceCaptureRight.white!=white)
                {
                    index = AppendMove(index, x + 1, y + dir, legalMoves);
                }
                //EN PASSANT
                Piece pieceLeft = board.GetGridPiece(x - 1, y,grid);
                Piece pieceRight = board.GetGridPiece(x + 1, y,grid);
                if (pieceLeft!=null && pieceLeft == PawnThatDidADoublePushLastRound && pieceLeft.white!=white)
                {
                    if (board.GetGridPiece(x - 1, y + dir,grid) == null)
                    {
                        index = AppendMove(index, x - 1, y + dir, legalMoves);
                    }
                }
                if (pieceRight!=null && pieceRight == PawnThatDidADoublePushLastRound && pieceRight.white!=white)
                {
                    if (board.GetGridPiece(x + 1, y + dir,grid) == null)
                    {
                        index = AppendMove(index, x + 1, y + dir, legalMoves);
                    }
                }


            }
            else if (type == "king")
            {
                for(int i = -1; i < 2; i++)
                {
                    for(int j = -1; j < 2; j++)
                    {
                        Piece squarepiece = board.GetGridPiece(x + i, y + j,grid);
                        if(squarepiece==null||(squarepiece!=null && squarepiece.white != white))
                        {
                            index = AppendMove(index, x + i, y + j, legalMoves);
                        }
                    }
                }
                if (!hasMoved)
                {
                    if (true )//|| !isKingInCheck(movedPiece.GetVec(), board.grid, board, board.PawnThatDidADoublePushLastRound, white)) //TODO very WEIRD stuff happens if I check if the king is in check here (which is necessary because castling is not allowed when the king is in check)
                    {
                        foreach(int rookColumn in rookColumns)
                        {
                            Piece startRook = board.GetGridPiece(rookColumn, y,grid);
                            if (startRook!=null && startRook.type == "rook" && !startRook.hasMoved && startRook.white == white)
                            {
                                bool free = true;
                                for(int i = Mathf.Min(x, rookColumn) + 1; i < Mathf.Max(x, rookColumn); i++)
                                {
                                    if (board.GetGridPiece(i, y,grid) != null) free = false;
                                }
                                if (free)
                                {
                                    int dir = Mathf.Min(x, rookColumn) == x ? 1 : -1;
                                    index = AppendMove(index, x + dir * 2, y, legalMoves);
                                }
                            }
                        }
                    }
                    
                }
            }
            else if (type == "rook" || type == "bishop" || type == "queen")
            {
                Vector2[] allowedDirections=slideDirections;
                if (type == "rook") allowedDirections = rookDirections;
                else if (type == "bishop") allowedDirections = bishopDirections;
                foreach(Vector2 direction in allowedDirections)
                {
                    Vector2 pos = new Vector2(x, y);
                    while (true)
                    {
                        pos += direction;
                        if (!board.isValidCoordinate((int)pos.x, (int)pos.y)) break;
                        Piece obstaclePiece = board.GetGridPiece((int)pos.x,(int)pos.y,grid);
                        if (obstaclePiece != null && obstaclePiece.white == white) break;
                        index = AppendMove(index, (int)pos.x, (int)pos.y, legalMoves);
                        if (obstaclePiece != null) break;
                    }
                }

            }
            else if (type == "knight")
            {
                Vector2 left, right;
                foreach (Vector2 direction in rookDirections) //directions are the same as the rook
                {
                    Vector2 pos = new Vector2(x, y);
                    pos += direction * 2;
                    if (direction.x == 0) { left = Vector2.left;right = Vector2.right; }
                    else { left = Vector2.up;right = Vector2.down; }
                    Vector2 leftPos = pos + left;
                    Vector2 rightPos = pos + right;
                    Piece leftPiece = board.GetGridPiece((int)leftPos.x,(int)leftPos.y,grid);
                    Piece rightPiece = board.GetGridPiece((int)rightPos.x, (int)rightPos.y, grid);
                    if (leftPiece == null || (leftPiece != null && leftPiece.white != white)){ index = AppendMove(index, (int)leftPos.x, (int)leftPos.y, legalMoves); }
                    if (rightPiece == null || (rightPiece != null && rightPiece.white != white)){ index = AppendMove(index, (int)rightPos.x, (int)rightPos.y, legalMoves); }

                }
            }
            return legalMoves;
        }

        /// <summary>
        /// Gets all pseudolegal moves for a piece on a board object, see <see cref="GetAllPseudoLegalMovesGrid(Piece, Piece[], Piece, Board)"/>
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="board"></param>
        /// <returns></returns>
        public Vector2[] GetAllPseudoLegalMoves(Piece piece,Board board)
        {
            return GetAllPseudoLegalMovesGrid(piece, board.grid, board.PawnThatDidADoublePushLastRound,board);
        }

        /// <summary>
        /// Optimization to detect if the king is in check, some basic sanity checks to see if a piece of the specified type could even reach the threatened position to begin with
        /// </summary>
        /// <param name="opponentPosition"></param>
        /// <param name="threatenedPosition"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool isCaptureFeasible(Vector2 opponentPosition,Vector2 threatenedPosition,string type)
        {
            if (type == "rook") return (opponentPosition.x == threatenedPosition.x | opponentPosition.y == threatenedPosition.y);
            else if (type == "pawn") return (Mathf.Abs(opponentPosition.x - threatenedPosition.x) == 1 && Mathf.Abs(opponentPosition.y - threatenedPosition.y) == 1);
            else if (type == "knight") return (Mathf.Abs(opponentPosition.x - threatenedPosition.x) < 3 && Mathf.Abs(opponentPosition.y - threatenedPosition.y) < 3);
            else if (type == "bishop") return (Mathf.Abs(opponentPosition.x - threatenedPosition.x) == Mathf.Abs(opponentPosition.y - threatenedPosition.y));
            else if (type == "queen") return (opponentPosition.x == threatenedPosition.x | opponentPosition.y == threatenedPosition.y) | (Mathf.Abs(opponentPosition.x - threatenedPosition.x) == Mathf.Abs(opponentPosition.y - threatenedPosition.y));
            else if (type == "king") return (Mathf.Abs((opponentPosition-threatenedPosition).magnitude)<3);
            else return true;
        }

        /// <summary>
        /// Check if the king is in check
        /// </summary>
        /// <param name="threatenedPos">Position of the king</param>
        /// <param name="grid">Grid to check on (<see cref="Board.GetBoardGrid"/>)</param>
        /// <param name="board">UdonSharp doesn't support static methods, pass a reference to a board to use its methods</param>
        /// <param name="PawnThatDidADoublePush">Pawn that did a double push in the previous move, to account of en passant</param>
        /// <param name="white">The side that the king is in</param>
        /// <returns></returns>
        public bool isKingInCheck(Vector2 threatenedPos,Piece[] grid,Board board,Piece PawnThatDidADoublePush,bool white)
        {
            bool isKingChecked = false;
            if (grid == null) { Debug.LogWarning("Empty grid, might be first turn");return false; }
            foreach (Piece opponentPiece in board.GetAllPieces())
            {
                if (opponentPiece.white != white)
                {
                    if (isCaptureFeasible(opponentPiece.GetVec(), threatenedPos, opponentPiece.type))
                    {
                        if (board.GetGridPiece(opponentPiece.x, opponentPiece.y, grid) == opponentPiece) //not captured in the test move
                        {
                            Vector2[] opponentPseudoLegalMoves = GetAllPseudoLegalMovesGrid(opponentPiece, grid, PawnThatDidADoublePush, board);
                            foreach (Vector2 opponentPseudoLegalMove in opponentPseudoLegalMoves)
                            {
                                if (opponentPseudoLegalMove == legalMovesEndMarker) { break; }
                                else
                                {
                                    if (opponentPseudoLegalMove == threatenedPos)
                                    {
                                        isKingChecked = true;
                                        //Debug.Log("Oh no, the king which is at x:" + threatenedPos.x + " y:" + threatenedPos.y + " is in check bc the " + opponentPiece.type + " at " + opponentPiece.x + " " + opponentPiece.y + " could capture it at " + opponentPseudoLegalMove.x + " " + opponentPseudoLegalMove.y);
                                        break;

                                    }
                                }
                            }
                        }
                    }
                }
                if (isKingChecked) { break; }
            }
            return isKingChecked;
        }

        /// <summary>
        /// Get all legal moves for a piece
        /// </summary>
        /// <param name="movedPiece"></param>
        /// <param name="board"></param>
        /// <returns></returns>
        public Vector2[] GetAllLegalMoves(Piece movedPiece, Board board) //TODO needs some ACTUAL testing (Perft function)
        {
            Vector2[] pseudoLegalMoves = GetAllPseudoLegalMoves(movedPiece, board);
            Vector2 piecePos = movedPiece.GetVec();
            Piece king = movedPiece.white ? board.whiteKing : board.blackKing;
            if (king != null)
            {
                Vector2 kingPos = king.GetVec();
                Piece[] currentGrid = board.grid;
                Piece[] testGrid=new Piece[currentGrid.Length];
                for(int i=0;i<pseudoLegalMoves.Length;i++)
                {
                
                    Vector2 pseudoLegalMove = pseudoLegalMoves[i];
                    
                    if (pseudoLegalMove == legalMovesEndMarker) { break; } else
                    {
                        
                        Array.Copy(currentGrid, testGrid, currentGrid.Length);
                        
                        board.MoveGridPieceVec(piecePos, pseudoLegalMove, testGrid);
                        Piece PawnThatDidADoublePush=null;
                        if (movedPiece.type == "pawn" && (Mathf.Abs(movedPiece.x - (int)pseudoLegalMove.x) > 1)){
                            PawnThatDidADoublePush = movedPiece;
                        }
                        Vector2 threatenedPos = movedPiece.type != "king" ? kingPos : pseudoLegalMove;
                        if (isKingInCheck(threatenedPos, testGrid, board, PawnThatDidADoublePush, movedPiece.white)) { pseudoLegalMoves[i] = legalMovesIgnoreMarker; }
                    
                    }
                }

            }
            return pseudoLegalMoves;
            
        }

        /// <summary>
        /// Gets the result of moving a piece to the specified position, as well as performing capture, en passant or castling
        /// </summary>
        /// <remarks>
        /// Will recalculate legal moves, if you already have a list of legal moves 
        /// (for example during <see cref="Piece.PieceDropped(int, int)"/>) use <see cref="MoveLegalCheck(Piece, int, int, Board, Vector2[])"/> to pass them
        /// </remarks>
        /// <param name="movedPiece"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="board"></param>
        /// <returns>0 if the move is not allowed, 1 if the move was successful, 2 if the move resulted in a capture</returns>
        public int Move(Piece movedPiece,int x,int y,Board board)
        {
            int result = 0;
            if (anarchy)
            {
                Piece targetPiece = board.GetPiece(x, y);
                result = 1;
                if (targetPiece != null && targetPiece!=movedPiece) { targetPiece._Capture(); result = 2; }
                movedPiece._SetPosition(x, y);
                return result;
            }
            else
            {
                Vector2[] legalMoves = GetAllLegalMoves(movedPiece, board);
                return MoveLegalCheck(movedPiece, x, y, board, legalMoves);
            }
            
        }

        /// <summary>
        /// Gets the result of moving a piece to a specified position, as well as performing capture, en passant or castling, based on the legal moves being passed
        /// </summary>
        /// <param name="movedPiece"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="board"></param>
        /// <param name="legalMoves"></param>
        /// <returns>0 if the move is not allowed, 1 if the move was successful, 2 if the move resulted in a capture</returns>
        public int MoveLegalCheck(Piece movedPiece,int x,int y, Board board,Vector2[] legalMoves)
        {
            int result = 0;
            Piece targetPiece = board.GetPiece(x,y);
            Vector2 move = new Vector2(x, y);
            bool legal = false;
            foreach(Vector2 legalMove in legalMoves) {
                if (legalMove != legalMovesIgnoreMarker)
                {
                    if (legalMove == legalMovesEndMarker) break;
                    if (move == legalMove) { legal = true; break; }
                }
            }
            if (legal)
            {
                if (targetPiece != null) { targetPiece._Capture(); result = 2; } else { result = 1; }

                if (movedPiece.type == "pawn" && movedPiece.x != x && board.GetPiece(x, y) == null)//EN PASSANT
                {
                    board.PawnThatDidADoublePushLastRound._Capture();
                    result = 2;
                }

                if (movedPiece.type == "king" && Mathf.Abs(x-movedPiece.x)==2) //CASTLING
                {
                    int dir = Mathf.Min(x, movedPiece.x) == x ? -1 : 1;
                    Piece rookCastle = dir == 1 ? board.GetPiece(7, y) : board.GetPiece(0, y);
                    rookCastle._SetPosition(x + (dir * -1), y);

                }
                if (movedPiece.type == "pawn" && Mathf.Abs(y - movedPiece.y) == 2)
                {
                    board.PawnThatDidADoublePushLastRound = movedPiece; //NOTICE, to have it be synced I also do the same check in the Piece refresh function (won't be synced for those who join right after this move, but it's fine)
                }
                else
                {
                    board.PawnThatDidADoublePushLastRound = null;
                }
                movedPiece.hasMoved = true;
                movedPiece._SetPosition(x, y);
                
                return result;
            }
            else
            {
                movedPiece._SetPosition(movedPiece.x, movedPiece.y);
                return 0;
            }
            
        }
        /// <summary>
        /// Resets the board with all pieces in the correct starting positions
        /// </summary>
        /// <param name="board"></param>
        public void ResetBoard(Board board)
        {
            board.ReadFENString("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
        }
        /// <summary>
        /// Set anarchy mode (no rule checking)
        /// </summary>
        /// <param name="enabled"></param>
        public void SetAnarchy(bool enabled)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            anarchy = enabled;
            RequestSerialization();
        }

    }

}
