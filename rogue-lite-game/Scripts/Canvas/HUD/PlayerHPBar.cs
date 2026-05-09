using Godot;
using static PlayerDataStructures;

public partial class PlayerHPBar : Node2D
{
	[Export] public NodePath PlayerDataPath;

	[Export] public float Width = 200f;
	[Export] public float Height = 18f;
	[Export] public float CornerRadius = 3f;

	private float _percent = 1f;

	public override void _Ready()
	{
		if (string.IsNullOrEmpty(PlayerDataPath))
		{
			GD.PushError("PlayerHPBar: PlayerDataPath is not set in the inspector");
			return;
		}

		var playerData = GetNodeOrNull<PlayerData>(PlayerDataPath);
		if (playerData == null)
		{
			GD.PushError($"PlayerHPBar: PlayerData not found at path '{PlayerDataPath}'");
			return;
		}

		playerData.OnResourceChanged += OnResourceChanged;

		_percent = playerData.GetMaxHealth() > 0
			? playerData.GetCurrentHealth() / playerData.GetMaxHealth()
			: 1f;
	}

	private void OnResourceChanged(ResourceData data)
	{
		if (data.Type != ResourceType.Health) return;
		_percent = data.Max > 0 ? data.Current / data.Max : 0f;
		QueueRedraw();
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(0, 0, Width, Height), new Color(0.1f, 0.1f, 0.1f, 0.85f));
		float fillW = Width * Mathf.Clamp(_percent, 0f, 1f);
		if (fillW > 0f)
		{
			var color = _percent > 0.5f
				? new Color(0.15f, 0.8f, 0.15f)
				: _percent > 0.25f
					? new Color(0.9f, 0.6f, 0.1f)
					: new Color(0.9f, 0.1f, 0.1f);
			DrawRect(new Rect2(0, 0, fillW, Height), color);
		}
		DrawRect(new Rect2(0, 0, Width, Height), new Color(0f, 0f, 0f, 0.6f), false, 1.5f);
		DrawString(ThemeDB.FallbackFont, new Vector2(4, Height - 4), "HP", HorizontalAlignment.Left, -1, 11, new Color(1, 1, 1, 0.9f));
	}
}
