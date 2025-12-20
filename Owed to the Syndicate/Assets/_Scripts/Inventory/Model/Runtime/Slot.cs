[System.Serializable]
public enum CardLocation
{
    Draw,
    Hand,
    Discard
}

[System.Serializable]
public class Slot
{
    // Card assigned to this slot (null when empty)
    public Card CardSlot { get; set; }

    // state of the card in this slot
    public CardLocation Location { get; set; } = CardLocation.Draw;

    public bool IsEmpty => CardSlot == null;

    public void SetItem(Card card, CardLocation location = CardLocation.Hand)
    {
        CardSlot = card;
        Location = location;
    }
}
