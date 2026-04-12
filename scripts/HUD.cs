using Godot;

public partial class HUD : Control
{
	private ProgressBar _healthBar;
	private Label _healthLabel;
	private PlayerHealth _playerHealth;

	public override void _Ready()
	{
		_healthBar = GetNode<ProgressBar>("UILayer/HealthBarContainer/HealthBar");
		_healthLabel = GetNode<Label>("UILayer/HealthBarContainer/HealthLabel");

		_playerHealth = GetTree().GetFirstNodeInGroup("player") as PlayerHealth;

		if (_playerHealth != null)
		{
			_playerHealth.HealthChanged += OnHealthChanged;
			_healthBar.MaxValue = _playerHealth.MaxHealth;
			_healthBar.Value = _playerHealth.CurrentHealth;
			UpdateLabel(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
		}
		else
		{
			GD.PrintErr("HUD could not find PlayerHealth node!");
		}
	}

	private void OnHealthChanged(int currentHealth, int maxHealth)
	{
		_healthBar.MaxValue = maxHealth;
		_healthBar.Value = currentHealth;
		UpdateLabel(currentHealth, maxHealth);
	}

	private void UpdateLabel(int currentHealth, int maxHealth)
	{
		_healthLabel.Text = $"{currentHealth} / {maxHealth}";
	}

	public override void _ExitTree()
	{
		if (_playerHealth != null)
			_playerHealth.HealthChanged -= OnHealthChanged;
	}
}
