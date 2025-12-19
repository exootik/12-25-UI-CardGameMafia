using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RuntimeInventory
{
    /// <summary>
    /// Runtime representation of the inventory. Holds slots containing cards and exposes
    /// methods to modify them. Fires events to notify observers of changes.
    /// </summary>

    /// <summary>Slots that currently contain cards.</summary>
    public List<Slot> Slots { get; private set; } = new List<Slot>();

    /// <summary>Fired when any inventory change occurs.</summary>
    public event Action OnInventoryChanged;

    /// <summary>Fired when a slot is added.</summary>
    public event Action<Slot> OnSlotAdded;

    /// <summary>Fired when a slot is removed.</summary>
    public event Action<Slot> OnSlotRemoved;

    /// <summary>Fired when a slot is updated.</summary>
    public event Action<Slot> OnSlotUpdated;

    /// <summary>Create a runtime inventory.</summary>
    /// <param name="slotCount">The number of slots in the inventory.</param>
    public RuntimeInventory(int slotCount)
    {
        // no preallocated empty slots
    }

    /// <summary>Number of subscribers to OnInventoryChanged (helper).</summary>
    public int InventoryChangedSubscriberCount => OnInventoryChanged?.GetInvocationList().Length ?? 0;

    private void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    /// <summary>Add a card to the inventory at the specified location.</summary>
    /// <param name="card">The card to add.</param>
    /// <param name="location">The location to add the card to.</param>
    /// <returns>True if the item was added, false otherwise.</returns>
    public bool AddItem(Card card, CardLocation location = CardLocation.Hand)
    {
        if (card == null)
        {
            Debug.LogWarning("RuntimeInventory.AddItem: card is null");
            return false;
        }

        var slot = new Slot();
        slot.SetItem(card, location);
        Slots.Add(slot);

        OnSlotAdded?.Invoke(slot);
        NotifyInventoryChanged();
        return true;
    }

    /// <summary>Remove a card from the inventory.</summary>
    /// <param name="card">The card to remove.</param>
    /// <returns>True if the item was removed, false otherwise.</returns>
    public bool RemoveItem(Card card)
    {
        if (card == null)
        {
            Debug.LogWarning("RuntimeInventory.RemoveItem: card is null");
            return false;
        }

        var slot = Slots.FirstOrDefault(s => s.CardSlot == card);
        if (slot != null)
        {
            OnSlotRemoved?.Invoke(slot);
            Slots.Remove(slot);
            NotifyInventoryChanged();
            return true;
        }
        return false;
    }

    /// <summary>Move a card to a target location (Draw/Hand/Discard).</summary>
    /// <param name="card">The card to move.</param>
    /// <param name="targetLocation">The location to move the card to.</param>
    /// <returns>True if the card was moved, false otherwise.</returns>
    public bool MoveCard(Card card, CardLocation targetLocation)
    {
        if (card == null) return false;
        var slot = Slots.FirstOrDefault(s => s.CardSlot == card);
        if (slot == null) return false;
        slot.Location = targetLocation;
        OnSlotUpdated?.Invoke(slot);
        NotifyInventoryChanged();
        return true;
    }

    /// <summary>Get occupied slots, optionally filtered by location.</summary>
    /// <param name="location">The location to filter by.</param>
    /// <returns>An IEnumerable of occupied slots.</returns>
    public IEnumerable<Slot> GetOccupiedSlots(CardLocation? location = null)
    {
        var query = Slots.Where(s => s.CardSlot != null);
        if (location.HasValue)
            query = query.Where(s => s.Location == location.Value);
        return query;
    }

    /// <summary>Draw up to <paramref name="count"/> cards from draw pile into hand.</summary>
    /// <param name="count">The number of cards to draw.</param>
    /// <returns>A list of drawn cards.</returns>
    public List<Card> Draw(int count = 1)
    {
        var drawn = new List<Card>();
        if (count <= 0) 
            return null;

        var drawSlots = Slots.Where(s => s.CardSlot != null && s.Location == CardLocation.Draw).Take(count).ToList();

        foreach (var src in drawSlots)
        {
            src.Location = CardLocation.Hand;
            drawn.Add(src.CardSlot);
            OnSlotUpdated?.Invoke(src);
        }

        if (drawn.Count > 0)
            NotifyInventoryChanged();

        return drawn;
    }

    /// <summary>Discard a card into the discard pile.</summary>
    /// <param name="card">The card to discard.</param>
    /// <returns>True if the card was discarded, false otherwise.</returns>
    public bool Discard(Slot card)
    {
        if (card == null) return false;

        var slot = Slots.FirstOrDefault(s => s == card && s.Location == CardLocation.Hand);
        if (slot == null)
            slot = Slots.FirstOrDefault(s => s == card);
        if (slot == null) return false;

        slot.Location = CardLocation.Discard;
        OnSlotUpdated?.Invoke(slot);
        NotifyInventoryChanged();
        return true;
    }

    /// <summary>Shuffle all discard cards back into the draw pile and return moved count.</summary>
    /// <returns>The number of cards moved.</returns>
    public int ShuffleDiscardToDraw()
    {
        var discardedSlots = Slots.Where(s => s.CardSlot != null && s.Location == CardLocation.Discard).ToList();
        if (!discardedSlots.Any())
        {
            return 0;
        }

        var cards = discardedSlots.Select(s => s.CardSlot).ToList();

        foreach (var s in discardedSlots)
        {
            OnSlotRemoved?.Invoke(s);
            Slots.Remove(s);
        }

        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var tmp = cards[i];
            cards[i] = cards[j];
            cards[j] = tmp;
        }

        foreach (var card in cards)
        {
            var newSlot = new Slot();
            newSlot.SetItem(card, CardLocation.Draw);
            Slots.Add(newSlot);
            OnSlotAdded?.Invoke(newSlot);
        }

        NotifyInventoryChanged();
        return cards.Count;
    }
}
