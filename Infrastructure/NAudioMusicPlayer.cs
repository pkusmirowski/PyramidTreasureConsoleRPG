using NAudio.Wave;

namespace PyramidTreasureConsoleRPG.Infrastructure;

/// <summary>
/// Muzyka w tle (NAudio, zapętlona). Działa tylko na Windows; na innych systemach
/// cicho się wyłącza zamiast psuć grę.
/// </summary>
public sealed class NAudioMusicPlayer : IMusicPlayer, IDisposable
{
    private readonly string path;
    private WaveOutEvent? output;
    private AudioFileReader? reader;
    private bool disposing;

    public NAudioMusicPlayer(string path)
    {
        this.path = path;
    }

    public bool IsPlaying => output is not null && output.PlaybackState == PlaybackState.Playing;

    public bool IsAvailable { get; private set; } = true;

    public void Play(int volumePercent)
    {
        if (!IsAvailable || !OperatingSystem.IsWindows() || !File.Exists(path))
        {
            IsAvailable = false;
            return;
        }

        try
        {
            if (output is null)
            {
                reader = new AudioFileReader(path);
                output = new WaveOutEvent();
                output.Init(reader);
                output.PlaybackStopped += OnPlaybackStopped;
            }

            SetVolume(volumePercent);
            output.Play();
        }
        catch (Exception)
        {
            // Brak urządzenia audio, brak sterowników itp. – gra działa dalej bez muzyki.
            IsAvailable = false;
            StopPlayback();
        }
    }

    public void StopPlayback()
    {
        try
        {
            output?.Stop();
        }
        catch (Exception)
        {
            // Ignorujemy – i tak wyłączamy muzykę.
        }
    }

    public void SetVolume(int volumePercent)
    {
        if (output is not null)
        {
            output.Volume = Math.Clamp(volumePercent, 0, 100) / 100f;
        }
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        // Zapętlenie – ale nie wtedy, gdy właśnie zwalniamy zasoby lub gracz wyłączył muzykę.
        if (disposing || output is null || reader is null || e.Exception is not null)
        {
            return;
        }

        if (reader.Position >= reader.Length)
        {
            reader.Position = 0;
            output.Play();
        }
    }

    public void Dispose()
    {
        disposing = true;
        if (output is not null)
        {
            output.PlaybackStopped -= OnPlaybackStopped;
            output.Stop();
            output.Dispose();
            output = null;
        }

        reader?.Dispose();
        reader = null;
    }
}
