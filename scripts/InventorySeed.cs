using Godot;

// Temporary seed script — adds sample items to David's inventory at startup
// so we have things to display and test combat with during development.
// Delete this once the real pickup/loot system is in place.
public partial class InventorySeed : Node
{
	public override void _Ready()
	{
		var inventory = InventoryManager.Instance.GetInventory(InventoryManager.CharacterId.David);

		// Create and equip a Short Sword
		var sword = ShortSword.Create();
		inventory.AddItem(sword);
		inventory.EquipWeapon(sword, toMainHand: true);

		// 15 health potions
		var potions = new Potion
		{
			Id = "health_potion",
			DisplayName = "Health Potion",
			Effect = Potion.PotionEffect.Heal,
			EffectAmount = 25,
			CurrentStack = 15,
			MaxStack = 20,
			Shape = ShapeBuilder.Single()
		};
		inventory.AddItem(potions);

		// A library key
		var libraryKey = new Key
		{
			Id = "key_library",
			DisplayName = "Library Key",
			DoorId = "castle_library_door",
			Location = "Castle Library"
		};
		inventory.AddItem(libraryKey);

		GD.Print($"Inventory seeded. Equipped: {inventory.MainHand?.DisplayName ?? "(nothing)"}");
	}
}
