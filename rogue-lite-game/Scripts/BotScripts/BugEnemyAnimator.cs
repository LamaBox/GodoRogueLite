using Godot;

public partial class BugEnemyAnimator : Node
{
    [Export] public NodePath AnimatedSpritePath;
    [Export] public NodePath BugEnemyPath;

    private AnimatedSprite2D _sprite;
    private BugEnemy _enemy;

    private string _current = "";
    private bool _locked;
    private bool _isDead;
    private SceneTreeTimer _stunFallbackTimer;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>(AnimatedSpritePath);
        _enemy = GetNode<BugEnemy>(BugEnemyPath);

        _enemy.OnAnimationRequest += RequestAnimation;
        _enemy.OnDied += OnEnemyDied;

        _sprite.AnimationFinished += OnAnimationFinished;
        _sprite.FrameChanged += OnFrameChanged;

        Play("Idle");
    }

    public override void _Process(double delta)
    {
        // Flip sprite to face direction — no root Scale involved
        _sprite.FlipH = _enemy.IsFacingRight;
    }

    private void RequestAnimation(string animName)
    {
        if (_isDead) return;
        if (_locked && animName != "Stun") return;
        Play(animName);

        // Fallback timer in case Stun animation is accidentally set to loop
        if (animName == "Stun")
        {
            _stunFallbackTimer = GetTree().CreateTimer(1.2f);
            _stunFallbackTimer.Timeout += () =>
            {
                if (_enemy.IsFacingRight == _enemy.IsFacingRight) // still alive check
                    ForceEndStun();
            };
        }
    }

    private void ForceEndStun()
    {
        if (_isDead || _current != "Stun") return;
        _locked = false;
        _enemy.EndStun();
        Play("Idle");
    }

    private void Play(string animName)
    {
        if (_current == animName) return;
        if (_sprite.SpriteFrames == null || !_sprite.SpriteFrames.HasAnimation(animName)) return;

        _current = animName;
        _sprite.Play(animName);
        _locked = !_sprite.SpriteFrames.GetAnimationLoop(animName);
    }

    private void OnFrameChanged()
    {
        if (_current != "Attack") return;
        if (_sprite.Frame == 4)
            _enemy.PerformAttack();
    }

    private void OnAnimationFinished()
    {
        _locked = false;

        switch (_current)
        {
            case "Attack":
                _enemy.EndAttack();
                Play("Idle");
                break;
            case "Stun":
                if (!_isDead)
                {
                    _enemy.EndStun();
                    Play("Idle");
                }
                break;
        }
    }

    private void OnEnemyDied()
    {
        _isDead = true;
        _locked = false;

        _sprite.AnimationFinished -= OnAnimationFinished;
        _sprite.AnimationFinished += OnDeathAnimationFinished;

        _current = "Death";
        _sprite.Play("Death");
    }

    private void OnDeathAnimationFinished()
    {
        GetTree().CreateTimer(0.3).Timeout += () =>
        {
            if (IsInstanceValid(_enemy))
                _enemy.QueueFree();
        };
    }
}
