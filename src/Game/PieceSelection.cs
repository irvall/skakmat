namespace skakmat.Game;
public readonly struct PieceSelection(int pieceType, int squareIndex, ulong legalMoves)
{
    public int PieceType { get; } = pieceType;
    public int SquareIndex { get; } = squareIndex;
    public ulong LegalMoves { get; } = legalMoves;
}
