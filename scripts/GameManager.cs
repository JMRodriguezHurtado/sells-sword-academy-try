using Godot;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	public Vector2 LastCheckpointPosition { get; private set; } = Vector2.Zero;
	private bool _hasCheckpoint = false;

	public override void _Ready()
	{
		// Singleton pattern — one GameManager exists for the whole game
		if (Instance != null)
		{
			QueueFree();
			return;
		}
		Instance = this;
	}

	public void SetCheckpoint(Vector2 position)
	{
		LastCheckpointPosition = position;
		_hasCheckpoint = true;
		GD.Print($"Checkpoint saved at {position}");
	}

	public Vector2 GetRespawnPosition(Vector2 fallbackPosition)
	{
		return _hasCheckpoint ? LastCheckpointPosition : fallbackPosition;
	}
}
