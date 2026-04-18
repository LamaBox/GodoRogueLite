using Godot;
using System;

// Прикрепи к ProgressBar ноде. Value от 0 до 100.
public partial class HPBarController : ProgressBar
{
    private float _hp = 1f;
    private float _maxHp = 1f;

    public override void _Ready()
    {
        MinValue = 0;
        MaxValue = 100;
        UpdateBar();
    }

    public void SetHp(float value)
    {
        _hp = Mathf.Clamp(value, 0, _maxHp);
        UpdateBar();
    }

    public void SetMaxHp(float value)
    {
        if (value <= 0) return;
        _maxHp = value;
        _hp = Mathf.Min(_hp, _maxHp);
        UpdateBar();
    }

    public void AddHp(float value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        SetHp(_hp + value);
    }

    public void ReduceHp(float value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        SetHp(_hp - value);
    }

    public bool IsZeroHp() => _hp <= 0;

    private void UpdateBar() => Value = _maxHp > 0 ? (_hp / _maxHp) * 100f : 0;
}
