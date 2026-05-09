using Godot;
using System;

public abstract partial class BotBase : CharacterBody2D, IDamageable
{
    public event Action<float, float, float> OnHealthChanged;
    public event Action OnDead;
    public event Action OnDamagedEvent;

    [Export] public float MaxHealth = 100f;
    [Export] public float Damage = 10f;
    [Export] public float MoveSpeed = 120f;
    [Export] public float AttackSpeed = 1f;
    [Export] public float AttackDistance = 60f;

    protected float CurrentHealth;

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        OnBaseReady();
    }

    protected virtual void OnBaseReady() { }

    public virtual void TakeDamage(float damageInp)
    {
        if (damageInp < 0)
        {
            GD.PrintErr($"{Name}: damage cannot be negative ({damageInp})");
            return;
        }

        CurrentHealth = Mathf.Clamp(CurrentHealth - damageInp, 0f, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth, CurrentHealth / MaxHealth);
        OnDamagedEvent?.Invoke();

        if (CurrentHealth == 0)
            OnDead?.Invoke();
    }

    public void DealDamageToPlayer(PlayerData playerData)
    {
        playerData.ChangeValueResource(
            -Damage,
            PlayerDataStructures.ResourceType.Health,
            PlayerDataStructures.ResourceValueType.Current,
            true);
    }

    public float GetCurrentHealth() => CurrentHealth;
    public float GetMaxHealth() => MaxHealth;
}
