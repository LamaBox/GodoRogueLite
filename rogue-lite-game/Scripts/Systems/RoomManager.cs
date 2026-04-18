using Godot;

// Заглушка — полная реализация будет позже
public partial class RoomManager : Node
{
    public static RoomManager Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance != null && Instance != this) { QueueFree(); return; }
        Instance = this;
    }

    public void ResetRun()
    {
        GD.Print("RoomManager: ResetRun called");
    }
}
