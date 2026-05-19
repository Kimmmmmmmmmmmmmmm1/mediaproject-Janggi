using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI panel animator: fades a CanvasGroup and scales the RectTransform when showing/hiding.
/// Attach to a panel GameObject (with CanvasGroup or it will be added) and call Show()/Hide().
/// </summary>
[DisallowMultipleComponent]
public class PanelAnimator : MonoBehaviour
{
    [Header("Animation")]
    [Tooltip("Duration in seconds for show/hide animations")]
    [SerializeField] private float duration = 0.22f;
    [SerializeField] private float hideScale = 0.96f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine running;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    // Ensure references exist — calling this is safe even if Awake hasn't run yet.
    private void EnsureInitialized()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    public void Show(bool instant = false)
    {
        EnsureInitialized();
        if (running != null) StopCoroutine(running);
        gameObject.SetActive(true);
        if (instant)
        {
            canvasGroup.alpha = 1f;
            if (rectTransform != null) rectTransform.localScale = Vector3.one;
            return;
        }
        running = StartCoroutine(DoShow());
    }

    public void Hide(bool instant = false)
    {
        EnsureInitialized();
        if (running != null) StopCoroutine(running);
        // If the object is already inactive in hierarchy, don't try to start coroutines on it.
        if (!gameObject.activeInHierarchy)
        {
            // Ensure final hidden state without running coroutine
            canvasGroup.alpha = 0f;
            if (rectTransform != null) rectTransform.localScale = Vector3.one * hideScale;
            gameObject.SetActive(false);
            running = null;
            return;
        }

        if (instant)
        {
            canvasGroup.alpha = 0f;
            if (rectTransform != null) rectTransform.localScale = Vector3.one * hideScale;
            gameObject.SetActive(false);
            return;
        }

        running = StartCoroutine(DoHide());
    }

    private IEnumerator DoShow()
    {
        EnsureInitialized();
        float t = 0f;
        float startAlpha = canvasGroup.alpha;
        Vector3 startScale = rectTransform != null ? rectTransform.localScale : Vector3.one * hideScale;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = Mathf.SmoothStep(0f, 1f, p);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, eased);
            if (rectTransform != null) rectTransform.localScale = Vector3.Lerp(startScale, Vector3.one, eased);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        if (rectTransform != null) rectTransform.localScale = Vector3.one;
        running = null;
    }

    private IEnumerator DoHide()
    {
        float t = 0f;
        float startAlpha = canvasGroup.alpha;
        Vector3 startScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = Mathf.SmoothStep(0f, 1f, p);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, eased);
            if (rectTransform != null) rectTransform.localScale = Vector3.Lerp(startScale, Vector3.one * hideScale, eased);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        if (rectTransform != null) rectTransform.localScale = Vector3.one * hideScale;
        gameObject.SetActive(false);
        running = null;
    }
}
