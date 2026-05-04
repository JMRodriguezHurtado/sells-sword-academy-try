using Godot;
using System.Collections.Generic;

public enum CursorSection
{
	EquippedMainHand,
	EquippedOffHand,
	WeaponsGrid,
	UtilsGrid
}

// Read-only inventory display for Phase 2.
// Shows the active character's inventory: portrait, bio, equipped weapons,
// weapons grid, utils grid, and keyring.
// Phase 2.5 will add drag-and-drop and equip/unequip interactions.
public partial class InventoryScreen : CanvasLayer
{
	[Export] public int CellSize = 48;
	[Export] public int CellMargin = 4;
	[Export] public int KeyIconSize = 40;
	[Export] public int KeyIconMargin = 6;

	private CursorSection _cursorSection = CursorSection.WeaponsGrid;
	private Vector2I _cursorCell = new Vector2I(0, 0);  // position within current grid section
	private ColorRect _cursorHighlight;                  // visual indicator
	[Export] public Color CursorColor = new Color(1, 1, 0, 0.4f);  // yellow translucent
	[Export] public int CursorBorderWidth = 3;
	private Button _closeButton;
	private Label _bioLabel;
	private ColorRect _mainHandSlot;
	private ColorRect _offHandSlot;
	private Control _weaponsGridContainer;
	private Control _utilsGridContainer;
	private Control _keyringContainer;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		_closeButton = GetNode<Button>("MainPanel/Root/TitleBar/CloseButton");
		_bioLabel = GetNode<Label>("MainPanel/Root/Content/LeftPanel/BioLabel");
		_mainHandSlot = GetNode<ColorRect>("MainPanel/Root/Content/RightPanel/EquippedSlots/MainHandSlot");
		_offHandSlot = GetNode<ColorRect>("MainPanel/Root/Content/RightPanel/EquippedSlots/OffHandSlot");
		_weaponsGridContainer = GetNode<Control>("MainPanel/Root/Content/RightPanel/WeaponsGridContainer");
		_utilsGridContainer = GetNode<Control>("MainPanel/Root/Content/RightPanel/UtilsGridContainer");
		_keyringContainer = GetNode<Control>("MainPanel/Root/Content/RightPanel/KeyringContainer");
		_closeButton.Pressed += OnClosePressed;

		InventoryManager.Instance.GetActiveInventory().InventoryChanged += Refresh;

		Visible = false;
	}

public void Open()
	{
		Visible = true;
		Refresh();
		_cursorSection = CursorSection.WeaponsGrid;
		_cursorCell = new Vector2I(0, 0);
		UpdateCursorPosition();
	}

	public void Close()
	{
		Visible = false;
	}

	private void OnClosePressed()
	{
		Close();
	}

