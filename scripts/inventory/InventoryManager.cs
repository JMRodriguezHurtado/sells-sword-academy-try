using Godot;
using System.Collections.Generic;

// Global singleton that holds an Inventory for each character.
// Add this as an autoload in Project Settings.
//
// Usage:
//   InventoryManager.Instance.GetInventory(CharacterId.David).AddItem(weapon);
//   InventoryManager.Instance.SetActiveCharacter(CharacterId.Clark);
public partial class InventoryManager : Node
{
	public enum CharacterId { David, Clark, Megan }

	public static InventoryManager Instance { get; private set; }

	private Dictionary<CharacterId, Inventory> _inventories = new();

	public CharacterId ActiveCharacter { get; private set; } = CharacterId.David;

	[Signal] public delegate void ActiveCharacterChangedEventHandler(int newCharacter);

	public override void _Ready()
	{
		// Singleton pattern
		if (Instance != null)
		{
			QueueFree();
			return;
		}
		Instance = this;

		// Persist across scene changes (character switching, level transitions)
		ProcessMode = ProcessModeEnum.Always;

		// Initialize one inventory per character
		_inventories[CharacterId.David] = new Inventory();
		_inventories[CharacterId.Clark] = new Inventory();
		_inventories[CharacterId.Megan] = new Inventory();

		GD.Print("InventoryManager ready — 3 character inventories initialized.");
	}

	// Returns the inventory for a specific character
	public Inventory GetInventory(CharacterId character)
	{
		return _inventories[character];
	}

	// Returns the inventory of whoever is currently being played
	public Inventory GetActiveInventory()
	{
		return _inventories[ActiveCharacter];
	}

	// Switches the active character (called by tag-rooms in the world)
	public void SetActiveCharacter(CharacterId character)
	{
		if (ActiveCharacter == character) return;
		ActiveCharacter = character;
		EmitSignal(SignalName.ActiveCharacterChanged, (int)character);
		GD.Print($"Active character switched to: {character}");
	}
}
