using Raylib_cs;
using skakmat.Helpers;

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
        moveSound = RaylibHelper.LoadSound("move.wav");
        checkSound = RaylibHelper.LoadSound("check.wav");
        horseSound = RaylibHelper.LoadSound("horse.wav");
        checkmateSound = RaylibHelper.LoadSound("wilhelm.wav");
        captureSound = RaylibHelper.LoadSound("capture.wav");
        stalemateSound = RaylibHelper.LoadSound("stalemate.wav");
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