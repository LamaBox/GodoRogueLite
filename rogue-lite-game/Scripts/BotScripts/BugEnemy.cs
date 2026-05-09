using Godot;
using System;

public partial class BugEnemy : BotBase
{
    [Export] public NodePath Waypoint1Path;
    [Export] public NodePath Waypoint2Path;

    public event Action<string> OnAnimationRequest;
    public event Action OnDied;

    public bool IsFacingRight { get; private set; } = true;

    private Vector2[] _waypointPositions = Array.Empty<Vector2>();
    private int _currentWaypointIndex;

    private CharacterBody2D _targetPlayer;
    private Vector2 _lastPlayerPosition;
    private Area2D _visionArea;

    private bool _isAttacking;
    private bool _isStunned;
    private bool _isDead;
    private bool _isMovingToLastPlayerPosition;
    private int _damageHitCounter;

    private const float Gravity = 980f;
    private const float KnockbackForce = 220f;

    protected override void OnBaseReady()
    {
        var w1 = GetNodeOrNull<Marker2D>(Waypoint1Path);
        var w2 = GetNodeOrNull<Marker2D>(Waypoint2Path);
        if (w1 != null && w2 != null)
            _waypointPositions = new[] { w1.GlobalPosition, w2.GlobalPosition };

        _visionArea = GetNodeOrNull<Area2D>("VisionArea");
        if (_visionArea != null)
        {
            _visionArea.BodyEntered += OnBodyEnteredVision;
            _visionArea.BodyExited += OnBodyExitedVision;
        }

        OnDead += Death;
        OnDamagedEvent += HandleDamage;
    }

    private void OnBodyEnteredVision(Node2D body)
    {
        if (_targetPlayer != null) return;
        if (body is not CharacterBody2D player) return;
        if (player.GetNodeOrNull<PlayerData>("PlayerData") == null) return;
        _targetPlayer = player;
        _isMovingToLastPlayerPosition = false;
    }

    private void OnBodyExitedVision(Node2D body)
    {
        if (body == _targetPlayer)
            LosePlayer();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isDead) return;

        if (!IsOnFloor())
            Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * (float)delta);

        if (_isAttacking)
        {
            Velocity = new Vector2(0f, Velocity.Y);
            MoveAndSlide();
            return;
        }

        if (_isStunned)
        {
            Velocity = new Vector2(Velocity.X * 0.75f, Velocity.Y);
            MoveAndSlide();
            return;
        }

        if (_targetPlayer != null && IsInstanceValid(_targetPlayer))
        {
            if (IsPlayerInAttackRange())
            {
                Velocity = new Vector2(0f, Velocity.Y);
                TryStartAttack();
            }
            else
                ChasePlayer();
        }
        else
        {
            if (_targetPlayer != null)
                LosePlayer();
            else
                TryDetectPlayer();

            if (_isMovingToLastPlayerPosition)
                MoveToLastKnownPosition();
            else
                Patrol();
        }

        UpdateFacing();
        MoveAndSlide();
    }

    // Fallback detection: handles the case where the player was already inside
    // the VisionArea when _targetPlayer was cleared (e.g. after stun).
    private void TryDetectPlayer()
    {
        if (_visionArea == null) return;
        foreach (var body in _visionArea.GetOverlappingBodies())
        {
            if (body is not CharacterBody2D player) continue;
            if (player.GetNodeOrNull<PlayerData>("PlayerData") == null) continue;
            _targetPlayer = player;
            _isMovingToLastPlayerPosition = false;
            return;
        }
    }

    private bool IsPlayerInAttackRange()
    {
        if (_targetPlayer == null) return false;
        return Mathf.Abs(_targetPlayer.GlobalPosition.X - GlobalPosition.X) <= AttackDistance;
    }

    private void TryStartAttack()
    {
        if (_isAttacking) return;
        _isAttacking = true;
        IsFacingRight = _targetPlayer.GlobalPosition.X >= GlobalPosition.X;
        OnAnimationRequest?.Invoke("Attack");
    }

    private void ChasePlayer()
    {
        float dir = Mathf.Sign(_targetPlayer.GlobalPosition.X - GlobalPosition.X);
        Velocity = new Vector2(dir * MoveSpeed, Velocity.Y);
        OnAnimationRequest?.Invoke("Walk");
    }

    private void LosePlayer()
    {
        if (_targetPlayer == null) return;
        _lastPlayerPosition = _targetPlayer.GlobalPosition;
        _targetPlayer = null;
        _isMovingToLastPlayerPosition = true;
    }

    private void MoveToLastKnownPosition()
    {
        float diffX = _lastPlayerPosition.X - GlobalPosition.X;
        if (Mathf.Abs(diffX) <= 5f)
        {
            _isMovingToLastPlayerPosition = false;
            Velocity = new Vector2(0f, Velocity.Y);
            OnAnimationRequest?.Invoke("Idle");
            return;
        }
        Velocity = new Vector2(Mathf.Sign(diffX) * MoveSpeed, Velocity.Y);
        OnAnimationRequest?.Invoke("Walk");
    }

    private void Patrol()
    {
        if (_waypointPositions.Length == 0)
        {
            Velocity = new Vector2(0f, Velocity.Y);
            OnAnimationRequest?.Invoke("Idle");
            return;
        }

        float targetX = _waypointPositions[_currentWaypointIndex].X;
        float diffX = targetX - GlobalPosition.X;

        if (Mathf.Abs(diffX) <= 5f)
        {
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypointPositions.Length;
            return;
        }

        Velocity = new Vector2(Mathf.Sign(diffX) * MoveSpeed * 0.5f, Velocity.Y);
        OnAnimationRequest?.Invoke("Walk");
    }

    private void UpdateFacing()
    {
        if (Velocity.X > 0.1f) IsFacingRight = true;
        else if (Velocity.X < -0.1f) IsFacingRight = false;
    }

    public void PerformAttack()
    {
        if (_isDead || _targetPlayer == null || !IsInstanceValid(_targetPlayer)) return;

        var playerData = _targetPlayer.GetNodeOrNull<PlayerData>("PlayerData");
        if (playerData != null)
            DealDamageToPlayer(playerData);
    }

    public void EndAttack() => _isAttacking = false;

    public void EndStun() => _isStunned = false;

    private void HandleDamage()
    {
        if (_isDead) return;

        _damageHitCounter++;
        bool stun = false;

        if (_damageHitCounter == 2)
            stun = GD.Randf() <= 0.7f;
        else if (_damageHitCounter >= 3)
        {
            stun = GD.Randf() <= 0.3f;
            if (!stun) _damageHitCounter = 0;
        }

        if (!stun) return;

        _damageHitCounter = 0;
        _isStunned = true;
        _isAttacking = false;
        _isMovingToLastPlayerPosition = false;

        float knockDir = _targetPlayer != null
            ? Mathf.Sign(GlobalPosition.X - _targetPlayer.GlobalPosition.X)
            : (IsFacingRight ? -1f : 1f);
        Velocity = new Vector2(knockDir * KnockbackForce, -80f);

        _targetPlayer = null;
        OnAnimationRequest?.Invoke("Stun");
    }

    private void Death()
    {
        if (_isDead) return;
        _isDead = true;
        _isAttacking = false;
        _isStunned = false;
        Velocity = Vector2.Zero;
        SetCollisionLayerValue(1, false);
        OnDied?.Invoke();
    }
}
