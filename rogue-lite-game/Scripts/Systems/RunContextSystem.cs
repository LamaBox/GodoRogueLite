using Godot;
using static PlayerDataStructures;

// Сохраняет данные игрока между уровнями.
// Добавь этот узел как Autoload в Project → Project Settings → Autoloads
// чтобы он сохранялся между сценами.
public partial class RunContextSystem : Node
{
    public static RunContextSystem Instance { get; private set; }

    private float _savedCurrentHealth;
    private float _savedMaxHealth;
    private float _savedCurrentMana;
    private float _savedMaxMana;
    private int _savedScore;
    private bool _hasSavedData = false;

    public override void _Ready()
    {
        if (Instance != null && Instance != this) { QueueFree(); return; }
        Instance = this;
    }

    public void SaveRunContext()
    {
        var player = GetTree().GetFirstNodeInGroup("Player") as Node;
        if (player != null)
        {
            var data = player.GetNodeOrNull<PlayerData>("PlayerData");
            if (data != null)
            {
                _savedCurrentHealth = data.GetCurrentHealth();
                _savedMaxHealth = data.GetMaxHealth();
                _savedCurrentMana = data.GetCurrentMana();
                _savedMaxMana = data.GetMaxMana();
            }
        }
        else GD.PushWarning("RunContextSystem: Player not found on save!");

        if (ScoreCounter.Instance != null)
            _savedScore = ScoreCounter.Instance.GetScore();

        _hasSavedData = true;
        GD.Print("RunContextSystem: Run data saved.");
    }

    public void LoadRunContext()
    {
        if (!_hasSavedData) { GD.Print("RunContextSystem: No saved data (first level?)."); return; }

        var player = GetTree().GetFirstNodeInGroup("Player") as Node;
        if (player != null)
        {
            var data = player.GetNodeOrNull<PlayerData>("PlayerData");
            if (data != null)
            {
                data.ChangeValueResource(_savedMaxHealth, ResourceType.Health, ResourceValueType.Maximum);
                data.ChangeValueResource(_savedCurrentHealth, ResourceType.Health, ResourceValueType.Current);
                data.ChangeValueResource(_savedMaxMana, ResourceType.Mana, ResourceValueType.Maximum);
                data.ChangeValueResource(_savedCurrentMana, ResourceType.Mana, ResourceValueType.Current);
                data.BroadcastAllData();
            }
        }
        else GD.PushWarning("RunContextSystem: Player not found on load!");

        if (ScoreCounter.Instance != null)
            ScoreCounter.Instance.SetScore(_savedScore);

        GD.Print("RunContextSystem: Run data loaded.");
    }

    public void ResetRun()
    {
        GD.Print("RunContextSystem: Run reset.");
        QueueFree();
    }
}
