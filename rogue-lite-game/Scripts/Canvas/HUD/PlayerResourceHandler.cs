using Godot;
using static PlayerDataStructures;

public partial class PlayerResourceHandler : Node
{
    [Export] public NodePath PlayerDataPath;
    [Export] public NodePath HpBarPath;
    [Export] public NodePath ManaBarPath;
    [Export] public NodePath StaminaBarPath;
    [Export] public bool EnableLogging = false;

    private PlayerData _playerData;
    private HPBarController _hpBar;
    private MannaBarController _manaBar;
    private StaminaController _staminaBar;

    public override void _Ready()
    {
        _playerData = GetNodeOrNull<PlayerData>(PlayerDataPath);
        _hpBar = GetNodeOrNull<HPBarController>(HpBarPath);
        _manaBar = GetNodeOrNull<MannaBarController>(ManaBarPath);
        _staminaBar = GetNodeOrNull<StaminaController>(StaminaBarPath);

        if (_playerData == null) { GD.PushError("PlayerResourceHandler: PlayerData not found!"); return; }

        _playerData.OnResourceChanged += HandleResourceChanged;
    }

    public override void _ExitTree()
    {
        if (_playerData != null)
            _playerData.OnResourceChanged -= HandleResourceChanged;
    }

    private void HandleResourceChanged(ResourceData data)
    {
        switch (data.Type)
        {
            case ResourceType.Health:
                _hpBar?.SetHp(data.Current);
                _hpBar?.SetMaxHp(data.Max);
                break;
            case ResourceType.Mana:
                _manaBar?.SetManna(data.Current);
                _manaBar?.SetMaxManna(data.Max);
                break;
            case ResourceType.Stamina:
                _staminaBar?.SetStamina(data.Current);
                _staminaBar?.SetMaxStamina(data.Max);
                break;
        }

        if (EnableLogging) GD.Print($"Resource updated: {data.Type} = {data.Current}/{data.Max}");
    }
}
