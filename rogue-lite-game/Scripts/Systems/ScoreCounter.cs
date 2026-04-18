using Godot;
using System;

public partial class ScoreCounter : Node
{
    public static ScoreCounter Instance { get; private set; }

    public event Action<int> OnScoreChanged;

    private int _currentScore = 0;

    public override void _Ready()
    {
        if (Instance != null && Instance != this) { QueueFree(); return; }
        Instance = this;
    }

    public void AddScore(int amount)
    {
        _currentScore += amount;
        OnScoreChanged?.Invoke(_currentScore);
        GD.Print($"Score changed: {_currentScore}");
    }

    public int GetScore() => _currentScore;

    public void SetScore(int amount)
    {
        _currentScore = amount;
        OnScoreChanged?.Invoke(_currentScore);
    }

    public void ResetScore()
    {
        _currentScore = 0;
        OnScoreChanged?.Invoke(_currentScore);
    }
}
