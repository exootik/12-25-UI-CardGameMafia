using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlotView : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _description;
    [SerializeField] private TMP_Text _price;
    [SerializeField] private TMP_Text _pricePurchase;
    [SerializeField] private Image _quality;

    [Header("Forced Size")]
    [SerializeField] private float forcedWidth =200f;
    [SerializeField] private float forcedHeight =300f;

    public event System.Action<CardSlotView> OnRemoveRequested;
    public event System.Action<CardSlotView> OnDiscardRequested;
    public event System.Action<Vector3> OnDragMove;
    public event System.Action UnDrag;

    public Card CurrentCard { get; private set; }

    public bool BlockVisualDrag { get; set; } = false;

    // Header: UpdateUISlot - populate UI from Slot and enforce forced size
    public void UpdateUISlot(Slot slot, bool isActive)
    {
        gameObject.SetActive(isActive);

        if (!isActive)
            return;

        if (slot == null || slot.CardSlot == null)
        {
            CurrentCard = null;
            if (_nameText != null) _nameText.text = string.Empty;
            if (_iconImage != null) { _iconImage.sprite = null; _iconImage.enabled = false; }
            if (_description != null) _description.text = string.Empty;
            if (_price != null) _price.text = string.Empty;
            if (_pricePurchase != null) _pricePurchase.text = string.Empty;
            if (_quality != null) _quality.sprite = null;

            EnsureForcedSize();
            return;
        }

        CurrentCard = slot.CardSlot;

        if (_nameText != null) _nameText.text = slot.CardSlot.Name ?? string.Empty;
        if (_iconImage != null)
        {
            _iconImage.sprite = slot.CardSlot.Icon;
            _iconImage.enabled = slot.CardSlot.Icon != null;
        }
        if (_description != null) _description.text = slot.CardSlot.Description ?? string.Empty;
        if (_price != null) _price.text = slot.CardSlot.Price.ToString();
        if (_pricePurchase != null) _pricePurchase.text = slot.CardSlot.PricePurchase.ToString();
        if (_quality != null) _quality.sprite = slot.CardSlot.Quality;

        EnsureForcedSize();
    }

    // Header: SetCard - populate UI from Card instance and enforce forced size
    public void SetCard(Card card, bool isActive)
    {
        gameObject.SetActive(isActive);

        if (!isActive)
            return;

        if (card == null)
        {
            CurrentCard = null;
            if (_nameText != null) _nameText.text = string.Empty;
            if (_iconImage != null) { _iconImage.sprite = null; _iconImage.enabled = false; }
            if (_description != null) _description.text = string.Empty;
            if (_price != null) _price.text = string.Empty;
            if (_pricePurchase != null) _pricePurchase.text = string.Empty;
            if (_quality != null) _quality.sprite = null;

            EnsureForcedSize();
            return;
        }

        CurrentCard = card;

        if (_nameText != null) _nameText.text = card.Name ?? string.Empty;
        if (_iconImage != null)
        {
            _iconImage.sprite = card.Icon;
            _iconImage.enabled = card.Icon != null;
        }
        if (_description != null) _description.text = card.Description ?? string.Empty;
        if (_price != null) _price.text = card.Price.ToString();
        if (_pricePurchase != null) _pricePurchase.text = card.Price.ToString();
        if (_quality != null) _quality.sprite = card.Quality;

        EnsureForcedSize();
    }

    // Header: RequestDiscard - notify discard request
    public void RequestDiscard()
    {
        OnDiscardRequested?.Invoke(this);
    }

    // Header: RequestRemove - notify remove request
    public void RequestRemove()
    {
        OnRemoveRequested?.Invoke(this);
    }

    // Header: Dragged - forward pointer position to controller
    public void Dragged(BaseEventData pointer)
    {
        PointerEventData _pointer = (PointerEventData)pointer;
        OnDragMove?.Invoke(new Vector3(_pointer.position.x, _pointer.position.y,0f));
    }

    // Header: UnDragEnd - forward un-drag event to controller
    public void UnDragEnd()
    {
        UnDrag?.Invoke();
    }

    // Header: EnsureForcedSize - enforce exact size for this card view
    private void EnsureForcedSize()
    {
        var rect = GetComponent<RectTransform>();
        if (rect == null) return;

        var layout = GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = gameObject.AddComponent<LayoutElement>();
        }

        layout.minWidth = forcedWidth;
        layout.minHeight = forcedHeight;
        layout.preferredWidth = forcedWidth;
        layout.preferredHeight = forcedHeight;
        layout.flexibleWidth =0f;
        layout.flexibleHeight =0f;

        rect.anchorMin = new Vector2(0.5f,0.5f);
        rect.anchorMax = new Vector2(0.5f,0.5f);
        rect.pivot = new Vector2(0.5f,0.5f);

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, forcedWidth);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, forcedHeight);

        rect.localScale = Vector3.one;
    }
}
