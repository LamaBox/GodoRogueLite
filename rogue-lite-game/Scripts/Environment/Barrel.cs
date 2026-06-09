using Godot;

public partial class Barrel : StaticBody2D, IDamageable
{
    private AnimatedSprite2D _sprite;
    private CollisionShape2D _collision;
    private bool _isBroken = false;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _collision = GetNode<CollisionShape2D>("CollisionShape2D");
        _sprite.AnimationFinished += OnAnimationFinished;
    }

    public void TakeDamage(float damage)
    {
        if (_isBroken) return;
        Break();
    }

    private void Break()
    {
        _isBroken = true;
        _collision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        _sprite.Play("Break");
        SfxManager.Instance?.PlayAt(SfxId.BarrelBreak, GlobalPosition);
    }

    private void OnAnimationFinished()
    {
        if (_sprite.Animation == "Break")
            QueueFree();
    }
}
