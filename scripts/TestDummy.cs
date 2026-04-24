using Godot;

public partial class TestDummy : CharacterBody2D
{
	[Export] public int MaxHealth = 100;
	[Export] public float RespawnDelay = 5.0f;
	[Export] public float KnockbackFriction = 1200.0f; // how fast knockback slows down

	private int _currentHealth;
	private Label _label;
	private Sprite2D _sprite;
	private CollisionShape2D _collisionShape;
	private Vector2 _originalPosition;
	private bool _isDead = false;

	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_label = GetNode<Label>("Label");
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		_label.Text = $"HP: {_currentHealth}";
		_originalPosition = GlobalPosition;
		AddToGroup("enemy");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		float fDelta = (float)delta;

		// Apply gravity
		if (!IsOnFloor())
			velocity.Y += _gravity * fDelta;

		// Apply friction to horizontal velocity (slow down from knockback)
		if (velocity.X > 0)
			velocity.X = Mathf.Max(0, velocity.X - KnockbackFriction * fDelta);
		else if (velocity.X < 0)
			velocity.X = Mathf.Min(0, velocity.X + KnockbackFriction * fDelta);

		Velocity = velocity;
		MoveAndSlide();
	}

	public void TakeDamage(int amount)
	{
		if (_isDead) return;

		_currentHealth -= amount;
		_currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);
		_label.Text = $"HP: {_currentHealth}";
		GD.Print($"Dummy took {amount} damage! HP: {_currentHealth}/{MaxHealth}");

		_sprite.Modulate = new Color(1, 0, 0);
		GetTree().CreateTimer(0.15f).Timeout += () =>
		{
			if (!_isDead) _sprite.Modulate = new Color(1, 1, 1);
		};

		if (_currentHealth <= 0)
			Die();
	}

	public void TakeDamageWithKnockback(int amount, float knockback, int direction)
	{
		if (_isDead) return;

		TakeDamage(amount);

		// Apply real physics knockback
		Velocity = new Vector2(knockback * direction, -100); // small upward bump for feel
		GD.Print($"Dummy knocked back with force {knockback} in direction {direction}");
	}

	private void Die()
	{
		_isDead = true;
		GD.Print("Dummy defeated! Respawning in " + RespawnDelay + "s");

		_sprite.Visible = false;
		_label.Visible = false;
		_collisionShape.SetDeferred("disabled", true);

		GetTree().CreateTimer(RespawnDelay).Timeout += Respawn;
	}

	private void Respawn()
	{
		_currentHealth = MaxHealth;
		_isDead = false;
		_label.Text = $"HP: {_currentHealth}";
		_sprite.Visible = true;
		_sprite.Modulate = new Color(1, 1, 1);
		_label.Visible = true;
		_collisionShape.SetDeferred("disabled", false);
		GlobalPosition = _originalPosition; // reset to spawn point
		Velocity = Vector2.Zero;
		GD.Print("Dummy respawned at original position!");
	}
}
