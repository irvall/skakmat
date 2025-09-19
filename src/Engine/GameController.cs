using skakmat.Chess;
using skakmat.Game;
using skakmat.Helpers;

namespace skakmat.Engine;

internal class GameController
{

    public event Action<GameEvent>? GameEventOccurred;
    internal IReadOnlyList<Position> Positions => [.. boardPositions];
    internal IReadOnlyList<Move> Moves => [.. boardPositions.Skip(1).Select(s => s.LastMovePlayed!)];
    internal Position RecentPosition => boardPositions[^1];
    internal Position CurrentPosition => Positions[positionIndex];
    internal bool WhiteToPlay => RecentPosition.WhiteToPlay;
    internal bool KingIsUnderAttack => moveGenerator.IsKingUnderAttack(CurrentPosition);
    private readonly Board board;
    private readonly MoveTables moveTables;
    private readonly MoveGenerator moveGenerator;
    private readonly List<Position> boardPositions;
    private List<Move> validMovesCache = [];
    private bool movesShouldUpdate = true;
    private int positionIndex = 0;

    public GameController()
    {
        board = new Board();
        moveTables = new MoveTables();
        moveGenerator = new MoveGenerator(moveTables, board);
        var initialPosition = board.CreatePosition();
        boardPositions = [initialPosition];
    }


    internal GameStatus Status { get; private set; } = GameStatus.Ongoing;
    internal PieceSelection? SelectedPiece { get; private set; }

    internal void SelectPiece(int squareIndex)
    {
        SelectedPiece = TrySelectPiece(squareIndex);
    }

    internal List<Move> GetValidMoves(int squareIndex)
    {
        return moveGenerator.GenerateMovesForSquare(squareIndex, RecentPosition);
    }

    internal List<Move> GetValidMoves()
    {
        return moveGenerator.GenerateMoves();
    }

    internal PieceSelection? TrySelectPiece(int squareIndex)
    {
        var pieceIndex = board.GetPieceIndexAt(squareIndex);
        if (pieceIndex == Piece.EmptySquare || !Piece.IsCorrectColor(pieceIndex, RecentPosition.WhiteToPlay))
            return null;

        var validMoves = GetValidMoves(squareIndex);
        return new PieceSelection(pieceIndex, squareIndex, validMoves);
    }


    private void UpdateGameStatus()
    {
        if (movesShouldUpdate)
            UpdateValidMoves();

        var wasInCheck = KingIsUnderAttack;
        var kingBoard = RecentPosition.Bitboards[Piece.WhiteKing] | RecentPosition.Bitboards[Piece.BlackKing];
        if (RecentPosition.AllPieces == kingBoard)
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
            Status = RecentPosition.WhiteToPlay ? GameStatus.BlackWon : GameStatus.WhiteWon;
            GameEventOccurred?.Invoke(new GameEvent { Type = GameEventType.Checkmate, Status = Status });
            System.Console.WriteLine(RecentPosition.WhiteToPlay ? "0 - 1 Black wins by checkmate" : "1 - 0 White wins by checkmate");
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
        var updatedPosition = board.ApplyMove(actualMove);
        boardPositions.Add(updatedPosition);
        BoardHelper.PrintMoveHistory([.. Moves]);

        var isKnight = move.PieceIndex == Piece.WhiteKnight || move.PieceIndex == Piece.BlackKnight;
        var wasCapture = capturedPiece != Piece.EmptySquare;

        if (isKnight)
            GameEventOccurred?.Invoke(new GameEvent { Type = GameEventType.MovePlayed, Move = actualMove, PieceType = PieceType.Knight });
        else if (wasCapture)
            GameEventOccurred?.Invoke(new GameEvent { Type = GameEventType.PieceCaptured, Move = actualMove });
        else
            GameEventOccurred?.Invoke(new GameEvent { Type = GameEventType.MovePlayed, Move = actualMove });

        positionIndex = boardPositions.Count - 1;
        UpdateGameStatus();
        SelectedPiece = null;
    }

    internal bool IsValidMove(Move move)
    {
        if (!AtMostRecentPosition())
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
        if (positionIndex == 0) return;
        positionIndex--;
    }

    internal void StepForward()
    {
        if (AtMostRecentPosition())
            return;
        positionIndex++;
    }

    internal bool AtMostRecentPosition()
    {
        return positionIndex == (Positions.Count - 1);
    }
}
