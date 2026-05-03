using Godot;
using System.Collections.Generic;

// The Short Sword — David's starting weapon.
// Balanced and versatile: medium damage, medium range, decent durability.
//
// Per-attack abilities:
//   Q (light slash): no special abilities — pure damage
//   E (heavy slash): Pushback — knocks enemies back
//   R (thrust):      Bleed — chance to inflict bleed on impact
//   F (counter):     no abilities — placeholder for future parry mechanic
//
// Whole-weapon abilities:
//   Crit (10% chance to deal double damage on any attack)
public static class ShortSword
{
	public const string Id = "short_sword";
	public const string DisplayName = "Short Sword";
	public const string Description = "A reliable, balanced sword. Equally good for slashing and thrusting.";

	// Factory method — creates a fresh Short Sword instance.
	// currentDurability lets us spawn damaged swords (e.g., picked up from a fallen enemy).
	public static Weapon Create(int currentDurability = -1)
	{
		var sword = new Weapon
		{
			Id = Id,
			DisplayName = DisplayName,
			Description = Description,
			Type = Weapon.WeaponType.Sword,
			Hand = Weapon.WeaponHand.MainHand,
			MaxDurability = 60,
			CurrentDurability = currentDurability < 0 ? 60 : currentDurability,
			EnemyHitCost = 1,
			WallHitCost = 3,
			Shape = ShapeBuilder.ShortSword(),
			Attacks = BuildAttacks(),
			WeaponAbilities = new List<Ability>
			{
				new Crit { TriggerChance = 0.1f, DamageMultiplier = 2.0f }
			}
		};

		return sword;
	}

	private static Dictionary<string, AttackData> BuildAttacks()
	{
		return new Dictionary<string, AttackData>
		{
			// Q — Light slash (fast, low damage, no special effects)
			["attack1"] = new AttackData
			{
				AnimationName = "attack1",
				Damage = 20,
				Duration = 0.4f,
				Cooldown = 0.0f,
				HitboxRange = 80.0f,
				AttackAbilities = new List<Ability>()
			},

			// E — Heavy slash (slower, more damage, knocks enemies back)
			["attack2"] = new AttackData
			{
				AnimationName = "attack2",
				Damage = 30,
				Duration = 0.6f,
				Cooldown = 0.5f,
				HitboxRange = 90.0f,
				AttackAbilities = new List<Ability>
				{
					new Pushback { Force = 350.0f, Lift = -100.0f }
				}
			},

			// R — Thrust (medium range, chance to bleed)
			// Note: animation will be added later when we have thrust frames
			["attack3"] = new AttackData
			{
				AnimationName = "attack2",   // reuse attack2 animation for now
				Damage = 25,
				Duration = 0.5f,
				Cooldown = 0.3f,
				HitboxRange = 110.0f,
				AttackAbilities = new List<Ability>
				{
					new Bleed { TriggerChance = 0.4f, DamagePerTick = 5, Ticks = 3 }
				}
			},

			// F — Counter / parry (placeholder — full parry logic comes later)
			["attack4"] = new AttackData
			{
				AnimationName = "attack1",   // reuse attack1 animation for now
				Damage = 15,
				Duration = 0.4f,
				Cooldown = 0.6f,
				HitboxRange = 70.0f,
				Tags = new[] { "parry_window" },  // controller will detect this tag later
				AttackAbilities = new List<Ability>()
			}
		};
	}
}
