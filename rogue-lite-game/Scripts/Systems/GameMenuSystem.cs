using Godot;

// UI-ноды назначаются через инспектор как NodePath.
// Меню-ноды (menuPause, settings и т.д.) должны иметь ProcessMode = Always,
// чтобы работать во время паузы.
public partial class GameMenuSystem : Node
{
    public static GameMenuSystem Instance { get; private set; }

    [Export] public NodePath MenuPausePath;
    [Export] public NodePath SettingsPath;
    [Export] public NodePath BackgroundImagePath;
    [Export] public NodePath LosePanelPath;
    [Export] public NodePath WinPanelPath;
    [Export] public NodePath HpBarPath;
    [Export] public NodePath MnBarPath;
    [Export] public NodePath ScoreBarPath;

    private CanvasItem _menuPause;
    private CanvasItem _settings;
    private CanvasItem _backgroundImage;
    private CanvasItem _losePanel;
    private CanvasItem _winPanel;
    private CanvasItem _hpBar;
    private CanvasItem _mnBar;
    private CanvasItem _scoreBar;

    private PlayerData _playerData;
    private Node _player;
    private bool _isPaused = false;
    private bool _isGameEnded = false;

    public override void _Ready()
    {
        if (Instance != null && Instance != this) { QueueFree(); return; }
        Instance = this;

        _menuPause = GetNodeOrNull<CanvasItem>(MenuPausePath);
        _settings = GetNodeOrNull<CanvasItem>(SettingsPath);
        _backgroundImage = GetNodeOrNull<CanvasItem>(BackgroundImagePath);
        _losePanel = GetNodeOrNull<CanvasItem>(LosePanelPath);
        _winPanel = GetNodeOrNull<CanvasItem>(WinPanelPath);
        _hpBar = GetNodeOrNull<CanvasItem>(HpBarPath);
        _mnBar = GetNodeOrNull<CanvasItem>(MnBarPath);
        _scoreBar = GetNodeOrNull<CanvasItem>(ScoreBarPath);

        SetVisible(_menuPause, false);
        SetVisible(_settings, false);
        SetVisible(_backgroundImage, false);
        SetVisible(_losePanel, false);
        SetVisible(_winPanel, false);

        GetTree().Paused = false;

        // Ищем игрока в группе "Player"
        _player = GetTree().GetFirstNodeInGroup("Player") as Node;
        if (_player != null)
        {
            _playerData = _player.GetNodeOrNull<PlayerData>("PlayerData");
            if (_playerData != null) _playerData.OnDead += LoseGame;
        }
        else GD.PushWarning("GameMenuSystem: Player not found!");
    }

    public override void _ExitTree()
    {
        if (_playerData != null) _playerData.OnDead -= LoseGame;
    }

    public void TogglePauseManual()
    {
        if (_isGameEnded) return;
        if (!_isPaused) PauseGame();
        else ResumeGame();
    }

    private void PauseGame()
    {
        _isPaused = true;
        GetTree().Paused = true;
        SetVisible(_menuPause, true);
        SetVisible(_backgroundImage, true);
    }

    private void ResumeGame()
    {
        _isPaused = false;
        GetTree().Paused = false;
        SetVisible(_menuPause, false);
        SetVisible(_backgroundImage, false);
        SetVisible(_settings, false);
    }

    public void ResumeButton() => ResumeGame();

    public void ToSettings()
    {
        SetVisible(_menuPause, false);
        SetVisible(_settings, true);
    }

    public void ToMainPause()
    {
        SetVisible(_menuPause, true);
        SetVisible(_settings, false);
    }

    public void Quit()
    {
        GetTree().Paused = false;
        if (RoomManager.Instance != null) RoomManager.Instance.ResetRun();
        if (RunContextSystem.Instance != null) RunContextSystem.Instance.ResetRun();
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
    }

    private void LoseGame()
    {
        if (_isGameEnded) return;
        EndGameSequence();
        SetVisible(_losePanel, true);
    }

    public void WinGame()
    {
        if (_isGameEnded) return;
        EndGameSequence();
        SetVisible(_winPanel, true);
    }

    private void EndGameSequence()
    {
        _isGameEnded = true;
        if (_playerData != null) _playerData.OnDead -= LoseGame;

        GetTree().Paused = true;
        SetVisible(_backgroundImage, true);
        SetVisible(_hpBar, false);
        SetVisible(_mnBar, false);
        SetVisible(_scoreBar, false);

        if (_player != null) _player.ProcessMode = ProcessModeEnum.Disabled;
    }

    private void SetVisible(CanvasItem node, bool visible)
    {
        if (node != null) node.Visible = visible;
    }
}
