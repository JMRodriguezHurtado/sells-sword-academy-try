using Godot;

// Basic melee enemy — chases David and swings at him.
// Visual sword is just for show; mechanically swings generic attacks.
public partial class Grunt : EnemyController
{
	[Export] public float DetectionRange = 400.0f;
	[Export] public float DetectionVerticalTolerance = 200.0f;
	[Export] public float AttackRange = 60.0f;

	[Export] public float AttackWindup = 0.3f;       // before hitbox activates
	[Export] public float AttackStrike = 0.2f;       // hitbox is active
	[Export] public float AttackRecovery = 0.4f;     // locked in place after strike
	[Export] public float HitboxOffset = 140.0f;

	[Export] public int AttackDamage = 10;
	[Export] public float AttackKnockback = 200.0f;
	[Export] public float AttackKnockbackLift = -150.0f;

	private Area2D _attackHitbox;
	private CollisionShape2D _attackHitboxShape;

	// Attack timing state
	private float _attackTimer = 0f;
	private enum AttackPhase { Windup, Strike, Recovery }
	private AttackPhase _attackPhase;
	private bool _hitDealtThisAttack = false;

	protected override void OnReady()
	{
		// Find the attack hitbox (configured in the scene)
		_attackHitbox = GetNode<Area2D>("AttackHitbox");
		_attackHitboxShape = _attackHitbox.GetNode<CollisionShape2D>("CollisionShape2D");

		// Disable hitbox until we're striking
		_attackHitbox.Monitoring = false;
	}

	// === STATE BEHAVIORS ===

	protected override void HandleIdle(ref Vector2 velocity, float fDelta)
	{
		velocity.X = 0f;
		PlayAnimation("idle");

		// Detect player → chase
		if (CanSeePlayer(DetectionRange, DetectionVerticalTolerance))
			TransitionTo(EnemyState.Chase);
	}

	protected override void HandleChase(ref Vector2 velocity, float fDelta)
	{
		// Always face and move toward David
		_facingDirection = DirectionToPlayer();
		velocity.X = MoveSpeed * _facingDirection;

		PlayAnimation("walk");

		// In attack range? Start attacking
		if (DistanceToPlayer() <= AttackRange)
		{
			TransitionTo(EnemyState.Attack);
		}
	}

	protected override void HandleAttack(ref Vector2 velocity, float fDelta)
	{
		velocity.X = 0f; // locked in place
		_attackTimer -= fDelta;

		switch (_attackPhase)
		{
			case AttackPhase.Windup:
				PlayAnimation("attack");
				if (_attackTimer <= 0f)
				{
					_attackPhase = AttackPhase.Strike;
					_attackTimer = AttackStrike;
					_hitDealtThisAttack = false;
					_attackHitbox.Monitoring = true;
				}
				break;

			case AttackPhase.Strike:
				CheckHitboxOverlap();
				if (_attackTimer <= 0f)
				{
					_attackPhase = AttackPhase.Recovery;
					_attackTimer = AttackRecovery;
					_attackHitbox.Monitoring = false;
				}
				break;

			case AttackPhase.Recovery:
				if (_attackTimer <= 0f)
				{
					// Decide what to do next
					if (DistanceToPlayer() <= AttackRange)
					{
						StartAttack(); // chain another attack
					}
					else
					{
						TransitionTo(EnemyState.Chase);
					}
				}
				break;
		}
	}

	protected override void OnStateEntered(EnemyState newState)
	{
		if (newState == EnemyState.Attack)
			StartAttack();

		if (newState == EnemyState.Hurt)
		{
			// Cancel any in-progress attack on hurt
			_attackHitbox.Monitoring = false;
		}
	}

	// === ATTACK LOGIC ===

private void StartAttack()
	{
		_attackPhase = AttackPhase.Windup;
		_attackTimer = AttackWindup;
		_hitDealtThisAttack = false;

		_attackHitboxShape.Position = new Vector2(HitboxOffset * _facingDirection, 0);

		GD.Print($"{Name} starts attack toward direction {_facingDirection}");
		GD.Print($"  Hitbox at local pos {_attackHitboxShape.Position}, global pos {_attackHitbox.GlobalPosition}");
		GD.Print($"  Player at global pos {_player?.GlobalPosition}");
	}

	private void CheckHitboxOverlap()
	{
		if (_hitDealtThisAttack) return;

		foreach (var body in _attackHitbox.GetOverlappingBodies())
		{
			if (body.IsInGroup("player"))
			{
				DealDamageToPlayer(body);
				_hitDealtThisAttack = true;
				break;
			}
		}
	}

	private void DealDamageToPlayer(Node body)
	{
		// Find the PlayerHealth node on David
		var playerHealth = body.GetNodeOrNull<PlayerHealth>("PlayerHealth");
		if (playerHealth != null)
		{
			playerHealth.TakeDamage(AttackDamage);
			GD.Print($"{Name} hit player for {AttackDamage} damage!");
		}

		// Apply knockback if the player has the method
		if (body.HasMethod("ApplyKnockbackFromEnemy"))
		{
			body.Call("ApplyKnockbackFromEnemy", AttackKnockback, AttackKnockbackLift, _facingDirection);
		}
	}
}
