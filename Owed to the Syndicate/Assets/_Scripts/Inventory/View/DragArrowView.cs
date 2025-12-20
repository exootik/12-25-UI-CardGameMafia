using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère l'affichage d'une flèche entre la carte draggée et le cercle d'effet
/// </summary>
public class DragArrowView : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private RectTransform arrowContainer;
    [SerializeField] private Image arrowLine;
    [SerializeField] private Image arrowHead;
    [SerializeField] private Canvas worldCanvas;

    [Header("Visual Settings")]
    [SerializeField] private Color arrowColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private float arrowWidth = 5f;
    [SerializeField] private float arrowHeadSize = 5f;
    [SerializeField] private float minDistance = 50f; // Distance minimale pour afficher la flèche

    private bool isVisible = false;

    private void Awake()
    {
        if (arrowContainer == null)
        {
            CreateArrowObjects();
        }

        HideArrow();
    }

    /// <summary>
    /// Crée les objets de flèche si non assignés
    /// </summary>
    private void CreateArrowObjects()
    {
        // Créer le conteneur
        GameObject containerObj = new GameObject("ArrowContainer");
        arrowContainer = containerObj.AddComponent<RectTransform>();
        arrowContainer.SetParent(transform, false);

        // Créer la ligne
        GameObject lineObj = new GameObject("ArrowLine");
        RectTransform lineRect = lineObj.AddComponent<RectTransform>();
        lineRect.SetParent(arrowContainer, false);
        arrowLine = lineObj.AddComponent<Image>();
        arrowLine.color = arrowColor;
        arrowLine.raycastTarget = false;

        // Créer la tête de flèche
        GameObject headObj = new GameObject("ArrowHead");
        RectTransform headRect = headObj.AddComponent<RectTransform>();
        headRect.SetParent(arrowContainer, false);
        headRect.sizeDelta = new Vector2(arrowHeadSize, arrowHeadSize);
        arrowHead = headObj.AddComponent<Image>();
        arrowHead.color = arrowColor;
        arrowHead.raycastTarget = false;

        // Créer un triangle simple pour la tête de flèche
        headObj.AddComponent<ArrowHeadShape>();
    }

    /// <summary>
    /// Affiche et met à jour la flèche entre deux positions
    /// </summary>
    /// <param name="startPos">Position de départ (carte)</param>
    /// <param name="endPos">Position d'arrivée (cercle)</param>
    public void ShowArrow(Vector3 startPos, Vector3 endPos)
    {
        // Calculer la distance
        float distance = Vector3.Distance(startPos, endPos);

        // Ne pas afficher si trop proche
        if (distance < minDistance)
        {
            HideArrow();
            return;
        }

        if (!isVisible)
        {
            arrowContainer.gameObject.SetActive(true);
            isVisible = true;
        }

        // Direction de la carte VERS le cercle
        Vector3 direction = startPos - endPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Distance à laquelle la flèche s'arrête avant le cercle
        float stopDistance = arrowHeadSize * 0.8f;

        // Position où la flèche s'arrête (avant le cercle)
        Vector3 arrowEndPos = endPos - direction.normalized * stopDistance;

        // Nouvelle distance pour la ligne (plus courte)
        float lineDistance = Vector3.Distance(startPos, arrowEndPos);

        // Position de la ligne (au milieu entre start et arrowEndPos)
        Vector3 midPoint = (startPos + arrowEndPos) / 2f;

        // Configurer la ligne
        RectTransform lineRect = arrowLine.GetComponent<RectTransform>();
        lineRect.position = midPoint;
        lineRect.sizeDelta = new Vector2(lineDistance, arrowWidth);
        lineRect.localRotation = Quaternion.Euler(0f, 0f, angle);
        lineRect.pivot = new Vector2(0.5f, 0.5f);

        // Configurer la tête de flèche
        RectTransform headRect = arrowHead.GetComponent<RectTransform>();

        // La tête est positionnée juste avant le cercle
        headRect.position = arrowEndPos;
        headRect.localRotation = Quaternion.Euler(0f, 0f, angle);
        headRect.pivot = new Vector2(0.5f, 0.5f);
    }

    /// <summary>
    /// Met à jour uniquement la position de fin de la flèche (optimisation)
    /// </summary>
    public void UpdateArrowEndPosition(Vector3 startPos, Vector3 endPos)
    {
        if (!isVisible) return;
        ShowArrow(startPos, endPos);
    }

    /// <summary>
    /// Cache la flèche
    /// </summary>
    public void HideArrow()
    {
        if (arrowContainer != null)
        {
            arrowContainer.gameObject.SetActive(false);
        }
        isVisible = false;
    }

    /// <summary>
    /// Change la couleur de la flèche
    /// </summary>
    public void SetArrowColor(Color color)
    {
        arrowColor = color;
        if (arrowLine != null) arrowLine.color = color;
        if (arrowHead != null) arrowHead.color = color;
    }

    private void OnDestroy()
    {
        if (arrowContainer != null)
        {
            Destroy(arrowContainer.gameObject);
        }
    }
}

/// <summary>
/// Composant helper pour créer une forme de triangle pour la tête de flèche
/// </summary>
public class ArrowHeadShape : MonoBehaviour
{
    private void Start()
    {
        var image = GetComponent<Image>();
        if (image != null && image.sprite == null)
        {
        }
        image.sprite = CreateTriangleSprite();

    }

    private Sprite CreateTriangleSprite()
    {
        int size = 8;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalizedY = (float)y / size;
                float normalizedX = (float)x / size;

                float triangleHeight = normalizedX; // Plus on va vers la droite, plus c'est étroit

                if (normalizedX >= 0.2f && // Commence un peu après le bord gauche
                    normalizedY >= (0.5f - triangleHeight * 0.5f) && // Limite basse
                    normalizedY <= (0.5f + triangleHeight * 0.5f))   // Limite haute
                {
                    pixels[y * size + x] = Color.white;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f)
        );
    }
}