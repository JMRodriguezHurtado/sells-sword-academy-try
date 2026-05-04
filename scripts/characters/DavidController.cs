using Godot;

// David's specific controller — weapon-driven combat.
// All attack values (damage, duration, range, hitbox, abilities) come from the
// equipped weapon's AttackData. The controller is just an orchestrator.
//
// Weapon hand mapping:
//   MainHand → Q (attack1) and E (attack2)
//   OffHand  → R (attack3) and F (attack4)
//   When single-wielding, MainHand provides all four attacks.
//   When unarmed, fallback Unarmed weapon provides all four attacks.
public partial class DavidController : PlayerController
{
	private AttackHitbox _attackHitbox;
	private Weapon _unarmedWeapon;

	private bool _isAttacking = false;
	private float _attackTimer = 0f;
	private string _currentAttackKey = "";        // e.g. "attack1"
	private Weapon _currentAttackWeapon = null;   // which weapon performed the active attack

	// Per-attack cooldowns — keyed by attack input
	private System.Collections.Generic.Dictionary<string, float> _cooldowns = new();

	protected override void OnReady()
	{
		_attackHitbox = GetNode<AttackHitbox>("AttackHitbox");
		_attackHitbox.DeactivateHitbox();

		// Pre-build the unarmed fallback weapon (one instance, reused)
		_unarmedWeapon = Unarmed.Create();
	}

	protected override void UpdateTimers(float fDelta)
	{
		// Tick down all per-attack cooldowns
		var keys = new System.Collections.Generic.List<string>(_cooldowns.Keys);
		foreach (var key in keys)
		{
			if (_cooldowns[key] > 0f)
				_cooldowns[key] -= fDelta;
		}
	}

	protected override bool HandleCharacterStates(ref Vector2 velocity, float fDelta)
	{
		// --- ATTACK STATE ---
		if (_isAttacking)
		{
			_attackTimer -= fDelta;

			if (_attackTimer <= 0f)
			{
				_isAttacking = false;
				_attackHitbox.DeactivateHitbox();
				_currentAttackKey = "";
				_currentAttackWeapon = null;
			}

			velocity.X = 0f;
			if (!IsOnFloor())
				velocity.Y += _gravity * fDelta;

			return true;
		}

		return false;
	}

	protected override bool HandleCharacterInput(ref Vector2 velocity)
	{
		if (!IsOnFloor()) return false;

		// Map input actions to (attack key, hand)
		if (Input.IsActionJustPressed("attack1"))
			return TryAttack("attack1", mainHand: true, ref velocity);

		if (Input.IsActionJustPressed("attack2"))
			return TryAttack("attack2", mainHand: true, ref velocity);

		if (Input.IsActionJustPressed("attack3"))
			return TryAttack("attack3", mainHand: false, ref velocity);

		if (Input.IsActionJustPressed("attack4"))
			return TryAttack("attack4", mainHand: false, ref velocity);

		return false;
	}

	private bool TryAttack(string attackKey, bool mainHand, ref Vector2 velocity)
	{
		// Check cooldown
		if (_cooldowns.TryGetValue(attackKey, out float cd) && cd > 0f)
			return false;

		// Determine which weapon owns this attack
		Weapon weapon = GetWeaponForAttack(mainHand);
		if (weapon == null || weapon.IsBroken)
			return false;

		AttackData attack = weapon.GetAttack(attackKey);
		if (attack == null)
		{
			GD.Print($"Weapon {weapon.DisplayName} has no attack mapped to {attackKey}");
			return false;
		}

		// Start the attack
		StartAttack(weapon, attack, attackKey);
		velocity.X = 0f;
		return true;
	}

	// Returns the weapon that should provide this attack.
	// Falls back to unarmed if no weapon is equipped on that hand.
	private Weapon GetWeaponForAttack(bool mainHand)
	{
		var inventory = InventoryManager.Instance.GetActiveInventory();
		Weapon equipped = inventory.GetEquippedWeapon(mainHand);

		// If the requested hand has nothing but the OTHER hand has a weapon
		// that's two-handed, use that weapon for all attacks.
		if (equipped == null)
		{
			Weapon otherHand = inventory.GetEquippedWeapon(!mainHand);
			if (otherHand != null && otherHand.IsTwoHanded)
				return otherHand;
		}

		// Default fallback: unarmed
		return equipped ?? _unarmedWeapon;
	}

	private void StartAttack(Weapon weapon, AttackData attack, string attackKey)
	{
		_isAttacking = true;
		_attackTimer = attack.Duration;
		_currentAttackKey = attackKey;
		_currentAttackWeapon = weapon;
		_cooldowns[attackKey] = attack.Cooldown;

		_sprite.FlipH = _facingDirection == -1;
		PlayAnimation(attack.AnimationName, force: true);

		// Configure and activate the hitbox
		_attackHitbox.Configure(weapon, attack, this, _facingDirection);
		_attackHitbox.ActivateHitbox();

		GD.Print($"David attacks with {weapon.DisplayName} ({attackKey}): {attack.Damage} damage, {attack.HitboxWidth}x{attack.HitboxHeight} hitbox");
	}
}
