using Godot;

public partial class AttackHitbox : Area2D
{
	[Export] public int DefaultDamage = 20;

	private int _damage;
	private float _knockback;
	private int _knockbackDirection;
	private bool _hasDealtDamage = false;

	public void Configure(int damage, float knockback, int direction)
	{
		_damage = damage;
		_knockback = knockback;
		_knockbackDirection = direction;
	}

	public void ActivateHitbox()
	{
		_hasDealtDamage = false;
		Monitoring = true;
		GD.Print($"Hitbox activated! Damage: {_damage}, Knockback: {_knockback}");
	}

	public void DeactivateHitbox()
	{
		Monitoring = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Monitoring || _hasDealtDamage) return;

		foreach (var body in GetOverlappingBodies())
		{
			if (body.IsInGroup("enemy"))
			{
				GD.Print($"Hit enemy: {body.Name}!");
				if (body.HasMethod("TakeDamage"))
				{
					// Try calling with knockback first, fall back to simple TakeDamage
					if (body.HasMethod("TakeDamageWithKnockback"))
						body.Call("TakeDamageWithKnockback", _damage, _knockback, _knockbackDirection);
					else
						body.Call("TakeDamage", _damage);
				}
				_hasDealtDamage = true;
			}
		}
	}
}
