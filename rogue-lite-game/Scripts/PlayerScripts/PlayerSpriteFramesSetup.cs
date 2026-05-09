using Godot;

[Tool]
public partial class PlayerSpriteFramesSetup : Node
{
	[Export] public NodePath AnimatedSpritePath;
	[Export] public Texture2D SpriteSheet;

	// Размер кадра: 544/8=68, 432/6=72
	private const int W = 68;
	private const int H = 72;

	public override void _Ready()
	{
		Setup();
	}

	// В редакторе пересобирать при смене SpriteSheet
	public override void _ValidateProperty(Godot.Collections.Dictionary property)
	{
		if (Engine.IsEditorHint() && (string)property["name"] == "SpriteSheet")
			Setup();
	}

	private void Setup()
	{
		if (SpriteSheet == null) return;
		if (string.IsNullOrEmpty(AnimatedSpritePath)) return;

		var sprite = GetNodeOrNull<AnimatedSprite2D>(AnimatedSpritePath);
		if (sprite == null) return;

		var frames = new SpriteFrames();
		frames.RemoveAnimation("default");

		//           name       row  count  loop   fps
		AddAnim(frames, "Idle",    1,  1,    true,   1f);
		AddAnim(frames, "Walk",    0,  8,    true,   9f);
		AddAnim(frames, "Dash",    2,  8,    false, 14f);
		AddAnim(frames, "Attack",  3,  4,    false, 10f);
		AddAnim(frames, "Jump",    4,  4,    false, 10f);
		AddAnim(frames, "Fall",    5,  5,    true,   8f);

		sprite.SpriteFrames = frames;
		sprite.Play("Idle");
	}

	private void AddAnim(SpriteFrames frames, string name, int row, int count, bool loop, float fps)
	{
		frames.AddAnimation(name);
		frames.SetAnimationSpeed(name, fps);
		frames.SetAnimationLoop(name, loop);

		for (int i = 0; i < count; i++)
		{
			var atlas = new AtlasTexture
			{
				Atlas = SpriteSheet,
				Region = new Rect2(i * W, row * H, W, H),
				FilterClip = true
			};
			frames.AddFrame(name, atlas);
		}
	}
}
