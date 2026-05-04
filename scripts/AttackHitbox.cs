using Godot;
using System.Collections.Generic;

// AttackHitbox runs the full ability pipeline when an enemy is hit.
// The flow:
//   1. Build an AbilityContext with attack info
//   2. Run all attack-specific abilities (OnHit) — Bleed, Pushback, etc.
//   3. Run all whole-weapon abilities (OnHit) — Crit, etc. (may modify FinalDamage)
//   4. Apply FinalDamage to the target via TakeDamage
public partial class AttackHitbox : Area2D
{
	private CollisionShape2D _collisionShape;
	private RectangleShape2D _rectangleShape;

	private Weapon _currentWeapon;
	private AttackData _currentAttack;
	private Node _attacker;
	private int _facingDirection;
	private bool _hasDealtDamage = false;

	public override void _Ready()
	{
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

		// Replace whatever shape was there with a RectangleShape2D we can resize
		_rectangleShape = new RectangleShape2D();
		_collisionShape.Shape = _rectangleShape;
	}

	// Called by DavidController when an attack starts.
	// Configures the hitbox with all data needed for the ability pipeline.
	public void Configure(Weapon weapon, AttackData attack, Node attacker, int facingDirection)
	{
		_currentWeapon = weapon;
		_currentAttack = attack;
		_attacker = attacker;
		_facingDirection = facingDirection;

		// Resize hitbox per-attack
		_rectangleShape.Size = new Vector2(attack.HitboxWidth, attack.HitboxHeight);

		// Position hitbox in front of player at the attack's range
		_collisionShape.Position = new Vector2(attack.HitboxRange * facingDirection, 0);
	}

	public void ActivateHitbox()
	{
		_hasDealtDamage = false;
		Monitoring = true;
		GD.Print($"Hitbox activated! Attack: {_currentAttack?.AnimationName}, Damage: {_currentAttack?.Damage}");
	}

	public void DeactivateHitbox()
	{
		Monitoring = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Monitoring || _hasDealtDamage || _currentAttack == null) return;

		foreach (var body in GetOverlappingBodies())
		{
			if (body.IsInGroup("enemy"))
			{
				ProcessHit(body);
				_hasDealtDamage = true;
				break; // one target per attack for now
			}
		}
	}

	// Runs the full ability pipeline against a single target.
	private void ProcessHit(Node target)
	{
		// Build the context that abilities will read and potentially modify
		var context = new AbilityContext
		{
			Attacker = _attacker,
			Target = target,
			Weapon = _currentWeapon,
			Attack = _currentAttack,
			FinalDamage = _currentAttack.Damage,
			FacingDirection = _facingDirection
		};

		// 1. Run per-attack abilities (Bleed, Pushback, etc.)
		foreach (var ability in _currentAttack.AttackAbilities)
			ability.OnHit(context);

		// 2. Run whole-weapon abilities (Crit, etc.)
		if (_currentWeapon != null)
		{
			foreach (var ability in _currentWeapon.WeaponAbilities)
				ability.OnHit(context);
		}

		// 3. Apply final damage to target
		if (target.HasMethod("TakeDamage"))
			target.Call("TakeDamage", context.FinalDamage);

		// 4. Register weapon hit for durability (skip for unarmed which has 0 cost)
		_currentWeapon?.RegisterEnemyHit();

		GD.Print($"Hit {target.Name}! Final damage: {context.FinalDamage} (weapon HP: {_currentWeapon?.CurrentDurability}/{_currentWeapon?.MaxDurability})");
	}
}
