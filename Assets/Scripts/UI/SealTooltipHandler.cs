using UnityEngine;
using UnityEngine.EventSystems;

public class SealTooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private SealData sealData;

    public void Initialize(SealData data)
    {
        sealData = data;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (sealData != null && TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(
                sealData.sealName,
                sealData.description,
                transform.position,
                sealData.flavorText,
                TooltipManager.TooltipPrioritySeal,
                gameObject);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip(gameObject);
        }
    }

    private void OnDisable()
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip(gameObject);
        }
    }
}
