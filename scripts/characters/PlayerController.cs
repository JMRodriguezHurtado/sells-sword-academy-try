using Godot;

// Base class for all playable characters (David, Clark, Megan).
// Contains shared movement, gravity, jump, dash, hurt, and animation framework.
// Character-specific attacks and abilities go in the derived classes.
public partial class PlayerController : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -700.0f;
	[Export] public float DashSpeed = 800.0f;
	[Export] public float DashDuration = 0.15f;
	[Export] public float DashCooldown = 0.5f;
	[Export] public float HurtDuration = 0.4f;

	protected AnimatedSprite2D _sprite;
	protected PlayerHealth _playerHealth;
	protected float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	protected int _facingDirection = 1;
	protected bool _isDashing = false;
	protected bool _isBackDash = false;
	protected bool _isHurt = false;
	protected float _dashTimer = 0f;
	protected float _dashCooldownTimer = 0f;
	protected float _hurtTimer = 0f;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_playerHealth = GetNode<PlayerHealth>("PlayerHealth");
		_playerHealth.HealthChanged += OnHealthChanged;
		OnReady();
	}

	// Override in derived classes for character-specific setup
	protected virtual void OnReady() { }

	private void OnHealthChanged(int currentHealth, int maxHealth)
	{
		if (currentHealth < maxHealth && !_playerHealth.IsDead)
			TriggerHurt();
	}

	public virtual void TriggerHurt()
	{
		_isHurt = true;
		_hurtTimer = HurtDuration;
		PlayAnimation("hurt");
		_sprite.FlipH = _facingDirection == -1;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		float fDelta = (float)delta;

		if (_dashCooldownTimer > 0f)
			_dashCooldownTimer -= fDelta;

		// Let derived class update its own timers/cooldowns
		UpdateTimers(fDelta);

		// --- HURT STATE ---
		if (_isHurt)
		{
			_hurtTimer -= fDelta;
			if (_hurtTimer <= 0f)
				_isHurt = false;

			if (!IsOnFloor())
				velocity.Y += _gravity * fDelta;

			Velocity = velocity;
			MoveAndSlide();
			return;
		}

		// --- CHARACTER-SPECIFIC STATES (attacks, etc.) ---
		if (HandleCharacterStates(ref velocity, fDelta))
		{
			Velocity = velocity;
			MoveAndSlide();
			return;
		}

		// --- DASH STATE ---
		if (_isDashing)
		{
			_dashTimer -= fDelta;
			if (_dashTimer <= 0f)
			{
				_isDashing = false;
				_isBackDash = false;
				velocity.X = 0f;
			}

			if (_isBackDash)
			{
				PlayAnimation("backDash");
				_sprite.FlipH = _facingDirection == 1;
			}
			else
			{
				PlayAnimation("frontDash");
				_sprite.FlipH = _facingDirection == -1;
			}

			Velocity = velocity;
			MoveAndSlide();
			return;
		}

		// --- GRAVITY ---
		if (!IsOnFloor())
			velocity.Y += _gravity * fDelta;

		// --- JUMP ---
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// --- HORIZONTAL MOVEMENT ---
		float direction = Input.GetAxis("move_left", "move_right");
		velocity.X = direction * Speed;

		if (Mathf.Abs(direction) > 0.2f)
			_facingDirection = direction > 0f ? 1 : -1;

		// --- CHARACTER-SPECIFIC INPUT (attacks, abilities) ---
		if (HandleCharacterInput(ref velocity))
		{
			Velocity = velocity;
			MoveAndSlide();
			return;
		}

		// --- DASH INPUT ---
		if (IsOnFloor() && _dashCooldownTimer <= 0f)
		{
			if (Input.IsActionJustPressed("dash_forward"))
				StartDash(ref velocity, _facingDirection, isBack: false);
			else if (Input.IsActionJustPressed("dash_back"))
				StartDash(ref velocity, -_facingDirection, isBack: true);
		}

		// --- DEFAULT ANIMATION ---
		if (!IsOnFloor())
		{
			PlayAnimation("jump");
			_sprite.FlipH = _facingDirection == -1;
		}
		else if (direction != 0f)
		{
			PlayAnimation("walk");
			_sprite.FlipH = direction < 0f;
		}
		else
		{
			PlayAnimation("idle");
			_sprite.FlipH = _facingDirection == -1;
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	// Override in derived classes to update character-specific timers (attack cooldowns, etc.)
	protected virtual void UpdateTimers(float fDelta) { }

	// Override in derived classes to handle character-specific states (attack state, casting, etc.).
	// Return true if the state took control this frame.
	protected virtual bool HandleCharacterStates(ref Vector2 velocity, float fDelta) => false;

	// Override in derived classes to handle character-specific input (attacks, abilities).
	// Return true if input was consumed this frame.
	protected virtual bool HandleCharacterInput(ref Vector2 velocity) => false;

	private void StartDash(ref Vector2 velocity, int direction, bool isBack)
	{
		_isDashing = true;
		_isBackDash = isBack;
		_dashTimer = DashDuration;
		_dashCooldownTimer = DashCooldown;
		velocity.X = DashSpeed * direction;
		velocity.Y = 0f;
	}

	protected void PlayAnimation(string animName, bool force = false)
	{
		if (force || _sprite.Animation != new StringName(animName))
			_sprite.Play(animName);
	}

	public override void _ExitTree()
	{
		if (_playerHealth != null)
			_playerHealth.HealthChanged -= OnHealthChanged;
	}
}
