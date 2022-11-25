namespace Emerita
{
    public static class Constants
    {
        #region Miscellaneous

        public const ulong BITBOARD_EMPTY = 0ul;
        public const int MAX_GAME_PLY = 1024;
        public const int MAX_SEARCH_PLY = 32;
        public const int MOVE_LIST_SIZE = MAX_SEARCH_PLY * 64;
        public const short BASE_CAPTURE_SCORE = 1000;
        public const short BASE_HASH_SCORE = 10000;
        public const string NEW_GAME_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public const string FEN_STRING_REGEX = @"^\s*([rnbqkpRNBQKP1-8]+/){7}[rnbqkpRNBQKP1-8]+\s[bw]\s(-|K?Q?k?q?)\s(-|[a-h][36])\s\d+\s\d+\s*$";

        #endregion

        #region Coords (File/Rank)

        public const int MIN_COORD = 0;
        public const int MAX_COORD = 7;

        #endregion

        #region Colors

        public const int COLOR_NONE = -1;
        public const int COLOR_WHITE = 0;
        public const int COLOR_BLACK = 1;
        public const int MAX_COLORS = 2;

        #endregion

        #region Board Squares

        public const int A1 = 0;
        public const int B1 = 1;
        public const int C1 = 2;
        public const int D1 = 3;
        public const int E1 = 4;
        public const int F1 = 5;
        public const int G1 = 6;
        public const int H1 = 7;

        public const int A2 = 8;
        public const int B2 = 9;
        public const int C2 = 10;
        public const int D2 = 11;
        public const int E2 = 12;
        public const int F2 = 13;
        public const int G2 = 14;
        public const int H2 = 15;

        public const int A3 = 16;
        public const int B3 = 17;
        public const int C3 = 18;
        public const int D3 = 19;
        public const int E3 = 20;
        public const int F3 = 21;
        public const int G3 = 22;
        public const int H3 = 23;

        public const int A4 = 24;
        public const int B4 = 25;
        public const int C4 = 26;
        public const int D4 = 27;
        public const int E4 = 28;
        public const int F4 = 29;
        public const int G4 = 30;
        public const int H4 = 31;

        public const int A5 = 32;
        public const int B5 = 33;
        public const int C5 = 34;
        public const int D5 = 35;
        public const int E5 = 36;
        public const int F5 = 37;
        public const int G5 = 38;
        public const int H5 = 39;

        public const int A6 = 40;
        public const int B6 = 41;
        public const int C6 = 42;
        public const int D6 = 43;
        public const int E6 = 44;
        public const int F6 = 45;
        public const int G6 = 46;
        public const int H6 = 47;

        public const int A7 = 48;
        public const int B7 = 49;
        public const int C7 = 50;
        public const int D7 = 51;
        public const int E7 = 52;
        public const int F7 = 53;
        public const int G7 = 54;
        public const int H7 = 55;

        public const int A8 = 56;
        public const int B8 = 57;
        public const int C8 = 58;
        public const int D8 = 59;
        public const int E8 = 60;
        public const int F8 = 61;
        public const int G8 = 62;
        public const int H8 = 63;

        public const int MAX_SQUARES = 64;

        #endregion

        #region Move Directions (from White's perspective)

        public const int DIR_N = 0;
        public const int DIR_NE = 1;
        public const int DIR_E = 2;
        public const int DIR_SE = 3;
        public const int DIR_S = 4;
        public const int DIR_SW = 5;
        public const int DIR_W = 6;
        public const int DIR_NW = 7;
        public const int MAX_DIRECTIONS = 8;

        #endregion

        #region Piece Types

        public const int PIECE_NONE = -1;
        public const int PIECE_PAWN = 0;
        public const int PIECE_KNIGHT = 1;
        public const int PIECE_BISHOP = 2;
        public const int PIECE_ROOK = 3;
        public const int PIECE_QUEEN = 4;
        public const int PIECE_KING = 5;
        public const int MAX_PIECES = 6;

        #endregion
    }
}
