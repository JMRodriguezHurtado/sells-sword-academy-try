using Godot;
using System.Collections.Generic;

// A weapon David can equip and use in combat.
// Defines its own attack mappings — the controller asks the weapon
// "what does this weapon do on Q?" rather than hardcoding attacks.
public partial class Weapon : Item
{
	public enum WeaponHand { MainHand, OffHand, EitherHand }
	public enum WeaponType { Sword, Dagger, Axe, Hammer, Shield, Staff, Bow, Other }

	[Export] public WeaponType Type { get; set; } = WeaponType.Sword;
	[Export] public WeaponHand Hand { get; set; } = WeaponHand.MainHand;

	// Durability — one pool for both enemy and wall hits
	[Export] public int MaxDurability { get; set; } = 60;
	[Export] public int CurrentDurability { get; set; } = 60;

	// Cost per hit type (wall hits cost more by design)
	[Export] public int EnemyHitCost { get; set; } = 1;
	[Export] public int WallHitCost { get; set; } = 3;

	// Attack mappings — each input action maps to an attack definition.
	// When dual-wielding, weapons in main hand provide Q/E, off-hand provides R/F.
	// When single-wielding, one weapon provides all four.
	public Dictionary<string, AttackData> Attacks { get; set; } = new();

	public bool IsBroken => CurrentDurability <= 0;

	// Returns true if hit was registered (false if weapon is already broken)
	public bool RegisterEnemyHit()
	{
		if (IsBroken) return false;
		CurrentDurability = Mathf.Max(0, CurrentDurability - EnemyHitCost);
		return true;
	}

	public bool RegisterWallHit()
	{
		if (IsBroken) return false;
		CurrentDurability = Mathf.Max(0, CurrentDurability - WallHitCost);
		return true;
	}

	public void Repair(int amount)
	{
		CurrentDurability = Mathf.Min(MaxDurability, CurrentDurability + amount);
	}

	public void RepairFull()
	{
		CurrentDurability = MaxDurability;
	}

	public AttackData GetAttack(string inputAction)
	{
		return Attacks.TryGetValue(inputAction, out var attack) ? attack : null;
	}
}

// Data describing a single attack — animation, damage, range, knockback, special effects.
// This is what controllers read to perform combat.
public partial class AttackData : Resource
{
	[Export] public string AnimationName { get; set; } = "attack1";
	[Export] public int Damage { get; set; } = 20;
	[Export] public float Duration { get; set; } = 0.4f;
	[Export] public float Cooldown { get; set; } = 0.0f;
	[Export] public float Knockback { get; set; } = 0.0f;
	[Export] public float HitboxRange { get; set; } = 80.0f;  // distance in front of player

	// Special behavior tags — controller checks these to know what's special
	// e.g. "parry_window" means the controller should listen for incoming attacks during this window
	[Export] public string[] Tags { get; set; } = new string[0];

	public bool HasTag(string tag) => System.Array.IndexOf(Tags, tag) >= 0;
}
