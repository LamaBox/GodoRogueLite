using Godot;

public partial class PlayerInitializer : Node
{
    [Export] public NodePath PlayerDataPath;
    [Export] public NodePath PlayerMovementPath;
    [Export] public NodePath PlayerAttackPath;

    private PlayerData _playerData;
    private PlayerMovement _playerMovement;
    private PlayerAttack _playerAttack;

    public override void _Ready()
    {
        _playerData = GetNode<PlayerData>(PlayerDataPath);
        _playerMovement = GetNode<PlayerMovement>(PlayerMovementPath);
        _playerAttack = GetNode<PlayerAttack>(PlayerAttackPath);

        if (_playerData == null) { GD.PushError("PlayerData not found on " + Name); return; }
        if (_playerMovement == null) { GD.PushError("PlayerMovement not found on " + Name); return; }
        if (_playerAttack == null) { GD.PushError("PlayerAttack not found on " + Name); return; }

        _playerData.OnAttackModifiersChanged += _playerAttack.OnAttackModifiersChanged;
        _playerData.OnDataInitialized += OnDataInitialized;

        _playerData.BroadcastAllData();
    }

    public override void _ExitTree()
    {
        if (_playerData == null) return;
        _playerData.OnAttackModifiersChanged -= _playerAttack.OnAttackModifiersChanged;
        _playerData.OnDataInitialized -= OnDataInitialized;
    }

    private void OnDataInitialized() { }
}
