using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPanelView : PagedAnimatedPanelView
{
    [SerializeField] private AchievementEntryView entryPrefab;

    [Header("Category Filter")]
    [SerializeField] private ToggleGroup categoryToggleGroup;
    [SerializeField] private Toggle allToggle;
    [SerializeField] private Toggle combatToggle;
    [SerializeField] private Toggle collectionToggle;
    [SerializeField] private Toggle otherToggle;

    [Header("Overall Progress")]
    [SerializeField] private TextMeshProUGUI overallProgressText;
    [SerializeField] private Image overallProgressFillImage;

    [Header("Category Toggle Colors")]
    [SerializeField] private Color selectedTabColor = new Color(0.23f, 0.66f, 1f, 1f);
    [SerializeField] private Color unselectedTabColor = new Color(0.22f, 0.22f, 0.22f, 1f);

    private bool isBindingToggles;
    private readonly List<AchievementData> filteredAchievements = new List<AchievementData>();

    private void Awake()
    {
        BindToggles();
        BindPaginationButtons();
        SelectAllCategoryWithoutRefresh();
    }

    private void OnEnable()
    {
        SubscribeAchievementEvents();

        if (EnsureCategorySelection())
        {
            Refresh();
        }

        Refresh();
        UpdateCategoryToggleVisuals();
        UpdateOverallProgressDisplay();
    }

    private void LateUpdate()
    {
        if (EnsureCategorySelection())
        {
            Refresh();
        }

        UpdateCategoryToggleVisuals();
        UpdateOverallProgressDisplay();
    }

    private void OnDestroy()
    {
        UnsubscribeAchievementEvents();
        UnbindToggles();
        UnbindPaginationButtons();
    }

    private void OnDisable()
    {
        StopEntryAnimations();
        UnsubscribeAchievementEvents();
    }

    public void Refresh()
    {
        if (contentRoot == null || entryPrefab == null)
        {
            UpdatePaginationControls(filteredAchievements.Count);
            return;
        }

        AchievementManager achievementManager = AchievementManager.Instance;
        if (achievementManager == null)
        {
            ClearContent();
            filteredAchievements.Clear();
            ResetPageIndex();
            UpdatePaginationControls(0);
            UpdateOverallProgressDisplay();
            return;
        }

        BuildFilteredAchievements(achievementManager);
        TrySetPageIndex(currentPageIndex, filteredAchievements.Count);

        ClearContent();
        RenderCurrentPage(achievementManager);

        UpdatePaginationControls(filteredAchievements.Count);
        UpdateOverallProgressDisplay();
    }

    public override void GoToPreviousPage()
    {
        if (TrySetPageIndex(currentPageIndex - 1, filteredAchievements.Count))
        {
            Refresh();
        }
    }

    public override void GoToNextPage()
    {
        if (TrySetPageIndex(currentPageIndex + 1, filteredAchievements.Count))
        {
            Refresh();
        }
    }

    private void BuildFilteredAchievements(AchievementManager achievementManager)
    {
        filteredAchievements.Clear();
        IReadOnlyList<AchievementData> achievements = achievementManager.GetVisibleAchievements(GetSelectedCategoryFilter());
        for (int i = 0; i < achievements.Count; i++)
        {
            AchievementData achievement = achievements[i];
            if (achievement == null)
            {
                continue;
            }

            filteredAchievements.Add(achievement);
        }
    }

    private void RenderCurrentPage(AchievementManager achievementManager)
    {
        int pageSize = GetEntriesPerPage();
        int startIndex = currentPageIndex * pageSize;
        int endIndexExclusive = Mathf.Min(startIndex + pageSize, filteredAchievements.Count);

        for (int i = startIndex; i < endIndexExclusive; i++)
        {
            AchievementData achievement = filteredAchievements[i];
            if (achievement == null)
            {
                continue;
            }

            Transform entryContainer = CreateEntryContainer($"AchievementEntry_{i}");
            AchievementEntryView entry = Instantiate(entryPrefab, entryContainer, false);
            int currentCount = achievementManager.GetCurrentCount(achievement.id);
            bool isUnlocked = achievementManager.IsUnlocked(achievement.id);
            string displayProgressText = achievementManager.GetDisplayProgressText(achievement.id);
            string clearTimeText = achievementManager.GetClearTimeText(achievement.id);
            entry.Initialize(achievement, currentCount, isUnlocked, displayProgressText, clearTimeText);
        }

        AnimateCurrentEntries();
    }

    private void BindToggles()
    {
        ResolveCategoryToggleGroup();

        if (categoryToggleGroup != null)
        {
            categoryToggleGroup.allowSwitchOff = false;
        }

        AssignGroup(allToggle);
        AssignGroup(combatToggle);
        AssignGroup(collectionToggle);
        AssignGroup(otherToggle);

        SetToggleListener(allToggle, OnAllToggleChanged);
        SetToggleListener(combatToggle, OnCombatToggleChanged);
        SetToggleListener(collectionToggle, OnCollectionToggleChanged);
        SetToggleListener(otherToggle, OnOtherToggleChanged);
    }

    private void UnbindToggles()
    {
        SetToggleListener(allToggle, OnAllToggleChanged, false);
        SetToggleListener(combatToggle, OnCombatToggleChanged, false);
        SetToggleListener(collectionToggle, OnCollectionToggleChanged, false);
        SetToggleListener(otherToggle, OnOtherToggleChanged, false);
    }

    private void SetToggleListener(Toggle toggle, UnityEngine.Events.UnityAction<bool> callback, bool add = true)
    {
        if (toggle == null)
        {
            return;
        }

        if (add)
        {
            toggle.onValueChanged.AddListener(callback);
        }
        else
        {
            toggle.onValueChanged.RemoveListener(callback);
        }
    }

    private void AssignGroup(Toggle toggle)
    {
        if (toggle == null || categoryToggleGroup == null)
        {
            return;
        }

        toggle.group = categoryToggleGroup;
    }

    private void ResolveCategoryToggleGroup()
    {
        if (categoryToggleGroup != null)
        {
            return;
        }

        if (allToggle != null && allToggle.group != null)
        {
            categoryToggleGroup = allToggle.group;
            return;
        }

        if (combatToggle != null && combatToggle.group != null)
        {
            categoryToggleGroup = combatToggle.group;
            return;
        }

        if (collectionToggle != null && collectionToggle.group != null)
        {
            categoryToggleGroup = collectionToggle.group;
            return;
        }

        if (otherToggle != null && otherToggle.group != null)
        {
            categoryToggleGroup = otherToggle.group;
            return;
        }

        categoryToggleGroup = GetComponentInChildren<ToggleGroup>(true);
    }

    private void SelectAllCategoryWithoutRefresh()
    {
        isBindingToggles = true;

        if (allToggle != null)
        {
            allToggle.isOn = true;
        }

        if (combatToggle != null)
        {
            combatToggle.isOn = false;
        }

        if (collectionToggle != null)
        {
            collectionToggle.isOn = false;
        }

        if (otherToggle != null)
        {
            otherToggle.isOn = false;
        }

        isBindingToggles = false;
        UpdateCategoryToggleVisuals();
    }

    private bool EnsureCategorySelection()
    {
        bool hasSelection =
            (allToggle != null && allToggle.isOn) ||
            (combatToggle != null && combatToggle.isOn) ||
            (collectionToggle != null && collectionToggle.isOn) ||
            (otherToggle != null && otherToggle.isOn);

        if (!hasSelection)
        {
            SelectAllCategoryWithoutRefresh();
            return true;
        }

        return false;
    }

    private AchievementCategory? GetSelectedCategoryFilter()
    {
        if (combatToggle != null && combatToggle.isOn)
        {
            return AchievementCategory.Combat;
        }

        if (collectionToggle != null && collectionToggle.isOn)
        {
            return AchievementCategory.Collection;
        }

        if (otherToggle != null && otherToggle.isOn)
        {
            return AchievementCategory.Other;
        }

        return null;
    }

    private void OnAllToggleChanged(bool isOn)
    {
        if (isBindingToggles)
        {
            return;
        }

        if (!isOn)
        {
            EnsureCategorySelection();
            return;
        }

        SelectAllCategoryWithoutRefresh();
        ResetPageIndex();
        Refresh();
        UpdateCategoryToggleVisuals();
    }

    private void OnCombatToggleChanged(bool isOn)
    {
        if (isBindingToggles)
        {
            return;
        }

        if (!isOn)
        {
            EnsureCategorySelection();
            return;
        }

        ResetPageIndex();
        Refresh();
        UpdateCategoryToggleVisuals();
    }

    private void OnCollectionToggleChanged(bool isOn)
    {
        if (isBindingToggles)
        {
            return;
        }

        if (!isOn)
        {
            EnsureCategorySelection();
            return;
        }

        ResetPageIndex();
        Refresh();
        UpdateCategoryToggleVisuals();
    }

    private void OnOtherToggleChanged(bool isOn)
    {
        if (isBindingToggles)
        {
            return;
        }

        if (!isOn)
        {
            EnsureCategorySelection();
            return;
        }

        ResetPageIndex();
        Refresh();
        UpdateCategoryToggleVisuals();
    }

    private void UpdateOverallProgressDisplay()
    {
        AchievementManager achievementManager = AchievementManager.Instance;
        if (achievementManager == null)
        {
            if (overallProgressText != null)
            {
                overallProgressText.text = "0 / 0 (0%)";
            }

            if (overallProgressFillImage != null)
            {
                overallProgressFillImage.type = Image.Type.Filled;
                overallProgressFillImage.fillMethod = Image.FillMethod.Horizontal;
                overallProgressFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                overallProgressFillImage.fillClockwise = true;
                overallProgressFillImage.fillAmount = 0f;
            }

            return;
        }

        int totalCount = achievementManager.GetTotalAchievementCount();
        int unlockedCount = achievementManager.GetUnlockedAchievementCount();
        float completionRatio = achievementManager.GetOverallCompletionRatio();
        int completionPercent = Mathf.RoundToInt(completionRatio * 100f);

        if (overallProgressText != null)
        {
            overallProgressText.text = $"{unlockedCount} / {totalCount} ({completionPercent}%)";
        }

        if (overallProgressFillImage != null)
        {
            overallProgressFillImage.type = Image.Type.Filled;
            overallProgressFillImage.fillMethod = Image.FillMethod.Horizontal;
            overallProgressFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            overallProgressFillImage.fillClockwise = true;
            overallProgressFillImage.fillAmount = completionRatio;
        }
    }

    private void UpdateCategoryToggleVisuals()
    {
        ApplyToggleColor(allToggle);
        ApplyToggleColor(combatToggle);
        ApplyToggleColor(collectionToggle);
        ApplyToggleColor(otherToggle);
    }

    private void ApplyToggleColor(Toggle toggle)
    {
        if (toggle == null)
        {
            return;
        }

        Image targetImage = toggle.targetGraphic as Image;
        if (targetImage == null)
        {
            targetImage = toggle.GetComponent<Image>();
        }

        if (targetImage == null)
        {
            return;
        }

        targetImage.color = toggle.isOn ? selectedTabColor : unselectedTabColor;
    }

    private void SubscribeAchievementEvents()
    {
        AchievementManager manager = AchievementManager.Instance;
        if (manager == null)
        {
            return;
        }

        manager.OnAchievementUnlocked -= OnAchievementStateChanged;
        manager.OnAchievementUnlocked += OnAchievementStateChanged;
        manager.OnAchievementProgressChanged -= OnAchievementProgressChanged;
        manager.OnAchievementProgressChanged += OnAchievementProgressChanged;
    }

    private void UnsubscribeAchievementEvents()
    {
        AchievementManager manager = AchievementManager.Instance;
        if (manager == null)
        {
            return;
        }

        manager.OnAchievementUnlocked -= OnAchievementStateChanged;
        manager.OnAchievementProgressChanged -= OnAchievementProgressChanged;
    }

    private void OnAchievementStateChanged(AchievementData _)
    {
        Refresh();
    }

    private void OnAchievementProgressChanged(AchievementData _, int __, int ___)
    {
        Refresh();
    }
}
