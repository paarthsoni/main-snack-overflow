using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class MemoryBarController : MonoBehaviour
{
    [Header("Timing")]
    public float showDurationSeconds = 15f;   // memory phase length (seconds)

    [Header("Shape Slots (drag the 4 Image objects here)")]
    public Image[] shapeSlots;

    [Header("Sprites (assign in Inspector)")]
    public Sprite squareSprite;
    public Sprite circleSprite;
    public Sprite triangleSprite;
    public Sprite pentagonSprite;

    [Header("UI")]
    public TMP_Text timerText;
    public GameObject rootBar;            // MemoryBar object (self if left empty)
    public RectTransform layoutRoot;      // ShapesRow (for layout rebuild)
    RectTransform barRect;                // cached
    CanvasGroup canvasGroup;              // for fade

    [Header("Blur / Pause (optional URP volume)")]
    public GameObject blurVolume;         // your Global Volume (can be null)
    public bool pauseGameDuringMemory = true;

    [Header("Intro Animation (unscaled time)")]
    public float introSlideY = 180f;      // starts this many px above, slides to current position
    public float introFadeDuration = 0.35f;

    [Header("Exit Animation (unscaled time)")]
    public float slideDistanceY = 300f;   // moves down by this many pixels
    public float animDuration = 0.6f;     // seconds (unscaled)
    [Range(0f,1f)] public float fadeTo = 0f;

    [Header("2D Blur (RenderTexture method)")]
    public RawImage worldBlurRawImage;    // WorldBlur RawImage
    public Camera mainCamera;             // Main Camera
    public RenderTexture worldRT;         // WorldRT

    [Header("Which shapes to show")]
    public List<PathShape.ShapeType> shapesToShow = new()
    {
        PathShape.ShapeType.Square,
        PathShape.ShapeType.Circle,
        PathShape.ShapeType.Triangle,
        PathShape.ShapeType.Pentagon
    };

    public event Action<List<PathShape.ShapeType>> OnMemoryPhaseComplete;

    void Awake()
    {
        if (!rootBar) rootBar = this.gameObject;
        barRect = rootBar.GetComponent<RectTransform>();
        canvasGroup = rootBar.GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = rootBar.AddComponent<CanvasGroup>();

        // Hidden by default; shown when BeginMemoryPhase() is called
        canvasGroup.alpha = 0f;
        rootBar.SetActive(false);
    }

    // ============ PUBLIC ENTRY ============
    public void BeginMemoryPhase()
    {
        ApplyShapesToSlots();

        // Blur + pause
        if (blurVolume) blurVolume.SetActive(true);
        if (worldBlurRawImage) worldBlurRawImage.gameObject.SetActive(true);
        if (mainCamera && worldRT) mainCamera.targetTexture = worldRT;
        if (pauseGameDuringMemory) Time.timeScale = 0f;

        // Prepare intro pose
        rootBar.SetActive(true);
        canvasGroup.alpha = 0f;
        var targetPos = barRect.anchoredPosition;
        barRect.anchoredPosition = targetPos + new Vector2(0f, Mathf.Abs(introSlideY));

        StopAllCoroutines();
        StartCoroutine(IntroThenCountdownThenOutro(targetPos));
    }

    IEnumerator IntroThenCountdownThenOutro(Vector2 targetPos)
    {
        // INTRO (fade-in from top)
        float t = 0f;
        Vector2 from = barRect.anchoredPosition;
        Vector2 to = targetPos;
        while (t < introFadeDuration)
        {
            float u = t / introFadeDuration; u = u * u * (3f - 2f * u); // smoothstep
            barRect.anchoredPosition = Vector2.Lerp(from, to, u);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, u);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        barRect.anchoredPosition = to;
        canvasGroup.alpha = 1f;

        // COUNTDOWN (unscaled time while game is paused)
        float remain = showDurationSeconds;
        while (remain > 0f)
        {
            if (timerText) timerText.text = Mathf.CeilToInt(remain).ToString();
            remain -= Time.unscaledDeltaTime;
            yield return null;
        }
        if (timerText) timerText.text = "0";

        // OUTRO (slide down + fade out)
        Vector2 start = barRect.anchoredPosition;
        Vector2 end = start + new Vector2(0f, -Mathf.Abs(slideDistanceY));
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            float u = elapsed / animDuration; u = u * u * (3f - 2f * u);
            barRect.anchoredPosition = Vector2.Lerp(start, end, u);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, fadeTo, u);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        barRect.anchoredPosition = end;
        canvasGroup.alpha = fadeTo;
        rootBar.SetActive(false);

        // Unblur + unpause
        if (blurVolume) blurVolume.SetActive(false);
        if (worldBlurRawImage) worldBlurRawImage.gameObject.SetActive(false);
        if (mainCamera) mainCamera.targetTexture = null;
        if (pauseGameDuringMemory) Time.timeScale = 1f;

        // Notify gameplay can begin
        OnMemoryPhaseComplete?.Invoke(new List<PathShape.ShapeType>(shapesToShow));
    }

    void ApplyShapesToSlots()
    {
        int n = Mathf.Min(shapeSlots.Length, shapesToShow.Count);
        for (int i = 0; i < n; i++)
        {
            var img = shapeSlots[i];
            if (!img) continue;
            img.preserveAspect = true;
            img.sprite = GetSpriteFor(shapesToShow[i]);
            img.enabled = true;
            img.color = Color.white;
        }
        for (int i = n; i < shapeSlots.Length; i++)
            if (shapeSlots[i]) shapeSlots[i].enabled = true;

        if (layoutRoot) LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
    }

    Sprite GetSpriteFor(PathShape.ShapeType shape)
    {
        switch (shape)
        {
            case PathShape.ShapeType.Square:   return squareSprite;
            case PathShape.ShapeType.Circle:   return circleSprite;
            case PathShape.ShapeType.Triangle: return triangleSprite;
            case PathShape.ShapeType.Pentagon: return pentagonSprite;
            default: return null;
        }
    }
}
