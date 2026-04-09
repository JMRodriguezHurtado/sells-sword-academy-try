using Godot;

public partial class PlayerController : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -700.0f;

	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add gravity
		if (!IsOnFloor())
			velocity.Y += _gravity * (float)delta;

		// Jump
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Horizontal movement
		float direction = Input.GetAxis("ui_left", "ui_right");
		velocity.X = direction * Speed;

		Velocity = velocity;
		MoveAndSlide();
	}
}
