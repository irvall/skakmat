using Raylib_cs;
using skakmat.Utilities;

class SoundController
{

    private Sound moveSound;
    private Sound checkSound;
    private Sound horseSound;
    private Sound checkmateSound;
    private Sound captureSound;
    private Sound stalemateSound;

    public SoundController()
    {
        Initialize();
    }

    private void Initialize()
    {
        Raylib.InitAudioDevice();
        moveSound = RaylibUtility.LoadSound("move.wav");
        checkSound = RaylibUtility.LoadSound("check.wav");
        horseSound = RaylibUtility.LoadSound("horse.wav");
        checkmateSound = RaylibUtility.LoadSound("wilhelm.wav");
        captureSound = RaylibUtility.LoadSound("capture.wav");
        stalemateSound = RaylibUtility.LoadSound("stalemate.wav");
    }

    internal void PlayCheckSound()
    {
        Raylib.PlaySound(checkSound);
    }

    internal void PlayStalemateSound()
    {
        Raylib.PlaySound(stalemateSound);
    }


    internal void PlayMoveSound()
    {
        Raylib.PlaySound(moveSound);
    }

    internal void PlayCheckmateSound()
    {
        Raylib.PlaySound(checkmateSound);
    }

    internal void PlayHorseSound()
    {
        Raylib.PlaySound(horseSound);
    }

    internal void PlayCaptureSound()
    {
        Raylib.PlaySound(captureSound);
    }
}