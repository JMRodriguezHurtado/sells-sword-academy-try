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
	// === NAMED WEAPON SHAPES ===
	// Each weapon class uses these so dimensions live in one place.
	// If we ever want to make swords slightly bigger, change here once.

	// Short sword: 1 wide × 3 tall
	public static List<Vector2I> ShortSword() => VerticalLine(3);

	// Long sword: 1 wide × 4 tall — needs both hands
	public static List<Vector2I> LongSword() => VerticalLine(4);

	// Dagger: 1 wide × 2 tall
	public static List<Vector2I> Dagger() => VerticalLine(2);

	// Battle axe: 3 tall on the left + 2-wide foot at the bottom (L-shape)
	public static List<Vector2I> BattleAxe() => LShape(3, 2);

	// Hammer: similar to axe but heavier — bigger foot
	public static List<Vector2I> Hammer() => LShape(3, 3);

	// Shield: 2 wide × 2 tall block
	public static List<Vector2I> Shield() => Rectangle(2, 2);

	// Staff: 1 wide × 4 tall — needs both hands
	public static List<Vector2I> Staff() => VerticalLine(4);

	// Bow: T-shape — 3-wide top with a 2-cell stem
	public static List<Vector2I> Bow() => TShape(3, 2);
	
}
