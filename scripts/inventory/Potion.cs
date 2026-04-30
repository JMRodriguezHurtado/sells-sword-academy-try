using Godot;

// A consumable item that can stack in a single inventory slot.
// Examples: health potions, mana potions, throwing daggers (if we add them later).
public partial class Potion : Item
{
	public enum PotionEffect { Heal, RestoreMana, Buff, Damage, Other }

	[Export] public PotionEffect Effect { get; set; } = PotionEffect.Heal;
	[Export] public int EffectAmount { get; set; } = 25;       // HP healed, mana restored, etc.
	[Export] public int CurrentStack { get; set; } = 1;        // how many of this potion in the slot
	[Export] public int MaxStack { get; set; } = 20;           // hard cap per slot

	// Returns how many were actually added (rest must overflow to a new slot)
	public int AddToStack(int amount)
	{
		int spaceLeft = MaxStack - CurrentStack;
		int actuallyAdded = Mathf.Min(spaceLeft, amount);
		CurrentStack += actuallyAdded;
		return actuallyAdded;
	}

	// Removes one from the stack. Returns true if there was something to remove.
	public bool ConsumeOne()
	{
		if (CurrentStack <= 0) return false;
		CurrentStack--;
		return true;
	}

	public bool IsEmpty => CurrentStack <= 0;
	public bool IsFull => CurrentStack >= MaxStack;
}
