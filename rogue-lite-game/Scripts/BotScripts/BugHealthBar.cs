using Godot;

public partial class BugHealthBar : Node2D
{
    [Export] public NodePath BugEnemyPath;
    [Export] public float Width = 50f;
    [Export] public float Height = 6f;

    private BugEnemy _enemy;
    private float _healthPercent = 1f;

    public override void _Ready()
    {
        _enemy = GetNode<BugEnemy>(BugEnemyPath);
        _enemy.OnHealthChanged += OnHealthChanged;
        _enemy.OnDied += OnDied;
    }

    private void OnHealthChanged(float current, float max, float percent)
    {
        _healthPercent = percent;
        QueueRedraw();
    }

    private void OnDied() => Visible = false;

    public override void _Draw()
    {
        // Background
        DrawRect(new Rect2(-Width / 2f, -Height / 2f, Width, Height), new Color(0.15f, 0.15f, 0.15f));
        // Health fill
        float fillWidth = Width * _healthPercent;
        var fillColor = _healthPercent > 0.5f ? new Color(0.2f, 0.85f, 0.2f) : new Color(0.9f, 0.15f, 0.15f);
        DrawRect(new Rect2(-Width / 2f, -Height / 2f, fillWidth, Height), fillColor);
    }
}
