using Godot;

public partial class PlayerController : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -700.0f;
	[Export] public float DashSpeed = 800.0f;
	[Export] public float DashDuration = 0.15f;
	[Export] public float DashCooldown = 0.5f;

	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private int _facingDirection = 1; // 1 = right, -1 = left
	private bool _isDashing = false;
	private float _dashTimer = 0f;
	private float _dashCooldownTimer = 0f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		float fDelta = (float)delta;

		// Tick cooldown timer down every frame
		if (_dashCooldownTimer > 0f)
			_dashCooldownTimer -= fDelta;

		// --- DASH STATE ---
		if (_isDashing)
		{
			_dashTimer -= fDelta;

			// Keep dash velocity, ignore gravity
			if (_dashTimer <= 0f)
			{
				// Dash finished
				_isDashing = false;
				velocity.X = 0f;
			}

			Velocity = velocity;
			MoveAndSlide();
			return; // Skip all other input while dashing
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

		// Track facing direction when moving
		if (direction != 0f)
			_facingDirection = direction > 0f ? 1 : -1;

		// --- DASH INPUT (only on ground, only when cooldown is ready) ---
		if (IsOnFloor() && _dashCooldownTimer <= 0f)
		{
			if (Input.IsActionJustPressed("dash_forward"))
				StartDash(ref velocity, _facingDirection);
			else if (Input.IsActionJustPressed("dash_back"))
				StartDash(ref velocity, -_facingDirection);
		}

		Velocity = velocity;
		
		MoveAndSlide();
	}

	private void StartDash(ref Vector2 velocity, int direction)
	{
		_isDashing = true;
		_dashTimer = DashDuration;
		_dashCooldownTimer = DashCooldown;
		velocity.X = DashSpeed * direction;
		velocity.Y = 0f; // Kill vertical momentum — pure horizontal burst
	}
}
