using Godot;

public partial class PlayerHealth : Node
{
	[Export] public int MaxHealth = 100;
	[Export] public float IFrameDuration = 1.0f;
	[Export] public float FlashInterval = 0.1f;

	public int CurrentHealth { get; private set; }
	public bool IsInvincible { get; private set; } = false;
	public bool IsDead { get; private set; } = false;

	private float _iFrameTimer = 0f;
	private float _flashTimer = 0f;
	private bool _flashVisible = true;
	private CharacterBody2D _player;
	private AnimatedSprite2D _sprite;
	private Vector2 _startPosition;

	// Signal so other systems (HUD, enemies) can react to health changes
	[Signal] public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
	[Signal] public delegate void PlayerDiedEventHandler();

	public override void _Ready()
	{
		CurrentHealth = MaxHealth;
		_player = GetParent<CharacterBody2D>();
		_sprite = _player.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_startPosition = _player.GlobalPosition;
	}

	public override void _Process(double delta)
	{
		if (!IsInvincible) return;

		float fDelta = (float)delta;

		// Count down iframes
		_iFrameTimer -= fDelta;

		// Flash the sprite on and off
		_flashTimer -= fDelta;
		if (_flashTimer <= 0f)
		{
			_flashVisible = !_flashVisible;
			_sprite.Visible = _flashVisible;
			_flashTimer = FlashInterval;
		}

		// iFrames finished — restore visibility
		if (_iFrameTimer <= 0f)
		{
			IsInvincible = false;
			_sprite.Visible = true;
		}
	}

	public void TakeDamage(int amount)
	{
		// Ignore damage if already invincible or dead
		if (IsInvincible || IsDead) return;

		CurrentHealth -= amount;
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);

		GD.Print($"Player took {amount} damage. HP: {CurrentHealth}/{MaxHealth}");

		if (CurrentHealth <= 0)
			Die();
		else
			StartIFrames();
	}

	public void Heal(int amount)
	{
		if (IsDead) return;

		CurrentHealth += amount;
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
		GD.Print($"Player healed {amount}. HP: {CurrentHealth}/{MaxHealth}");
	}

	private void StartIFrames()
	{
		IsInvincible = true;
		_iFrameTimer = IFrameDuration;
		_flashTimer = FlashInterval;
	}

	private void Die()
	{
		IsDead = true;
		_sprite.Visible = true;
		EmitSignal(SignalName.PlayerDied);
		GD.Print("Player died — respawning...");
		Respawn();
	}

	private void Respawn()
	{
		CurrentHealth = MaxHealth;
		IsDead = false;
		IsInvincible = false;
		_sprite.Visible = true;

		// Ask GameManager where to respawn
		Vector2 respawnPos = GameManager.Instance != null
			? GameManager.Instance.GetRespawnPosition(_startPosition)
			: _startPosition;

		_player.GlobalPosition = respawnPos;

		EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
		GD.Print($"Respawned at {respawnPos}");
	}
}
