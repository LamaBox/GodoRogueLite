using Godot;
using System;

public partial class MannaBarController : ProgressBar
{
    private float _mana = 1f;
    private float _maxMana = 1f;

    public override void _Ready()
    {
        MinValue = 0;
        MaxValue = 100;
        UpdateBar();
    }

    public void SetManna(float value)
    {
        _mana = Mathf.Clamp(value, 0, _maxMana);
        UpdateBar();
    }

    public void SetMaxManna(float value)
    {
        if (value <= 0) return;
        _maxMana = value;
        _mana = Mathf.Min(_mana, _maxMana);
        UpdateBar();
    }

    public void AddManna(float value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        SetManna(_mana + value);
    }

    public void ReduceManna(float value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        SetManna(_mana - value);
    }

    public bool IsZeroMana() => _mana <= 0;

    private void UpdateBar() => Value = _maxMana > 0 ? (_mana / _maxMana) * 100f : 0;
}
