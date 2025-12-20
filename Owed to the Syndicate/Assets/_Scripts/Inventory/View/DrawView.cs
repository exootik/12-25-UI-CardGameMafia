using TMPro;
using UnityEngine;

public class DrawView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI CardCount;

    public void UpdateText(int count)
    {
        CardCount.text = count.ToString();
    }
}
