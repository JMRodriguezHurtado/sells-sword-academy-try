using Godot;

// David's specific controller — sword combat with attack1 and attack2 (knockback thrust).
// Future: weapon collection system that modifies attacks/combos.
public partial class DavidController : PlayerController
{
	[Export] public float AttackDuration = 0.4f;
	[Export] public float Attack2Duration = 0.6f;
	[Export] public float Attack2Cooldown = 0.5f;
	[Export] public int Attack1Damage = 20;
	[Export] public int Attack2Damage = 30;
	[Export] public float Attack2Knockback = 350.0f;

	private AttackHitbox _attackHitbox;
	private bool _isAttacking = false;
	private float _attackTimer = 0f;
	private float _attack2CooldownTimer = 0f;

	protected override void OnReady()
	{
		_attackHitbox = GetNode<AttackHitbox>("AttackHitbox");
		_attackHitbox.DeactivateHitbox();
	}

	protected override void UpdateTimers(float fDelta)
	{
		if (_attack2CooldownTimer > 0f)
			_attack2CooldownTimer -= fDelta;
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
			}

			velocity.X = 0f;
			if (!IsOnFloor())
				velocity.Y += _gravity * fDelta;

			return true; // we took control this frame
		}

		return false;
	}

	protected override bool HandleCharacterInput(ref Vector2 velocity)
	{
		// --- ATTACK 1 INPUT ---
		if (Input.IsActionJustPressed("attack1") && IsOnFloor())
		{
			StartAttack("attack1", AttackDuration, Attack1Damage, 0f);
			velocity.X = 0f;
			return true;
		}

		// --- ATTACK 2 INPUT ---
		if (Input.IsActionJustPressed("attack2") && IsOnFloor() && _attack2CooldownTimer <= 0f)
		{
			StartAttack("attack2", Attack2Duration, Attack2Damage, Attack2Knockback);
			_attack2CooldownTimer = Attack2Cooldown;
			velocity.X = 0f;
			return true;
		}

		return false;
	}

	private void StartAttack(string animName, float duration, int damage, float knockback)
	{
		_isAttacking = true;
		_attackTimer = duration;
		_sprite.FlipH = _facingDirection == -1;
		PlayAnimation(animName, force: true);

		_attackHitbox.Position = new Vector2(80 * _facingDirection, 0);
		_attackHitbox.Scale = new Vector2(_facingDirection, 1);
		_attackHitbox.Configure(damage, knockback, _facingDirection);
		_attackHitbox.ActivateHitbox();
	}
}
