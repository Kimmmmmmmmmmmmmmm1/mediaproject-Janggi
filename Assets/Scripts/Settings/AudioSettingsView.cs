using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AudioSettingsView : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private SettingsSliderSelector masterSlider;
    [SerializeField] private SettingsSliderSelector bgmSlider;
    [SerializeField] private SettingsSliderSelector sfxSlider;
    [SerializeField] private SettingsSliderSelector uiSlider;

    [Header("Mute Toggles")]
    [SerializeField] private Toggle muteMasterToggle;
    [SerializeField] private Toggle muteBgmToggle;
    [SerializeField] private Toggle muteSfxToggle;
    [SerializeField] private Toggle muteUiToggle;
    [SerializeField] private Toggle muteInBackgroundToggle;

    private GameObject lastSelectedObject;

    private void OnEnable()
    {
        UpdateSelectionVisuals();
    }

    private void Start()
    {
        SettingsManager.EnsureInstance();
    }

    private void Update()
    {
        SyncSelectionVisuals();
    }

    private void OnDestroy()
    {
        UnbindUI();
    }

    private SettingsData staged;

    public void SetTarget(SettingsData target)
    {
        staged = target;
        ApplyToUI();
        BindUI();
        UpdateSelectionVisuals();
    }

    private void ApplyToUI()
    {
        var s = staged ?? (SettingsManager.Instance != null ? SettingsManager.Instance.Settings : null);
        if (s == null) return;

        if (masterSlider != null) masterSlider.SetValueWithoutNotify(s.masterVolume);
        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(s.bgmVolume);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(s.sfxVolume);
        if (uiSlider != null) uiSlider.SetValueWithoutNotify(s.uiVolume);

        if (muteMasterToggle != null) muteMasterToggle.isOn = s.muteMaster;
        if (muteBgmToggle != null) muteBgmToggle.isOn = s.muteBgm;
        if (muteSfxToggle != null) muteSfxToggle.isOn = s.muteSfx;
        if (muteUiToggle != null) muteUiToggle.isOn = s.muteUi;
        if (muteInBackgroundToggle != null) muteInBackgroundToggle.isOn = s.muteInBackground;

        UpdateAudioInteractivity(s);
        UpdateSelectionVisuals();
    }

    private void BindUI()
    {
        UnbindUI();
        if (masterSlider != null) masterSlider.OnValueChanged += OnMasterChanged;
        if (bgmSlider != null) bgmSlider.OnValueChanged += OnBgmChanged;
        if (sfxSlider != null) sfxSlider.OnValueChanged += OnSfxChanged;
        if (uiSlider != null) uiSlider.OnValueChanged += OnUiChanged;
        if (muteMasterToggle != null) muteMasterToggle.onValueChanged.AddListener(OnMuteMasterChanged);
        if (muteBgmToggle != null) muteBgmToggle.onValueChanged.AddListener(OnMuteBgmChanged);
        if (muteSfxToggle != null) muteSfxToggle.onValueChanged.AddListener(OnMuteSfxChanged);
        if (muteUiToggle != null) muteUiToggle.onValueChanged.AddListener(OnMuteUiChanged);
        if (muteInBackgroundToggle != null) muteInBackgroundToggle.onValueChanged.AddListener(OnMuteBackgroundChanged);
    }

    private void UnbindUI()
    {
        if (masterSlider != null) masterSlider.OnValueChanged -= OnMasterChanged;
        if (bgmSlider != null) bgmSlider.OnValueChanged -= OnBgmChanged;
        if (sfxSlider != null) sfxSlider.OnValueChanged -= OnSfxChanged;
        if (uiSlider != null) uiSlider.OnValueChanged -= OnUiChanged;
        if (muteMasterToggle != null) muteMasterToggle.onValueChanged.RemoveListener(OnMuteMasterChanged);
        if (muteBgmToggle != null) muteBgmToggle.onValueChanged.RemoveListener(OnMuteBgmChanged);
        if (muteSfxToggle != null) muteSfxToggle.onValueChanged.RemoveListener(OnMuteSfxChanged);
        if (muteUiToggle != null) muteUiToggle.onValueChanged.RemoveListener(OnMuteUiChanged);
        if (muteInBackgroundToggle != null) muteInBackgroundToggle.onValueChanged.RemoveListener(OnMuteBackgroundChanged);
    }

    private void OnMasterChanged(float v)
    {
        if (staged == null) return;
        staged.masterVolume = v;
        UpdateSelectionVisuals();
    }

    private void OnBgmChanged(float v)
    {
        if (staged == null) return;
        staged.bgmVolume = v;
        UpdateSelectionVisuals();
    }

    private void OnSfxChanged(float v)
    {
        if (staged == null) return;
        staged.sfxVolume = v;
        UpdateSelectionVisuals();
    }

    private void OnUiChanged(float v)
    {
        if (staged == null) return;
        staged.uiVolume = v;
        UpdateSelectionVisuals();
    }

    private void OnMuteMasterChanged(bool val)
    {
        if (staged == null) return;
        staged.muteMaster = val;

        UpdateAudioInteractivity(staged);
        UpdateSelectionVisuals();
    }

    private void OnMuteBgmChanged(bool val)
    {
        if (staged == null) return;
        staged.muteBgm = val;
        UpdateAudioInteractivity(staged);
        UpdateSelectionVisuals();
    }

    private void OnMuteSfxChanged(bool val)
    {
        if (staged == null) return;
        staged.muteSfx = val;
        UpdateAudioInteractivity(staged);
        UpdateSelectionVisuals();
    }

    private void OnMuteUiChanged(bool val)
    {
        if (staged == null) return;
        staged.muteUi = val;
        UpdateAudioInteractivity(staged);
        UpdateSelectionVisuals();
    }

    private void OnMuteBackgroundChanged(bool val)
    {
        if (staged == null) return;
        staged.muteInBackground = val;
        UpdateSelectionVisuals();
    }

    private void SyncSelectionVisuals()
    {
        GameObject currentSelected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        if (currentSelected == lastSelectedObject)
        {
            return;
        }

        lastSelectedObject = currentSelected;
        UpdateSelectionVisuals();
    }

    private void EnsureSelection()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (IsCurrentSelectionInView(currentSelected))
        {
            return;
        }

        SelectFirstFocusable();
    }

    private void SelectFirstFocusable()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        Selectable firstSelectable = GetFirstFocusable();
        if (firstSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
            lastSelectedObject = firstSelectable.gameObject;
        }
    }

    private Selectable GetFirstFocusable()
    {
        if (masterSlider != null && masterSlider.SelectableControl != null) return masterSlider.SelectableControl;
        if (bgmSlider != null && bgmSlider.SelectableControl != null) return bgmSlider.SelectableControl;
        if (sfxSlider != null && sfxSlider.SelectableControl != null) return sfxSlider.SelectableControl;
        if (uiSlider != null && uiSlider.SelectableControl != null) return uiSlider.SelectableControl;
        if (muteMasterToggle != null) return muteMasterToggle;
        if (muteBgmToggle != null) return muteBgmToggle;
        if (muteSfxToggle != null) return muteSfxToggle;
        if (muteUiToggle != null) return muteUiToggle;
        return muteInBackgroundToggle;
    }

    private bool IsCurrentSelectionInView(GameObject selectedObject)
    {
        return selectedObject == (masterSlider != null ? masterSlider.gameObject : null)
            || selectedObject == (bgmSlider != null ? bgmSlider.gameObject : null)
            || selectedObject == (sfxSlider != null ? sfxSlider.gameObject : null)
            || selectedObject == (uiSlider != null ? uiSlider.gameObject : null)
            || selectedObject == (muteMasterToggle != null ? muteMasterToggle.gameObject : null)
            || selectedObject == (muteBgmToggle != null ? muteBgmToggle.gameObject : null)
            || selectedObject == (muteSfxToggle != null ? muteSfxToggle.gameObject : null)
            || selectedObject == (muteUiToggle != null ? muteUiToggle.gameObject : null)
            || selectedObject == (muteInBackgroundToggle != null ? muteInBackgroundToggle.gameObject : null);
    }

    private void UpdateSelectionVisuals()
    {
        SettingsSelectionTextUtility.SetMarkedToggleText(muteMasterToggle, IsSelected(muteMasterToggle));
        SettingsSelectionTextUtility.SetMarkedToggleText(muteBgmToggle, IsSelected(muteBgmToggle));
        SettingsSelectionTextUtility.SetMarkedToggleText(muteSfxToggle, IsSelected(muteSfxToggle));
        SettingsSelectionTextUtility.SetMarkedToggleText(muteUiToggle, IsSelected(muteUiToggle));
        SettingsSelectionTextUtility.SetMarkedToggleText(muteInBackgroundToggle, IsSelected(muteInBackgroundToggle));
    }

    private bool IsSelected(Selectable selectable)
    {
        return EventSystem.current != null && selectable != null && EventSystem.current.currentSelectedGameObject == selectable.gameObject;
    }

    private void UpdateAudioInteractivity(SettingsData s)
    {
        if (s == null) return;

        bool masterEnabled = !s.muteMaster;
        if (masterSlider != null) masterSlider.SetInteractable(masterEnabled);

        if (bgmSlider != null) bgmSlider.SetInteractable(masterEnabled && !s.muteBgm);
        if (sfxSlider != null) sfxSlider.SetInteractable(masterEnabled && !s.muteSfx);
        if (uiSlider != null) uiSlider.SetInteractable(masterEnabled && !s.muteUi);

        if (muteBgmToggle != null) muteBgmToggle.interactable = masterEnabled;
        if (muteSfxToggle != null) muteSfxToggle.interactable = masterEnabled;
        if (muteUiToggle != null) muteUiToggle.interactable = masterEnabled;
    }
}
