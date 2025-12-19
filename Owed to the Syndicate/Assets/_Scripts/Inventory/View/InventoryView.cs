using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the visual layout and animations for the hand container.
/// Handles card positioning in a fan layout and various card animations.
/// </summary>
public class InventoryView : MonoBehaviour
{
    [Header("Container")]
    [SerializeField] private RectTransform handContainer;

    [Header("Hand Layout Settings")]
    [SerializeField] private float cardSpacing = 120f;
    [SerializeField] private float fanAngle = 15f;
    [SerializeField] private float verticalCurve = 30f;
    [SerializeField] private float layoutAnimationDuration = 0.3f;

    [Header("Hover Settings")]
    [SerializeField] private float hoverYOffset = 50f;
    [SerializeField] private float hoverScale = 1.2f;
    [Header("Dynamic Spread Settings")]
    [SerializeField] private float spreadAmount = 120f;

    private Dictionary<RectTransform, Coroutine> activeAnimations = new Dictionary<RectTransform, Coroutine>();

    /// <summary>
    /// Returns the hand container transform.
    /// </summary>
    public Transform GetHandContainer()
    {
        return handContainer;
    }

    /// <summary>
    /// Layout all cards in the hand in a fan formation.
    /// </summary>
    public void LayoutHand(List<RectTransform> cardRects)
    {
        if (cardRects == null || cardRects.Count == 0)
            return;

        int totalCards = cardRects.Count;

        for (int i = 0; i < totalCards; i++)
        {
            var cardRect = cardRects[i];
            if (cardRect == null) continue;

            var transformData = GetFanTransformForIndex(i, totalCards);

            // Animer vers la nouvelle position
            AnimateToPosition(cardRect, transformData.position, transformData.rotation, Vector3.one, layoutAnimationDuration);
        }
    }

    /// <summary>
    /// Calculate the fan transform (position and rotation) for a card at a specific index.
    /// </summary>
    public (Vector3 position, Quaternion rotation) GetFanTransformForIndex(int index, int totalCards)
    {
        if (totalCards == 0)
            return (Vector3.zero, Quaternion.identity);

        // Centre de l'éventail
        float centerOffset = (totalCards - 1) * 0.5f;
        float normalizedIndex = index - centerOffset;

        // Position X (espacement horizontal)
        float xPos = normalizedIndex * cardSpacing;

        // Courbe verticale (arc parabolique)
        float normalizedPosition = totalCards > 1 ? (float)index / (totalCards - 1) : 0.5f;
        float curveT = normalizedPosition * 2f - 1f; // -1 à 1
        float yPos = -verticalCurve * (1f - curveT * curveT); // Parabole inversée

        // Rotation Z (angle de l'éventail)
        float zAngle = -normalizedIndex * fanAngle;

        Vector3 position = new Vector3(xPos, yPos, 0f);
        Quaternion rotation = Quaternion.Euler(0f, 0f, zAngle);

        return (position, rotation);
    }

    /// <summary>
    /// Animate a card to a specific position and rotation.
    /// </summary>
    private void AnimateToPosition(RectTransform rectTransform, Vector3 targetPos, Quaternion targetRot, Vector3 targetScale, float duration)
    {
        if (rectTransform == null) return;

        // Arrêter l'animation en cours pour cette carte
        if (activeAnimations.ContainsKey(rectTransform))
        {
            if (activeAnimations[rectTransform] != null)
                StopCoroutine(activeAnimations[rectTransform]);
            activeAnimations.Remove(rectTransform);
        }

        // Démarrer la nouvelle animation
        var coroutine = StartCoroutine(AnimateTransformCoroutine(rectTransform, targetPos, targetRot, targetScale, duration));
        activeAnimations[rectTransform] = coroutine;
    }

    private IEnumerator AnimateTransformCoroutine(RectTransform rectTransform, Vector3 targetPos, Quaternion targetRot, Vector3 targetScale, float duration)
    {
        Vector3 startPos = rectTransform.localPosition;
        Quaternion startRot = rectTransform.localRotation;
        Vector3 startScale = rectTransform.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easeT = EaseOutCubic(t);

            if (rectTransform != null)
            {
                rectTransform.localPosition = Vector3.Lerp(startPos, targetPos, easeT);
                rectTransform.localRotation = Quaternion.Slerp(startRot, targetRot, easeT);
                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, easeT);
            }

