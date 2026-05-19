using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SettingsButtonSelector : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private TextMeshProUGUI buttonText; 

    private Button button;
    private string labelBaseText = string.Empty;
    private string buttonBaseText = string.Empty;
    private bool isSelected;
    private bool isEditing;
    private int editFrame;

    public Button ButtonControl => button;
    public Selectable SelectableControl => button;
    public bool IsEditing => isEditing;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (labelText != null && string.IsNullOrEmpty(labelBaseText))
        {
            labelBaseText = labelText.text;
        }
        
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (buttonText != null && string.IsNullOrEmpty(buttonBaseText))
        {
            buttonBaseText = buttonText.text.Trim();
            // 기본값에 > < 가 붙어있다면 제거해둡니다
            if (buttonBaseText.StartsWith(">") && buttonBaseText.EndsWith("<"))
            {
                buttonBaseText = buttonBaseText.Substring(1, buttonBaseText.Length - 2).Trim();
            }
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

    private void Update()
    {
        if (!isSelected || !isEditing || button == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.frameCount == editFrame) return; // 같은 프레임 입력 방지
            button.onClick?.Invoke();
            ExitEditMode(); // 버튼은 실행 후 편집 모드를 바로 빠져나오는 것이 자연스럽습니다.
        }
        else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            // 버튼은 좌우 방향키로 조절할 값이 없으므로 방향키를 눌러도 편집 모드를 빠져나가거나 무시합니다.
            ExitEditMode();
        }
    }

    public void SetLabel(string label)
    {
        labelBaseText = label ?? string.Empty;
        RefreshVisuals();
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
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
        isEditing = false;
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
        if (button == null)
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
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SettingPanelView.Instance?.NotifyMouseInteraction();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        OnSubmit(null);
    }

    private void RefreshVisuals()
    {
        bool hideSelectionMarker = SettingPanelView.Instance != null && SettingPanelView.Instance.IsMouseInteractionMode;

        if (labelText != null)
        {
            labelText.text = (isSelected && !isEditing && !hideSelectionMarker) ? $">{labelBaseText}<" : labelBaseText;
        }

        if (buttonText != null)
        {
            buttonText.text = isEditing ? $">{buttonBaseText}<" : buttonBaseText;
        }
    }
}