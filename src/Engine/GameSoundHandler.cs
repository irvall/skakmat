namespace skakmat.Engine;
public class GameSoundHandler()
{
    private readonly SoundController soundController = new();

    public void HandleGameEvent(GameEvent gameEvent)
    {
        switch (gameEvent.Type)
        {
            case GameEventType.MovePlayed:
                if (gameEvent.PieceType == Game.PieceType.Knight)
                    soundController.PlayHorseSound();
                else soundController.PlayMoveSound();
                break;
            case GameEventType.PieceCaptured:
                soundController.PlayCaptureSound();
                break;
            case GameEventType.Check:
                soundController.PlayCheckSound();
                break;
            case GameEventType.Checkmate:
                soundController.PlayCheckmateSound();
                break;
            case GameEventType.Stalemate:
                soundController.PlayStalemateSound();
                break;
        }
    }
}
