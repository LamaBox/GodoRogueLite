using Godot;

public partial class PlayerInputHandler : Node
{
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("pause"))
        {
            if (GameMenuSystem.Instance != null)
                GameMenuSystem.Instance.TogglePauseManual();
        }
    }
}
