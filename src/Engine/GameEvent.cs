using skakmat.Game;

namespace skakmat.Engine;
public readonly struct GameEvent
{
    internal GameEventType Type { get; init; }
    internal Move? Move { get; init; }
    internal GameStatus? Status { get; init; }
    internal PieceType? PieceType { get; init; }
}

public enum GameEventType
{
    MovePlayed,
    Check,
    Checkmate,
    Stalemate,
    PieceCaptured
}