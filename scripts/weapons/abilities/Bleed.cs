using Godot;

// Inflicts damage-over-time on the target.
// The target needs to support an ApplyBleed method for this to work.
public partial class Bleed : Ability
{
	[Export] public int DamagePerTick = 5;
	[Export] public float TickInterval = 1.0f;  // seconds between ticks
	[Export] public int Ticks = 3;              // total number of damage ticks

	public Bleed()
	{
		Name = "Bleed";
		Description = "Inflicts damage over time.";
		TriggerChance = 0.5f;  // 50% by default
	}

	public override void OnHit(AbilityContext context)
	{
		if (!RollChance()) return;
		if (context.Target == null) return;

		if (context.Target.HasMethod("ApplyBleed"))
		{
			context.Target.Call("ApplyBleed", DamagePerTick, TickInterval, Ticks);
			GD.Print($"Bleed applied to {context.Target.Name}!");
		}
	}
}
