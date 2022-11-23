using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Microsoft.Extensions.Logging;

namespace Emerita
{
    public sealed class Board : ICloneable
    {
        #region Current Board State

        private readonly int[] board = new int[Constants.MAX_SQUARES];
        private readonly BitBoardArray2D bbaPieces = new(Constants.MAX_COLORS, Constants.MAX_PIECES);
        private readonly ulong[] bbaUnits = new ulong[Constants.MAX_COLORS];
        private ulong bbAll;

        private int ply;
        private int halfMoveClock;
        private int fullMoveCounter;
        private CastlingFlags castling;
        private int? enPassant;
        private int? enPassantValidated;
        private int sideToMove;
        private int opponent;
        private ulong hash;
        private ulong pawnHash;

        private readonly IntegerArray2D history = new(Constants.MAX_SQUARES, Constants.MAX_SQUARES);

        #endregion

        #region Static Masks Used for Calculations

        private static readonly BitBoardArray2D bbaPawnDefends = new(Constants.MAX_COLORS, Constants.MAX_SQUARES);
        private static readonly BitBoardArray2D bbaPawnCaptures = new(Constants.MAX_COLORS, Constants.MAX_SQUARES);
        private static readonly BitBoardArray2D bbaLeft = new(Constants.MAX_COLORS, Constants.MAX_SQUARES);
        private static readonly BitBoardArray2D bbaRight = new(Constants.MAX_COLORS, Constants.MAX_SQUARES);
        private static readonly BitBoardArray2D bbaPawnMoves = new(Constants.MAX_COLORS, Constants.MAX_SQUARES);
        private static readonly ulong[] bbaKnightMoves = new ulong[Constants.MAX_SQUARES];
        private static readonly ulong[] bbaBishopMoves = new ulong[Constants.MAX_SQUARES];
        private static readonly ulong[] bbaRookMoves = new ulong[Constants.MAX_SQUARES];
        private static readonly ulong[] bbaQueenMoves = new ulong[Constants.MAX_SQUARES];
        private static readonly ulong[] bbaKingMoves = new ulong[Constants.MAX_SQUARES];
        private static readonly BitBoardArray2D bbaBetween = new(Constants.MAX_SQUARES, Constants.MAX_SQUARES);
        private static readonly BitBoardArray2D bbaVectorBits = new(Constants.MAX_SQUARES, Constants.MAX_SQUARES);
        private static readonly BitBoardArray2D bbaMaskVectors = new(Constants.MAX_SQUARES, Constants.MAX_DIRECTIONS);
        private static readonly BitBoardArray2D bbaMaskPassed = new(Constants.MAX_COLORS, Constants.MAX_SQUARES);
        private static readonly BitBoardArray2D bbaMaskPath = new(Constants.MAX_COLORS, Constants.MAX_SQUARES);
        private static readonly ulong[] bbaMask = new ulong[Constants.MAX_SQUARES];
        private static readonly ulong[] bbaNotMask = new ulong[Constants.MAX_SQUARES];
        private static readonly ulong[] bbaMaskFiles = new ulong[Constants.MAX_SQUARES];
        private static readonly ulong[] bbaMaskRanks = new ulong[Constants.MAX_SQUARES];
        private static readonly ulong[] bbaMaskIsolated = new ulong[Constants.MAX_SQUARES];
        private static readonly ulong bbMaskKingSide;
        private static readonly ulong bbMaskQueenSide;
        private static readonly ulong bbNotAFile;
        private static readonly ulong bbNotHFile;
        private static readonly IntegerArray2D pawnLeft = new(Constants.MAX_COLORS, Constants.MAX_SQUARES);
        private static readonly IntegerArray2D pawnRight = new(Constants.MAX_COLORS, Constants.MAX_SQUARES);
        private static readonly IntegerArray2D pawnPlus = new(Constants.MAX_COLORS, Constants.MAX_SQUARES);
        private static readonly IntegerArray2D pawnDouble = new(Constants.MAX_COLORS, Constants.MAX_SQUARES);
        private static readonly IntegerArray2D qrbMoves = new(Constants.MAX_SQUARES, Constants.MAX_DIRECTIONS);
        private static readonly IntegerArray2D knightMoves = new(Constants.MAX_SQUARES, Constants.MAX_DIRECTIONS);
        private static readonly IntegerArray2D kingMoves = new(Constants.MAX_SQUARES, Constants.MAX_DIRECTIONS);
        private static readonly int[] epOffset = { -8, 8 };
        private static readonly IntegerArray2D captureScores = new(Constants.MAX_PIECES, Constants.MAX_PIECES);

        private static readonly int[] nwDiag =
        {
            14, 13, 12, 11, 10, 9, 8, 7,
            13, 12, 11, 10,  9, 8, 7, 6,
            12, 11, 10,  9,  8, 7, 6, 5,
            11, 10,  9,  8,  7, 6, 5, 4,
            10,  9,  8,  7,  6, 5, 4, 3,
             9,  8,  7,  6,  5, 4, 3, 2,
             8,  7,  6,  5,  4, 3, 2, 1,
             7,  6,  5,  4,  3, 2, 1, 0
        };

        private static readonly int[] neDiag =
        {
            7, 8, 9, 10, 11, 12, 13, 14,
            6, 7, 8,  9, 10, 11, 12, 13,
            5, 6, 7,  8,  9, 10, 11, 12,
            4, 5, 6,  7,  8,  9, 10, 11,
            3, 4, 5,  6,  7,  8,  9, 10,
            2, 3, 4,  5,  6,  7,  8,  9,
            1, 2, 3,  4,  5,  6,  7,  8,
            0, 1, 2,  3,  4,  5,  6,  7
        };

        private static readonly int[] castleMask =
        {
            13, 15, 15, 15, 12, 15, 15, 14,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
            15, 15, 15, 15, 15, 15, 15, 15,
             7, 15, 15, 15,  3, 15, 15, 11
        };

        public struct CastlingRookMove
        {
            public int KingTo;
            public int KingMoveThrough;
            public int RookFrom;
            public int RookTo;

            public CastlingRookMove(int kingTo, int kingMoveThrough, int rookFrom, int rookTo)
            {
                KingTo = kingTo;
                KingMoveThrough = kingMoveThrough;
                RookFrom = rookFrom;
                RookTo = rookTo;
            }
        }

        private static readonly CastlingRookMove[] castlingRookMoves =
        {
            new(Constants.C1, Constants.D1, Constants.A1, Constants.D1),
            new(Constants.G1, Constants.F1, Constants.H1, Constants.F1),
            new(Constants.C8, Constants.D8, Constants.A8, Constants.D8),
            new(Constants.G8, Constants.F8, Constants.H8, Constants.F8)
        };

        #endregion

