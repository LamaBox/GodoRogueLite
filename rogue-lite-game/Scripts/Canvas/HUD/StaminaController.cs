using Godot;
using System;

public partial class StaminaController : ProgressBar
{
    private float _stamina = 1f;
    private float _maxStamina = 1f;

    public override void _Ready()
    {
        MinValue = 0;
        MaxValue = 100;
        UpdateBar();
    }

    public void SetStamina(float value)
    {
        _stamina = Mathf.Clamp(value, 0, _maxStamina);
        UpdateBar();
    }

    public void SetMaxStamina(float value)
    {
        if (value <= 0) return;
        _maxStamina = value;
        _stamina = Mathf.Min(_stamina, _maxStamina);
        UpdateBar();
    }

    public void AddStamina(float value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        SetStamina(_stamina + value);
    }

    public void ReduceStamina(float value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        SetStamina(_stamina - value);
    }

    public bool IsZeroStamina() => _stamina <= 0;

    private void UpdateBar() => Value = _maxStamina > 0 ? (_stamina / _maxStamina) * 100f : 0;
}
