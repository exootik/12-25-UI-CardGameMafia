using UnityEngine;
using UnityEngine.UI;

public class SpawnArea : MonoBehaviour
{
    [Tooltip("RectTransform qui servira de référence (panel parent)")]
    public RectTransform rootRect;

    [Tooltip("GameObject contenant les child RectTransforms définissant les 'zones' de spawn. ")]
    public GameObject cellsContainer;

    private void Reset()
    {
        if (rootRect == null) rootRect = GetComponent<RectTransform>();
    }

    public Vector2 GetRandomLocalPoint()
    {
        if (rootRect == null) { Debug.LogWarning("[SpawnArea] rootRect null"); return Vector2.zero; }

        if (cellsContainer != null && cellsContainer.transform.childCount > 0)
        {
            int idx = Random.Range(0, cellsContainer.transform.childCount);
            var child = cellsContainer.transform.GetChild(idx) as RectTransform;
            if (child != null)
            {
                Vector3[] corners = new Vector3[4];
                child.GetWorldCorners(corners);
                float minX = corners[0].x, maxX = corners[2].x;
                float minY = corners[0].y, maxY = corners[2].y;
                Vector3 worldPoint = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);

                Vector2 localPoint = rootRect.InverseTransformPoint(worldPoint);
                return localPoint;
            }
        }

        // fallback
        return SampleInsideRectTransform(rootRect);
    }

    private Vector2 SampleInsideRectTransform(RectTransform rt)
    {
        var r = rt.rect;
        float x = Random.Range(r.xMin, r.xMax);
        float y = Random.Range(r.yMin, r.yMax);
        return new Vector2(x, y);
    }

    private void OnValidate() => Reset();
}