public void Refresh()
	{
		var inventory = InventoryManager.Instance.GetActiveInventory();

		UpdateBio();
		UpdateEquipped(inventory);
		UpdateGrid(_weaponsGridContainer, inventory.WeaponsGrid);
		UpdateGrid(_utilsGridContainer, inventory.UtilsGrid);
		UpdateKeyring(inventory);
		UpdateCursorPosition();  // NEW — keep cursor visible after grid redraws
	}

	private void UpdateBio()
	{
		_bioLabel.Text = "David — A skilled warrior trained in the art of swordsmanship. " +
						 "His journey is just beginning.";
	}

	private void UpdateEquipped(Inventory inventory)
	{
		_mainHandSlot.Color = inventory.MainHand != null
			? GetColorForWeapon(inventory.MainHand)
			: new Color(0.2f, 0.2f, 0.2f, 1.0f);

		_offHandSlot.Color = inventory.OffHand != null
			? GetColorForWeapon(inventory.OffHand)
			: new Color(0.2f, 0.2f, 0.2f, 1.0f);
	}

	private void UpdateGrid(Control container, InventoryGrid grid)
	{
		foreach (Node child in container.GetChildren())
			child.QueueFree();

		int totalWidth = grid.Width * (CellSize + CellMargin) + CellMargin;
		int totalHeight = grid.Height * (CellSize + CellMargin) + CellMargin;
		container.CustomMinimumSize = new Vector2(totalWidth, totalHeight);

		for (int y = 0; y < grid.Height; y++)
		{
			for (int x = 0; x < grid.Width; x++)
			{
				var cell = new ColorRect
				{
					Color = new Color(0.15f, 0.15f, 0.15f, 1.0f),
					Position = new Vector2(
						x * (CellSize + CellMargin) + CellMargin,
						y * (CellSize + CellMargin) + CellMargin),
					Size = new Vector2(CellSize, CellSize)
				};
				container.AddChild(cell);
			}
		}

		foreach (var placed in grid.PlacedItems)
			DrawPlacedItem(container, placed);
	}

	private void DrawPlacedItem(Control container, PlacedItem placed)
	{
		Color color = GetColorForItem(placed.Item);

		foreach (var offset in placed.CurrentShape)
		{
			Vector2I cell = placed.Anchor + offset;
			var rect = new ColorRect
			{
				Color = color,
				Position = new Vector2(
					cell.X * (CellSize + CellMargin) + CellMargin,
					cell.Y * (CellSize + CellMargin) + CellMargin),
				Size = new Vector2(CellSize, CellSize)
			};
			container.AddChild(rect);
		}

		var label = new Label
		{
			Text = GetItemDisplayText(placed.Item),
			Position = new Vector2(
				placed.Anchor.X * (CellSize + CellMargin) + CellMargin + 4,
				placed.Anchor.Y * (CellSize + CellMargin) + CellMargin + 4),
			Size = new Vector2(CellSize - 8, CellSize - 8)
		};
		label.AddThemeColorOverride("font_color", new Color(1, 1, 1));
		label.AddThemeFontSizeOverride("font_size", 10);
		label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		container.AddChild(label);
	}

	// Renders the keys as small gold icons in a horizontal row.
	// Player cannot interact with them, just sees what they have.
	private void UpdateKeyring(Inventory inventory)
	{
		foreach (Node child in _keyringContainer.GetChildren())
			child.QueueFree();

		var keys = inventory.Keyring;

		if (keys.Count == 0)
		{
			var emptyLabel = new Label
			{
				Text = "(no keys collected yet)",
				Position = new Vector2(0, 10)
			};
			emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
			emptyLabel.AddThemeFontSizeOverride("font_size", 11);
			_keyringContainer.AddChild(emptyLabel);
			return;
		}

		for (int i = 0; i < keys.Count; i++)
		{
			var key = keys[i];

			// Gold icon for the key
			var icon = new ColorRect
			{
				Color = new Color(0.95f, 0.85f, 0.2f),
				Position = new Vector2(i * (KeyIconSize + KeyIconMargin * 4), 0),
				Size = new Vector2(KeyIconSize, KeyIconSize),
				TooltipText = key.DisplayName // hover shows full name
			};
			_keyringContainer.AddChild(icon);

			// Short text label below icon (just the location or display name shortened)
			var label = new Label
			{
				Text = key.DisplayName,
				Position = new Vector2(i * (KeyIconSize + KeyIconMargin * 4), KeyIconSize + 2),
				Size = new Vector2(KeyIconSize + KeyIconMargin * 4 - 4, 20)
			};
			label.AddThemeFontSizeOverride("font_size", 10);
			label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
			_keyringContainer.AddChild(label);
		}
	}

	private string GetItemDisplayText(Item item)
	{
		if (item is Potion p)
			return $"{p.DisplayName}\n{p.CurrentStack}/{p.MaxStack}";
		return item.DisplayName;
	}

	private Color GetColorForItem(Item item)
	{
		if (item is Weapon w)
			return GetColorForWeapon(w);
		if (item is Potion)
			return new Color(0.4f, 0.85f, 0.4f);
		if (item is Key)
			return new Color(0.95f, 0.85f, 0.2f);
		return new Color(0.7f, 0.7f, 0.7f);
	}

	private Color GetColorForWeapon(Weapon weapon)
	{
		return weapon.Type switch
		{
			Weapon.WeaponType.Sword  => new Color(0.3f, 0.5f, 0.9f),
			Weapon.WeaponType.Dagger => new Color(0.9f, 0.3f, 0.3f),
			Weapon.WeaponType.Axe    => new Color(0.85f, 0.55f, 0.2f),
			Weapon.WeaponType.Hammer => new Color(0.55f, 0.55f, 0.55f),
			Weapon.WeaponType.Shield => new Color(0.4f, 0.75f, 0.4f),
			Weapon.WeaponType.Staff  => new Color(0.7f, 0.4f, 0.85f),
			Weapon.WeaponType.Bow    => new Color(0.9f, 0.85f, 0.3f),
			_ => new Color(0.8f, 0.8f, 0.8f)
		};
	}
	
	private void CreateCursorHighlight()
	{
		_cursorHighlight = new ColorRect
		{
			Color = CursorColor,
			Size = new Vector2(CellSize, CellSize),
			MouseFilter = Control.MouseFilterEnum.Ignore  // don't block clicks
		};
		// We'll add it to whichever container the cursor is currently in
	}

