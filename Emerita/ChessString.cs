using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emerita
{
    public static class ChessString
    {
        private static readonly string[] pieceNames = { "P", "N", "B", "R", "Q", "K" };
        public static string IndexToString(int index)
        {
            Util.Assert(ChessMath.IsValidIndex(index));
            ChessMath.IndexToCoords(index, out int file, out int rank);
            return $"{FileToString(file)}{RankToString(rank)}";
        }

        public static bool TryParseIndex(string sqName, out int index)
        {
            if (sqName.Length < 2 || (sqName[0] is < 'a' or > 'h') || (sqName[1] is < '1' or > '8'))
            {
                index = 0;
                return false;
            }

            index = ChessMath.CoordToIndex(sqName[0] - 'a', sqName[1] - '1');
            return true;
        }

        public static string FileToString(int file)
        {
            Util.Assert(ChessMath.IsValidCoord(file));
            return $"{(char)(file + 'a')}";
        }

        public static string RankToString(int rank)
        {
            Util.Assert(ChessMath.IsValidCoord(rank));
            return $"{(char)(rank + '1')}";
        }

        public static string PieceToString(int piece)
        {
            Util.Assert(piece == Constants.PIECE_NONE || ChessMath.IsValidPiece(piece));
            if (piece == Constants.PIECE_NONE)
            {
                return string.Empty;
            }

            return pieceNames[piece];
        }

        public static void ParseFenPiece(char fenPiece, out int color, out int piece)
        {
            color = char.IsUpper(fenPiece) ? Constants.COLOR_WHITE : Constants.COLOR_BLACK;
            piece = Constants.PIECE_NONE;
            string pc = char.ToUpper(fenPiece).ToString();
            for (int i = 0; i < Constants.MAX_PIECES; ++i)
            {
                if (string.Equals(pieceNames[i], pc))
                {
                    piece = i;
                    break;
                }
            }
        }

        public static int ParseFenColor(char fenColor)
        {
            return fenColor == 'w' ? Constants.COLOR_WHITE : Constants.COLOR_BLACK;
        }

        public static char ToFenPiece(int color, int piece)
        {
            char fenPiece = pieceNames[piece][0];
            if (!ChessMath.IsColorWhite(color))
            {
                fenPiece = char.ToLower(fenPiece);
            }

            return fenPiece;
        }

        public static string BitBoardToString(ulong bitBoard)
        {
            StringBuilder sb = new();
            for (int j = 56; j >= 0; j -= 8)
            {
                for (int i = 0; i < 8; i++)
                {
                    sb.Append(ChessMath.GetBit(bitBoard, i + j));
                    sb.Append(' ');
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
