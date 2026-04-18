using Godot;
using static PlayerDataStructures;

public partial class PlayerMovement : CharacterBody2D
{
	[Export] public float MoveSpeed = 500f;
	[Export] public float JumpSpeed = 2500f;
	[Export] public float DashSpeed = 2000f;
	[Export] public float DashCooldown = 1.5f;
	[Export] public float GravityScale = 9800f;

	private float _axis = 0f;
	private bool _isFacingRight = true;
	private bool _isMovementLocked = false;

	private float _dashCooldownTimer = 0f;
	private bool _isDashing = false;
	private bool _isDashPhysicsActive = false;
	private float _lockedDashDirection = 0f;
	private float _dashSafetyTimer = 0f;
	private const float MaxDashSafetyTime = 0.15f;
	private bool _autoStartDash = false;

	private float _doubleTapRightTimer = 0f;
	private float _doubleTapLeftTimer = 0f;
	private bool _waitingSecondTapRight = false;
	private bool _waitingSecondTapLeft = false;
	private const float DoubleTapWindow = 0.3f;

	public bool GetIsDashing() => _isDashing;
	public float GetAxis() => _axis;
	public bool IsMovementLocked => _isMovementLocked;
	public bool IsDashing => _isDashing;
	public float Axis => _axis;
	public bool IsFacingRight => _isFacingRight;

	public override void _Process(double delta)
	{
		float dt = (float)delta;

		_axis = Input.GetAxis("move_left", "move_right");

		if (_dashCooldownTimer > 0)
			_dashCooldownTimer -= dt;

		if (_isDashing)
		{
			_dashSafetyTimer -= dt;
			if (_dashSafetyTimer <= 0)
				StopDash();

			if (!_isDashPhysicsActive && !_autoStartDash)
			{
				_autoStartDash = true;
				StartDashPhysics();
			}
		}

		if (Input.IsActionJustPressed("jump") && IsOnFloor() && !_isDashing && !_isMovementLocked)
		{
			var vel = Velocity;
			vel.Y = -JumpSpeed;
			Velocity = vel;
		}

		if (Input.IsActionJustPressed("dash"))
			TryStartDash(_isFacingRight ? 1f : -1f);

		if (Input.IsActionJustPressed("move_right"))
		{
			if (_waitingSecondTapRight && _doubleTapRightTimer > 0f)
			{
				TryStartDash(1f);
				_waitingSecondTapRight = false;
			}
			else
			{
				_waitingSecondTapRight = true;
				_doubleTapRightTimer = DoubleTapWindow;
			}
		}
		if (_waitingSecondTapRight)
		{
			_doubleTapRightTimer -= dt;
			if (_doubleTapRightTimer <= 0f)
				_waitingSecondTapRight = false;
		}

		if (Input.IsActionJustPressed("move_left"))
		{
			if (_waitingSecondTapLeft && _doubleTapLeftTimer > 0f)
			{
				TryStartDash(-1f);
				_waitingSecondTapLeft = false;
			}
			else
			{
				_waitingSecondTapLeft = true;
				_doubleTapLeftTimer = DoubleTapWindow;
			}
		}
		if (_waitingSecondTapLeft)
		{
			_doubleTapLeftTimer -= dt;
			if (_doubleTapLeftTimer <= 0f)
				_waitingSecondTapLeft = false;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		var velocity = Velocity;

		if (_isMovementLocked && !_isDashing)
		{
			velocity.X = 0f;
			if (!IsOnFloor()) velocity.Y += GravityScale * dt;
			Velocity = velocity;
			MoveAndSlide();
			return;
		}

		if (_isDashing)
		{
			velocity = _isDashPhysicsActive
				? new Vector2(_lockedDashDirection * DashSpeed, 0f)
				: Vector2.Zero;
		}
		else
		{
			if (!IsOnFloor())
				velocity.Y += GravityScale * dt;

			velocity.X = _axis * MoveSpeed;

			if (_axis > 0 && !_isFacingRight) Flip();
			else if (_axis < 0 && _isFacingRight) Flip();
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void Flip()
	{
		_isFacingRight = !_isFacingRight;
		var scale = Scale;
		scale.X *= -1;
		Scale = scale;
	}

	private void TryStartDash(float direction)
	{
		if (!_isDashing && _dashCooldownTimer <= 0 && !_isMovementLocked)
		{
			_isDashing = true;
			_isDashPhysicsActive = false;
			_autoStartDash = false;
			_dashCooldownTimer = DashCooldown;
			_dashSafetyTimer = MaxDashSafetyTime;
			_lockedDashDirection = direction;
		}
	}

	public void StartDashPhysics()
	{
		if (!_isDashing) return;
		_isDashPhysicsActive = true;
		var vel = Velocity;
		vel.Y = 0f;
		Velocity = vel;
	}

	public void StopDash()
	{
		if (!_isDashing) return;
		_isDashing = false;
		_isDashPhysicsActive = false;
		_autoStartDash = false;
		Velocity = Vector2.Zero;
	}

	public void LockMovement()
	{
		_isMovementLocked = true;
		if (!_isDashing)
		{
			var vel = Velocity;
			vel.X = 0f;
			Velocity = vel;
		}
	}

	public void UnlockMovement() => _isMovementLocked = false;

	public void OnMovementModifiersChanged(MovementModifiersData data)
	{
		MoveSpeed = data.PlayerSpeed;
		JumpSpeed = data.JumpHeight;
		DashSpeed = data.DashSpeed;
		DashCooldown = data.DashCooldown;
		GravityScale = data.GravityScale;
	}
}
