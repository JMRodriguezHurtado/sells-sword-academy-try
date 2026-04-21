using Godot;

public partial class PlayerController : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -700.0f;
	[Export] public float DashSpeed = 800.0f;
	[Export] public float DashDuration = 0.15f;
	[Export] public float DashCooldown = 0.5f;
	[Export] public float HurtDuration = 0.4f; // how long hurt animation plays

	private AnimatedSprite2D _sprite;
	private PlayerHealth _playerHealth;
	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	private int _facingDirection = 1;
	private bool _isDashing = false;
	private bool _isBackDash = false;
	private bool _isHurt = false; // NEW
	private float _dashTimer = 0f;
	private float _dashCooldownTimer = 0f;
	private float _hurtTimer = 0f; // NEW

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_playerHealth = GetNode<PlayerHealth>("PlayerHealth");
		_playerHealth.HealthChanged += OnHealthChanged;
	}

	private void OnHealthChanged(int currentHealth, int maxHealth)
	{
		// Only trigger hurt if we actually lost health (not a heal)
		if (currentHealth < maxHealth && !_playerHealth.IsDead)
			TriggerHurt();
	}

	public void TriggerHurt()
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

		// --- HURT STATE ---
		if (_isHurt)
		{
			_hurtTimer -= fDelta;
			if (_hurtTimer <= 0f)
				_isHurt = false;

			// Still apply gravity and movement during hurt
			if (!IsOnFloor())
				velocity.Y += _gravity * fDelta;

			Velocity = velocity;
			MoveAndSlide();
			return; // Skip input and animation logic
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

		// --- DASH INPUT ---
		if (IsOnFloor() && _dashCooldownTimer <= 0f)
		{
			if (Input.IsActionJustPressed("dash_forward"))
				StartDash(ref velocity, _facingDirection, isBack: false);
			else if (Input.IsActionJustPressed("dash_back"))
				StartDash(ref velocity, -_facingDirection, isBack: true);
		}

		// --- ANIMATION ---
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

	private void StartDash(ref Vector2 velocity, int direction, bool isBack)
	{
		_isDashing = true;
		_isBackDash = isBack;
		_dashTimer = DashDuration;
		_dashCooldownTimer = DashCooldown;
		velocity.X = DashSpeed * direction;
		velocity.Y = 0f;
	}

	private void PlayAnimation(string animName)
	{
		if (_sprite.Animation != new StringName(animName))
			_sprite.Play(animName);
	}

	public override void _ExitTree()
	{
		if (_playerHealth != null)
			_playerHealth.HealthChanged -= OnHealthChanged;
	}
}