private void UpdateCursorPosition()
	{
		// Remove the old cursor if it exists and is still valid
		if (_cursorHighlight != null && IsInstanceValid(_cursorHighlight))
		{
			_cursorHighlight.QueueFree();
			_cursorHighlight = null;
		}

		// Create a fresh cursor
		_cursorHighlight = new ColorRect
		{
			Color = CursorColor,
			MouseFilter = Control.MouseFilterEnum.Ignore
		};

		// Find target container based on section
		Control container = null;
		Vector2 cellPos = Vector2.Zero;
		Vector2 cellSize = new Vector2(CellSize, CellSize);

		switch (_cursorSection)
		{
			case CursorSection.WeaponsGrid:
				container = _weaponsGridContainer;
				cellPos = new Vector2(
					_cursorCell.X * (CellSize + CellMargin) + CellMargin,
					_cursorCell.Y * (CellSize + CellMargin) + CellMargin);
				break;

			case CursorSection.UtilsGrid:
				container = _utilsGridContainer;
				cellPos = new Vector2(
					_cursorCell.X * (CellSize + CellMargin) + CellMargin,
					_cursorCell.Y * (CellSize + CellMargin) + CellMargin);
				break;

			case CursorSection.EquippedMainHand:
				container = _mainHandSlot.GetParent<Control>();
				cellPos = _mainHandSlot.Position;
				cellSize = _mainHandSlot.Size;
				break;

			case CursorSection.EquippedOffHand:
				container = _offHandSlot.GetParent<Control>();
				cellPos = _offHandSlot.Position;
				cellSize = _offHandSlot.Size;
				break;
		}

		if (container != null)
		{
			_cursorHighlight.Position = cellPos;
			_cursorHighlight.Size = cellSize;
			container.AddChild(_cursorHighlight);
		}
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (!Visible) return;

		if (@event.IsActionPressed("move_up")) HandleMove(0, -1);
		else if (@event.IsActionPressed("move_down")) HandleMove(0, 1);
		else if (@event.IsActionPressed("move_left")) HandleMove(-1, 0);
		else if (@event.IsActionPressed("move_right")) HandleMove(1, 0);
	}

	private void HandleMove(int dx, int dy)
	{
		var inventory = InventoryManager.Instance.GetActiveInventory();

		switch (_cursorSection)
		{
			case CursorSection.WeaponsGrid:
				MoveInWeaponsGrid(dx, dy, inventory);
				break;

			case CursorSection.UtilsGrid:
				MoveInUtilsGrid(dx, dy, inventory);
				break;

			case CursorSection.EquippedMainHand:
				MoveFromMainHand(dx, dy);
				break;

			case CursorSection.EquippedOffHand:
				MoveFromOffHand(dx, dy);
				break;
		}

		UpdateCursorPosition();
	}

	private void MoveInWeaponsGrid(int dx, int dy, Inventory inventory)
	{
		var grid = inventory.WeaponsGrid;
		Vector2I newPos = _cursorCell + new Vector2I(dx, dy);

		// Moving up from top row → go to equipped slots
		if (newPos.Y < 0)
		{
			// Choose main hand if cursor is in left half, off hand if right half
			_cursorSection = _cursorCell.X < grid.Width / 2
				? CursorSection.EquippedMainHand
				: CursorSection.EquippedOffHand;
			return;
		}

		// Moving down from bottom row → go to utils grid
		if (newPos.Y >= grid.Height)
		{
			_cursorSection = CursorSection.UtilsGrid;
			// Snap into utils grid at same column (clamped)
			int utilsX = Mathf.Clamp(_cursorCell.X, 0, inventory.UtilsGrid.Width - 1);
			_cursorCell = new Vector2I(utilsX, 0);
			return;
		}

		// Horizontal clamping — don't wrap, just stop at edges
		if (newPos.X < 0 || newPos.X >= grid.Width) return;

		_cursorCell = newPos;
	}

	private void MoveInUtilsGrid(int dx, int dy, Inventory inventory)
	{
		var grid = inventory.UtilsGrid;
		Vector2I newPos = _cursorCell + new Vector2I(dx, dy);

		// Moving up from top row → go back to weapons grid (at the bottom row)
		if (newPos.Y < 0)
		{
			_cursorSection = CursorSection.WeaponsGrid;
			int weaponsX = Mathf.Clamp(_cursorCell.X, 0, inventory.WeaponsGrid.Width - 1);
			_cursorCell = new Vector2I(weaponsX, inventory.WeaponsGrid.Height - 1);
			return;
		}

		// Moving down past the bottom of utils does nothing (no section below)
		if (newPos.Y >= grid.Height) return;

		// Horizontal clamping
		if (newPos.X < 0 || newPos.X >= grid.Width) return;

		_cursorCell = newPos;
	}

	private void MoveFromMainHand(int dx, int dy)
	{
		// Moving right from main hand → off hand
		if (dx > 0)
		{
			_cursorSection = CursorSection.EquippedOffHand;
			return;
		}

		// Moving down → into weapons grid (top-left)
		if (dy > 0)
		{
			_cursorSection = CursorSection.WeaponsGrid;
			_cursorCell = new Vector2I(0, 0);
			return;
		}

		// Up or left from main hand: nothing
	}

	private void MoveFromOffHand(int dx, int dy)
	{
		// Moving left from off hand → main hand
		if (dx < 0)
		{
			_cursorSection = CursorSection.EquippedMainHand;
			return;
		}

		// Moving down → into weapons grid (top-right area)
		if (dy > 0)
		{
			_cursorSection = CursorSection.WeaponsGrid;
			var inventory = InventoryManager.Instance.GetActiveInventory();
			_cursorCell = new Vector2I(inventory.WeaponsGrid.Width - 1, 0);
			return;
		}

		// Up or right from off hand: nothing
	}
	public override void _Process(double delta)
	{
		if (!Visible) return;

		if (Input.IsActionJustPressed("attack1"))  // Q
			HandleAction();
		else if (Input.IsActionJustPressed("attack2"))  // E — alt action
			HandleAltAction();
	}

	private void HandleAction()
	{
		var inventory = InventoryManager.Instance.GetActiveInventory();

		switch (_cursorSection)
		{
			case CursorSection.WeaponsGrid:
				ActionOnWeaponsGrid(inventory);
				break;

			case CursorSection.UtilsGrid:
				ActionOnUtilsGrid(inventory);
				break;

			case CursorSection.EquippedMainHand:
				ActionOnEquipped(inventory, mainHand: true);
				break;

			case CursorSection.EquippedOffHand:
				ActionOnEquipped(inventory, mainHand: false);
				break;
		}
	}

	private void ActionOnWeaponsGrid(Inventory inventory)
	{
		Item item = inventory.WeaponsGrid.GetItemAtCell(_cursorCell);
		if (item is not Weapon weapon)
		{
			GD.Print("No weapon to equip on this cell.");
			return;
		}

		// Try MainHand first, then OffHand
		if (inventory.MainHand == null)
		{
			inventory.EquipWeapon(weapon, toMainHand: true);
			GD.Print($"Equipped {weapon.DisplayName} to Main Hand");
		}
		else if (inventory.OffHand == null)
		{
			inventory.EquipWeapon(weapon, toMainHand: false);
			GD.Print($"Equipped {weapon.DisplayName} to Off Hand");
		}
		else
		{
			GD.Print("Both hands are full. Unequip a weapon first (E on equipped slot).");
		}
	}

	private void ActionOnUtilsGrid(Inventory inventory)
	{
		Item item = inventory.UtilsGrid.GetItemAtCell(_cursorCell);
		if (item is not Potion potion)
		{
			GD.Print("Nothing to use on this cell.");
			return;
		}

		// For now, just consume one and print the effect.
		// Later we'll wire this to actually heal David, restore mana, etc.
		if (potion.ConsumeOne())
		{
			GD.Print($"Used {potion.DisplayName} ({potion.Effect}: {potion.EffectAmount})");

			// If the stack is now empty, remove it from the grid
			if (potion.IsEmpty)
			{
				inventory.UtilsGrid.RemoveItem(potion);
				inventory.EmitSignal(Inventory.SignalName.InventoryChanged);
			}
			else
			{
				// Just refresh to update the stack count display
				Refresh();
			}
		}
	}

	private void ActionOnEquipped(Inventory inventory, bool mainHand)
	{
		// Q on equipped = same as E on equipped: unequip back to grid
		if (inventory.UnequipWeapon(mainHand))
		{
			string side = mainHand ? "Main Hand" : "Off Hand";
			GD.Print($"Unequipped from {side}");
		}
		else
		{
			GD.Print("No weapon to unequip or no space in grid.");
		}
	}
	private void HandleAltAction()
	{
		var inventory = InventoryManager.Instance.GetActiveInventory();

		switch (_cursorSection)
		{
			case CursorSection.WeaponsGrid:
				AltActionOnWeaponsGrid(inventory);
				break;

			case CursorSection.UtilsGrid:
				AltActionOnUtilsGrid(inventory);
				break;

			case CursorSection.EquippedMainHand:
				AltActionOnEquipped(inventory, mainHand: true);
				break;

			case CursorSection.EquippedOffHand:
				AltActionOnEquipped(inventory, mainHand: false);
				break;
		}
	}

	private void AltActionOnWeaponsGrid(Inventory inventory)
	{
		// Find the placed item under the cursor
		Item item = inventory.WeaponsGrid.GetItemAtCell(_cursorCell);
		if (item == null)
		{
			GD.Print("Nothing to rotate on this cell.");
			return;
		}

		// Find the PlacedItem entry to know its current shape
		PlacedItem placed = null;
		foreach (var p in inventory.WeaponsGrid.PlacedItems)
		{
			if (p.Item == item)
			{
				placed = p;
				break;
			}
		}
		if (placed == null) return;

		// Try the next rotation (90°). Remove and try to re-place at the same anchor.
		var newShape = item.GetRotatedShape(GetRotationDelta(item.Shape, placed.CurrentShape) + 1);
		Vector2I anchor = placed.Anchor;

		inventory.WeaponsGrid.RemoveItem(item);

		// Try to place rotated at same anchor
		if (inventory.WeaponsGrid.PlaceItem(item, anchor, newShape))
		{
			GD.Print($"Rotated {item.DisplayName}");
			inventory.EmitSignal(Inventory.SignalName.InventoryChanged);
			return;
		}

		// If it didn't fit, try auto-place anywhere with the new rotation
		// First put the original back so it doesn't disappear if everything fails
		if (inventory.WeaponsGrid.PlaceItem(item, anchor, placed.CurrentShape))
		{
			GD.Print($"Cannot rotate {item.DisplayName} here — no space.");
		}
	}

	// Determines how many 90° rotations the current shape is from the original
	private int GetRotationDelta(System.Collections.Generic.List<Vector2I> original, System.Collections.Generic.List<Vector2I> current)
	{
		for (int rot = 0; rot < 4; rot++)
		{
			var test = new Item { Shape = original }.GetRotatedShape(rot);
			if (ShapesMatch(test, current)) return rot;
		}
		return 0;
	}

	private bool ShapesMatch(System.Collections.Generic.List<Vector2I> a, System.Collections.Generic.List<Vector2I> b)
	{
		if (a.Count != b.Count) return false;
		foreach (var p in a)
			if (!b.Contains(p)) return false;
		return true;
	}

	private void AltActionOnUtilsGrid(Inventory inventory)
	{
		Item item = inventory.UtilsGrid.GetItemAtCell(_cursorCell);
		if (item is not Potion potion)
		{
			GD.Print("Nothing to drop on this cell.");
			return;
		}

		// Drop one from the stack
		if (potion.ConsumeOne())
		{
			GD.Print($"Dropped one {potion.DisplayName} (remaining: {potion.CurrentStack})");

			if (potion.IsEmpty)
			{
				inventory.UtilsGrid.RemoveItem(potion);
				inventory.EmitSignal(Inventory.SignalName.InventoryChanged);
			}
			else
			{
				Refresh();
			}
		}
	}

	private void AltActionOnEquipped(Inventory inventory, bool mainHand)
	{
		if (inventory.UnequipWeapon(mainHand))
		{
			string side = mainHand ? "Main Hand" : "Off Hand";
			GD.Print($"Unequipped from {side}");
		}
		else
		{
			GD.Print("No weapon to unequip or no space in grid.");
		}
	}
}
