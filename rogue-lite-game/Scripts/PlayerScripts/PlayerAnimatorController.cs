using Godot;

public partial class PlayerAnimatorController : Node
{
	[Export] public NodePath AnimatedSpritePath;
	[Export] public NodePath PlayerMovementPath;
	[Export] public NodePath PlayerAttackPath;
	[Export] public NodePath CharacterBodyPath;

	private AnimatedSprite2D _sprite;
	private PlayerMovement _playerMovement;
	private PlayerAttack _playerAttack;
	private CharacterBody2D _body;
	private bool _isAttacking = false;
	private bool _isDashAnimating = false;

	public override void _Ready()
	{
		if (!string.IsNullOrEmpty(AnimatedSpritePath))
			_sprite = GetNode<AnimatedSprite2D>(AnimatedSpritePath);

		if (!string.IsNullOrEmpty(PlayerMovementPath))
			_playerMovement = GetNode<PlayerMovement>(PlayerMovementPath);

		if (!string.IsNullOrEmpty(PlayerAttackPath))
		{
			_playerAttack = GetNode<PlayerAttack>(PlayerAttackPath);
			_playerAttack.OnAttackPerformed += PlayAttackAnimation;
		}

		if (!string.IsNullOrEmpty(CharacterBodyPath))
			_body = GetNode<CharacterBody2D>(CharacterBodyPath);

		if (_sprite != null)
			_sprite.AnimationFinished += OnAnimationFinished;
	}

	public override void _ExitTree()
	{
		if (_playerAttack != null)
			_playerAttack.OnAttackPerformed -= PlayAttackAnimation;
		if (_sprite != null)
			_sprite.AnimationFinished -= OnAnimationFinished;
	}

	public override void _Process(double delta)
	{
		if (_sprite == null || _playerMovement == null) return;

		// Dash takes priority: interrupt attack, play dash animation fully
		if (_playerMovement.IsDashing && !_isDashAnimating)
		{
			_isAttacking = false;
			_isDashAnimating = true;
			Play("Dash");
			return;
		}

		if (_isDashAnimating) return;
		if (_isAttacking) return;

		UpdateAnimations();
	}

	private void PlayAttackAnimation()
	{
		if (_isDashAnimating) return;
		_isAttacking = true;
		Play("Attack");
	}

	private void OnAnimationFinished()
	{
		if (_sprite.Animation == "Attack")
			_isAttacking = false;
		else if (_sprite.Animation == "Dash")
			_isDashAnimating = false;
	}

	private void UpdateAnimations()
	{
		bool isJumping = _body != null && _body.Velocity.Y < -0.1f;
		bool isFalling = _body != null && _body.Velocity.Y > 0.1f && !_body.IsOnFloor();
		bool isWalking = Mathf.Abs(_playerMovement.Axis) > 0.1f;

		if      (isJumping)  Play("Jump");
		else if (isFalling)  Play("Fall");
		else if (isWalking)  Play("Walk");
		else                 Play("Idle");
	}

	private void Play(string animName)
	{
		if (_sprite?.SpriteFrames == null) return;
		if (_sprite.SpriteFrames.HasAnimation(animName) && _sprite.Animation != animName)
			_sprite.Play(animName);
	}
}
