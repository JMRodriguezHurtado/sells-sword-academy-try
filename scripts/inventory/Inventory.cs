using Godot;
using System.Collections.Generic;

// Top-level inventory for a single character.
// Combines the weapons grid, utils grid, equipped weapons, and keyring.
public partial class Inventory : Resource
{
	// Two separate Tetris-style grids
	public InventoryGrid WeaponsGrid { get; set; } = new InventoryGrid { Width = 3, Height = 4 };
	public InventoryGrid UtilsGrid { get; set; } = new InventoryGrid { Width = 3, Height = 3 };

	// Equipped weapons — these don't occupy grid space
	public Weapon MainHand { get; private set; }
	public Weapon OffHand { get; private set; }

	// Keyring — keys cannot be dropped, no grid layout, just a list
	public List<Key> Keyring { get; private set; } = new();

	// Signals for UI to react to inventory changes
	[Signal] public delegate void InventoryChangedEventHandler();
	[Signal] public delegate void EquipmentChangedEventHandler();

	// === EQUIPPING ===

	// Equips a weapon to the appropriate hand. The previously equipped weapon
	// (if any) goes back into the WeaponsGrid. Returns true if successful.
	public bool EquipWeapon(Weapon weapon, bool toMainHand)
	{
		if (weapon == null) return false;

		// Validate hand compatibility
		if (toMainHand && weapon.Hand == Weapon.WeaponHand.OffHand) return false;
		if (!toMainHand && weapon.Hand == Weapon.WeaponHand.MainHand) return false;

		// Remove from grid first
		if (!WeaponsGrid.RemoveItem(weapon)) return false;

		// Move currently equipped weapon back to grid (if any)
		Weapon previouslyEquipped = toMainHand ? MainHand : OffHand;
		if (previouslyEquipped != null)
		{
			if (!WeaponsGrid.TryAutoPlace(previouslyEquipped))
			{
				// Not enough room — abort the swap, put the new weapon back
				WeaponsGrid.TryAutoPlace(weapon);
				return false;
			}
		}

		// Equip the new weapon
		if (toMainHand)
			MainHand = weapon;
		else
			OffHand = weapon;

		EmitSignal(SignalName.EquipmentChanged);
		EmitSignal(SignalName.InventoryChanged);
		return true;
	}

	// Unequips a weapon and returns it to the grid. Returns false if no space.
	public bool UnequipWeapon(bool fromMainHand)
	{
		Weapon weapon = fromMainHand ? MainHand : OffHand;
		if (weapon == null) return false;

		if (!WeaponsGrid.TryAutoPlace(weapon))
			return false; // No room — keep it equipped

		if (fromMainHand)
			MainHand = null;
		else
			OffHand = null;

		EmitSignal(SignalName.EquipmentChanged);
		EmitSignal(SignalName.InventoryChanged);
		return true;
	}

	// === ADDING ITEMS ===

	// Routes the item to the correct grid or list based on its type.
	// Returns true if the item was successfully added.
	public bool AddItem(Item item)
	{
		if (item == null) return false;

		bool result = item switch
		{
			Key key => AddKey(key),
			Potion potion => UtilsGrid.TryAddPotion(potion) == 0,
			Weapon weapon => WeaponsGrid.TryAutoPlace(weapon),
			_ => UtilsGrid.TryAutoPlace(item)
		};

		if (result)
			EmitSignal(SignalName.InventoryChanged);

		return result;
	}

	private bool AddKey(Key key)
	{
		// Don't add duplicate keys (same DoorId)
		foreach (var existing in Keyring)
		{
			if (existing.DoorId == key.DoorId)
				return false;
		}
		Keyring.Add(key);
		return true;
	}

	// === REMOVING ITEMS ===

	public bool RemoveItem(Item item)
	{
		if (item == null) return false;
		if (!item.CanBeDropped) return false;

		bool result = false;
		if (WeaponsGrid.RemoveItem(item) || UtilsGrid.RemoveItem(item))
			result = true;

		if (result)
			EmitSignal(SignalName.InventoryChanged);

		return result;
	}

	// === GRID UPGRADES ===

	public void UpgradeWeaponsGrid(int newWidth, int newHeight)
	{
		var overflow = WeaponsGrid.Resize(newWidth, newHeight);
		EmitSignal(SignalName.InventoryChanged);

		if (overflow.Count > 0)
			GD.Print($"Warning: {overflow.Count} items didn't fit after weapons grid upgrade.");
	}

	public void UpgradeUtilsGrid(int newWidth, int newHeight)
	{
		var overflow = UtilsGrid.Resize(newWidth, newHeight);
		EmitSignal(SignalName.InventoryChanged);

		if (overflow.Count > 0)
			GD.Print($"Warning: {overflow.Count} items didn't fit after utils grid upgrade.");
	}

	// === QUERIES ===

	public bool HasKey(string doorId)
	{
		foreach (var key in Keyring)
		{
			if (key.DoorId == doorId)
				return true;
		}
		return false;
	}

	public Weapon GetEquippedWeapon(bool mainHand) => mainHand ? MainHand : OffHand;
}
