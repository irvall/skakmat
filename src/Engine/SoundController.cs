using Raylib_cs;
using skakmat.Utilities;

class SoundController
{

    private Sound moveSound;
    private Sound checkSound;

    public SoundController()
    {
        Initialize();
    }

    private void Initialize()
    {
        Raylib.InitAudioDevice();
        moveSound = RaylibUtility.LoadSound("move.wav");
        checkSound = RaylibUtility.LoadSound("wilhelm.wav");
    }

    internal void PlayCheckSound()
    {
        Raylib.PlaySound(checkSound);
    }

    internal void PlayMoveSound()
    {
        Raylib.PlaySound(moveSound);
    }
}