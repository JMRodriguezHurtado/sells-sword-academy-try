using Godot;
using System.Collections.Generic;

// A Tetris-style inventory grid that holds items at specific positions.
// Items occupy multiple cells based on their Shape.
// The grid can grow over time (story milestones, secret rooms unlock more rows/columns).
public partial class InventoryGrid : Resource
{
	[Export] public int Width { get; set; } = 3;
	[Export] public int Height { get; set; } = 4;

	// Each placed item, with its top-left anchor position and its current shape (post-rotation)
	public List<PlacedItem> PlacedItems { get; private set; } = new();

	// Returns true if the item fits at the given anchor position with the given shape
	public bool CanPlaceItem(Item item, Vector2I anchor, List<Vector2I> shape)
	{
		foreach (var offset in shape)
		{
			Vector2I cell = anchor + offset;

			// Out of bounds
			if (cell.X < 0 || cell.X >= Width || cell.Y < 0 || cell.Y >= Height)
				return false;

			// Cell occupied by another item?
			if (GetItemAtCell(cell) != null)
				return false;
		}
		return true;
	}

	// Places an item at the anchor with the given shape (rotated or not).
	// Returns true if successful.
	public bool PlaceItem(Item item, Vector2I anchor, List<Vector2I> shape)
	{
		if (!CanPlaceItem(item, anchor, shape))
			return false;

		PlacedItems.Add(new PlacedItem
		{
			Item = item,
			Anchor = anchor,
			CurrentShape = new List<Vector2I>(shape)
		});
		return true;
	}

	// Tries to place an item anywhere it fits. Tries all 4 rotations.
	// Returns true on success.
	public bool TryAutoPlace(Item item)
	{
		for (int rotation = 0; rotation < 4; rotation++)
		{
			var shape = item.GetRotatedShape(rotation);
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					var anchor = new Vector2I(x, y);
					if (CanPlaceItem(item, anchor, shape))
					{
						return PlaceItem(item, anchor, shape);
					}
				}
			}
		}
		return false;
	}

	// Removes an item from the grid
	public bool RemoveItem(Item item)
	{
		var placed = PlacedItems.Find(p => p.Item == item);
		if (placed == null) return false;
		PlacedItems.Remove(placed);
		return true;
	}

	// Returns the item occupying a given cell, or null if empty
	public Item GetItemAtCell(Vector2I cell)
	{
		foreach (var placed in PlacedItems)
		{
			foreach (var offset in placed.CurrentShape)
			{
				if (placed.Anchor + offset == cell)
					return placed.Item;
			}
		}
		return null;
	}

	// Tries to add a potion — first stacks onto existing matching potions, then places remainder.
	// Returns the number of items that didn't fit (0 = all fit).
	public int TryAddPotion(Potion newPotion)
	{
		int remaining = newPotion.CurrentStack;

		// Try stacking on existing matching potions first
		foreach (var placed in PlacedItems)
		{
			if (placed.Item is Potion existing && existing.Id == newPotion.Id && !existing.IsFull)
			{
				int added = existing.AddToStack(remaining);
				remaining -= added;
				if (remaining <= 0) return 0;
			}
		}

		// Place remainder as new stacks
		while (remaining > 0)
		{
			var newStack = new Potion
			{
				Id = newPotion.Id,
				DisplayName = newPotion.DisplayName,
				Description = newPotion.Description,
				IconPath = newPotion.IconPath,
				Effect = newPotion.Effect,
				EffectAmount = newPotion.EffectAmount,
				MaxStack = newPotion.MaxStack,
				CurrentStack = Mathf.Min(remaining, newPotion.MaxStack),
				Shape = new List<Vector2I>(newPotion.Shape)
			};

			if (!TryAutoPlace(newStack))
				return remaining; // No space left

			remaining -= newStack.CurrentStack;
		}

		return 0;
	}

	// Resize the grid (for backpack upgrades). Items that no longer fit return as overflow.
	public List<Item> Resize(int newWidth, int newHeight)
	{
		var overflow = new List<Item>();
		var oldItems = new List<PlacedItem>(PlacedItems);

		Width = newWidth;
		Height = newHeight;
		PlacedItems.Clear();

		// Try to re-place everything in the new grid
		foreach (var placed in oldItems)
		{
			if (!TryAutoPlace(placed.Item))
				overflow.Add(placed.Item);
		}

		return overflow;
	}
}

// Represents an item placed at a specific position in the grid with a specific rotation
public partial class PlacedItem : Resource
{
	public Item Item { get; set; }
	public Vector2I Anchor { get; set; }
	public List<Vector2I> CurrentShape { get; set; } = new();
}
