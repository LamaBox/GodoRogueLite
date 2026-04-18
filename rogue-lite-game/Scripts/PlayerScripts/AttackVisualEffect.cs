using Godot;

public partial class AttackVisualEffect : Node2D
{
    [Export] public NodePath PlayerAttackPath;

    private PlayerAttack _playerAttack;
    private Polygon2D _slash;

    public override void _Ready()
    {
        _slash = GetNode<Polygon2D>("SlashShape");
        _slash.Modulate = new Color(1f, 0.65f, 0f, 0f);

        if (!string.IsNullOrEmpty(PlayerAttackPath))
        {
            _playerAttack = GetNode<PlayerAttack>(PlayerAttackPath);
            _playerAttack.OnAttackPerformed += Play;
        }
    }

    public override void _ExitTree()
    {
        if (_playerAttack != null)
            _playerAttack.OnAttackPerformed -= Play;
    }

    private void Play()
    {
        _slash.Scale = new Vector2(0.4f, 0.4f);
        _slash.Modulate = new Color(1f, 0.65f, 0f, 1f);

        var tween = CreateTween().SetParallel(true);
        tween.TweenProperty(_slash, "scale", new Vector2(1.3f, 1.3f), 0.12f);
        tween.TweenProperty(_slash, "modulate:a", 0.0f, 0.22f);
    }
}