        #region struct BoardState

        public struct BoardState
        {
            public Move Move;
            public int Ply;
            public int HalfMoveClock;
            public int FullMoveCounter;
            public CastlingFlags Castling;
            public int? EnPassant;
            public int? EnPassantValidated;
            public int SideToMove;
            public ulong Hash;
            public ulong PawnHash;

            public BoardState(Board board, Move move)
            {
                Move = move;
                Ply = board.ply;
                HalfMoveClock = board.halfMoveClock;
                FullMoveCounter = board.fullMoveCounter;
                Castling = board.castling;
                EnPassant = board.enPassant;
                EnPassantValidated = board.enPassantValidated;
                SideToMove = board.sideToMove;
                Hash = board.hash;
                PawnHash = board.pawnHash;
            }

            public void Restore(Board board)
            {
                board.ply = Ply;
                board.halfMoveClock = HalfMoveClock;
                board.fullMoveCounter = FullMoveCounter;
                board.castling = Castling;
                board.enPassant = EnPassant;
                board.enPassantValidated = EnPassantValidated;
                board.SideToMove = SideToMove;
                board.hash = Hash;
                board.pawnHash = PawnHash;
            }
        }

        private readonly SimpleStack<BoardState> boardStateStack = new(Constants.MAX_GAME_PLY);
        private readonly SimpleStack<ulong> repeatStack = new(Constants.MAX_GAME_PLY);

        #endregion

        #region Constructors

