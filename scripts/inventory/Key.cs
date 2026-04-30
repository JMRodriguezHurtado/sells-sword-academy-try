using Godot;

// A key item that opens specific doors in the world.
// Keys cannot be dropped or traded — once collected, they stay forever.
public partial class Key : Item
{
	[Export] public string DoorId { get; set; } = "";          // matches a door's identifier in the world
	[Export] public string Location { get; set; } = "";        // descriptive location, e.g. "Castle Library"

	public Key()
	{
		// Keys are always undroppable
		CanBeDropped = false;
	}
}
