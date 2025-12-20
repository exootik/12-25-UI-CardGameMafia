using System;
using System.Collections;
using UnityEngine;

public class CardAnimationManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private RectTransform handContainerRect;
	[SerializeField] private GameObject drawCardBackPrefab;
	[SerializeField] private GameObject discardCardBackPrefab;

	private InventoryController inventoryController;

	[Header("Animation Settings")]
	[SerializeField] private float defaultDrawDuration =0.45f;
	[SerializeField] private float defaultDiscardDuration =0.35f;
	[SerializeField] private float drawStartScale =0.5f;
	[SerializeField] private float drawTargetScale =1f;
	[SerializeField] private float discardStartScale =1f;
	[SerializeField] private float discardTargetScale =0.5f;

	// Header: Awake - cache inventory controller reference
	private void Awake()
	{
		inventoryController = GetComponent<InventoryController>();
	}

	// Header: AnimateCardToWorld - animate a RectTransform to a world-space target
	public void AnimateCardToWorld(RectTransform cardRect, Vector3 endWorldPos, Quaternion endWorldRot, Action onComplete = null, float duration =0.45f, float startScaleFactor =1f, float targetScaleFactor =1f)
	{
		if (cardRect == null)
		{
			onComplete?.Invoke();
			return;
		}
		StartCoroutine(AnimateMoveToWorld(cardRect, endWorldPos, endWorldRot, startScaleFactor, targetScaleFactor, duration, onComplete));
	}

	// Header: AnimateMoveToWorld - coroutine to move a RectTransform in world space
	private IEnumerator AnimateMoveToWorld(RectTransform cardRect, Vector3 endWorldPos, Quaternion endWorldRot, float startScale, float targetScale, float duration, Action onComplete = null)
	{
		var originalParent = cardRect.parent as RectTransform;

		Vector3 startWorldPos = cardRect.position;
		Quaternion startWorldRot = cardRect.rotation;
		Vector3 baseScale = cardRect.localScale / startScale;

		float elapsed =0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			float easeT = EaseInOutCubic(t);

			cardRect.position = Vector3.Lerp(startWorldPos, endWorldPos, easeT);
			cardRect.rotation = Quaternion.Slerp(startWorldRot, endWorldRot, easeT);

			float currentScaleFactor = Mathf.Lerp(startScale, targetScale, easeT);
			cardRect.localScale = baseScale * currentScaleFactor;

			yield return null;
		}

		cardRect.position = endWorldPos;
		cardRect.rotation = endWorldRot;
		cardRect.localScale = baseScale * targetScale;

		if (originalParent != null)
		{
			cardRect.localPosition = originalParent.InverseTransformPoint(endWorldPos);
			cardRect.localRotation = Quaternion.Inverse(originalParent.rotation) * endWorldRot;
		}

		onComplete?.Invoke();
	}

	// Header: AnimateCardFromHandToDragged - animate card from hand into a dragged container
	public void AnimateCardFromHandToDragged(RectTransform cardRect, RectTransform targetContainer, int targetIndex, int finalHandCount, Action onComplete = null, float duration =0.45f, float startScaleFactor =0.5f, float targetScaleFactor =1f)
	{
		if (cardRect == null || handContainerRect == null)
		{
			onComplete?.Invoke();
			return;
		}

		var targetData = GetFanTransformForIndex(targetIndex, finalHandCount);
		Vector3 targetLocalPos = targetData.position;
		Quaternion targetLocalRot = targetData.rotation;

		Vector3 endWorldPos = targetContainer.TransformPoint(targetLocalPos);
		Quaternion endWorldRot = targetContainer.rotation * targetLocalRot;

		StartCoroutine(AnimateMoveToWorld(
			cardRect,
			endWorldPos,
			endWorldRot,
			startScaleFactor,
			targetScaleFactor,
			duration,
			onComplete
		));
	}

	// Header: AnimateCardFromDeckToHand - animate draw from deck into hand with flip
	public void AnimateCardFromDeckToHand(RectTransform cardRect, RectTransform deckContainer, int targetIndex, int finalHandCount, Action onComplete = null, float duration =0.45f, float startScaleFactor =0.5f, float targetScaleFactor =1f)
	{
        if (cardRect == null || deckContainer == null || handContainerRect == null)
		{
			onComplete?.Invoke();
			return;
		}

		GameObject backVisual = null;
		if (drawCardBackPrefab != null)
		{
			backVisual = Instantiate(drawCardBackPrefab, cardRect);
			backVisual.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f,0.5f);
			backVisual.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f,0.5f);
			backVisual.GetComponent<RectTransform>().SetAsLastSibling();
		}

		var targetData = GetFanTransformForIndex(targetIndex, finalHandCount);
		Vector3 targetLocalPos = targetData.position;
		Quaternion targetLocalRot = targetData.rotation;

		cardRect.SetParent(handContainerRect, worldPositionStays: true);

		StartCoroutine(AnimateMoveBetweenContainers(
			cardRect,
			deckContainer,
			handContainerRect,
			targetLocalPos,
			targetLocalRot,
			startScaleFactor,
			targetScaleFactor,
			backVisual,
			duration,
			shouldFlip: true,
			flipFromBack: true,
			onComplete: () =>
			{
				if (cardRect.parent != handContainerRect)
				{
					cardRect.SetParent(handContainerRect, worldPositionStays: false);
				}
				cardRect.localPosition = targetLocalPos;
				cardRect.localRotation = targetLocalRot;
				cardRect.localScale = Vector3.one * targetScaleFactor;
				onComplete?.Invoke();
			}
		));
	}

	// Header: AnimateCardFromHandToDiscard - animate card from hand to discard with flip
	public void AnimateCardFromHandToDiscard(RectTransform cardRect, RectTransform discardContainer, Action onComplete = null, float duration =0.35f, float startScaleFactor =1f, float targetScaleFactor =0.5f)
	{
		if (cardRect == null || discardContainer == null || handContainerRect == null)
		{
			onComplete?.Invoke();
			return;
		}

		GameObject backVisual = null;
		if (discardCardBackPrefab != null)
		{
			backVisual = Instantiate(discardCardBackPrefab, cardRect);
			backVisual.SetActive(false);
		}

		Vector3 targetLocalPos = Vector3.zero;
		Quaternion targetLocalRot = Quaternion.identity;

		StartCoroutine(AnimateMoveBetweenContainers(
			cardRect,
			handContainerRect,
			discardContainer,
			targetLocalPos,
			targetLocalRot,
			startScaleFactor,
		 targetScaleFactor,
		 backVisual,
		 duration,
		 shouldFlip: true,
		 flipFromBack: false,
		 onComplete: () =>
			{
				cardRect.SetParent(discardContainer, worldPositionStays: false);
				cardRect.localPosition = targetLocalPos;
				cardRect.localRotation = targetLocalRot;
				cardRect.localScale = Vector3.one * targetScaleFactor;
				onComplete?.Invoke();
			}
		));
	}

	// Header: AnimateCardToContainer - animate card to a target container
	public void AnimateCardToContainer(RectTransform cardRect, RectTransform targetContainer, Vector3 targetLocalPos, Quaternion targetLocalRot, Action onComplete = null, float duration =0.45f, float startScaleFactor =1f, float targetScaleFactor =1f)
	{
		if (cardRect == null || targetContainer == null)
		{
			onComplete?.Invoke();
			return;
		}

		StartCoroutine(AnimateMoveToContainer(
			cardRect,
			targetContainer,
			targetLocalPos,
			targetLocalRot,
			startScaleFactor,
			targetScaleFactor,
			duration,
			onComplete
		));
	}

	// Header: AnimateMoveBetweenContainers - core movement and optional flip animation
	private IEnumerator AnimateMoveBetweenContainers(
		RectTransform cardRect,
		RectTransform startContainer,
		RectTransform endContainer,
		Vector3 targetLocalPos,
		Quaternion targetLocalRot,
		float startScale,
		float targetScale,
		GameObject backVisual,
		float duration,
		bool shouldFlip = false,
		bool flipFromBack = true,
		Action onComplete = null)
	{
		if (cardRect == null) yield break;

		Vector3 startWorldPos = cardRect.position;
		Quaternion startWorldRot = cardRect.rotation;
		Vector3 baseScale = cardRect.localScale;

		Vector3 endWorldPos = endContainer.TransformPoint(targetLocalPos);
		Quaternion endWorldRot = endContainer.rotation * targetLocalRot;

		bool hasFlipped = false;

		if (shouldFlip && backVisual != null)
		{
			backVisual.SetActive(flipFromBack);
			var backRect = backVisual.GetComponent<RectTransform>();
			if (backRect != null)
			{
				backRect.localPosition = Vector3.zero;
				backRect.localRotation = Quaternion.identity;
				backRect.localScale = Vector3.one;
				backRect.anchorMin = Vector3.zero;
				backRect.anchorMax = Vector3.one;
				backRect.sizeDelta = Vector3.zero;
			}
		}

		Vector3 initialLocalEuler = cardRect.localEulerAngles;

		float elapsed =0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			float easeT = EaseInOutCubic(t);

			if (cardRect == null) yield break;

			cardRect.position = Vector3.Lerp(startWorldPos, endWorldPos, easeT);

			float currentScale = Mathf.Lerp(startScale, targetScale, easeT);
			cardRect.localScale = baseScale * currentScale;

			if (shouldFlip)
			{
				float flipAngle = t *180f;

				if (flipAngle >=90f && !hasFlipped)
				{
					hasFlipped = true;
					if (backVisual != null)
					{
						backVisual.SetActive(!flipFromBack);
					}
				}

				cardRect.rotation = Quaternion.Slerp(startWorldRot, endWorldRot, easeT);

				Vector3 currentEuler = cardRect.localEulerAngles;
				currentEuler.y = flipAngle;
				cardRect.localEulerAngles = currentEuler;
			}
			else
			{
				cardRect.rotation = Quaternion.Slerp(startWorldRot, endWorldRot, easeT);
			}

			yield return null;
		}

		if (cardRect != null)
		{
			cardRect.position = endWorldPos;
			cardRect.rotation = endWorldRot;
			cardRect.localScale = baseScale * targetScale;

			if (shouldFlip)
			{
				Vector3 finalEuler = cardRect.localEulerAngles;
				finalEuler.y =180f;
				cardRect.localEulerAngles = finalEuler;
			}
		}

		if (backVisual != null)
		{
			Destroy(backVisual);
		}

		onComplete?.Invoke();
	}

	// Header: AnimateMoveToContainer - helper to animate between containers
	private IEnumerator AnimateMoveToContainer(
		RectTransform cardRect,
		RectTransform endContainer,
		Vector3 targetLocalPos,
		Quaternion targetLocalRot,
		float startScale,
		float targetScale,
		float duration,
		Action onComplete = null)
	{
		var startContainer = cardRect != null ? cardRect.parent as RectTransform : null;

		yield return StartCoroutine(AnimateMoveBetweenContainers(
			cardRect,
			startContainer,
			endContainer,
			targetLocalPos,
			targetLocalRot,
			startScale,
			targetScale,
			null,
			duration,
			shouldFlip: false,
			flipFromBack: true,
			onComplete: onComplete
		));
	}

	// Header: EaseInOutCubic - easing function
	private float EaseInOutCubic(float t)
	{
		return t <0.5f ?4f * t * t * t :1f - Mathf.Pow(-2f * t +2f,3f) /2f;
	}

	// Header: GetFanTransformForIndex - compute fan position and rotation
	private (Vector3 position, Quaternion rotation) GetFanTransformForIndex(int index, int totalCards)
	{
		if (inventoryController != null)
		{
			return inventoryController.GetFanTransformForIndex(index, totalCards);
		}

		float cardWidth =100f;
		float fanSpread =20f;

		float centerOffset = (totalCards -1) *0.5f;
		float xPos = (index - centerOffset) * cardWidth;
		float angle = (index - centerOffset) * fanSpread;

		Vector3 position = new Vector3(xPos,0f,0f);
		Quaternion rotation = Quaternion.Euler(0f,0f, -angle);

		return (position, rotation);
	}

    // Header: EnsureCardsInCorrectContainers - ensure hand cards parented correctly
    public void EnsureCardsInCorrectContainers()
    {
        if (inventoryController == null) return;

        var handSlots = inventoryController.GetRuntimeInventory()?.GetOccupiedSlots(CardLocation.Hand);
        if (handSlots == null || handContainerRect == null) return;

        foreach (var slot in handSlots)
        {
            var ctrl = FindControllerForSlot(slot);
            if (ctrl == null) continue;

            var rect = ctrl.transform as RectTransform;
            if (rect != null && rect.parent != handContainerRect)
            {
                rect.SetParent(handContainerRect, worldPositionStays: false);
            }
        }
    }

    private CardSlotController FindControllerForSlot(Slot slot)
    {
        return null;
    }
}