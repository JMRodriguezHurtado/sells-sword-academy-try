using Godot;

public partial class Checkpoint : Area2D
{
	[Export] public bool IsActivated { get; private set; } = false;

	private CollisionShape2D _collisionShape;

	[Signal] public delegate void CheckpointActivatedEventHandler();

	public override void _Ready()
	{
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (IsActivated) return;
		if (body is not PlayerController) return;

		IsActivated = true;
		GameManager.Instance.SetCheckpoint(GlobalPosition);
		EmitSignal(SignalName.CheckpointActivated);
		GD.Print("Checkpoint activated!");
	}
}
