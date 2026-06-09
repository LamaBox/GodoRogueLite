using Godot;
using System.Collections.Generic;

/// <summary>
/// Global sound-effect player (registered as an autoload singleton).
/// Streams are loaded once at startup; call sites trigger them by <see cref="SfxId"/>.
/// Use <see cref="Play"/> for screen-centred effects (player/UI) and
/// <see cref="PlayAt"/> for world effects that should pan/attenuate by position.
/// </summary>
public partial class SfxManager : Node
{
    public static SfxManager Instance { get; private set; }

    private const string SfxBus = "SFX";

    private readonly Dictionary<SfxId, AudioStream> _streams = new();

    public override void _Ready()
    {
        if (Instance != null && Instance != this) { QueueFree(); return; }
        Instance = this;

        Load(SfxId.PlayerJump,   "res://Audio/Sound/Player/Прыжок.mp3");
        Load(SfxId.PlayerDash,    "res://Audio/Sound/Player/Дэш.mp3");
        Load(SfxId.PlayerMelee,   "res://Audio/Sound/Player/Удар рукой ГГ.mp3");
        Load(SfxId.FireballCast,  "res://Audio/Sound/Player/Звук каста заклинания.mp3");
        Load(SfxId.FireballFly,   "res://Audio/Sound/Player/Music/Полет фаерболла.mp3");

        Load(SfxId.BugStep1,      "res://Audio/Sound/1 шаг жука-моба.mp3");
        Load(SfxId.BugStep2,      "res://Audio/Sound/2 шаг жука-моба.mp3");
        Load(SfxId.BugSwing,      "res://Audio/Sound/Звук замаха жука (1).mp3");
        Load(SfxId.BugHit,        "res://Audio/Sound/МелкийУдар.mp3");
        Load(SfxId.BugDamage,     "res://Audio/Sound/Урон по жуку.mp3");

        Load(SfxId.BarrelBreak,   "res://Audio/Sound/Поломка бочки.mp3");
    }

    private void Load(SfxId id, string path)
    {
        var stream = GD.Load<AudioStream>(path);
        if (stream != null) _streams[id] = stream;
        else GD.PushWarning($"SfxManager: missing audio at {path}");
    }

    /// <summary>Play a non-positional one-shot (heard at full volume everywhere).</summary>
    public void Play(SfxId id, float volumeDb = 0f, float pitchScale = 1f)
    {
        if (!_streams.TryGetValue(id, out var stream)) return;

        var player = new AudioStreamPlayer
        {
            Stream = stream,
            Bus = SfxBus,
            VolumeDb = volumeDb,
            PitchScale = pitchScale
        };
        AddChild(player);
        player.Finished += player.QueueFree;
        player.Play();
    }

    /// <summary>Play a one-shot at a world position with stereo panning/attenuation.</summary>
    public void PlayAt(SfxId id, Vector2 globalPosition, float volumeDb = 0f, float pitchScale = 1f)
    {
        if (!_streams.TryGetValue(id, out var stream)) return;

        var player = new AudioStreamPlayer2D
        {
            Stream = stream,
            Bus = SfxBus,
            VolumeDb = volumeDb,
            PitchScale = pitchScale,
            GlobalPosition = globalPosition,
            MaxDistance = 4000f,
            Attenuation = 0.5f
        };
        AddChild(player);
        player.Finished += player.QueueFree;
        player.Play();
    }
}

public enum SfxId
{
    PlayerJump,
    PlayerDash,
    PlayerMelee,
    FireballCast,
    FireballFly,
    BugStep1,
    BugStep2,
    BugSwing,
    BugHit,
    BugDamage,
    BarrelBreak
}
