using Godot;
using System.Collections.Generic;

// Base class for everything that can live in an inventory.
// Items are pure data — they describe what something IS, not how it behaves.
// Behavior (using a potion, swinging a weapon) is handled by character controllers.
public partial class Item : Resource
{
	[Export] public string Id { get; set; } = "";              // unique identifier, e.g. "short_sword"
	[Export] public string DisplayName { get; set; } = "";     // shown in UI
	[Export] public string Description { get; set; } = "";     // shown in UI
	[Export] public string IconPath { get; set; } = "";        // path to icon texture (Phase 2)
	[Export] public bool CanBeDropped { get; set; } = true;    // keys override this to false

	// Shape of the item in the inventory grid (relative offsets from anchor)
	public List<Vector2I> Shape { get; set; } = new() { new Vector2I(0, 0) }; // 1x1 default

	// Returns shape rotated 90° clockwise
	public List<Vector2I> GetRotatedShape(int rotations = 1)
	{
		var result = new List<Vector2I>(Shape);
		for (int i = 0; i < rotations; i++)
		{
			var rotated = new List<Vector2I>();
			foreach (var offset in result)
				rotated.Add(new Vector2I(-offset.Y, offset.X));
			result = rotated;
		}
		return result;
	}

	// Returns the bounding box width and height of the shape
	public Vector2I GetBoundingBox(List<Vector2I> shape = null)
	{
		shape ??= Shape;
		if (shape.Count == 0) return new Vector2I(1, 1);

		int minX = int.MaxValue, minY = int.MaxValue;
		int maxX = int.MinValue, maxY = int.MinValue;
		foreach (var offset in shape)
		{
			if (offset.X < minX) minX = offset.X;
			if (offset.Y < minY) minY = offset.Y;
			if (offset.X > maxX) maxX = offset.X;
			if (offset.Y > maxY) maxY = offset.Y;
		}
		return new Vector2I(maxX - minX + 1, maxY - minY + 1);
	}
}
