using Godot;

public partial class Fireball : Node2D
{
    [Export] public float Speed = 700f;
    [Export] public float Lifetime = 0.35f;
    [Export] public float ExplosionRadius = 120f;
    [Export] public float Damage = 20f;
    [Export] public uint DamageLayerMask = 9;

    public Vector2 Direction = Vector2.Right;

    private bool _exploded = false;
    private float _timer = 0f;
    private AnimatedSprite2D _sprite;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _sprite.FlipH = Direction.X > 0;
        _sprite.Play("fly");
        _sprite.AnimationFinished += OnAnimationFinished;
    }

    public override void _Process(double delta)
    {
        if (_exploded) return;
        _timer += (float)delta;
        if (_timer >= Lifetime) { Explode(); return; }
        Position += Direction * Speed * (float)delta;
        CheckImpact();
    }

    private void CheckImpact()
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = new CircleShape2D { Radius = 12f },
            Transform = new Transform2D(0, GlobalPosition),
            CollisionMask = DamageLayerMask,
            CollideWithBodies = true,
            CollideWithAreas = false
        };
        foreach (var result in spaceState.IntersectShape(query))
        {
            var node = result["collider"].As<Node>();
            if (node is IDamageable)
            {
                Explode();
                return;
            }
            if (node is IIgnitable i)
                i.Ignite();
        }
    }

    private void Explode()
    {
        if (_exploded) return;
        _exploded = true;
        SetProcess(false);
        _sprite.Play("explode");
        ApplyDamage();
    }

    private void ApplyDamage()
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = new CircleShape2D { Radius = ExplosionRadius },
            Transform = new Transform2D(0, GlobalPosition),
            CollisionMask = DamageLayerMask,
            CollideWithBodies = true,
            CollideWithAreas = false
        };
        foreach (var result in spaceState.IntersectShape(query))
        {
            var node = result["collider"].As<Node>();
            if (node is IDamageable d)
                d.TakeDamage(Damage);
            if (node is IIgnitable i)
                i.Ignite();
        }
    }

    private void OnAnimationFinished()
    {
        if (_sprite.Animation == "explode")
            QueueFree();
    }
}
