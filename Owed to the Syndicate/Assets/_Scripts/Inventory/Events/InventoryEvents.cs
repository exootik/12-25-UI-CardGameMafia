using System;
using UnityEngine;

/// <summary>
/// Centralized event system for inventory operations.
/// Decouples components and allows loose coupling.
/// </summary>
public class InventoryEvents
{
    public event Action<Slot> OnCardDrawn;
    public event Action<Slot> OnCardDiscarded;
    public event Action<Card> OnCardRemoved;
    public event Action OnInventoryChanged;

    public event Action<CardSlotController, bool> OnCardHoverChanged;
    public event Action<CardSlotController> OnCardDragStarted;
    public event Action<CardSlotController, Vector3> OnCardDragMoved;
    public event Action<CardSlotController, Vector3> OnCardDragEnded;

    public event Action<Vector3, CardEffect> OnCardEffectRequested;

    // Événement mis à jour : position de la carte + position du cercle
    public event Action<Vector3, Vector3, CardEffect> OnEffectPreviewShow;
    public event Action OnEffectPreviewHide;

    public event Action OnPileCountsChanged;

    public event Action OnShuffleRequested;
    public event Action OnShuffleCompleted;

    public void CardDrawn(Slot slot) => OnCardDrawn?.Invoke(slot);
    public void CardDiscarded(Slot slot) => OnCardDiscarded?.Invoke(slot);
    public void CardRemoved(Card card) => OnCardRemoved?.Invoke(card);
    public void InventoryChanged() => OnInventoryChanged?.Invoke();

    public void CardHoverChanged(CardSlotController card, bool hovered) => OnCardHoverChanged?.Invoke(card, hovered);
    public void CardDragStarted(CardSlotController card) => OnCardDragStarted?.Invoke(card);
    public void CardDragMoved(CardSlotController card, Vector3 position) => OnCardDragMoved?.Invoke(card, position);
    public void CardDragEnded(CardSlotController card, Vector3 position) => OnCardDragEnded?.Invoke(card, position);

    public void CardEffectRequested(Vector3 position, CardEffect effect) => OnCardEffectRequested?.Invoke(position, effect);

    // Méthode mise à jour : cardPosition + circlePosition
    public void EffectPreviewShow(Vector3 cardPosition, Vector3 circlePosition, CardEffect effect)
        => OnEffectPreviewShow?.Invoke(cardPosition, circlePosition, effect);

    public void EffectPreviewHide() => OnEffectPreviewHide?.Invoke();

    public void PileCountsChanged() => OnPileCountsChanged?.Invoke();

    public void ShuffleRequested() => OnShuffleRequested?.Invoke();
    public void ShuffleCompleted() => OnShuffleCompleted?.Invoke();
}