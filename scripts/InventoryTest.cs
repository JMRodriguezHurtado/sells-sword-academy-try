using Godot;
using System.Collections.Generic;

// Temporary test script — verifies the inventory data layer works end-to-end.
// Attach to a Node in test_room.tscn, run the game, check the Output panel.
// Delete this script and the node after testing.
public partial class InventoryTest : Node
{
	public override void _Ready()
	{
		GD.Print("\n========================================");
		GD.Print("  INVENTORY DATA LAYER TEST");
		GD.Print("========================================\n");

		TestItemCreation();
		TestShapeRotation();
		TestGridPlacement();
		TestPotionStacking();
		TestEquipping();
		TestKeyring();
		TestDurability();
		TestGridResize();

		GD.Print("\n========================================");
		GD.Print("  ALL TESTS COMPLETE");
		GD.Print("========================================\n");
	}

	private void TestItemCreation()
	{
		GD.Print("--- TEST 1: Item Creation ---");

		var sword = new Weapon
		{
			Id = "short_sword",
			DisplayName = "Short Sword",
			Description = "A basic short sword.",
			Type = Weapon.WeaponType.Sword,
			Hand = Weapon.WeaponHand.MainHand,
			MaxDurability = 60,
			CurrentDurability = 60,
			Shape = ShapeBuilder.VerticalLine(3)
		};

		GD.Print($"Created: {sword.DisplayName} | Shape cells: {sword.Shape.Count} | Durability: {sword.CurrentDurability}/{sword.MaxDurability}");
		GD.Print("");
	}

	private void TestShapeRotation()
	{
		GD.Print("--- TEST 2: Shape Rotation ---");

		var axe = new Weapon
		{
			Id = "battle_axe",
			DisplayName = "Battle Axe",
			Shape = ShapeBuilder.LShape(3, 2)
		};

		GD.Print($"Original L-shape ({axe.Shape.Count} cells): {FormatShape(axe.Shape)}");
		GD.Print($"Rotated 90°:  {FormatShape(axe.GetRotatedShape(1))}");
		GD.Print($"Rotated 180°: {FormatShape(axe.GetRotatedShape(2))}");
		GD.Print($"Rotated 270°: {FormatShape(axe.GetRotatedShape(3))}");
		GD.Print("");
	}

	private void TestGridPlacement()
	{
		GD.Print("--- TEST 3: Grid Placement ---");

		var inventory = InventoryManager.Instance.GetInventory(InventoryManager.CharacterId.David);

		var dagger = new Weapon { Id = "dagger", DisplayName = "Dagger", Shape = ShapeBuilder.VerticalLine(2) };
		var sword = new Weapon { Id = "short_sword", DisplayName = "Short Sword", Shape = ShapeBuilder.VerticalLine(3) };

		bool placed1 = inventory.WeaponsGrid.TryAutoPlace(dagger);
		bool placed2 = inventory.WeaponsGrid.TryAutoPlace(sword);

		GD.Print($"Placed dagger: {placed1}");
		GD.Print($"Placed sword: {placed2}");
		GD.Print($"Items in grid: {inventory.WeaponsGrid.PlacedItems.Count}");
		GD.Print("");
	}

	private void TestPotionStacking()
	{
		GD.Print("--- TEST 4: Potion Stacking ---");

		var inventory = InventoryManager.Instance.GetInventory(InventoryManager.CharacterId.David);

		var potion1 = new Potion { Id = "health_potion", DisplayName = "Health Potion", CurrentStack = 5, MaxStack = 20 };
		var potion2 = new Potion { Id = "health_potion", DisplayName = "Health Potion", CurrentStack = 10, MaxStack = 20 };
		var potion3 = new Potion { Id = "health_potion", DisplayName = "Health Potion", CurrentStack = 15, MaxStack = 20 };

		inventory.AddItem(potion1);
		GD.Print($"Added 5 potions. Items in utils grid: {inventory.UtilsGrid.PlacedItems.Count}");

		inventory.AddItem(potion2);
		GD.Print($"Added 10 more potions. Items in utils grid: {inventory.UtilsGrid.PlacedItems.Count} (should still be 1, stacked to 15)");

		inventory.AddItem(potion3);
		GD.Print($"Added 15 more potions. Items in utils grid: {inventory.UtilsGrid.PlacedItems.Count} (should be 2, first stack at 20, second at 10)");

		// Print actual stacks
		foreach (var placed in inventory.UtilsGrid.PlacedItems)
		{
			if (placed.Item is Potion p)
				GD.Print($"  Stack at {placed.Anchor}: {p.CurrentStack}/{p.MaxStack}");
		}
		GD.Print("");
	}

