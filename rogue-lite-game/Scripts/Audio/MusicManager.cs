using Godot;
using System.Collections.Generic;

/// <summary>
/// Global looping background-music player (registered as an autoload singleton).
/// Switches tracks with a short volume crossfade. The game currently launches
/// straight into the cave level, so the level track starts automatically;
/// call <see cref="Play"/> from a menu/other scene to switch.
/// </summary>
public partial class MusicManager : Node
{
    public static MusicManager Instance { get; private set; }

    private const string MusicBus = "Music";
    private const float FadeDuration = 1.0f;
    private const float TargetVolumeDb = 0f;
    private const float SilentVolumeDb = -40f;

    private readonly Dictionary<Track, AudioStream> _tracks = new();

    private AudioStreamPlayer _player;
    private Track? _current;
    private Tween _fade;

    public override void _Ready()
    {
        if (Instance != null && Instance != this) { QueueFree(); return; }
        Instance = this;

        Load(Track.Level, "res://Audio/Music/Музыка уровня - Пещера.mp3");
        Load(Track.Menu,  "res://Audio/Music/Музыка главного меню.mp3");
        Load(Track.Boss,  "res://Audio/Music/Босс.mp3");

        _player = new AudioStreamPlayer { Bus = MusicBus, VolumeDb = TargetVolumeDb };
        AddChild(_player);

        // Single playable scene is the cave level — start its theme.
        Play(Track.Level);
    }

    private void Load(Track track, string path)
    {
        var stream = GD.Load<AudioStream>(path);
        if (stream == null) { GD.PushWarning($"MusicManager: missing music at {path}"); return; }
        // Loop background music regardless of the source format's import settings.
        if (stream is AudioStreamMP3 mp3) mp3.Loop = true;
        else if (stream is AudioStreamWav wav) wav.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
        _tracks[track] = stream;
    }

    public void Play(Track track)
    {
        if (_current == track && _player.Playing) return;
        if (!_tracks.TryGetValue(track, out var stream)) return;

        _current = track;
        _fade?.Kill();

        if (!_player.Playing)
        {
            // Nothing playing yet: just fade in.
            _player.Stream = stream;
            _player.VolumeDb = SilentVolumeDb;
            _player.Play();
            _fade = CreateTween();
            _fade.TweenProperty(_player, "volume_db", TargetVolumeDb, FadeDuration);
            return;
        }

        // Fade the current track out, swap, then fade the new one in.
        _fade = CreateTween();
        _fade.TweenProperty(_player, "volume_db", SilentVolumeDb, FadeDuration * 0.5f);
        _fade.TweenCallback(Callable.From(() =>
        {
            _player.Stream = stream;
            _player.Play();
        }));
        _fade.TweenProperty(_player, "volume_db", TargetVolumeDb, FadeDuration * 0.5f);
    }

    public void Stop()
    {
        _fade?.Kill();
        _current = null;
        _fade = CreateTween();
        _fade.TweenProperty(_player, "volume_db", SilentVolumeDb, FadeDuration);
        _fade.TweenCallback(Callable.From(() => _player.Stop()));
    }

    public enum Track
    {
        Level,
        Menu,
        Boss
    }
}
