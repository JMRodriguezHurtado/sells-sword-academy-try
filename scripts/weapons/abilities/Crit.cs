using Godot;

// Whole-weapon ability — chance to deal multiplied damage.
// This is the canonical example of a passive that applies to ALL attacks.
public partial class Crit : Ability
{
	[Export] public float DamageMultiplier = 2.0f;

	public Crit()
	{
		Name = "Critical Strike";
		Description = "Chance to deal multiplied damage.";
		TriggerChance = 0.2f;  // 20% by default
	}

	public override void OnHit(AbilityContext context)
	{
		if (!RollChance()) return;

		int boostedDamage = Mathf.RoundToInt(context.FinalDamage * DamageMultiplier);
		GD.Print($"CRIT! {context.FinalDamage} → {boostedDamage}");
		context.FinalDamage = boostedDamage;
	}
}