        static Board()
        {
            try
            {
                for (int x = 0; x < Constants.MAX_SQUARES; ++x)
                {
                    ChessMath.IndexToCoords(x, out int xFile, out int xRank);

                    #region Initialize bbMaskKingSide, bbMaskQueenSide

                    if (xFile < 2)
                    {
                        bbMaskQueenSide = ChessMath.SetBit(bbMaskQueenSide, x);
                    }
                    else if (xFile > 5)
                    {
                        bbMaskKingSide = ChessMath.SetBit(bbMaskKingSide, x);
                    }

                    #endregion

                    #region Initialize bbaMaskPassed, bbaMaskIsolated, bbaMaskPath, bbaMaskFiles, bbaBetween, bbaVectorBits

                    for (int y = 0; y < Constants.MAX_SQUARES; ++y)
                    {
                        ChessMath.IndexToCoords(y, out int yFile, out int yRank);
                        int diff = Math.Abs(xFile - yFile);
                        if (diff < 2)
                        {
                            if (xRank < yRank && yRank < 7)
                            {
                                bbaMaskPassed[Constants.COLOR_WHITE, x] =
                                    ChessMath.SetBit(bbaMaskPassed[Constants.COLOR_WHITE, x], y);
                            }
                            else if (xRank > yRank && yRank > 0)
                            {
                                bbaMaskPassed[Constants.COLOR_BLACK, x] =
                                    ChessMath.SetBit(bbaMaskPassed[Constants.COLOR_BLACK, x], y);
                            }

                            if (diff == 1)
                            {
                                bbaMaskIsolated[x] = ChessMath.SetBit(bbaMaskIsolated[x], y);
                            }
                            else if (diff == 0)
                            {
                                if (xRank < yRank)
                                {
                                    bbaMaskPath[Constants.COLOR_WHITE, x] =
                                        ChessMath.SetBit(bbaMaskPath[Constants.COLOR_WHITE, x], y);
                                }
                                else if (xRank > yRank)
                                {
                                    bbaMaskPath[Constants.COLOR_BLACK, x] =
                                        ChessMath.SetBit(bbaMaskPath[Constants.COLOR_BLACK, x], y);
                                }

                                bbaMaskFiles[x] = ChessMath.SetBit(bbaMaskFiles[x], y);
                            }
                        }

                        int zStart = Math.Min(x, y);
                        int zEnd = Math.Max(x, y);
                        if (xRank == yRank)
                        {
                            for (int z = zStart + 1; z < zEnd; ++z)
                            {
                                bbaBetween[x, y] = ChessMath.SetBit(bbaBetween[x, y], z);
                            }

                            if (y > x)
                            {
                                for (int z = zStart + 1; z <= zEnd; ++z)
                                {
                                    bbaVectorBits[x, y] = ChessMath.SetBit(bbaVectorBits[x, y], z);
                                }
                            }
                            else
                            {
                                for (int z = zStart; z < zEnd; ++z)
                                {
                                    bbaVectorBits[x, y] = ChessMath.SetBit(bbaVectorBits[x, y], z);
                                }
                            }

                            bbaMaskRanks[x] = ChessMath.SetBit(bbaMaskRanks[x], y);
                        }

                        if (xFile == yFile)
                        {
                            for (int z = zStart + 8; z < zEnd; z += 8)
                            {
                                bbaBetween[x, y] = ChessMath.SetBit(bbaBetween[x, y], z);
                            }

                            if (y > x)
                            {
                                for (int z = zStart + 8; z <= zEnd; z += 8)
                                {
                                    bbaVectorBits[x, y] = ChessMath.SetBit(bbaVectorBits[x, y], z);
                                }
                            }
                            else
                            {
                                for (int z = zStart; z < zEnd; z += 8)
                                {
                                    bbaVectorBits[x, y] = ChessMath.SetBit(bbaVectorBits[x, y], z);
                                }
                            }
                        }

                        if (nwDiag[x] == nwDiag[y])
                        {
                            for (int z = zStart + 7; z < zEnd; z += 7)
                            {
                                bbaBetween[x, y] = ChessMath.SetBit(bbaBetween[x, y], z);
                            }

                            if (y > x)
                            {
                                for (int z = zStart + 7; z <= zEnd; z += 7)
                                {
                                    bbaVectorBits[x, y] = ChessMath.SetBit(bbaVectorBits[x, y], z);
                                }
                            }
                            else
                            {
                                for (int z = zStart; z < zEnd; z += 7)
                                {
                                    bbaVectorBits[x, y] = ChessMath.SetBit(bbaVectorBits[x, y], z);
                                }
                            }
                        }

                        if (neDiag[x] == neDiag[y])
                        {
                            for (int z = zStart + 9; z < zEnd; z += 9)
                            {
                                bbaBetween[x, y] = ChessMath.SetBit(bbaBetween[x, y], z);
                            }

                            if (y > x)
                            {
                                for (int z = zStart + 9; z <= zEnd; z += 9)
                                {
                                    bbaVectorBits[x, y] = ChessMath.SetBit(bbaVectorBits[x, y], z);
                                }
                            }
                            else
                            {
                                for (int z = zStart; z < zEnd; z += 9)
                                {
                                    bbaVectorBits[x, y] = ChessMath.SetBit(bbaVectorBits[x, y], z);
                                }
                            }
                        }
                    }

                    #endregion

                    #region Initialize bbaMask, bbaNotMask

                    bbaMask[x] = ChessMath.SetBit(bbaMask[x], x);
                    bbaNotMask[x] = ~bbaMask[x];

                    #endregion

                    #region Initialize pawnLeft, pawnRight, bbaPawnCaptures, bbaLeft, bbaRight

                    pawnLeft[Constants.COLOR_WHITE, x] = -1;
                    pawnLeft[Constants.COLOR_BLACK, x] = -1;
                    pawnRight[Constants.COLOR_WHITE, x] = -1;
                    pawnRight[Constants.COLOR_BLACK, x] = -1;

                    int n;
                    if (xFile > 0)
                    {

                        if (xRank < 7)
                        {
                            n = x + 7;
                            pawnLeft[Constants.COLOR_WHITE, x] = n;
                            bbaPawnCaptures[Constants.COLOR_WHITE, x] =
                                ChessMath.SetBit(bbaPawnCaptures[Constants.COLOR_WHITE, x], n);
                            bbaLeft[Constants.COLOR_WHITE, x] = ChessMath.SetBit(bbaLeft[Constants.COLOR_WHITE, x], n);
                        }

                        if (xRank > 0)
                        {
                            n = x - 9;
                            pawnLeft[Constants.COLOR_BLACK, x] = n;
                            bbaPawnCaptures[Constants.COLOR_BLACK, x] =
                                ChessMath.SetBit(bbaPawnCaptures[Constants.COLOR_BLACK, x], n);
                            bbaLeft[Constants.COLOR_BLACK, x] = ChessMath.SetBit(bbaLeft[Constants.COLOR_BLACK, x], n);
                        }
                    }

                    if (xFile < 7)
                    {
                        if (xRank < 7)
                        {
                            n = x + 9;
                            pawnRight[Constants.COLOR_WHITE, x] = n;
                            bbaPawnCaptures[Constants.COLOR_WHITE, x] =
                                ChessMath.SetBit(bbaPawnCaptures[Constants.COLOR_WHITE, x], n);
                            bbaRight[Constants.COLOR_WHITE, x] = ChessMath.SetBit(bbaRight[Constants.COLOR_WHITE, x], n);
                        }

                        if (xRank > 0)
                        {
                            n = x - 7;
                            pawnRight[Constants.COLOR_BLACK, x] = n;
                            bbaPawnCaptures[Constants.COLOR_BLACK, x] =
                                ChessMath.SetBit(bbaPawnCaptures[Constants.COLOR_BLACK, x], n);
                            bbaRight[Constants.COLOR_BLACK, x] = ChessMath.SetBit(bbaRight[Constants.COLOR_BLACK, x], n);
                        }
                    }

                    #endregion

                    #region Initialize bbaPawnDefends, pawnPlus, pawnDouble

                    bbaPawnDefends[Constants.COLOR_WHITE, x] = bbaPawnCaptures[Constants.COLOR_BLACK, x];
                    bbaPawnDefends[Constants.COLOR_BLACK, x] = bbaPawnCaptures[Constants.COLOR_WHITE, x];

                    if (xRank < 7)
                    {
                        pawnPlus[Constants.COLOR_WHITE, x] = x + 8;
                    }

                    if (xRank < 6)
                    {
                        pawnDouble[Constants.COLOR_WHITE, x] = x + 16;
                    }

                    if (xRank > 0)
                    {
                        pawnPlus[Constants.COLOR_BLACK, x] = x - 8;
                    }

                    if (xRank > 1)
                    {
                        pawnDouble[Constants.COLOR_BLACK, x] = x - 16;
                    }

                    #endregion
                }

                #region Initialize bbNotAFile, bbNotHFile

                bbNotAFile = ~bbaMaskFiles[0];
                bbNotHFile = ~bbaMaskFiles[7];

                #endregion

                knightMoves.Fill(-1);
                kingMoves.Fill(-1);
                qrbMoves.Fill(-1);

                for (int x = 0; x < Constants.MAX_SQUARES; ++x)
                {
                    ChessMath.IndexToCoords(x, out int xFile, out int xRank);

                    #region Initialize bbaMaskVectors

                    bbaMaskVectors[x, Constants.DIR_N] = bbaVectorBits[x, 56 + xFile];
                    bbaMaskVectors[x, Constants.DIR_S] = bbaVectorBits[x, xFile];
                    bbaMaskVectors[x, Constants.DIR_W] = bbaVectorBits[x, xRank << 3];
                    bbaMaskVectors[x, Constants.DIR_E] = bbaVectorBits[x, (xRank << 3) + 7];

                    if (xFile > 0 && xRank < 7)
                    {
                        bbaMaskVectors[x, Constants.DIR_NW] = bbaVectorBits[x, GetEdge(x, 7)];
                    }

                    if (xFile < 7 && xRank < 7)
                    {
                        bbaMaskVectors[x, Constants.DIR_NE] = bbaVectorBits[x, GetEdge(x, 9)];
                    }

                    if (xFile > 0 && xRank > 0)
                    {
                        bbaMaskVectors[x, Constants.DIR_SW] = bbaVectorBits[x, GetEdge(x, -9)];
                    }

                    if (xFile < 7 && xRank > 0)
                    {
                        bbaMaskVectors[x, Constants.DIR_SE] = bbaVectorBits[x, GetEdge(x, -7)];
                    }

                    #endregion

                    #region Initialize knightMoves, kingMoves, qrbMoves

                    int k = 0;
                    if (xRank < 6 && xFile < 7)
                    {
                        knightMoves[x, k++] = x + 17;
                    }

                    if (xRank < 7 && xFile < 6)
                    {
                        knightMoves[x, k++] = x + 10;
                    }

                    if (xRank < 6 && xFile > 0)
                    {
                        knightMoves[x, k++] = x + 15;
                    }

                    if (xRank < 7 && xFile > 1)
                    {
                        knightMoves[x, k++] = x + 6;
                    }

                    if (xRank > 1 && xFile < 7)
                    {
                        knightMoves[x, k++] = x - 15;
                    }

                    if (xRank > 0 && xFile < 6)
                    {
                        knightMoves[x, k++] = x - 6;
                    }

                    if (xRank > 1 && xFile > 0)
                    {
                        knightMoves[x, k++] = x - 17;
                    }

                    if (xRank > 0 && xFile > 1)
                    {
                        knightMoves[x, k] = x - 10;
                    }

                    k = 0;

                    if (xFile > 0)
                    {
                        qrbMoves[x, Constants.DIR_W] = x - 1;
                        kingMoves[x, k++] = x - 1;
                    }

                    if (xFile < 7)
                    {
                        qrbMoves[x, Constants.DIR_E] = x + 1;
                        kingMoves[x, k++] = x + 1;
                    }

                    if (xRank > 0)
                    {
                        qrbMoves[x, Constants.DIR_S] = x - 8;
                        kingMoves[x, k++] = x - 8;
                    }

                    if (xRank < 7)
                    {
                        qrbMoves[x, Constants.DIR_N] = x + 8;
                        kingMoves[x, k++] = x + 8;
                    }

                    if (xFile < 7 && xRank < 7)
                    {
                        qrbMoves[x, Constants.DIR_NE] = x + 9;
                        kingMoves[x, k++] = x + 9;
                    }

                    if (xFile > 0 && xRank < 7)
                    {
                        qrbMoves[x, Constants.DIR_NW] = x + 7;
                        kingMoves[x, k++] = x + 7;
                    }

                    if (xFile > 0 && xRank > 0)
                    {
                        qrbMoves[x, Constants.DIR_SW] = x - 9;
                        kingMoves[x, k++] = x - 9;
                    }

                    if (xFile < 7 && xRank > 0)
                    {
                        qrbMoves[x, Constants.DIR_SE] = x - 7;
                        kingMoves[x, k] = x - 7;
                    }

                    #endregion
                }

                #region Initialize bbaKnightMoves, bbaKingMoves, bbaQrbMoves

                for (int x = 0; x < Constants.MAX_SQUARES; ++x)
                {
                    for (int k = 0; k < Constants.MAX_DIRECTIONS && knightMoves[x, k] != -1; ++k)
                    {
                        bbaKnightMoves[x] |= bbaMask[knightMoves[x, k]];
                    }

                    for (int k = 0; k < Constants.MAX_DIRECTIONS && kingMoves[x, k] != -1; ++k)
                    {
                        bbaKingMoves[x] |= bbaMask[kingMoves[x, k]];
                    }

                    ChessMath.IndexToCoords(x, out int xFile, out int xRank);
                    for (int y = 0; y < Constants.MAX_SQUARES; ++y)
                    {
                        ChessMath.IndexToCoords(y, out int yFile, out int yRank);

                        if (x == y)
                        {
                            continue;
                        }

                        if (nwDiag[x] == nwDiag[y] || neDiag[x] == neDiag[y])
                        {
                            bbaBishopMoves[x] |= bbaMask[y];
                            bbaQueenMoves[x] |= bbaMask[y];
                        }

                        if (xRank == yRank || xFile == yFile)
                        {
                            bbaRookMoves[x] |= bbaMask[y];
                            bbaQueenMoves[x] |= bbaMask[y];
                        }
                    }

                }

                #endregion

                #region Initialize captureScores

                ReadOnlySpan<short> captureValues = stackalloc short[] { 100, 300, 300, 500, 900, 15000 };

                for (int p1 = Constants.PIECE_PAWN; p1 < Constants.MAX_PIECES; ++p1)
                {
                    for (int p2 = Constants.PIECE_PAWN; p2 < Constants.MAX_PIECES; ++p2)
                    {
                        captureScores[p1, p2] =
                            Constants.BASE_CAPTURE_SCORE + (captureValues[p1] * 10) / captureValues[p2];
                    }
                }

                #endregion

            }
            catch (Exception e)
            {
                Util.TraceError(e.Message);
                throw new Exception("Exception occurred in static constructor.", e);
            }
        }

