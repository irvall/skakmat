namespace skakmat.Game;
public readonly struct PieceSelection(int pieceType, int squareIndex, List<Move> validMoves)
{
    public int PieceType { get; } = pieceType;
    public int SquareIndex { get; } = squareIndex;
    public List<Move> ValidMoves { get; } = validMoves;
}
