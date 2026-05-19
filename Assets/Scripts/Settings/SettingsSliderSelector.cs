using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SettingsSliderSelector : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private float step = 1f;
    [SerializeField] private bool showAsInteger = true;
    [SerializeField] private bool isInteractive = true;

    private Slider slider;
    private string labelBaseText = string.Empty;
    private bool isSelected;
    private bool isEditing;
    private int editFrame;

    public event Action<float> OnValueChanged;

    public Slider SliderControl => slider;
    public Selectable SelectableControl => slider;
    public float Value => slider != null ? slider.value : 0f;
    public bool IsEditing => isEditing;

    private void Awake()
    {
        slider = GetComponent<Slider>();

        if (labelText != null && string.IsNullOrEmpty(labelBaseText))
        {
            labelBaseText = labelText.text;
        }

        if (slider != null)
        {
            slider.onValueChanged.AddListener(HandleSliderChanged);
        }

        RefreshVisuals();
    }

    private void OnEnable()
    {
        RefreshVisuals();
    }

    private void OnDisable()
    {
        isSelected = false;
        isEditing = false;
        RefreshVisuals();
    }

    private void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(HandleSliderChanged);
        }
    }

    private void Update()
    {
        if (!isSelected || !isEditing || slider == null || !isInteractive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            Adjust(-1f);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            Adjust(1f);
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.frameCount == editFrame) return; // Prevent same-frame exit
            ExitEditMode();
        }
    }

    public void SetLabel(string label)
    {
        labelBaseText = label ?? string.Empty;
        RefreshVisuals();
    }

    public void SetValueWithoutNotify(float value)
    {
        if (slider == null)
        {
            return;
        }

        slider.SetValueWithoutNotify(value);
        RefreshVisuals();
    }

    public void SetInteractable(bool interactable)
    {
        if (slider != null)
        {
            slider.interactable = interactable;
        }
    }

    public void EnterEditMode()
    {
        isEditing = true;
        editFrame = Time.frameCount;
        RefreshVisuals();
    }

    public void ExitEditMode()
    {
        isEditing = false;
        RefreshVisuals();
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        isEditing = false;  // Reset to label selection state
        RefreshVisuals();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        isEditing = false;
        RefreshVisuals();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (slider == null || !isInteractive)
        {
            return;
        }

        if (!isEditing)
        {
            isEditing = true;
            editFrame = Time.frameCount;
            RefreshVisuals();
            return;
        }

        ExitEditMode();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SettingPanelView.Instance?.NotifyMouseInteraction();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        if (IsValueTextPointerHit(eventData))
        {
            Adjust(1f);
            ExitEditMode();
            return;
        }

        OnSubmit(null);
    }

    private void HandleSliderChanged(float value)
    {
        RefreshVisuals();
        OnValueChanged?.Invoke(value);
    }

    private void Adjust(float direction)
    {
        if (slider == null)
        {
            return;
        }

        float delta = step * direction;
        float nextValue = Mathf.Clamp(slider.value + delta, slider.minValue, slider.maxValue);
        slider.value = nextValue; // Update standard slider value, which fires HandleSliderChanged
    }

    private bool IsValueTextPointerHit(PointerEventData eventData)
    {
        if (valueText == null || eventData == null)
        {
            return false;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(
            valueText.rectTransform,
            eventData.position,
            eventData.pressEventCamera);
    }

    private void RefreshVisuals()
    {
        bool hideSelectionMarker = SettingPanelView.Instance != null && SettingPanelView.Instance.IsMouseInteractionMode;
        Color textColor = isInteractive ? Color.white : new Color(0.7f, 0.7f, 0.7f); // 밝은 회색

        if (labelText != null)
        {
            labelText.text = (isSelected && !isEditing && !hideSelectionMarker) ? $">{labelBaseText}<" : labelBaseText;
            labelText.color = textColor;
        }

        UpdateValueText(textColor);

        if (slider != null)
        {
            slider.interactable = isInteractive;
        }
    }

    private void UpdateValueText(Color textColor)
    {
        if (valueText == null || slider == null)
        {
            return;
        }

        string text = showAsInteger ? Mathf.RoundToInt(slider.value).ToString() : slider.value.ToString("0.##");
        valueText.text = isEditing ? $">{text}<" : text;
        valueText.color = textColor;
    }
}
