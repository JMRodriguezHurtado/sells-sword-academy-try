using Godot;

// Global singleton that manages game pause state.
// Add this as an autoload in Project Settings.
public partial class PauseManager : Node
{
	public static PauseManager Instance { get; private set; }

	[Signal] public delegate void GamePausedEventHandler();
	[Signal] public delegate void GameResumedEventHandler();

	public bool IsPaused { get; private set; } = false;

	public override void _Ready()
	{
		// Singleton pattern
		if (Instance != null)
		{
			QueueFree();
			return;
		}
		Instance = this;

		// This node should always process even when paused
		ProcessMode = ProcessModeEnum.Always;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("pause"))
			TogglePause();
	}

	public void TogglePause()
	{
		if (IsPaused)
			Resume();
		else
			Pause();
	}

	public void Pause()
	{
		if (IsPaused) return;
		IsPaused = true;
		GetTree().Paused = true;
		EmitSignal(SignalName.GamePaused);
		GD.Print("Game paused");
	}

	public void Resume()
	{
		if (!IsPaused) return;
		IsPaused = false;
		GetTree().Paused = false;
		EmitSignal(SignalName.GameResumed);
		GD.Print("Game resumed");
	}
}
