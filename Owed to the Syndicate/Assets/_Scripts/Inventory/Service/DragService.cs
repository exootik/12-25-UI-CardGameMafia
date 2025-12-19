using UnityEngine;

/// <summary>
/// Service: Manages drag and drop logic and zone detection.
/// </summary>
public class DragService
{
    private readonly RectTransform dragZone;
    private readonly RectTransform targetZone;
    private readonly InventoryEvents events;

    private CardSlotController currentDraggingCard;
    private bool isDragging;
    private Vector3 currentCardPosition;

    public bool IsDragging => isDragging;
    public CardSlotController CurrentDraggingCard => currentDraggingCard;

    public DragService(RectTransform dragZone, RectTransform targetZone, InventoryEvents events)
    {
        this.dragZone = dragZone;
        this.targetZone = targetZone;
        this.events = events;
    }

    /// <summary>
    /// Starts dragging a card.
    /// </summary>
    public void StartDrag(CardSlotController card)
    {
        Debug.Log("DragService: StartDrag called.");
        if (card == null) return;

        currentDraggingCard = card;
        isDragging = true;

        // Obtenir la position initiale de la carte
        RectTransform cardRect = card.GetComponent<RectTransform>();
        if (cardRect != null)
        {
            currentCardPosition = cardRect.position;
        }

        events?.CardDragStarted(card);
    }

    /// <summary>
    /// Updates drag position and checks zones.
    /// </summary>
    public void UpdateDrag(CardSlotController card, Vector3 pointerPosition)
    {
        if (!isDragging || card != currentDraggingCard) return;

        // Mettre à jour la position de la carte
        RectTransform cardRect = card.GetComponent<RectTransform>();
        if (cardRect != null)
        {
            currentCardPosition = cardRect.position;
        }

        events?.CardDragMoved(card, pointerPosition);

        if (IsOutsideDragZone(pointerPosition))
        {
            Card cardData = card.AssignedSlot?.CardSlot;
            if (cardData != null && cardData.Effect != null)
            {
                // Passer à la fois la position de la carte ET la position du cercle
                events?.EffectPreviewShow(currentCardPosition, pointerPosition, cardData.Effect);
            }
        }
        else
        {
            events?.EffectPreviewHide();
        }
    }

    /// <summary>
    /// Ends drag and returns whether card should be played.
    /// </summary>
    public bool EndDrag(CardSlotController card, Vector3 pointerPosition)
    {
        if (!isDragging || card != currentDraggingCard)
        {
            return false;
        }

        bool wasOutside = IsOutsideDragZone(pointerPosition);

        currentDraggingCard = null;
        isDragging = false;
        events?.CardDragEnded(card, pointerPosition);
        events?.EffectPreviewHide();

        return wasOutside;
    }

    /// <summary>
    /// Checks if pointer is outside valid drag zone.
    /// </summary>
    public bool IsOutsideDragZone(Vector3 pointerPosition)
    {
        if (dragZone == null) return true;
        return !RectTransformUtility.RectangleContainsScreenPoint(dragZone, pointerPosition);
    }

    /// <summary>
    /// Checks if pointer is in target zone.
    /// </summary>
    public bool IsInTargetZone(Vector3 pointerPosition)
    {
        if (targetZone == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(targetZone, pointerPosition);
    }

    /// <summary>
    /// Gets target zone position for snapping.
    /// </summary>
    public Vector3 GetTargetZonePosition()
    {
        return targetZone != null ? targetZone.position : Vector3.zero;
    }

    /// <summary>
    /// Gets the current position of the dragged card.
    /// </summary>
    public Vector3 GetCurrentCardPosition()
    {
        return currentCardPosition;
    }
}