        public Board()
        {
            Clear();
        }

        #endregion

        #region Properties To Access Board State

        public ReadOnlySpan<int> GameBoard => new(board);
        public ulong Pieces(int color, int piece)
        {
            return bbaPieces[color, piece];
        }
        public ulong Units(int color)
        {
            return bbaUnits[color];
        }
        public ulong All => bbAll;
        public int Ply => ply;
        public int HalfMoveClock => halfMoveClock;
        public int FullMoveCounter => fullMoveCounter;
        public CastlingFlags Castling => castling;
        public int? EnPassant => enPassant;
        public int? EnPassantValidated => enPassantValidated;

        public int SideToMove
        {
            get => sideToMove;
            set
            {
                sideToMove = value;
                opponent = sideToMove ^ 1;
            }
        }

        public ulong Hash => hash;
        public ulong PawnHash => pawnHash;

        public int History(int from, int to)
        {
            return history[from, to];
        }

        public int OpponentColor => opponent;

        #endregion

        public void Clear()
        {
            Array.Fill(board, Constants.PIECE_NONE);
            bbaPieces.Fill(Constants.BITBOARD_EMPTY);
            bbaUnits[Constants.COLOR_WHITE] = Constants.BITBOARD_EMPTY;
            bbaUnits[Constants.COLOR_BLACK] = Constants.BITBOARD_EMPTY;
            bbAll = Constants.BITBOARD_EMPTY;
            ply = 0;
            SideToMove = Constants.COLOR_WHITE;
            castling = CastlingFlags.None;
            enPassant = null;
            enPassantValidated = null;
            halfMoveClock = 0;
            fullMoveCounter = 0;
            hash = 0ul;
            pawnHash = 0ul;
            boardStateStack.Clear();
            history.Clear();
            repeatStack.Clear();
        }

