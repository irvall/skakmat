namespace skakmat;
readonly struct PieceSelection(ulong bit, ulong legalMoves, int pieceIndex, int squareIndex)
{
    public ulong Bit { get; } = bit;
    public ulong LegalMoves { get; } = legalMoves;
    public int PieceIndex { get; } = pieceIndex;
    public int SquareIndex { get; } = squareIndex;

    public override readonly string ToString() => $"({Bit}, {PieceIndex}, {SquareIndex})";
}
