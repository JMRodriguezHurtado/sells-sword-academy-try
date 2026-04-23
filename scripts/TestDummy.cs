using Godot;

public partial class TestDummy : StaticBody2D
{
	[Export] public int MaxHealth = 100;

	private int _currentHealth;
	private Label _label;
	private Sprite2D _sprite;

	public override void _Ready()
	{
		_currentHealth = MaxHealth;
		_label = GetNode<Label>("Label");
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_label.Text = $"HP: {_currentHealth}";

		// Must be in enemy group so hitbox can find it
		AddToGroup("enemy");
	}

	public void TakeDamage(int amount)
	{
		_currentHealth -= amount;
		_currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);
		_label.Text = $"HP: {_currentHealth}";
		GD.Print($"Dummy took {amount} damage! HP: {_currentHealth}/{MaxHealth}");

		// Flash red to show hit
		_sprite.Modulate = new Color(1, 0, 0);
		GetTree().CreateTimer(0.15f).Timeout += () => _sprite.Modulate = new Color(1, 1, 1);

		if (_currentHealth <= 0)
		{
			GD.Print("Dummy defeated!");
			_label.Text = "DEAD";
		}
	}
}
