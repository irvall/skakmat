namespace skakmat;
readonly struct PieceSelection(ulong bit, ulong legalMoves, PieceType type, int pieceIndex, int squareIndex)
{
    public ulong Bit { get; } = bit;
    public ulong LegalMoves { get; } = legalMoves;
    public PieceType Type { get; } = type;
    public int PieceIndex { get; } = pieceIndex;
    public int SquareIndex { get; } = squareIndex;

    public override readonly string ToString() => $"({Bit}, {Type}, {SquareIndex})";
}
