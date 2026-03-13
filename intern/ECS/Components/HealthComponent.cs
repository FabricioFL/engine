namespace Engine.ECS.Components;

public struct HealthComponent : IComponent
{
    public float CurrentHealth;
    public float MaxHealth;
    public float DamageCooldown;     // Time remaining before can take damage again
    public float CooldownDuration;   // How long the cooldown lasts

    public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;
    public bool IsAlive => CurrentHealth > 0;
    public bool CanTakeDamage => DamageCooldown <= 0;

    public static HealthComponent Create(float maxHealth, float cooldownDuration = 0.5f) => new()
    {
        CurrentHealth = maxHealth,
        MaxHealth = maxHealth,
        DamageCooldown = 0,
        CooldownDuration = cooldownDuration
    };

    public void TakeDamage(float amount)
    {
        if (!CanTakeDamage || !IsAlive) return;
        CurrentHealth = System.Math.Max(0, CurrentHealth - amount);
        DamageCooldown = CooldownDuration;
    }

    public void UpdateCooldown(float deltaTime)
    {
        if (DamageCooldown > 0)
            DamageCooldown = System.Math.Max(0, DamageCooldown - deltaTime);
    }
}