            yield return null;
        }

        if (rectTransform != null)
        {
            rectTransform.localPosition = targetPos;
            rectTransform.localRotation = targetRot;
            rectTransform.localScale = targetScale;
        }

        if (activeAnimations.ContainsKey(rectTransform))
            activeAnimations.Remove(rectTransform);
    }

    /// <summary>
    /// Animate card removal (fade out and scale down).
    /// </summary>
    public void AnimateCardRemove(RectTransform cardRect, Action onComplete = null)
    {
        if (cardRect == null)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(AnimateCardRemoveCoroutine(cardRect, onComplete));
    }

    private IEnumerator AnimateCardRemoveCoroutine(RectTransform cardRect, Action onComplete)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        Vector3 startScale = cardRect.localScale;
        CanvasGroup canvasGroup = cardRect.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = cardRect.gameObject.AddComponent<CanvasGroup>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (cardRect != null)
            {
                cardRect.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                canvasGroup.alpha = 1f - t;
            }

            yield return null;
        }

        if (cardRect != null)
        {
            cardRect.gameObject.SetActive(false);
        }

        onComplete?.Invoke();
    }

    /// <summary>
    /// Animate a card moving from one container to another.
    /// Used for shuffle operations (discard to draw pile).
    /// </summary>
    public void AnimateMoveCard(RectTransform cardRect, RectTransform fromContainer, RectTransform toContainer, Vector3 targetLocalPos, Quaternion targetLocalRot, Action onComplete = null)
    {
        if (cardRect == null || fromContainer == null || toContainer == null)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(AnimateMoveCardCoroutine(cardRect, fromContainer, toContainer, targetLocalPos, targetLocalRot, onComplete));
    }

    private IEnumerator AnimateMoveCardCoroutine(RectTransform cardRect, RectTransform fromContainer, RectTransform toContainer, Vector3 targetLocalPos, Quaternion targetLocalRot, Action onComplete)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        // Sauvegarder position de départ (do not change parent during animation)
        Vector3 startWorldPos = cardRect.position;
        Quaternion startWorldRot = cardRect.rotation;
        Vector3 startScale = cardRect.localScale;

        // Calculer position d'arrivée (do not change parent during animation)
        Vector3 endWorldPos = toContainer.TransformPoint(targetLocalPos);
        Quaternion endWorldRot = toContainer.rotation * targetLocalRot;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easeT = EaseInOutCubic(t);

            if (cardRect != null)
            {
                cardRect.position = Vector3.Lerp(startWorldPos, endWorldPos, easeT);
                cardRect.rotation = Quaternion.Slerp(startWorldRot, endWorldRot, easeT);

                // Optionnel : réduire la taille pendant le mouvement
                float scale = Mathf.Lerp(1f, 0.5f, Mathf.Sin(t * Mathf.PI));
                cardRect.localScale = startScale * (1f - scale * 0.3f);
            }

            yield return null;
        }

        if (cardRect != null)
        {
            // Finalize: set parent to destination container and set local transform
            cardRect.SetParent(toContainer, worldPositionStays: false);
            cardRect.localPosition = targetLocalPos;
            cardRect.localRotation = targetLocalRot;
            cardRect.localScale = startScale;
        }

        onComplete?.Invoke();
    }


    public void UpdateHandVisuals(List<RectTransform> cardRects, int hoveredIndex)
    {
        int totalCards = cardRects.Count;

        for (int i = 0; i < totalCards; i++)
        {
            var cardRect = cardRects[i];
            if (cardRect == null) continue;

            // 1. Obtenir la position de base dans l'éventail
            var transformData = GetFanTransformForIndex(i, totalCards);
            Vector3 targetPos = transformData.position;
            Vector3 targetScale = Vector3.one; // Always keep scale at 1

            // 2. Appliquer la logique d'écartement
            if (hoveredIndex != -1)
            {
                if (i == hoveredIndex)
                {
                    // La carte survolée monte but no longer scales
                    targetPos += new Vector3(0, hoverYOffset, 0);
                    cardRect.SetAsLastSibling(); // Passe devant les autres
                }
                else if (i < hoveredIndex)
                {
                    // Les cartes à gauche s'écartent vers la gauche
                    targetPos += new Vector3(-spreadAmount, 0, 0);
                }
                else if (i > hoveredIndex)
                {
                    // Les cartes à droite s'écartent vers la droite
                    targetPos += new Vector3(spreadAmount, 0, 0);
                }
            }

            // 3. Animer
            AnimateToPosition(cardRect, targetPos, transformData.rotation, targetScale, 0.15f);
        }
    }

    /// <summary>
    /// Handle card hover effect (raise and scale up).
    /// </summary>
    public void Hover(RectTransform cardRect, int cardIndex, int totalCards)
    {
        if (cardRect == null) return;

        // Keep scale unchanged on hover
        Vector3 _hoverScale = Vector3.one;
        var baseTransform = GetFanTransformForIndex(cardIndex, totalCards);

        // Move up only
        AnimateToPosition(cardRect, baseTransform.position + new Vector3(0, hoverYOffset, 0), baseTransform.rotation, _hoverScale, 0.15f);
    }


    /// <summary>
    /// Handle card hover exit (return to normal position).
    /// </summary>
    public void OnCardHoverExit(RectTransform cardRect, int cardIndex, int totalCards)
    {
        if (cardRect == null) return;

        var baseTransform = GetFanTransformForIndex(cardIndex, totalCards);
        AnimateToPosition(cardRect, baseTransform.position, baseTransform.rotation, Vector3.one, 0.15f);
    }

    // Easing functions
    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    private void OnDestroy()
    {
        // Arrêter toutes les animations en cours
        foreach (var kvp in activeAnimations)
        {
            if (kvp.Value != null)
                StopCoroutine(kvp.Value);
        }
        activeAnimations.Clear();
    }
}