        public Board Clone()
        {
            Board clone = new();
            Array.Copy(board, clone.board, clone.board.Length);
            clone.bbaPieces.Copy(bbaPieces);
            Array.Copy(bbaUnits, clone.bbaUnits, clone.bbaUnits.Length);
            clone.bbAll = bbAll;
            clone.ply = ply;
            clone.halfMoveClock = halfMoveClock;
            clone.fullMoveCounter = fullMoveCounter;
            clone.castling = castling;
            clone.enPassant = enPassant;
            clone.enPassantValidated = enPassantValidated;
            clone.sideToMove = sideToMove;
            clone.opponent = opponent;
            clone.hash = hash;
            clone.pawnHash = pawnHash;
            clone.history.Copy(history);
            clone.boardStateStack.Copy(boardStateStack);
            clone.repeatStack.Copy(repeatStack);
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #region Move Generation

        public void GenerateMoves(MoveList moveList)
        {
            moveList.StartPly(ply);

            GenerateEnPassant(moveList);
            GenerateCastling(moveList);

            ulong bb1, bb2, bb3, bb4;
            int from, to;

            #region Generate Pawn Moves

            if (ChessMath.IsColorWhite(sideToMove))
            {
                bb1 = bbaPieces[sideToMove, Constants.PIECE_PAWN] & (ChessMath.AndNot(bbaUnits[OpponentColor], bbaMaskFiles[7]) >> 7);
                bb2 = bbaPieces[sideToMove, Constants.PIECE_PAWN] & (ChessMath.AndNot(bbaUnits[OpponentColor], bbaMaskFiles[0]) >> 9);
                bb3 = ChessMath.AndNot(bbaPieces[sideToMove, Constants.PIECE_PAWN], bbAll >> 8);
                bb4 = ChessMath.AndNot(bb3 & bbaMaskRanks[Constants.A2], bbAll >> 16);
            }
            else
            {
                bb1 = bbaPieces[sideToMove, Constants.PIECE_PAWN] & (ChessMath.AndNot(bbaUnits[OpponentColor], bbaMaskFiles[7]) << 9);
                bb2 = bbaPieces[sideToMove, Constants.PIECE_PAWN] & (ChessMath.AndNot(bbaUnits[OpponentColor], bbaMaskFiles[0]) << 7);
                bb3 = ChessMath.AndNot(bbaPieces[sideToMove, Constants.PIECE_PAWN], bbAll << 8);
                bb4 = ChessMath.AndNot(bb3 & bbaMaskRanks[Constants.A7], bbAll << 16);
            }

            while (bb1 != 0)
            {
                from = ChessMath.LowestSetBitIndex(bb1);
                ChessMath.ResetLowestSetBit(ref bb1);
                to = pawnLeft[sideToMove, from];
                AddPawnMove(moveList, from, to, MoveFlags.Capture, capture: board[to],
                    score: captureScores[board[to], Constants.PIECE_PAWN]);
            }

            while (bb2 != 0)
            {
                from = ChessMath.LowestSetBitIndex(bb2);
                ChessMath.ResetLowestSetBit(ref bb2);
                to = pawnRight[sideToMove, from];
                AddPawnMove(moveList, from, to, MoveFlags.Capture, capture: board[to],
                    score: captureScores[board[to], Constants.PIECE_PAWN]);
            }

            while (bb3 != 0)
            {
                from = ChessMath.LowestSetBitIndex(bb3);
                ChessMath.ResetLowestSetBit(ref bb3);
                to = pawnPlus[sideToMove, from];
                AddPawnMove(moveList, from, to, score: history[from, to]);
            }

            while (bb4 != 0)
            {
                from = ChessMath.LowestSetBitIndex(bb4);
                ChessMath.ResetLowestSetBit(ref bb4);
                to = pawnDouble[sideToMove, from];
                moveList.Add(Constants.PIECE_PAWN, from, to, MoveFlags.DblPawnMove, score: history[from, to]);
            }

            #endregion

            #region Generate Knight Moves

            bb1 = bbaPieces[sideToMove, Constants.PIECE_KNIGHT];
            while (bb1 != 0)
            {
                from = ChessMath.LowestSetBitIndex(bb1);
                ChessMath.ResetLowestSetBit(ref bb1);
                bb2 = bbaKnightMoves[from] & bbaUnits[OpponentColor];
                while (bb2 != 0)
                {
                    to = ChessMath.LowestSetBitIndex(bb2);
                    ChessMath.ResetLowestSetBit(ref bb2);
                    moveList.Add(Constants.PIECE_KNIGHT, from, to, MoveFlags.Capture, capture: board[to],
                        score: captureScores[board[to], Constants.PIECE_KNIGHT]);
                }

                bb2 = ChessMath.AndNot(bbaKnightMoves[from], bbAll);
                while (bb2 != 0)
                {
                    to = ChessMath.LowestSetBitIndex(bb2);
                    ChessMath.ResetLowestSetBit(ref bb2);
                    moveList.Add(Constants.PIECE_KNIGHT, from, to, score: history[from, to]);
                }
            }

            #endregion

            #region Generate Bishop Moves

            bb1 = bbaPieces[sideToMove, Constants.PIECE_BISHOP];
            while (bb1 != 0)
            {
                from = ChessMath.LowestSetBitIndex(bb1);
                ChessMath.ResetLowestSetBit(ref bb1);
                for (int dir = Constants.DIR_NE; dir < Constants.MAX_DIRECTIONS; dir += 2)
                {
                    for (to = qrbMoves[from, dir]; to > -1; to = qrbMoves[to, dir])
                    {
                        if ((bbaMask[to] & bbAll) != 0)
                        {
                            if ((bbaMask[to] & bbaUnits[OpponentColor]) != 0)
                            {
                                moveList.Add(Constants.PIECE_BISHOP, from, to, MoveFlags.Capture,
                                    capture: board[to], score: captureScores[board[to], Constants.PIECE_BISHOP]);
                            }
                            break;
                        }
                        moveList.Add(Constants.PIECE_BISHOP, from, to, score: history[from, to]);
                    }
                }
            }

            #endregion

            #region Generate Rook Moves

            bb1 = bbaPieces[sideToMove, Constants.PIECE_ROOK];
            while (bb1 != 0)
            {
                from = ChessMath.LowestSetBitIndex(bb1);
                ChessMath.ResetLowestSetBit(ref bb1);
                for (int dir = Constants.DIR_N; dir < Constants.MAX_DIRECTIONS; dir += 2)
                {
                    for (to = qrbMoves[from, dir]; to > -1; to = qrbMoves[to, dir])
                    {
                        if ((bbaMask[to] & bbAll) != 0)
                        {
                            if ((bbaMask[to] & bbaUnits[OpponentColor]) != 0)
                            {
                                moveList.Add(Constants.PIECE_ROOK, from, to, MoveFlags.Capture,
                                    capture: board[to], score: captureScores[board[to], Constants.PIECE_ROOK]);
                            }

                            break;
                        }
                        moveList.Add(Constants.PIECE_ROOK, from, to, score: history[from, to]);
                    }
                }
            }

            #endregion

            #region Generate Queen Moves

            bb1 = bbaPieces[sideToMove, Constants.PIECE_QUEEN];
            while (bb1 != 0)
            {
                from = ChessMath.LowestSetBitIndex(bb1);
                ChessMath.ResetLowestSetBit(ref bb1);
                for (int dir = Constants.DIR_N; dir < Constants.MAX_DIRECTIONS; ++dir)
                {
                    for (to = qrbMoves[from, dir]; to > -1; to = qrbMoves[to, dir])
                    {
                        if ((bbaMask[to] & bbAll) != 0)
                        {
                            if ((bbaMask[to] & bbaUnits[OpponentColor]) != 0)
                            {
                                moveList.Add(Constants.PIECE_QUEEN, from, to, MoveFlags.Capture,
                                    capture: board[to], score: captureScores[board[to], Constants.PIECE_QUEEN]);
                            }

                            break;
                        }
                        moveList.Add(Constants.PIECE_QUEEN, from, to, score: history[from, to]);
                    }
                }
            }

            #endregion

            #region Generate King Moves

            from = ChessMath.LowestSetBitIndex(bbaPieces[sideToMove, Constants.PIECE_KING]);
            bb1 = bbaKingMoves[from] & bbaUnits[OpponentColor];
            while (bb1 != 0)
            {
                to = ChessMath.LowestSetBitIndex(bb1);
                ChessMath.ResetLowestSetBit(ref bb1);
                moveList.Add(Constants.PIECE_KING, from, to, MoveFlags.Capture, capture: board[to],
                    score: captureScores[board[to], Constants.PIECE_KING]);
            }

            bb1 = ChessMath.AndNot(bbaKingMoves[from], bbAll);
            while (bb1 != 0)
            {
                to = ChessMath.LowestSetBitIndex(bb1);
                ChessMath.ResetLowestSetBit(ref bb1);
                moveList.Add(Constants.PIECE_KING, from, to, score: history[from, to]);
            }

            #endregion

            moveList.EndPly();
        }

        public static void AddPawnMove(MoveList moveList, int from, int to, MoveFlags flags = MoveFlags.PawnMove, int capture = Constants.PIECE_NONE, int score = 0)
        {
            int rank = ChessMath.IndexToRank(to);
            if (rank == Constants.MIN_COORD || rank == Constants.MAX_COORD)
            {
                flags = flags == MoveFlags.Capture ? MoveFlags.PromoteCapture : MoveFlags.Promote;
                for (int p = Constants.PIECE_KNIGHT; p <= Constants.PIECE_QUEEN; ++p)
                {
                    moveList.Add(Constants.PIECE_PAWN, from, to, flags, capture, p, score);
                }
            }
            else
            {
                moveList.Add(Constants.PIECE_PAWN, from, to, flags, capture, score: score);
            }
        }

        public void GenerateEnPassant(MoveList moveList)
        {
            if (enPassantValidated.HasValue)
            {
                ulong bb = bbaPawnDefends[sideToMove, enPassantValidated.Value] & bbaPieces[sideToMove, Constants.PIECE_PAWN];
                while (bb != 0)
                {
                    int from = ChessMath.LowestSetBitIndex(bb);
                    ChessMath.ResetLowestSetBit(ref bb);
                    int captIndex = enPassantValidated.Value + epOffset[sideToMove];
                    moveList.Add(Constants.PIECE_PAWN, from, enPassantValidated.Value, MoveFlags.EnPassant,
                        board[captIndex], score: captureScores[board[captIndex], Constants.PIECE_PAWN]);
                }
            }
        }

        public void GenerateCastling(MoveList moveList)
        {
            if (ChessMath.IsColorWhite(sideToMove))
            {
                if ((castling & CastlingFlags.WhiteKingSide) != 0 && (bbaBetween[Constants.H1, Constants.E1] & bbAll) == 0)
                {
                    moveList.Add(Constants.PIECE_KING, Constants.E1, Constants.G1, MoveFlags.Castle,
                        score: history[Constants.E1, Constants.G1]);
                }

                if ((castling & CastlingFlags.WhiteQueenSide) != 0 && (bbaBetween[Constants.A1, Constants.E1] & bbAll) == 0)
                {
                    moveList.Add(Constants.PIECE_KING, Constants.E1, Constants.C1, MoveFlags.Castle,
                        score: history[Constants.E1, Constants.C1]);
                }
            }
            else
            {
                if ((castling & CastlingFlags.BlackKingSide) != 0 && (bbaBetween[Constants.E8, Constants.H8] & bbAll) == 0)
                {
                    moveList.Add(Constants.PIECE_KING, Constants.E8, Constants.G8, MoveFlags.Castle,
                        score: history[Constants.E8, Constants.G8]);
                }

                if ((castling & CastlingFlags.BlackQueenSide) != 0 && (bbaBetween[Constants.E8, Constants.A8] & bbAll) == 0)
                {
                    moveList.Add(Constants.PIECE_KING, Constants.E8, Constants.C8, MoveFlags.Castle,
                        score: history[Constants.E8, Constants.C8]);
                }
            }
        }

        #endregion

        #region Make/Unmake Move

        public bool MakeMove(Move move)
        {
            PushBoardState(move);

            if (enPassantValidated.HasValue)
            {
                ZobristHash.HashEnPassant(ref hash, enPassantValidated.Value);
            }
            enPassant = null;
            enPassantValidated = null;

            ZobristHash.HashCastling(ref hash, castling);
            ply++;


            switch (move.Flags)
            {
                case MoveFlags.Normal:
                    UpdatePiece(sideToMove, move.Piece, move.From, move.To);
                    castling &= (CastlingFlags)(castleMask[move.From] & castleMask[move.To]);
                    halfMoveClock++;
                    break;

                case MoveFlags.Capture:
                    RemovePiece(OpponentColor, move.Capture, move.To);
                    UpdatePiece(sideToMove, move.Piece, move.From, move.To);
                    castling &= (CastlingFlags)(castleMask[move.From] & castleMask[move.To]);
                    halfMoveClock = 0;
                    break;

                case MoveFlags.Castle:
                    CastlingRookMove rookMove = LookupRookMove(move.To);
                    if (IsSquareAttackedByColor(move.From, OpponentColor) ||
                        IsSquareAttackedByColor(rookMove.KingMoveThrough, OpponentColor))
                    {
                        PopBoardState();
                        return false;
                    }
                    UpdatePiece(sideToMove, move.Piece, move.From, move.To);
                    UpdatePiece(sideToMove, Constants.PIECE_ROOK, rookMove.RookFrom, rookMove.RookTo);
                    castling &= (CastlingFlags)(castleMask[move.From] & castleMask[move.To]);
                    halfMoveClock++;
                    break;

                case MoveFlags.EnPassant:
                    int captureOffset = epOffset[sideToMove];
                    RemovePiece(OpponentColor, move.Capture, move.To + captureOffset);
                    UpdatePiece(sideToMove, move.Piece, move.From, move.To);
                    halfMoveClock = 0;
                    break;

                case MoveFlags.PawnMove:
                    UpdatePiece(sideToMove, move.Piece, move.From, move.To);
                    halfMoveClock = 0;
                    break;

                case MoveFlags.DblPawnMove:
                    UpdatePiece(sideToMove, move.Piece, move.From, move.To);
                    enPassant = move.To + epOffset[sideToMove];
                    if (IsEnPassantValid(OpponentColor))
                    {
                        enPassantValidated = enPassant;
                        ZobristHash.HashEnPassant(ref hash, enPassantValidated.Value);
                    }
                    halfMoveClock = 0;
                    break;

                case MoveFlags.Promote:
                    RemovePiece(sideToMove, move.Piece, move.From);
                    AddPiece(sideToMove, move.Promote, move.To);
                    halfMoveClock = 0;
                    break;

                case MoveFlags.PromoteCapture:
                    RemovePiece(OpponentColor, move.Capture, move.To);
                    RemovePiece(sideToMove, move.Piece, move.From);
                    AddPiece(sideToMove, move.Promote, move.To);
                    castling &= (CastlingFlags)(castleMask[move.From] & castleMask[move.To]);
                    halfMoveClock = 0;
                    break;

                case MoveFlags.NullMove:
                    // don't move any pieces
                    break;

                default:
                    Util.Fail($"Invalid move encountered in MakeMove: ({move}).");
                    Util.TraceError($"Invalid move encountered in MakeMove: ({move}).");
                    break;
            }

            if (!ChessMath.IsColorWhite(sideToMove))
            {
                fullMoveCounter++;
            }

            ZobristHash.HashCastling(ref hash, castling);
            ZobristHash.HashActiveColor(ref hash, sideToMove);
            SideToMove = OpponentColor;

            if (IsCheck(sideToMove))
            {
                UnmakeMove();
                return false;
            }

            return true;
        }

        public void UnmakeMove()
        {
            SideToMove = OpponentColor;
            Move move = boardStateStack.Peek().Move;

            switch (move.Flags)
            {
                case MoveFlags.Normal:
                case MoveFlags.PawnMove:
                case MoveFlags.DblPawnMove:
                    UpdatePiece(sideToMove, move.Piece, move.To, move.From);
                    break;

                case MoveFlags.Capture:
                    UpdatePiece(sideToMove, move.Piece, move.To, move.From);
                    AddPiece(OpponentColor, move.Capture, move.To);
                    break;

                case MoveFlags.Castle:
                    CastlingRookMove rookMove = LookupRookMove(move.To);
                    UpdatePiece(sideToMove, Constants.PIECE_ROOK, rookMove.RookTo, rookMove.RookFrom);
                    UpdatePiece(sideToMove, move.Piece, move.To, move.From);
                    break;

                case MoveFlags.EnPassant:
                    int captureOffset = epOffset[sideToMove];
                    UpdatePiece(sideToMove, move.Piece, move.To, move.From);
                    AddPiece(OpponentColor, move.Capture, move.To + captureOffset);
                    break;

                case MoveFlags.Promote:
                    RemovePiece(sideToMove, move.Promote, move.To);
                    AddPiece(sideToMove, move.Piece, move.From);
                    break;

                case MoveFlags.PromoteCapture:
                    RemovePiece(sideToMove, move.Promote, move.To);
                    AddPiece(sideToMove, move.Piece, move.From);
                    AddPiece(OpponentColor, move.Capture, move.To);
                    break;

                case MoveFlags.NullMove:
                    // don't move any pieces
                    break;

                default:
                    Util.Fail($"Invalid move encountered in UnmakeMove: ({move}).");
                    Util.TraceError($"Invalid move encountered in UnmakeMove: ({move}).");
                    break;
            }

            PopBoardState();
        }

        #endregion

        #region FEN string processing

        public string ToFenString()
        {
            StringBuilder sb = new();
            Span<int> colors = stackalloc int[Constants.MAX_SQUARES];
            InitializeColorBoard(colors);

            for (int rank = Constants.MAX_COORD; rank >= Constants.MIN_COORD; --rank)
            {
                int emptyCount = 0;

                for (int file = 0; file <= Constants.MAX_COORD; ++file)
                {
                    int index = ChessMath.CoordToIndex(file, rank);
                    int color = colors[index];
                    int piece = board[index];
                    if (piece == Constants.PIECE_NONE)
                    {
                        ++emptyCount;
                    }
                    else
                    {
                        if (emptyCount > 0)
                        {
                            sb.Append(emptyCount);
                            emptyCount = 0;
                        }

                        sb.Append(ChessString.ToFenPiece(color, piece));
                    }
                }

                if (emptyCount > 0)
                {
                    sb.Append(emptyCount);
                }

                if (rank > Constants.MIN_COORD)
                {
                    sb.Append('/');
                }
            }

            sb.Append(ChessMath.IsColorWhite(sideToMove) ? " w" : " b");
            sb.Append(GetCastlingString());
            if (!enPassant.HasValue)
            {
                sb.Append(" -");
            }
            else
            {
                sb.Append(' ');
                sb.Append(ChessString.IndexToString(enPassant.Value));
            }

            sb.Append($" {halfMoveClock} {fullMoveCounter}");
            return sb.ToString();
        }

        public void LoadNewGamePosition()
        {
            LoadFenPosition(Constants.NEW_GAME_FEN);
        }

        public void LoadFenPosition(string fen)
        {
            if (!Regex.IsMatch(fen, Constants.FEN_STRING_REGEX))
            {
                Util.TraceWarning($"Attempted to load invalid FEN board: '{fen}'.");
                return;
            }

            Clear();
            string[] fenParts = fen.Trim().Split(' ');
            LoadFenPieces(fenParts[0]);
            LoadFenSideToMove(fenParts[1]);
            LoadFenCastling(fenParts[2]);
            LoadFenEnPassant(fenParts[3]);
            LoadFenHalfMoveClock(fenParts[4]);
            LoadFenFullMoveCounter(fenParts[5]);

            repeatStack.Push(hash);
        }

        public void LoadFenPieces(string fenPieces)
        {
            int rank = Constants.MAX_COORD, file = 0;

            foreach (char ch in fenPieces)
            {
                if (ch == '/')
                {
                    --rank;
                    file = 0;
                }
                else if (char.IsDigit(ch))
                {
                    file += (ch - '0');
                }
                else
                {
                    ChessString.ParseFenPiece(ch, out int color, out int piece);
                    AddPiece(color, piece, ChessMath.CoordToIndex(file++, rank));
                }
            }
        }

        public void LoadFenSideToMove(string fenSideToMove)
        {
            SideToMove = ChessString.ParseFenColor(fenSideToMove[0]);
            ZobristHash.HashActiveColor(ref hash, sideToMove);
        }

        public void LoadFenCastling(string fenCastling)
        {
            foreach (char ch in fenCastling)
            {
                switch (ch)
                {
                    case 'K':
                        castling |= CastlingFlags.WhiteKingSide;
                        break;

                    case 'Q':
                        castling |= CastlingFlags.WhiteQueenSide;
                        break;

                    case 'k':
                        castling |= CastlingFlags.BlackKingSide;
                        break;

                    case 'q':
                        castling |= CastlingFlags.BlackQueenSide;
                        break;

                    case '-':
                        // neither side can castle
                        break;

                    default:
                        Util.TraceWarning($"Invalid castling specification in FEN: '{fenCastling}'");
                        break;
                }
            }

            ZobristHash.HashCastling(ref hash, castling);
        }

        public void LoadFenEnPassant(string fenEnPassant)
        {
            enPassantValidated = null;
            enPassant = ChessString.TryParseIndex(fenEnPassant, out int ep) ? ep : null;
            if (enPassant.HasValue && IsEnPassantValid(sideToMove))
            {
                enPassantValidated = enPassant;
                ZobristHash.HashEnPassant(ref hash, enPassantValidated.Value);
            }
        }

        public void LoadFenHalfMoveClock(string s)
        {
            if (!int.TryParse(s, out int halfMove))
            {
                Util.TraceWarning("Invalid half move field encountered in FEN substring: '{s}'.", s);
                return;
            }
            halfMoveClock = halfMove;
        }

        public void LoadFenFullMoveCounter(string s)
        {
            if (!int.TryParse(s, out int moveCounter))
            {
                Util.TraceWarning("Invalid half move field encountered in FEN substring: '{s}'.", s);
                return;
            }

            fullMoveCounter = moveCounter;
        }


        public string GetCastlingString()
        {
            StringBuilder sb = new();

            sb.Append(' ');
            if ((castling & CastlingFlags.WhiteKingSide) != 0)
            {
                sb.Append('K');
            }

            if ((castling & CastlingFlags.WhiteQueenSide) != 0)
            {
                sb.Append('Q');
            }

            if ((castling & CastlingFlags.BlackKingSide) != 0)
            {
                sb.Append('k');
            }

            if ((castling & CastlingFlags.BlackQueenSide) != 0)
            {
                sb.Append('q');
            }

            if (castling == CastlingFlags.None)
            {
                sb.Append('-');
            }

            return sb.ToString();
        }

        #endregion

        public bool IsCheck()
        {
            return IsCheck(OpponentColor);
        }

        private bool IsCheck(int byColor)
        {
            int kingIndex = ChessMath.LowestSetBitIndex(bbaPieces[byColor ^ 1, Constants.PIECE_KING]);
            return IsSquareAttackedByColor(kingIndex, byColor);
        }

        public bool IsSquareAttackedByColor(int index, int color)
        {
            if ((bbaPawnDefends[color, index] & bbaPieces[color, Constants.PIECE_PAWN]) != 0)
            {
                return true;
            }

            if ((bbaKnightMoves[index] & bbaPieces[color, Constants.PIECE_KNIGHT]) != 0)
            {
                return true;
            }

            if ((bbaKingMoves[index] & bbaPieces[color, Constants.PIECE_KING]) != 0)
            {
                return true;
            }

            ulong bb = bbaRookMoves[index] &
                          (bbaPieces[color, Constants.PIECE_ROOK] | bbaPieces[color, Constants.PIECE_QUEEN]);

            bb |= bbaBishopMoves[index] &
                  (bbaPieces[color, Constants.PIECE_BISHOP] | bbaPieces[color, Constants.PIECE_QUEEN]);

            while (bb != 0)
            {
                int index2 = ChessMath.LowestSetBitIndex(bb);
                if ((bbaBetween[index2, index] & bbAll) == 0)
                {
                    return true;
                }
                
                ChessMath.ResetLowestSetBit(ref bb);
            }

            return false;
        }

        public void UpdatePiece(int color, int piece, int fromIndex, int toIndex)
        {
            HashPiece(color, piece, fromIndex);
            HashPiece(color, piece, toIndex);
            bbaUnits[color] = ChessMath.AndNot(bbaUnits[color], bbaMask[fromIndex]);
            bbaUnits[color] |= bbaMask[toIndex];
            bbAll = bbaUnits[Constants.COLOR_WHITE] | bbaUnits[Constants.COLOR_BLACK];
            board[toIndex] = piece;
            board[fromIndex] = Constants.PIECE_NONE;
            bbaPieces[color, piece] = ChessMath.AndNot(bbaPieces[color, piece], bbaMask[fromIndex]) | bbaMask[toIndex];
        }

        public void RemovePiece(int color, int piece, int index)
        {
            HashPiece(color, piece, index);
            board[index] = Constants.PIECE_NONE;
            bbaUnits[color] = ChessMath.AndNot(bbaUnits[color], bbaMask[index]);
            bbaPieces[color, piece] = ChessMath.AndNot(bbaPieces[color, piece], bbaMask[index]);
            bbAll = bbaUnits[Constants.COLOR_WHITE] | bbaUnits[Constants.COLOR_BLACK];
        }

        public void AddPiece(int color, int piece, int index)
        {
            HashPiece(color, piece, index);
            board[index] = piece;
            bbaUnits[color] |= bbaMask[index];
            bbaPieces[color, piece] |= bbaMask[index];
            bbAll = bbaUnits[Constants.COLOR_WHITE] | bbaUnits[Constants.COLOR_BLACK];
        }

        private void HashPiece(int color, int piece, int index)
        {
            ZobristHash.HashPiece(ref hash, color, piece, index);
            if (piece == Constants.PIECE_PAWN)
            {
                ZobristHash.HashPiece(ref pawnHash, color, piece, index);
            }
        }

        private void PushBoardState(Move move)
        {
            boardStateStack.Push(new BoardState(this, move));
        }

        private void PopBoardState()
        {
            boardStateStack.Pop().Restore(this);
        }

        public bool IsEnPassantValid(int color)
        {
            if (!enPassant.HasValue)
            {
                return false;
            }

            return (bbaPawnDefends[color, enPassant.Value] & bbaPieces[color, Constants.PIECE_PAWN]) != 0;
        }

        private void InitializeColorBoard(Span<int> colors)
        {
            colors.Fill(Constants.COLOR_NONE);
            ulong bb = bbaUnits[Constants.COLOR_WHITE];

            int index;
            while (bb != 0)
            {
                index = ChessMath.LowestSetBitIndex(bb);
                ChessMath.ResetLowestSetBit(ref bb);
                colors[index] = Constants.COLOR_WHITE;
            }

            bb = bbaUnits[Constants.COLOR_BLACK];
            while (bb != 0)
            {
                index = ChessMath.LowestSetBitIndex(bb);
                ChessMath.ResetLowestSetBit(ref bb);
                colors[index] = Constants.COLOR_BLACK;
            }
        }

        private static int GetEdge(int index, int delta)
        {
            int file, rank;

            do
            {
                index += delta;
                ChessMath.IndexToCoords(index, out file, out rank);
            } while (file > 0 && file < 7 && rank > 0 && rank < 7);

            return index;
        }

        private static CastlingRookMove LookupRookMove(int kingTo)
        {
            switch (kingTo)
            {
                case Constants.C1:
                    return castlingRookMoves[0];

                case Constants.G1:
                    return castlingRookMoves[1];

                case Constants.C8:
                    return castlingRookMoves[2];

                case Constants.G8:
                    return castlingRookMoves[3];

                default:
                    Util.Fail($"Invalid castling move with king moving to {kingTo}.");
                    return new CastlingRookMove();
            }
        }
    }
}
