using skakmat.Chess;
using skakmat.Game;
using skakmat.Utilities;

namespace skakmat.Engine;

internal class GameController
{

    public event Action<GameEvent>? GameEventOccurred;
    internal IReadOnlyList<BoardState> States => [.. boardStates];
    internal IReadOnlyList<Move> Moves => [.. boardStates.Skip(1).Select(s => s.LastMovePlayed!)];
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
    internal bool KingIsUnderAttack => moveGenerator.IsKingUnderAttack(GetCurrentState());
    private readonly Board board;
    private readonly MoveTables moveTables;
    private readonly MoveGenerator moveGenerator;
    private readonly List<BoardState> boardStates = [];
    private BoardState cachedBoardState;
    private bool boardStateShouldUpdate = true;
    private List<Move> validMovesCache = [];
    private bool movesShouldUpdate = true;
    public int stateIndex = 0;

    public GameController()
    {
        board = new Board();
        moveTables = new MoveTables();
        moveGenerator = new MoveGenerator(moveTables, board);
        boardStates.Add(BoardState);
    }


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


    private void UpdateGameStatus()
    {
        if (movesShouldUpdate)
            UpdateValidMoves();

        var wasInCheck = KingIsUnderAttack;
        var kingBoard = BoardState.Bitboards[Piece.WhiteKing] | BoardState.Bitboards[Piece.BlackKing];
        if (BoardState.AllPieces == kingBoard)
        {
            Status = GameStatus.Stalemate;
            GameEventOccurred?.Invoke(new GameEvent { Type = GameEventType.Stalemate, Status = Status });
        }
        else if (validMovesCache.Count > 0)
        {
            Status = GameStatus.Ongoing;
            if (wasInCheck)
                GameEventOccurred?.Invoke(new GameEvent { Type = GameEventType.Check });
            return;
        }

        else if (moveGenerator.IsKingUnderAttack())
        {
            Status = BoardState.WhiteToPlay ? GameStatus.BlackWon : GameStatus.WhiteWon;
            GameEventOccurred?.Invoke(new GameEvent { Type = GameEventType.Checkmate, Status = Status });
            System.Console.WriteLine(BoardState.WhiteToPlay ? "0 - 1 Black wins by checkmate" : "1 - 0 White wins by checkmate");
        }
        else
        {
            Status = GameStatus.Stalemate;
            GameEventOccurred?.Invoke(new GameEvent { Type = GameEventType.Stalemate, Status = Status });
            System.Console.WriteLine("0.5 - 0.5 Draw by stalemate");
        }
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
        movesShouldUpdate = true;
        boardStateShouldUpdate = true;
        board.ApplyMove(actualMove);
        boardStates.Add(BoardState);
        BoardUtility.PrintMoveHistory([.. Moves]);

        var isKnight = move.PieceIndex == Piece.WhiteKnight || move.PieceIndex == Piece.BlackKnight;
        var wasCapture = capturedPiece != Piece.EmptySquare;

        if (isKnight)
            GameEventOccurred?.Invoke(new GameEvent { Type = GameEventType.MovePlayed, Move = actualMove, PieceType = PieceType.Knight });
        else if (wasCapture)
            GameEventOccurred?.Invoke(new GameEvent { Type = GameEventType.PieceCaptured, Move = actualMove });
        else
            GameEventOccurred?.Invoke(new GameEvent { Type = GameEventType.MovePlayed, Move = actualMove });

        stateIndex = boardStates.Count - 1;
        UpdateGameStatus();
        SelectedPiece = null;
    }
    internal BoardState GetCurrentState()
    {
        return States[stateIndex];
    }

    internal bool IsValidMove(Move move)
    {
        if (!AtMostRecentState())
            return false;

        if (movesShouldUpdate)
            UpdateValidMoves();

        return validMovesCache.Any(m => m.Equals(move));
    }

    internal void ClearSelection()
    {
        SelectedPiece = null;
    }

    internal void StepBack()
    {
        if (stateIndex == 0) return;
        stateIndex--;
    }

    internal void StepForward()
    {
        if (AtMostRecentState())
            return;
        stateIndex++;
    }

    internal bool AtMostRecentState()
    {
        return stateIndex == (States.Count - 1);
    }
}
