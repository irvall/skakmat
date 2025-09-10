namespace skakmat.Game;
internal readonly struct PieceSelection(int pieceIndex, int squareIndex, List<Move> validMoves)
{
    internal int PieceIndex { get; } = pieceIndex;
    internal int SquareIndex { get; } = squareIndex;
    internal List<Move> ValidMoves { get; } = validMoves;
}
