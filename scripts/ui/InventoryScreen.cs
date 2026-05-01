using Godot;
using System.Collections.Generic;

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
}
