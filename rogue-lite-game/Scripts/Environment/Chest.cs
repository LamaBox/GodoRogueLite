using Godot;

public partial class Chest : StaticBody2D, IDamageable
{
    [Export] public bool IsGood = true;
    [Export] public int ScoreReward = 50;

    private AnimatedSprite2D _sprite;
    private CollisionShape2D _collision;
    private bool _isOpen = false;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _collision = GetNode<CollisionShape2D>("CollisionShape2D");
    }

    public void TakeDamage(float damage)
    {
        if (_isOpen) return;
        Open();
    }

    private void Open()
    {
        _isOpen = true;
        _collision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        _sprite.Play("Open");

        if (IsGood && ScoreCounter.Instance != null)
            ScoreCounter.Instance.AddScore(ScoreReward);
    }

}
