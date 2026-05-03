using Godot;

// Temporary seed script — adds sample items to David's inventory at startup
// so the inventory UI has something to display during Phase 2 testing.
// Delete this once the real pickup/loot system is in place.
public partial class InventorySeed : Node
{
	public override void _Ready()
	{
		var inventory = InventoryManager.Instance.GetInventory(InventoryManager.CharacterId.David);

// Use the new factory pattern to create a Short Sword
		var sword = ShortSword.Create();
		inventory.AddItem(sword);

		// 15 health potions (will stack into 1 slot)
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

		GD.Print("Inventory seeded with sample items.");
	}
}