	private void TestEquipping()
	{
		GD.Print("--- TEST 5: Equipping Weapons ---");

		var inventory = InventoryManager.Instance.GetInventory(InventoryManager.CharacterId.David);

		// Find the short sword we placed earlier
		Weapon swordToEquip = null;
		foreach (var placed in inventory.WeaponsGrid.PlacedItems)
		{
			if (placed.Item is Weapon w && w.Id == "short_sword")
			{
				swordToEquip = w;
				break;
			}
		}

		if (swordToEquip != null)
		{
			int gridCountBefore = inventory.WeaponsGrid.PlacedItems.Count;
			bool equipped = inventory.EquipWeapon(swordToEquip, toMainHand: true);
			int gridCountAfter = inventory.WeaponsGrid.PlacedItems.Count;

			GD.Print($"Equipped sword to main hand: {equipped}");
			GD.Print($"Grid count before: {gridCountBefore}, after: {gridCountAfter} (should be 1 less)");
			GD.Print($"Main hand: {inventory.MainHand?.DisplayName ?? "(empty)"}");
			GD.Print($"Off hand:  {inventory.OffHand?.DisplayName ?? "(empty)"}");
		}
		else
		{
			GD.Print("ERROR: short_sword not found in grid!");
		}
		GD.Print("");
	}

	private void TestKeyring()
	{
		GD.Print("--- TEST 6: Keyring ---");

		var inventory = InventoryManager.Instance.GetInventory(InventoryManager.CharacterId.David);

		var libraryKey = new Key
		{
			Id = "key_library",
			DisplayName = "Library Key",
			DoorId = "castle_library_door",
			Location = "Castle Library"
		};

		bool added = inventory.AddItem(libraryKey);
		GD.Print($"Added library key: {added}");
		GD.Print($"Has library key: {inventory.HasKey("castle_library_door")}");
		GD.Print($"Has nonexistent key: {inventory.HasKey("dungeon_door")}");

		// Try removing — should fail since keys can't be dropped
		bool removed = inventory.RemoveItem(libraryKey);
		GD.Print($"Tried to remove key (should be false): {removed}");
		GD.Print("");
	}

	private void TestDurability()
	{
		GD.Print("--- TEST 7: Weapon Durability ---");

		var inventory = InventoryManager.Instance.GetInventory(InventoryManager.CharacterId.David);
		var sword = inventory.MainHand;

		if (sword != null)
		{
			GD.Print($"Sword durability: {sword.CurrentDurability}/{sword.MaxDurability}");

			for (int i = 0; i < 5; i++)
				sword.RegisterEnemyHit();
			GD.Print($"After 5 enemy hits: {sword.CurrentDurability}/{sword.MaxDurability} (expected 55)");

			sword.RegisterWallHit();
			GD.Print($"After 1 wall hit: {sword.CurrentDurability}/{sword.MaxDurability} (expected 52)");

			sword.RepairFull();
			GD.Print($"After full repair: {sword.CurrentDurability}/{sword.MaxDurability} (expected 60)");
		}
		else
		{
			GD.Print("ERROR: no main hand weapon equipped!");
		}
		GD.Print("");
	}

	private void TestGridResize()
	{
		GD.Print("--- TEST 8: Grid Resize ---");

		var inventory = InventoryManager.Instance.GetInventory(InventoryManager.CharacterId.David);

		GD.Print($"Weapons grid before: {inventory.WeaponsGrid.Width}x{inventory.WeaponsGrid.Height}");
		GD.Print($"Items in grid: {inventory.WeaponsGrid.PlacedItems.Count}");

		inventory.UpgradeWeaponsGrid(4, 5);

		GD.Print($"Weapons grid after: {inventory.WeaponsGrid.Width}x{inventory.WeaponsGrid.Height}");
		GD.Print($"Items in grid: {inventory.WeaponsGrid.PlacedItems.Count} (should still be the same)");
		GD.Print("");
	}

	// Helper to print a shape's offsets in a readable way
	private string FormatShape(List<Vector2I> shape)
	{
		var parts = new List<string>();
		foreach (var p in shape)
			parts.Add($"({p.X},{p.Y})");
		return string.Join(", ", parts);
	}
}
