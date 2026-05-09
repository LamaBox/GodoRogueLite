using Godot;

[Tool]
public partial class BugEnemySpriteSetup : Node
{
    [Export] public NodePath AnimatedSpritePath;

    [Export] public Texture2D IdleTexture;     // bossID
    [Export] public Texture2D WalkTexture;     // bossWK
    [Export] public Texture2D RunTexture;      // bossRN
    [Export] public Texture2D Attack1Texture;  // bossAT1
    [Export] public Texture2D Attack2Texture;  // bossAT2
    [Export] public Texture2D HurtTexture;     // bossDT

    // Number of frames per animation strip — adjust to match actual sprites
    [Export] public int IdleFrameCount    = 5;
    [Export] public int WalkFrameCount    = 5;
    [Export] public int RunFrameCount     = 5;
    [Export] public int Attack1FrameCount = 5;
    [Export] public int Attack2FrameCount = 5;
    [Export] public int HurtFrameCount    = 5;

    private const int FrameHeight = 208;

    public override void _Ready() => Setup();

    public override void _ValidateProperty(Godot.Collections.Dictionary property)
    {
        if (!Engine.IsEditorHint()) return;
        string name = (string)property["name"];
        if (name.EndsWith("Texture") || name.EndsWith("FrameCount"))
            Setup();
    }

    private void Setup()
    {
        var sprite = GetNodeOrNull<AnimatedSprite2D>(AnimatedSpritePath);
        if (sprite == null) return;

        var frames = new SpriteFrames();
        frames.RemoveAnimation("default");

        AddAnim(frames, "Idle",    IdleTexture,    IdleFrameCount,    true,  4f);
        AddAnim(frames, "Walk",    WalkTexture,    WalkFrameCount,    true,  8f);
        AddAnim(frames, "Run",     RunTexture,     RunFrameCount,     true,  10f);
        AddAnim(frames, "Attack1", Attack1Texture, Attack1FrameCount, false, 10f);
        AddAnim(frames, "Attack2", Attack2Texture, Attack2FrameCount, false, 10f);
        AddAnim(frames, "Hurt",    HurtTexture,    HurtFrameCount,    false, 8f);

        sprite.SpriteFrames = frames;

        if (!Engine.IsEditorHint())
            sprite.Play("Idle");
    }

    private void AddAnim(SpriteFrames frames, string name, Texture2D texture, int count, bool loop, float fps)
    {
        if (texture == null || count <= 0) return;

        frames.AddAnimation(name);
        frames.SetAnimationSpeed(name, fps);
        frames.SetAnimationLoop(name, loop);

        int frameWidth = texture.GetWidth() / count;

        for (int i = 0; i < count; i++)
        {
            var atlas = new AtlasTexture
            {
                Atlas = texture,
                Region = new Rect2(i * frameWidth, 0, frameWidth, FrameHeight),
                FilterClip = true
            };
            frames.AddFrame(name, atlas);
        }
    }
}
