using Godot;

// Base class for all enemies in the game (Grunts, Archers, Bosses, etc.).
// Provides shared logic: gravity, health, hurt state, death + respawn,
// and the AI state machine framework.
//
// Subclasses override the virtual state methods (HandleIdle, HandleChase, etc.)
// to define their specific AI behavior.
public partial class EnemyController : CharacterBody2D
{
	public enum EnemyState { Idle, Chase, Attack, Hurt, Death }

	[Export] public int MaxHealth = 40;
	[Export] public float MoveSpeed = 150.0f;
	[Export] public float HurtDuration = 0.3f;
	[Export] public float RespawnDelay = 5.0f;
	[Export] public float KnockbackFriction = 1200.0f;

	protected AnimatedSprite2D _sprite;
	protected CollisionShape2D _collisionShape;

	protected int _currentHealth;
	protected EnemyState _state = EnemyState.Idle;
	protected int _facingDirection = -1; // -1 = left (default — they'll face David)
	protected float _hurtTimer = 0f;
	protected Vector2 _spawnPosition;
	protected bool _isDead = false;

	protected float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	// Reference to the player — found at runtime
	protected Node2D _player;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

		_currentHealth = MaxHealth;
		_spawnPosition = GlobalPosition;

		AddToGroup("enemy");

		// Find David in the scene
		var players = GetTree().GetNodesInGroup("player");
		if (players.Count > 0)
			_player = players[0] as Node2D;

		OnReady();
	}

	// Override in subclasses for additional setup
	protected virtual void OnReady() { }

	public override void _PhysicsProcess(double delta)
	{
		if (_isDead) return;

		Vector2 velocity = Velocity;
		float fDelta = (float)delta;

		// Gravity
		if (!IsOnFloor())
			velocity.Y += _gravity * fDelta;

		// Knockback friction (slows horizontal velocity over time)
		if (_state == EnemyState.Hurt)
		{
			if (velocity.X > 0)
				velocity.X = Mathf.Max(0, velocity.X - KnockbackFriction * fDelta);
			else if (velocity.X < 0)
				velocity.X = Mathf.Min(0, velocity.X + KnockbackFriction * fDelta);
		}

		// Run the state machine
		switch (_state)
		{
			case EnemyState.Idle:
				HandleIdle(ref velocity, fDelta);
				break;
			case EnemyState.Chase:
				HandleChase(ref velocity, fDelta);
				break;
			case EnemyState.Attack:
				HandleAttack(ref velocity, fDelta);
				break;
			case EnemyState.Hurt:
				HandleHurt(ref velocity, fDelta);
				break;
		}

		Velocity = velocity;
		MoveAndSlide();

		// Update sprite facing
		_sprite.FlipH = _facingDirection == 1;
	}

	// === STATE HANDLERS — subclasses override to define behavior ===

	protected virtual void HandleIdle(ref Vector2 velocity, float fDelta) { }
	protected virtual void HandleChase(ref Vector2 velocity, float fDelta) { }
	protected virtual void HandleAttack(ref Vector2 velocity, float fDelta) { }

	protected virtual void HandleHurt(ref Vector2 velocity, float fDelta)
	{
		_hurtTimer -= fDelta;
		velocity.X *= 0.5f; // additional damping on hurt

		if (_hurtTimer <= 0f)
			TransitionTo(EnemyState.Chase);

		PlayAnimation("hurt");
	}

	// === STATE TRANSITIONS ===

	protected virtual void TransitionTo(EnemyState newState)
	{
		if (_state == newState) return;
		_state = newState;
		OnStateEntered(newState);
	}

	// Override to react to state changes (e.g., reset timers)
	protected virtual void OnStateEntered(EnemyState newState) { }

	// === DAMAGE / KNOCKBACK / DEATH ===

	public virtual void TakeDamage(int amount)
	{
		if (_isDead) return;

		_currentHealth -= amount;
		_currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);
		GD.Print($"{Name} took {amount} damage! HP: {_currentHealth}/{MaxHealth}");

		// Flash red
		_sprite.Modulate = new Color(1, 0.3f, 0.3f);
		GetTree().CreateTimer(0.15f).Timeout += () =>
		{
			if (!_isDead) _sprite.Modulate = new Color(1, 1, 1);
		};

		if (_currentHealth <= 0)
		{
			Die();
			return;
		}

		// Enter hurt state
		_hurtTimer = HurtDuration;
		TransitionTo(EnemyState.Hurt);
	}

	public virtual void ApplyKnockback(float force, float lift, int direction)
	{
		if (_isDead) return;
		Velocity = new Vector2(force * direction, lift);
	}

	protected virtual void Die()
	{
		_isDead = true;
		_state = EnemyState.Death;
		GD.Print($"{Name} defeated! Respawning in {RespawnDelay}s");

		PlayAnimation("death");
		_collisionShape.SetDeferred("disabled", true);

		GetTree().CreateTimer(RespawnDelay).Timeout += Respawn;
	}

	protected virtual void Respawn()
	{
		_currentHealth = MaxHealth;
		_isDead = false;
		_state = EnemyState.Idle;
		_sprite.Modulate = new Color(1, 1, 1);
		_collisionShape.SetDeferred("disabled", false);
		GlobalPosition = _spawnPosition;
		Velocity = Vector2.Zero;
		GD.Print($"{Name} respawned!");
	}

	// === HELPERS ===

	protected void PlayAnimation(string animName, bool force = false)
	{
		if (_sprite == null || _sprite.SpriteFrames == null) return;
		if (!_sprite.SpriteFrames.HasAnimation(animName)) return; // skip silently if missing
		if (force || _sprite.Animation != new StringName(animName))
			_sprite.Play(animName);
	}

	// Returns horizontal distance to player (-1 if no player)
	protected float DistanceToPlayer()
	{
		if (_player == null) return -1f;
		return Mathf.Abs(_player.GlobalPosition.X - GlobalPosition.X);
	}

	// Returns -1 if player is to the left, 1 if to the right
	protected int DirectionToPlayer()
	{
		if (_player == null) return _facingDirection;
		return _player.GlobalPosition.X > GlobalPosition.X ? 1 : -1;
	}

	// Returns true if player is within detection range
	protected bool CanSeePlayer(float horizontalRange, float verticalTolerance)
	{
		if (_player == null) return false;
		float dx = Mathf.Abs(_player.GlobalPosition.X - GlobalPosition.X);
		float dy = Mathf.Abs(_player.GlobalPosition.Y - GlobalPosition.Y);
		return dx <= horizontalRange && dy <= verticalTolerance;
	}
	

}
