using skakmat.Chess;
using skakmat.Game;

namespace skakmat.Engine;
internal class GameController
{
    internal IReadOnlyList<Move> MovesPlayed => [.. moveHistory.Select(entry => entry.Move)];
    internal BoardState BoardState
    {
        get
        {
            if (boardStateShouldUpdate)
            {
                cachedBoardState = board.GetBoardState();
                boardStateShouldUpdate = false;
            }
            return cachedBoardState;
        }
    }
    internal bool WhiteToPlay => BoardState.WhiteToPlay;
    internal bool KingIsUnderAttack => moveGenerator.IsKingUnderAttack();
    private readonly Board board;
    private readonly MoveTables moveTables;
    private readonly MoveGenerator moveGenerator;
    private readonly SoundController soundController;
    private readonly List<HistoryEntry> moveHistory = [];
    private BoardState cachedBoardState;
    private bool boardStateShouldUpdate = true;
    private List<Move> validMovesCache = [];
    private bool movesShouldUpdate = true;

    public GameController()
    {
        board = new Board();
        moveTables = new MoveTables();
        moveGenerator = new MoveGenerator(moveTables, board);
        soundController = new SoundController();
    }

    internal enum GameStatus
    {
        Ongoing,
        BlackWon,
        WhiteWon,
        Stalemate
    };
    internal GameStatus Status { get; private set; } = GameStatus.Ongoing;
    internal PieceSelection? SelectedPiece { get; private set; }

    internal void SelectPiece(int squareIndex)
    {
        SelectedPiece = TrySelectPiece(squareIndex);
    }

    internal List<Move> GetValidMoves(int squareIndex)
    {
        return moveGenerator.GenerateMovesForSquare(squareIndex, BoardState);
    }

    internal List<Move> GetValidMoves()
    {
        return moveGenerator.GenerateMoves();
    }

    internal PieceSelection? TrySelectPiece(int squareIndex)
    {
        var pieceIndex = board.GetPieceIndexAt(squareIndex);
        if (pieceIndex == Piece.EmptySquare || !Piece.IsCorrectColor(pieceIndex, board.WhiteToPlay))
            return null;

        var validMoves = GetValidMoves(squareIndex);
        return new PieceSelection(pieceIndex, squareIndex, validMoves);
    }


    internal void UpdateGameStatus()
    {
        if (KingIsUnderAttack)
            soundController.PlayCheckSound();
        if (movesShouldUpdate)
            UpdateValidMoves();
        if (validMovesCache.Count > 0)
        {
            Status = GameStatus.Ongoing;
            return;
        }

        if (moveGenerator.IsKingUnderAttack())
        {
            Status = BoardState.WhiteToPlay ? GameStatus.BlackWon : GameStatus.WhiteWon;
            System.Console.WriteLine($"{Status}!!!");
        }
        else
        {
            Status = GameStatus.Stalemate;
            System.Console.WriteLine($"Stalemate...");
        }
    }

    internal struct HistoryEntry(Move move, int capturedPiece)
    {
        internal Move Move = move;
        internal int CapturedPiece = capturedPiece;
    }

    private void UpdateValidMoves()
    {
        validMovesCache = moveGenerator.GenerateMoves();
        movesShouldUpdate = false;
    }

    public List<Move> GetValidMovesForSelected()
    {
        if (!SelectedPiece.HasValue) return [];
        return SelectedPiece.Value.ValidMoves;
    }

    internal void MakeMove(Move move)
    {
        if (!IsValidMove(move)) return;
        var actualMove = validMovesCache.First(m => m.Equals(move));
        int capturedPiece = board.GetPieceIndexAt(actualMove.TargetBit);
        board.ApplyMove(actualMove);
        moveHistory.Add(new HistoryEntry(actualMove, capturedPiece));
        movesShouldUpdate = true;
        boardStateShouldUpdate = true;
        UpdateGameStatus();
        SelectedPiece = null;
        soundController.PlayMoveSound();
    }

    internal HistoryEntry? LastEntry()
    {
        if (moveHistory.Count == 0)
            return null;
        return moveHistory[^1];

    }

    internal void UndoLastMove()
    {
        if (moveHistory.Count == 0) return;

        var lastEntry = moveHistory[^1];
        moveHistory.RemoveAt(moveHistory.Count - 1);
        board.UndoMove(lastEntry.Move, lastEntry.CapturedPiece);
    }

    internal bool IsValidMove(Move move)
    {
        if (movesShouldUpdate)
            UpdateValidMoves();

        return validMovesCache.Any(m => m.Equals(move));
    }

    internal void ClearSelection()
    {
        SelectedPiece = null;
    }
}
