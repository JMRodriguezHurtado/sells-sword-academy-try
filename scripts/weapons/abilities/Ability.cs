using Godot;

// Base class for all weapon abilities.
// Abilities are reusable behaviors that can be attached to specific attacks
// (per-attack abilities) or to the whole weapon (passive effects).
//
// Subclasses override the lifecycle hooks they care about and ignore the rest.
// The combat controller fires these hooks at the right moments:
//   - OnAttackStart: when the attack animation begins
//   - OnHit: when the attack lands on an enemy (most common hook)
//   - OnAttackEnd: when the attack animation finishes
//   - OnBlock: when the player successfully blocks an incoming attack
//   - OnParry: when the player successfully parries an incoming attack
public partial class Ability : Resource
{
	[Export] public string Name { get; set; } = "";
	[Export] public string Description { get; set; } = "";
	[Export] public float TriggerChance { get; set; } = 1.0f;  // 0.0 = never, 1.0 = always

	// === LIFECYCLE HOOKS ===
	// Override only the ones your ability needs.

	// Called when the attack animation begins.
	public virtual void OnAttackStart(AbilityContext context) { }

	// Called when the attack lands on a target.
	// Most abilities will override this (Freeze, Bleed, Pushback, Crit, etc.).
	public virtual void OnHit(AbilityContext context) { }

	// Called when the attack animation ends.
	public virtual void OnAttackEnd(AbilityContext context) { }

	// Called when the player successfully blocks an incoming attack.
	public virtual void OnBlock(AbilityContext context) { }

	// Called when the player successfully parries an incoming attack.
	public virtual void OnParry(AbilityContext context) { }

	// === HELPERS ===

	// Returns true if the random roll passes the TriggerChance threshold.
	// Use this in OnHit etc. to gate effects: `if (!RollChance()) return;`
	protected bool RollChance()
	{
		if (TriggerChance >= 1.0f) return true;
		if (TriggerChance <= 0.0f) return false;
		return GD.Randf() < TriggerChance;
	}
}

// Bundles all the data an ability might need when it triggers.
// Passed to every lifecycle hook so abilities have full context.
public partial class AbilityContext : Resource
{
	public Node Attacker { get; set; }    // the player or enemy who attacked
	public Node Target { get; set; }      // who got hit (null for OnAttackStart/End)
	public Weapon Weapon { get; set; }    // the weapon being used
	public AttackData Attack { get; set; }// the specific attack being performed
	public int FinalDamage { get; set; }  // damage about to be dealt (mutable — abilities can modify)
	public int FacingDirection { get; set; } // 1 = right, -1 = left
}
