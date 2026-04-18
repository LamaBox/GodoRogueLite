using Godot;

public partial class FollowCamera : Camera2D
{
	[Export] public NodePath TargetPath;
	[Export] public float SmoothTime = 0.15f;
	[Export] public bool UseSmooth = true;
	[Export] public bool UseDeadZone = true;
	[Export] public Vector2 DeadZoneSize = new Vector2(300f, 200f);

	private Node2D _target;
	private Vector2 _targetPosition;
	private Vector2 _velocity = Vector2.Zero;

	public override void _Ready()
	{
		if (TargetPath != null && !TargetPath.IsEmpty)
		{
			_target = GetNode<Node2D>(TargetPath);
			GlobalPosition = _target.GlobalPosition;
			_targetPosition = GlobalPosition;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_target == null)
		{
			_target = GetTree().GetFirstNodeInGroup("Player") as Node2D;
			if (_target == null) return;
			GlobalPosition = _target.GlobalPosition;
			_targetPosition = GlobalPosition;
			return;
		}

		if (UseDeadZone)
			HandleDeadZone();
		else
			_targetPosition = _target.GlobalPosition;

		if (UseSmooth)
			GlobalPosition = SmoothDamp(GlobalPosition, _targetPosition, ref _velocity, SmoothTime, (float)delta);
		else
			GlobalPosition = _targetPosition;
	}

	private void HandleDeadZone()
	{
		Vector2 targetPos = _target.GlobalPosition;

		float halfW = DeadZoneSize.X * 0.5f;
		float halfH = DeadZoneSize.Y * 0.5f;

		if (targetPos.X < _targetPosition.X - halfW)
			_targetPosition.X = targetPos.X + halfW;
		else if (targetPos.X > _targetPosition.X + halfW)
			_targetPosition.X = targetPos.X - halfW;

		if (targetPos.Y < _targetPosition.Y - halfH)
			_targetPosition.Y = targetPos.Y + halfH;
		else if (targetPos.Y > _targetPosition.Y + halfH)
			_targetPosition.Y = targetPos.Y - halfH;
	}

	// Аналог Unity Vector3.SmoothDamp
	private static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime, float deltaTime)
	{
		smoothTime = Mathf.Max(0.0001f, smoothTime);
		float omega = 2f / smoothTime;
		float x = omega * deltaTime;
		float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

		float changeX = current.X - target.X;
		float changeY = current.Y - target.Y;

		float tempX = (currentVelocity.X + omega * changeX) * deltaTime;
		float tempY = (currentVelocity.Y + omega * changeY) * deltaTime;

		currentVelocity.X = (currentVelocity.X - omega * tempX) * exp;
		currentVelocity.Y = (currentVelocity.Y - omega * tempY) * exp;

		return new Vector2(
			target.X + (changeX + tempX) * exp,
			target.Y + (changeY + tempY) * exp
		);
	}
}
