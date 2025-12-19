using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ButtonCustom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Color pressedColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    public bool interactable = true;

    public event Action OnButtonClicked;

    private bool isHovering = false;

    private void Awake()
    {
        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }
    }

    private void Start()
    {
        UpdateVisualState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable) return;

        Debug.Log("[ButtonCustom] OnPointerClick");

        if (buttonImage != null)
        {
            buttonImage.color = pressedColor;
        }

        StartCoroutine(ResetColorAfterClick());

        OnButtonClicked?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;

        Debug.Log("[ButtonCustom] OnPointerEnter");
        isHovering = true;

        if (buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("[ButtonCustom] OnPointerExit");
        isHovering = false;

        UpdateVisualState();
    }

    private IEnumerator ResetColorAfterClick()
    {
        yield return new WaitForSeconds(0.1f);

        UpdateVisualState();
    }

    public void SetInteractable(bool value)
    {
        interactable = value;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (buttonImage == null) return;

        if (!interactable)
        {
            buttonImage.color = disabledColor;
        }
        else if (isHovering)
        {
            buttonImage.color = hoverColor;
        }
        else
        {
            buttonImage.color = normalColor;
        }
    }

    public void SetNormalColor()
    {
        if (buttonImage != null && interactable)
        {
            buttonImage.color = normalColor;
        }
    }
}