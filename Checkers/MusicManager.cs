using NAudio.Wave;

namespace Checkers;

public sealed class MusicManager : IDisposable
{
    private WaveOutEvent? _output;
    private AudioFileReader? _reader;
    private bool _isStopping;

    public string LastError { get; private set; } = string.Empty;

    public bool ApplySettings(GameSettings settings)
    {
        LastError = string.Empty;

        if (!settings.EnableBackgroundMusic)
        {
            Stop();
            return true;
        }

        string musicPath = Path.Combine(AppContext.BaseDirectory, "bgsound.mp3");
        if (!File.Exists(musicPath))
        {
            LastError = $"Music file not found: {musicPath}";
            Stop();
            return false;
        }

        EnsureOutput(musicPath);
        if (_reader is null || _output is null)
        {
            LastError = "Unable to initialize audio output.";
            return false;
        }

        _reader.Volume = Math.Clamp(settings.MusicVolumePercent / 100f, 0f, 1f);

        if (_output.PlaybackState != PlaybackState.Playing)
        {
            _output.Play();
        }

        return true;
    }

    public void Stop()
    {
        _isStopping = true;
        _output?.Stop();
        _isStopping = false;
    }

    private void EnsureOutput(string musicPath)
    {
        if (_reader is null)
        {
            _reader = new AudioFileReader(musicPath);
        }

        if (_output is null)
        {
            _output = new WaveOutEvent();
            _output.Init(_reader);
            _output.PlaybackStopped += Output_PlaybackStopped;
        }
    }

    private void Output_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (_isStopping || _output is null || _reader is null)
        {
            return;
        }

        if (!AppSession.Settings.EnableBackgroundMusic)
        {
            return;
        }

        _reader.Position = 0;
        _output.Play();
    }

    public void Dispose()
    {
        _output?.Dispose();
        _reader?.Dispose();
    }
}
