using Godot;

public partial class ScoreCounterUI : Label
{
    public override void _Ready()
    {
        if (ScoreCounter.Instance != null)
        {
            ScoreCounter.Instance.OnScoreChanged += UpdateScore;
            UpdateScore(ScoreCounter.Instance.GetScore());
        }
    }

    public override void _ExitTree()
    {
        if (ScoreCounter.Instance != null)
            ScoreCounter.Instance.OnScoreChanged -= UpdateScore;
    }

    private void UpdateScore(int score) => Text = score.ToString();
}
