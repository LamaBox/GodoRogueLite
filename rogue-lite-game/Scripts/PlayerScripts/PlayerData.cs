using Godot;
using System;
using static PlayerDataStructures;

public partial class PlayerData : Node
{
    // События
    public event Action<ResourceData> OnResourceChanged;
    public event Action<MovementModifiersData> OnMovementModifiersChanged;
    public event Action OnDataInitialized;
    public event Action<AttackModifiersData> OnAttackModifiersChanged;
    public event Action OnDead;

    [Export] public float MaxHealth = 100f;
    [Export] public float MaxMana = 100f;
    [Export] public float MaxStamina = 100f;

    [Export] public float BaseMoveSpeed = 500f;
    [Export] public float BaseSprintMultiplier = 1.5f;
    [Export] public float BaseJumpHeight = 1500f;
    [Export] public float BaseDashSpeed = 2000f;
    [Export] public float BaseDashCooldown = 1.5f;
    [Export] public float BaseGravityScale = 3500f;

    [Export] public float BaseDamage = 10f;
    [Export] public float BaseAttackSpeed = 1f;
    [Export] public float BaseAttackRange = 1.5f;

    [Export] public float HealHealthPerSec = 1f;
    [Export] public float HealManaPerSec = 0f;

    [Export] public bool DebugMode = false;

    private float _currentHealth;
    private float _currentMana;
    private float _currentStamina;
    private float _regenTimer = 0f;

    public override void _Ready()
    {
        _currentHealth = MaxHealth;
        _currentMana = MaxMana;
        _currentStamina = MaxStamina;

        if (DebugMode) GD.Print("PlayerData initialized");
    }

    public override void _Process(double delta)
    {
        _regenTimer += (float)delta;
        if (_regenTimer >= 1f)
        {
            _regenTimer = 0f;

            if (_currentHealth < MaxHealth && HealHealthPerSec > 0 && _currentHealth > 0)
            {
                _currentHealth = Mathf.Clamp(_currentHealth + HealHealthPerSec, 0, MaxHealth);
                BroadcastResourceChange(_currentHealth, MaxHealth, ResourceType.Health);
            }

            if (_currentMana < MaxMana && HealManaPerSec > 0)
            {
                _currentMana = Mathf.Clamp(_currentMana + HealManaPerSec, 0, MaxMana);
                BroadcastResourceChange(_currentMana, MaxMana, ResourceType.Mana);
            }
        }
    }

    public void BroadcastAllData()
    {
        BroadcastResourceChange(_currentHealth, MaxHealth, ResourceType.Health);
        BroadcastResourceChange(_currentMana, MaxMana, ResourceType.Mana);
        BroadcastResourceChange(_currentStamina, MaxStamina, ResourceType.Stamina);
        OnMovementModifiersChanged?.Invoke(GetCurrentMovementModifiers());
        OnAttackModifiersChanged?.Invoke(GetCurrentAttackModifiers());
        OnDataInitialized?.Invoke();
    }

    public void ChangeValueResource(float value, ResourceType type, ResourceValueType valueType, bool isAddition = false)
    {
        switch (type)
        {
            case ResourceType.Health:
                if (valueType == ResourceValueType.Maximum)
                    MaxHealth = isAddition ? MaxHealth + value : value;
                else
                {
                    _currentHealth = Mathf.Clamp(isAddition ? _currentHealth + value : value, 0, MaxHealth);
                }
                BroadcastResourceChange(_currentHealth, MaxHealth, ResourceType.Health);
                if (_currentHealth == 0) OnDead?.Invoke();
                break;

            case ResourceType.Mana:
                if (valueType == ResourceValueType.Maximum)
                    MaxMana = isAddition ? MaxMana + value : value;
                else
                    _currentMana = Mathf.Clamp(isAddition ? _currentMana + value : value, 0, MaxMana);
                BroadcastResourceChange(_currentMana, MaxMana, ResourceType.Mana);
                break;

            case ResourceType.Stamina:
                if (valueType == ResourceValueType.Maximum)
                    MaxStamina = isAddition ? MaxStamina + value : value;
                else
                    _currentStamina = Mathf.Clamp(isAddition ? _currentStamina + value : value, 0, MaxStamina);
                BroadcastResourceChange(_currentStamina, MaxStamina, ResourceType.Stamina);
                break;
        }
    }

    public float GetCurrentHealth() => _currentHealth;
    public float GetMaxHealth() => MaxHealth;
    public float GetCurrentMana() => _currentMana;
    public float GetMaxMana() => MaxMana;
    public float GetCurrentStamina() => _currentStamina;
    public float GetMaxStamina() => MaxStamina;

    private MovementModifiersData GetCurrentMovementModifiers() =>
        new MovementModifiersData(BaseMoveSpeed, BaseSprintMultiplier, BaseJumpHeight,
                                  BaseDashSpeed, BaseDashCooldown, BaseGravityScale);

    private AttackModifiersData GetCurrentAttackModifiers() =>
        new AttackModifiersData(BaseDamage, BaseAttackSpeed, BaseAttackRange);

    private void BroadcastResourceChange(float current, float max, ResourceType type) =>
        OnResourceChanged?.Invoke(new ResourceData(current, max, type));
}
