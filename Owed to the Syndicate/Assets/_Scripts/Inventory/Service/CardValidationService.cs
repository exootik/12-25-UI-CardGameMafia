/// <summary>
/// Service: Validates card actions (play, drag, etc.).
/// </summary>
public class CardValidationService
{
    /// <summary>
    /// Validates if a card can be played (enough mana, valid target, etc.).
    /// </summary>
    public bool CanPlayCard(Card card)
    {
        if (card == null) return false;

        var playerController = GameController.Instance?.GetPlayerController();
        if (playerController == null) return true;

        return playerController.HasEnoughMana(card.Price);
    }

    /// <summary>
    /// Attempts to consume resources to play a card.
    /// </summary>
    public bool TryConsumeResourcesForCard(Card card)
    {
        if (card == null) return false;

        var playerController = GameController.Instance?.GetPlayerController();
        if (playerController == null) return true;

        return playerController.TrySpendMana(card.Price);
    }

    /// <summary>
    /// Validates if a card can be dragged.
    /// </summary>
    public bool CanDragCard(Card card)
    {
        return CanPlayCard(card);
    }
}