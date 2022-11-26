namespace Emerita
{
    [Flags]
    public enum CastlingFlags : byte
    {
        None = 0,
        WhiteKingSide = 1,
        WhiteQueenSide = 2,
        BlackKingSide = 4,
        BlackQueenSide = 8,
        All = (WhiteKingSide | WhiteQueenSide | BlackKingSide | BlackQueenSide)
    }

    public enum MoveFlags : byte
    {
        Normal = 0,
        Capture,
        Castle,
        EnPassant,
        PawnMove,
        DblPawnMove,
        Promote,
        Check,
        PromoteCapture,
        NullMove
    }

    public enum GamePhase : byte
    {
        Opening,
        MidGame,
        EndGame
    }

    public enum UciCommand : byte
    {
        Debug,
        Go,
        IsReady,
        PonderHit,
        Position,
        Quit,
        Register,
        SetOption,
        Stop,
        Uci,
        UciNewGame
    }

    public enum UciGoOption : byte
    {
        BInc,
        BTime,
        Depth,
        Infinite,
        Mate,
        MovesToGo,
        MoveTime,
        Nodes,
        Ponder,
        SearchMoves,
        WInc,
        WTime
    }
}