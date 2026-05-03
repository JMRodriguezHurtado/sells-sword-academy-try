using Godot;

// Pushes the target backward when an attack lands.
// Replaces the hardcoded knockback that was on AttackData.
// Now any attack with this ability will push enemies; attacks without it won't.
public partial class Pushback : Ability
{
	[Export] public float Force = 350.0f;   // horizontal push force
	[Export] public float Lift = -100.0f;   // small upward bump for feel (negative = up)

	public Pushback()
	{
		Name = "Pushback";
		Description = "Knocks the target backward.";
	}

	public override void OnHit(AbilityContext context)
	{
		if (!RollChance()) return;
		if (context.Target == null) return;

		// Try to apply knockback if the target supports it
		if (context.Target.HasMethod("ApplyKnockback"))
		{
			context.Target.Call("ApplyKnockback", Force, Lift, context.FacingDirection);
		}
	}
}
