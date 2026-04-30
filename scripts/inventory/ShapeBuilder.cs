using Godot;
using System.Collections.Generic;

// Helper for creating common item shapes without manually listing offsets every time.
// All shapes use (0,0) as the anchor in the top-left of their bounding box.
// X = horizontal (right is positive), Y = vertical (down is positive).
public static class ShapeBuilder
{
	// Single 1x1 cell — potions, small trinkets, gems
	public static List<Vector2I> Single() => new()
	{
		new Vector2I(0, 0)
	};

	// Horizontal line: width x 1
	public static List<Vector2I> HorizontalLine(int width)
	{
		var shape = new List<Vector2I>();
		for (int x = 0; x < width; x++)
			shape.Add(new Vector2I(x, 0));
		return shape;
	}

	// Vertical line: 1 x height — daggers, short swords
	public static List<Vector2I> VerticalLine(int height)
	{
		var shape = new List<Vector2I>();
		for (int y = 0; y < height; y++)
			shape.Add(new Vector2I(0, y));
		return shape;
	}

	// Rectangle: width x height — books, large potions, shields
	public static List<Vector2I> Rectangle(int width, int height)
	{
		var shape = new List<Vector2I>();
		for (int y = 0; y < height; y++)
			for (int x = 0; x < width; x++)
				shape.Add(new Vector2I(x, y));
		return shape;
	}

	// L-shape: vertical part + horizontal foot — axes, hammers
	// Example: LShape(2, 3) creates an L that's 2 tall on the left, with a 3-wide foot
	//   X
	//   X
	//   X X X
	public static List<Vector2I> LShape(int height, int footWidth)
	{
		var shape = new List<Vector2I>();
		for (int y = 0; y < height; y++)
			shape.Add(new Vector2I(0, y));
		for (int x = 1; x < footWidth; x++)
			shape.Add(new Vector2I(x, height - 1));
		return shape;
	}

	// T-shape — fancy weapons, polearms
	//   X X X
	//     X
	//     X
	public static List<Vector2I> TShape(int topWidth, int stemHeight)
	{
		var shape = new List<Vector2I>();
		int center = topWidth / 2;
		for (int x = 0; x < topWidth; x++)
			shape.Add(new Vector2I(x, 0));
		for (int y = 1; y < stemHeight + 1; y++)
			shape.Add(new Vector2I(center, y));
		return shape;
	}
}
