using Godot;
using static PlayerDataStructures;

// Упрощённый вариант: прямо использует ProgressBar ноды для HP и Mana.
// Подключается к PlayerData через группу "Player".
public partial class BarsController : Node
{
    [Export] public NodePath HealthBarPath;
    [Export] public NodePath ManaBarPath;

    private ProgressBar _healthBar;
    private ProgressBar _manaBar;
    private PlayerData _playerData;

    public override void _Ready()
    {
        _healthBar = GetNodeOrNull<ProgressBar>(HealthBarPath);
        _manaBar = GetNodeOrNull<ProgressBar>(ManaBarPath);

        var playerNode = GetTree().GetFirstNodeInGroup("Player") as Node;
        if (playerNode == null) { GD.PushError("BarsController: Player not found!"); return; }

        _playerData = playerNode.GetNodeOrNull<PlayerData>("PlayerData");
        if (_playerData == null) { GD.PushError("BarsController: PlayerData not found!"); return; }

        _playerData.OnResourceChanged += UpdateBar;

        UpdateBar(new ResourceData(_playerData.GetCurrentHealth(), _playerData.GetMaxHealth(), ResourceType.Health));
        UpdateBar(new ResourceData(_playerData.GetCurrentMana(), _playerData.GetMaxMana(), ResourceType.Mana));
    }

    public override void _ExitTree()
    {
        if (_playerData != null)
            _playerData.OnResourceChanged -= UpdateBar;
    }

    private void UpdateBar(ResourceData data)
    {
        float fill = data.Max > 0 ? (data.Current / data.Max) * 100f : 0;
        switch (data.Type)
        {
            case ResourceType.Health:
                if (_healthBar != null) _healthBar.Value = fill;
                break;
            case ResourceType.Mana:
                if (_manaBar != null) _manaBar.Value = fill;
                break;
        }
    }
}
