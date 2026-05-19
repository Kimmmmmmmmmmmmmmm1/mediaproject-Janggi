using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SettingsKeyBindingSelector : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private TextMeshProUGUI keyText;
    [SerializeField] private bool isInteractive = true;

    private Button button;
    private string labelBaseText = string.Empty;
    private string keyBaseText = string.Empty;
    private bool isSelected;
    private bool isEditing;
    private FocusMode focusMode = FocusMode.Label;

    private enum FocusMode
    {
        Label,
        Key
    }

    public Button ButtonControl => button;
    public Selectable SelectableControl => button;
    public bool IsEditing => isEditing;

    public event Action OnEditModeEntered;
    public event Action OnEditModeExited;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (labelText != null && string.IsNullOrEmpty(labelBaseText))
        {
            labelBaseText = labelText.text;
        }

        if (keyText != null && string.IsNullOrEmpty(keyBaseText))
        {
            keyBaseText = StripMarkers(keyText.text);
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
        focusMode = FocusMode.Label;
        RefreshVisuals();
    }

    public void SetLabel(string label)
    {
        labelBaseText = label ?? string.Empty;
        RefreshVisuals();
    }

    public void SetKeyText(string keyName)
    {
        keyBaseText = keyName ?? string.Empty;
        RefreshVisuals();
    }

    public void EnterEditMode()
    {
        if (isEditing)
        {
            return;
        }

        isEditing = true;
        RefreshVisuals();
        OnEditModeEntered?.Invoke();
        SettingPanelView.Instance?.NotifySelectorEditModeChanged(true);
    }

    public void ExitEditMode()
    {
        if (!isEditing)
        {
            return;
        }

        isEditing = false;
        RefreshVisuals();
        OnEditModeExited?.Invoke();
        SettingPanelView.Instance?.NotifySelectorEditModeChanged(false);
    }

    public void FocusLabel()
    {
        focusMode = FocusMode.Label;
        RefreshVisuals();
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        if (!isEditing)
        {
            focusMode = FocusMode.Label;
        }

        RefreshVisuals();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        isEditing = false;
        focusMode = FocusMode.Label;
        RefreshVisuals();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (button == null || !isInteractive)
        {
            return;
        }

        if (!isSelected)
        {
            return;
        }

        if (isEditing)
        {
            return;
        }

        if (focusMode == FocusMode.Label)
        {
            focusMode = FocusMode.Key;
            RefreshVisuals();
            EnterEditMode();
            return;
        }

        if (!isEditing)
        {
            EnterEditMode();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SettingPanelView.Instance?.NotifyMouseInteraction();

        if (button == null)
        {
            return;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        OnSubmit(null);
    }

    private void RefreshVisuals()
    {
        bool hideSelectionMarker = SettingPanelView.Instance != null && SettingPanelView.Instance.IsMouseInteractionMode;
        Color textColor = isInteractive ? Color.white : new Color(0.7f, 0.7f, 0.7f);

        if (labelText != null)
        {
            bool showLabelMarker = isSelected && !isEditing && focusMode == FocusMode.Label && !hideSelectionMarker;
            labelText.text = showLabelMarker ? $">{labelBaseText}<" : labelBaseText;
            labelText.color = textColor;
        }

        if (keyText != null)
        {
            string displayText = string.IsNullOrEmpty(keyBaseText) ? string.Empty : keyBaseText;
            if (isEditing)
            {
                keyText.text = ">Press Key...<";
            }
            else if (isSelected && focusMode == FocusMode.Key && !hideSelectionMarker)
            {
                keyText.text = $">{displayText}<";
            }
            else
            {
                keyText.text = displayText;
            }
            keyText.color = textColor;
        }

        if (button != null)
        {
            button.interactable = isInteractive;
        }
    }

    private static string StripMarkers(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        string trimmed = text.Trim();
        if (trimmed.StartsWith(">") && trimmed.EndsWith("<") && trimmed.Length >= 2)
        {
            return trimmed.Substring(1, trimmed.Length - 2).Trim();
        }

        return trimmed;
    }
}
