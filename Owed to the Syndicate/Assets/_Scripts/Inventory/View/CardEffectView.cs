using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère l'affichage visuel de la zone d'effet d'une carte avec flèche
/// </summary>
public class CardEffectView : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject effectCirclePrefab;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private DragArrowView arrowView;

    private GameObject currentEffectCircle;
    private Image circleImage;
    private Vector3 currentCardPosition;
    private Vector3 currentCirclePosition;

    [Header("Visual Settings")]
    [SerializeField] private Color healColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Color damageColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private Color healArrowColor = new Color(0f, 1f, 0f, 0.8f);
    [SerializeField] private Color damageArrowColor = new Color(1f, 0f, 0f, 0.8f);
    [SerializeField] private float fadeInDuration = 0.2f;

    private void Awake()
    {
        if (arrowView == null)
        {
            // Créer automatiquement si non assigné
            GameObject arrowObj = new GameObject("DragArrow");
            arrowObj.transform.SetParent(worldCanvas.transform, false);
            arrowView = arrowObj.AddComponent<DragArrowView>();
        }
    }

    /// <summary>
    /// Affiche le cercle d'effet et la flèche depuis la carte
    /// </summary>
    public void ShowEffectCircle(Vector3 cardPosition, Vector3 circlePosition, float range, effect effectType)
    {
        currentCardPosition = cardPosition;
        currentCirclePosition = circlePosition;

        // Créer le cercle s'il n'existe pas
        if (currentEffectCircle == null)
        {
            currentEffectCircle = Instantiate(effectCirclePrefab, worldCanvas.transform);

            circleImage = currentEffectCircle.GetComponent<Image>();
            if (circleImage == null)
            {
                circleImage = currentEffectCircle.AddComponent<Image>();
            }
        }

        // Activer et positionner le cercle
        currentEffectCircle.SetActive(true);
        currentEffectCircle.transform.position = circlePosition;

        // Définir la taille en fonction du range
        RectTransform rectTransform = currentEffectCircle.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            float diameter = range * 2f;
            rectTransform.sizeDelta = new Vector2(diameter, diameter);
        }

        // Définir la couleur en fonction du type d'effet
        if (circleImage != null)
        {
            circleImage.color = effectType == effect.Heal ? healColor : damageColor;
        }

        // Afficher la flèche
        if (arrowView != null)
        {
            Color arrowColor = effectType == effect.Heal ? healArrowColor : damageArrowColor;
            arrowView.SetArrowColor(arrowColor);
            arrowView.ShowArrow(cardPosition, circlePosition);
        }
    }

    /// <summary>
    /// Met à jour les positions du cercle et de la flèche
    /// </summary>
    public void UpdateEffectCirclePosition(Vector3 cardPosition, Vector3 circlePosition)
    {
        currentCardPosition = cardPosition;
        currentCirclePosition = circlePosition;

        if (currentEffectCircle != null && currentEffectCircle.activeSelf)
        {
            currentEffectCircle.transform.position = circlePosition;
        }

        if (arrowView != null)
        {
            arrowView.UpdateArrowEndPosition(cardPosition, circlePosition);
        }
    }

    /// <summary>
    /// Cache le cercle d'effet et la flèche
    /// </summary>
    public void HideEffectCircle()
    {
        if (currentEffectCircle != null)
        {
            currentEffectCircle.SetActive(false);
        }

        if (arrowView != null)
        {
            arrowView.HideArrow();
        }
    }

    private void OnDestroy()
    {
        if (currentEffectCircle != null)
        {
            Destroy(currentEffectCircle);
        }
    }
}