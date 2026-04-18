public static class PlayerDataStructures
{
    public readonly struct ResourceData
    {
        public readonly float Current;
        public readonly float Max;
        public readonly ResourceType Type;

        public ResourceData(float current, float max, ResourceType type)
        {
            Current = current;
            Max = max;
            Type = type;
        }
    }

    public enum ResourceType { Health, Mana, Stamina }
    public enum ResourceValueType { Maximum, Current }

    public readonly struct AttackModifiersData
    {
        public readonly float Damage;
        public readonly float AttackSpeed;
        public readonly float AttackRange;

        public AttackModifiersData(float damage, float attackSpeed, float attackRange)
        {
            Damage = damage;
            AttackSpeed = attackSpeed;
            AttackRange = attackRange;
        }
    }

    public readonly struct MovementModifiersData
    {
        public readonly float PlayerSpeed;
        public readonly float SprintMultiplier;
        public readonly float JumpHeight;
        public readonly float DashSpeed;
        public readonly float DashCooldown;
        public readonly float GravityScale;

        public MovementModifiersData(float moveSpeed, float sprintMultiplier, float jumpHeight,
                                     float dashSpeed, float dashCooldown, float gravityScale)
        {
            PlayerSpeed = moveSpeed;
            SprintMultiplier = sprintMultiplier;
            JumpHeight = jumpHeight;
            DashSpeed = dashSpeed;
            DashCooldown = dashCooldown;
            GravityScale = gravityScale;
        }
    }
}
