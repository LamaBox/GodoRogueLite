using Godot;
using System;
using static PlayerDataStructures;

public partial class PlayerAttack : Node2D
{
    [Export] public NodePath PlayerMovementPath;
    [Export] public NodePath AttackPointPath;
    [Export] public PackedScene FireballScene;

    [Export] public float Damage = 20f;
    [Export] public float AttackSpeed = 8f;
    [Export] public float AttackRange = 50f;
    [Export] public uint DamageLayerMask = 9;

    public event Action OnAttackPerformed;

    private PlayerMovement _playerMovement;
    private Node2D _attackPoint;
    private float _attackTimer = 0f;
    private float _fireballTimer = 0f;
    private bool _canAttack = true;
    private bool _canFireball = true;
    private bool _isAttackLocked = false;

    public bool CanAttack => _canAttack;

    public override void _Ready()
    {
        if (!string.IsNullOrEmpty(PlayerMovementPath))
            _playerMovement = GetNode<PlayerMovement>(PlayerMovementPath);

        if (!string.IsNullOrEmpty(AttackPointPath))
            _attackPoint = GetNode<Node2D>(AttackPointPath);
    }

    public override void _Process(double delta)
    {
        if (!_canAttack)
        {
            _attackTimer -= (float)delta;
            if (_attackTimer <= 0f) _canAttack = true;
        }

        if (!_canFireball)
        {
            _fireballTimer -= (float)delta;
            if (_fireballTimer <= 0f) _canFireball = true;
        }

        bool isDashing = _playerMovement != null && _playerMovement.IsDashing;

        if (Input.IsActionJustPressed("attack") && _canAttack && !_isAttackLocked && !isDashing)
            PerformMeleeAttack();

        if (Input.IsActionJustPressed("fireball") && _canFireball && !_isAttackLocked && !isDashing)
            SpawnFireball();
    }

    public void LockAttack() => _isAttackLocked = true;
    public void UnlockAttack() => _isAttackLocked = false;

    private void PerformMeleeAttack()
    {
        _canAttack = false;
        _attackTimer = 1f / AttackSpeed;
        OnAttackPerformed?.Invoke();

        var origin = _attackPoint != null ? _attackPoint.GlobalPosition : GlobalPosition;
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = new CircleShape2D { Radius = AttackRange },
            Transform = new Transform2D(0, origin),
            CollisionMask = DamageLayerMask,
            CollideWithBodies = true,
            CollideWithAreas = false
        };
        foreach (var result in spaceState.IntersectShape(query))
        {
            var node = result["collider"].As<Node>();
            if (node is IDamageable d)
                d.TakeDamage(Damage);
            if (node is IExtinguishable e)
                e.Extinguish();
        }
    }

    private void SpawnFireball()
    {
        if (FireballScene == null) return;

        _canFireball = false;
        _fireballTimer = 1f / AttackSpeed;

        bool facingRight = _playerMovement == null || _playerMovement.IsFacingRight;
        float dirX = facingRight ? 1f : -1f;
        var origin = _attackPoint != null ? _attackPoint.GlobalPosition : GlobalPosition;

        var fireball = FireballScene.Instantiate<Fireball>();
        fireball.Direction = new Vector2(dirX, 0);
        fireball.Damage = Damage;
        fireball.GlobalPosition = origin;
        GetTree().CurrentScene.AddChild(fireball);
    }

    public void OnAttackModifiersChanged(AttackModifiersData data)
    {
        Damage = data.Damage;
        AttackSpeed = data.AttackSpeed;
        AttackRange = data.AttackRange;
    }
}
