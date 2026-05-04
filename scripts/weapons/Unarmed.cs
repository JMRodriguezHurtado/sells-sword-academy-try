using Godot;
using System.Collections.Generic;

// David's unarmed combat — what he uses when no weapon is equipped.
// Low damage but always available. Establishes the "scrappy fighter" identity.
//
// Q (jab):    fast, weak punch — good for combos
// E (cross):  slower, stronger punch
// R (kick):   medium-range kick
// F (push):   pushes target back without much damage (good for escape)
public static class Unarmed
{
	public static Weapon Create()
	{
		return new Weapon
		{
			Id = "unarmed",
			DisplayName = "Fists",
			Description = "Bare-handed combat. Better than nothing.",
			Type = Weapon.WeaponType.Other,
			Hand = Weapon.WeaponHand.MainHand,
			// Unarmed has no durability concept
			MaxDurability = int.MaxValue,
			CurrentDurability = int.MaxValue,
			EnemyHitCost = 0,
			WallHitCost = 0,
			Shape = ShapeBuilder.Single(),
			Attacks = BuildAttacks(),
			WeaponAbilities = new List<Ability>()
		};
	}

	private static Dictionary<string, AttackData> BuildAttacks()
	{
		return new Dictionary<string, AttackData>
		{
			["attack1"] = new AttackData
			{
				AnimationName = "attack1",
				Damage = 8,
				Duration = 0.3f,
				Cooldown = 0.0f,
				HitboxRange = 50.0f,
				HitboxWidth = 50.0f,
				HitboxHeight = 50.0f,
				AttackAbilities = new List<Ability>()
			},
			["attack2"] = new AttackData
			{
				AnimationName = "attack2",
				Damage = 12,
				Duration = 0.5f,
				Cooldown = 0.3f,
				HitboxRange = 55.0f,
				HitboxWidth = 55.0f,
				HitboxHeight = 50.0f,
				AttackAbilities = new List<Ability>()
			},
			["attack3"] = new AttackData
			{
				AnimationName = "attack1",
				Damage = 10,
				Duration = 0.4f,
				Cooldown = 0.2f,
				HitboxRange = 70.0f,
				HitboxWidth = 60.0f,
				HitboxHeight = 70.0f,
				AttackAbilities = new List<Ability>()
			},
			["attack4"] = new AttackData
			{
				AnimationName = "attack2",
				Damage = 5,
				Duration = 0.4f,
				Cooldown = 0.4f,
				HitboxRange = 60.0f,
				HitboxWidth = 60.0f,
				HitboxHeight = 60.0f,
				AttackAbilities = new List<Ability>
				{
					new Pushback { Force = 250.0f, Lift = -80.0f, TriggerChance = 1.0f }
				}
			}
		};
	}
}
