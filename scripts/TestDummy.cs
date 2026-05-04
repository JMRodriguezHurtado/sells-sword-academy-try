using Godot;

public partial class TestDummy : CharacterBody2D
{
	[Export] public int MaxHealth = 100;
	[Export] public float RespawnDelay = 5.0f;
	[Export] public float KnockbackFriction = 1200.0f;

	private int _currentHealth;
	private Label _label;
	private Sprite2D _sprite;
	private CollisionShape2D _collisionShape;
	private Vector2 _originalPosition;
	private bool _isDead = false;

	// Bleed state
	private bool _isBleeding = false;
	private int _bleedDamagePerTick = 0;
	private float _bleedTickInterval = 1.0f;
	private int _bleedTicksRemaining = 0;
	private float _bleedTimer = 0f;

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

		// Gravity
		if (!IsOnFloor())
			velocity.Y += _gravity * fDelta;

		// Friction (knockback decay)
		if (velocity.X > 0)
			velocity.X = Mathf.Max(0, velocity.X - KnockbackFriction * fDelta);
		else if (velocity.X < 0)
			velocity.X = Mathf.Min(0, velocity.X + KnockbackFriction * fDelta);

		Velocity = velocity;
		MoveAndSlide();

		// Tick bleed
		TickBleed(fDelta);
	}

	private void TickBleed(float fDelta)
	{
		if (!_isBleeding || _isDead) return;

		_bleedTimer -= fDelta;
		if (_bleedTimer <= 0f)
		{
			TakeDamage(_bleedDamagePerTick);
			_bleedTicksRemaining--;

			if (_bleedTicksRemaining <= 0)
			{
				_isBleeding = false;
				GD.Print($"{Name} stopped bleeding.");
			}
			else
			{
				_bleedTimer = _bleedTickInterval;
			}
		}
	}

	public void TakeDamage(int amount)
	{
		if (_isDead) return;

		_currentHealth -= amount;
		_currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);
		_label.Text = $"HP: {_currentHealth}";
		GD.Print($"Dummy took {amount} damage! HP: {_currentHealth}/{MaxHealth}");

		// Flash red
		_sprite.Modulate = new Color(1, 0, 0);
		GetTree().CreateTimer(0.15f).Timeout += () =>
		{
			if (!_isDead) _sprite.Modulate = new Color(1, 1, 1);
		};

		if (_currentHealth <= 0)
			Die();
	}

	// === ABILITY REACTIONS ===
	// Called by abilities through the AbilityContext pipeline.

	// Called by Pushback ability
	public void ApplyKnockback(float force, float lift, int direction)
	{
		if (_isDead) return;
		Velocity = new Vector2(force * direction, lift);
		GD.Print($"{Name} knocked back: force={force}, direction={direction}");
	}

	// Called by Bleed ability
	public void ApplyBleed(int damagePerTick, float tickInterval, int ticks)
	{
		if (_isDead) return;

		_isBleeding = true;
		_bleedDamagePerTick = damagePerTick;
		_bleedTickInterval = tickInterval;
		_bleedTicksRemaining = ticks;
		_bleedTimer = tickInterval;

		GD.Print($"{Name} is now bleeding: {damagePerTick} dmg every {tickInterval}s for {ticks} ticks");
	}

	private void Die()
	{
		_isDead = true;
		_isBleeding = false; // stop bleed on death
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
		_isBleeding = false;
		_label.Text = $"HP: {_currentHealth}";
		_sprite.Visible = true;
		_sprite.Modulate = new Color(1, 1, 1);
		_label.Visible = true;
		_collisionShape.SetDeferred("disabled", false);
		GlobalPosition = _originalPosition;
		Velocity = Vector2.Zero;
		GD.Print("Dummy respawned at original position!");
	}
}
