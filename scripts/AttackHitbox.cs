using Godot;

public partial class AttackHitbox : Area2D
{
	[Export] public int AttackDamage = 20;
	private bool _hasDealtDamage = false;

	public void ActivateHitbox()
	{
		_hasDealtDamage = false;
		Monitoring = true;
		GD.Print("Hitbox activated!");
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
			GD.Print($"Overlapping body: {body.Name}");
			if (body.IsInGroup("enemy"))
			{
				GD.Print($"Hit enemy: {body.Name}!");
				if (body.HasMethod("TakeDamage"))
					body.Call("TakeDamage", AttackDamage);
				_hasDealtDamage = true;
			}
		}
	}
}
