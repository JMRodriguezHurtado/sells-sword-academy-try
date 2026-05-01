using Godot;

public partial class PauseMenu : CanvasLayer
{
	private Control _background;
	private Button _resumeButton;
	private Button _inventoryButton;
	private Button _settingsButton;
	private Button _quitButton;

	public override void _Ready()
	{
		// This menu must process even when game is paused
		ProcessMode = ProcessModeEnum.Always;

		_resumeButton = GetNode<Button>("MenuButtons/ResumeButton");
		_inventoryButton = GetNode<Button>("MenuButtons/InventoryButton");
		_settingsButton = GetNode<Button>("MenuButtons/SettingsButton");
		_quitButton = GetNode<Button>("MenuButtons/QuitButton");

		_resumeButton.Pressed += OnResumePressed;
		_inventoryButton.Pressed += OnInventoryPressed;
		_settingsButton.Pressed += OnSettingsPressed;
		_quitButton.Pressed += OnQuitPressed;

		// Listen to pause manager
		PauseManager.Instance.GamePaused += Show;
		PauseManager.Instance.GameResumed += Hide;

		// Start hidden
		Visible = false;
	}

	private new void Show() => Visible = true;
	private new void Hide() => Visible = false;

	private void OnResumePressed()
	{
		PauseManager.Instance.Resume();
	}

private void OnInventoryPressed()
{
	var inventoryScreen = GetTree().Root.GetNodeOrNull<InventoryScreen>("TestRoom/InventoryScreen");
	if (inventoryScreen != null)
	{
		inventoryScreen.Open();
		Hide(); // hide the pause menu so inventory is on top
	}
	else
	{
		GD.PrintErr("InventoryScreen not found in scene!");
	}
}

	private void OnSettingsPressed()
	{
		GD.Print("Settings pressed (placeholder)");
	}

	private void OnQuitPressed()
	{
		GD.Print("Quit pressed (placeholder)");
		// Future: GetTree().ChangeSceneToFile("res://scenes/ui/MainMenu.tscn");
	}

	public override void _ExitTree()
	{
		if (PauseManager.Instance != null)
		{
			PauseManager.Instance.GamePaused -= Show;
			PauseManager.Instance.GameResumed -= Hide;
		}
	}
}
