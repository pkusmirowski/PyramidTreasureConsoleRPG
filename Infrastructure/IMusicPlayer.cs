namespace PyramidTreasureConsoleRPG.Infrastructure;

public interface IMusicPlayer
{
    /// <summary>False, gdy na tym systemie nie da się odtwarzać muzyki.</summary>
    bool IsAvailable { get; }

    void Play(int volumePercent);

    void StopPlayback();

    void SetVolume(int volumePercent);
}

/// <summary>Cicha implementacja na systemy bez obsługi audio i do testów.</summary>
public sealed class NullMusicPlayer : IMusicPlayer
{
    public bool IsAvailable => false;

    public void Play(int volumePercent)
    {
    }

    public void StopPlayback()
    {
    }

    public void SetVolume(int volumePercent)
    {
    }
}
