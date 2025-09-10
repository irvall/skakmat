using skakmat.Game;

namespace skakmat.Engine;
internal class GameController(Board board, MoveGenerator moveGenerator)
{
    internal int MovesPlayed => moveHistory.Count;
    private readonly List<HistoryEntry> moveHistory = [];

    internal enum GameStatus
    {
        Ongoing,
        BlackWon,
        WhiteWon,
        Stalemate
    };
    public GameStatus Status { get; private set; } = GameStatus.Ongoing;

    internal PieceSelection? TrySelectPiece(int index)
    {
        var state = board.GetBoardState();
        var pieceIndex = board.GetPieceIndexAt(index);

        if (pieceIndex == Piece.EmptySquare || !Piece.IsCorrectColor(pieceIndex, state.WhiteToPlay))
            return null;

        var validMoves = moveGenerator.GenerateMoves();
        return new PieceSelection(pieceIndex, index, validMoves);
    }

    public void UpdateGameStatus()
    {
        var state = board.GetBoardState();
        var moves = moveGenerator.GenerateMoves();
        if (moves.Count > 0 || !moveGenerator.IsKingUnderAttack())
            return;
        Status = state.WhiteToPlay ? GameStatus.BlackWon : GameStatus.WhiteWon;
    }

    internal struct HistoryEntry(Move move, int capturedPiece)
    {
        internal Move Move = move;
        internal int CapturedPiece = capturedPiece;
    }

    public void MakeMove(Move move)
    {
        int capturedPiece = board.GetPieceIndexAt(move.TargetBit);
        moveHistory.Add(new HistoryEntry(move, capturedPiece));
        board.ApplyMove(move);
    }

    internal HistoryEntry LastEntry()
    {
        if (moveHistory.Count == 0)
            throw new InvalidOperationException("No moves in history");
        return moveHistory[^1];

    }

    public void UndoLastMove()
    {
        if (moveHistory.Count == 0) return;

        var lastEntry = moveHistory[^1];
        moveHistory.RemoveAt(moveHistory.Count - 1);
        board.UndoMove(lastEntry.Move, lastEntry.CapturedPiece);
    }
}
