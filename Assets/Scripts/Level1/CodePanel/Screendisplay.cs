using UnityEngine;
using TMPro;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Drives the 3D code-panel screen.
///
/// Layout (auto-configured by the [Auto-Layout] context menu):
///   ┌──────────────────────────────────────────┐
///   │ SYSTEM LIVE                       12:34  │
///   │   Reading Info...                        │
///   │              _ _ _ _                     │
///   │            ACCESS GRANTED                │
///   └──────────────────────────────────────────┘
///
/// Setup:
///   1) Place this on the Quad that has the world-space Canvas as a child.
///   2) Drag the five TMP_Text fields into the slots.
///   3) Set Canvas Width / Canvas Height so they match your Quad's aspect ratio
///      (e.g. if your Quad is scaled 2.01 x 0.84, use 800 x 335).
///   4) Right-click component → "Auto-Layout (Fix Canvas + Texts)".
/// </summary>
[ExecuteAlways]
public class ScreenDisplay : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text systemLiveText;
    [SerializeField] private TMP_Text clockText;
    [SerializeField] private TMP_Text codeDisplay;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text flavorText;

    [Header("Optional Background")]
    [SerializeField] private Renderer screenBackgroundRenderer;
    [SerializeField] private Color screenBackgroundColor = new Color(0.04f, 0f, 0f, 1f);

    [Header("Header / Clock")]
    [SerializeField] private string systemLiveLabel = "SYSTEM LIVE";
    [SerializeField] private bool blinkClockColon = true;
    [SerializeField] private bool use24HourClock = true;

    [Header("Code Display")]
    [SerializeField] private int codeLength = 4;
    [SerializeField] private string emptySlotChar = "_";
    [SerializeField] private string slotSeparator = "  ";

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(1f, 0.15f, 0.15f);
    [SerializeField] private Color correctColor = new Color(0.3f, 1f, 0.3f);
    [SerializeField] private Color wrongColor = new Color(1f, 0f, 0f);

    [Header("Flavor Lines (optional)")]
    [SerializeField]
    private string[] flavorLines = new string[]
    {
        "Reading Info...",
        "Data Collecting...",
        "Scanning...",
        "Allocation...",
        "Class Recovered"
    };
    [SerializeField] private float flavorCycleInterval = 1.4f;

    // ── Auto-Layout Configuration ────────────────────────────────────────────
    [Header("Auto-Layout (Editor Only)")]
    [Tooltip("Canvas width in pixels. Should match your Quad's WIDTH aspect.")]
    [SerializeField] private float canvasWidth = 800f;
    [Tooltip("Canvas height in pixels. Should match your Quad's HEIGHT aspect. " +
             "Example: Quad scaled (2.01, 0.84, 2) → set this to ~335 (800 * 0.84/2.01).")]
    [SerializeField] private float canvasHeight = 335f;
    [Tooltip("World units per canvas pixel. 0.00125 means an 800x335 canvas " +
             "is 1.0 x 0.42 world units before the Quad's own scale is applied.")]
    [SerializeField] private float canvasPixelScale = 0.00125f;

    [Header("Font Sizes")]
    [SerializeField] private int headerFontSize = 44;
    [SerializeField] private int clockFontSize = 44;
    [SerializeField] private int flavorFontSize = 28;
    [SerializeField] private int codeFontSize = 110;
    [SerializeField] private int statusFontSize = 36;

    [Header("Padding (canvas pixels)")]
    [SerializeField] private float edgePadding = 20f;

    private float clockTimer = 0f;
    private bool colonOn = true;
    private int flavorIndex = 0;
    private float flavorTimer = 0f;

    private void Start()
    {
        if (!Application.isPlaying) return;

        if (systemLiveText != null) { systemLiveText.text = systemLiveLabel; systemLiveText.color = normalColor; }
        if (statusText != null) statusText.text = "";
        if (clockText != null) clockText.color = normalColor;
        if (codeDisplay != null) codeDisplay.color = normalColor;
        if (flavorText != null) flavorText.color = normalColor;

        if (screenBackgroundRenderer != null)
        {
            Material m = screenBackgroundRenderer.material;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", screenBackgroundColor);
            else if (m.HasProperty("_Color")) m.SetColor("_Color", screenBackgroundColor);
        }

        UpdateClock();
        UpdateCodeDisplay("");
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        clockTimer += Time.deltaTime;
        if (clockTimer >= 1f)
        {
            clockTimer = 0f;
            colonOn = !colonOn;
            UpdateClock();
        }

        if (flavorText != null && flavorLines != null && flavorLines.Length > 0)
        {
            flavorTimer += Time.deltaTime;
            if (flavorTimer >= flavorCycleInterval)
            {
                flavorTimer = 0f;
                flavorIndex = (flavorIndex + 1) % flavorLines.Length;
                flavorText.text = flavorLines[flavorIndex];
                flavorText.color = normalColor;
            }
        }
    }

    public void SetCurrentInput(string currentInput)
    {
        UpdateCodeDisplay(currentInput);
        if (codeDisplay != null) codeDisplay.color = normalColor;
        if (statusText != null) statusText.text = "";
    }

    public void ShowCorrect()
    {
        if (codeDisplay != null) codeDisplay.color = correctColor;
        if (statusText != null) { statusText.text = "ACCESS GRANTED"; statusText.color = correctColor; }
    }

    public void ShowWrong()
    {
        if (codeDisplay != null) codeDisplay.color = wrongColor;
        if (statusText != null) { statusText.text = "ACCESS DENIED"; statusText.color = wrongColor; }
    }

    public void ResetColors()
    {
        if (codeDisplay != null) codeDisplay.color = normalColor;
        if (statusText != null) statusText.text = "";
    }

    private void UpdateCodeDisplay(string currentInput)
    {
        if (codeDisplay == null) return;
        string display = "";
        for (int i = 0; i < codeLength; i++)
        {
            display += (i < currentInput.Length) ? currentInput[i].ToString() : emptySlotChar;
            if (i < codeLength - 1) display += slotSeparator;
        }
        codeDisplay.text = display;
    }

    private void UpdateClock()
    {
        if (clockText == null) return;
        DateTime now = DateTime.Now;
        string sep = (blinkClockColon && !colonOn) ? " " : ":";
        clockText.text = use24HourClock
            ? now.ToString($"HH{sep}mm")
            : now.ToString($"hh{sep}mm");
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-Layout (Fix Canvas + Texts)")]
    private void AutoLayout()
    {
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null)
        {
            Debug.LogWarning("ScreenDisplay: No child Canvas found.", this);
            return;
        }

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();
        Undo.RecordObject(canvasRT, "Auto-Layout Canvas");

        // Use the configured pixel dimensions + pixel scale.
        canvasRT.anchorMin = new Vector2(0.5f, 0.5f);
        canvasRT.anchorMax = new Vector2(0.5f, 0.5f);
        canvasRT.pivot = new Vector2(0.5f, 0.5f);
        canvasRT.sizeDelta = new Vector2(canvasWidth, canvasHeight);
        canvasRT.anchoredPosition = Vector2.zero;
        canvasRT.localScale = new Vector3(canvasPixelScale, canvasPixelScale, canvasPixelScale);
        canvasRT.localPosition = new Vector3(0f, 0f, -0.001f);
        canvasRT.localRotation = Quaternion.identity;

        // Layout zones, in canvas pixels.
        float p = edgePadding;
        float topRowY = -p;                                  // pinned to top
        float flavorRowY = topRowY - headerFontSize - 8f;       // just under header
        float bottomY = p;                                   // pinned to bottom

        // TOP-LEFT: System Live
        ConfigureText(systemLiveText, "SystemLive",
                      new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                      new Vector2(p, topRowY),
                      new Vector2(canvasWidth * 0.5f, headerFontSize + 10f),
                      headerFontSize,
                      HorizontalAlignmentOptions.Left, VerticalAlignmentOptions.Top,
                      systemLiveLabel);

        // TOP-RIGHT: Clock
        ConfigureText(clockText, "Clock",
                      new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                      new Vector2(-p, topRowY),
                      new Vector2(canvasWidth * 0.4f, clockFontSize + 10f),
                      clockFontSize,
                      HorizontalAlignmentOptions.Right, VerticalAlignmentOptions.Top,
                      "00:00");

        // FLAVOR: line under header
        ConfigureText(flavorText, "Flavor",
                      new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                      new Vector2(p, flavorRowY),
                      new Vector2(canvasWidth - 2f * p, flavorFontSize + 6f),
                      flavorFontSize,
                      HorizontalAlignmentOptions.Left, VerticalAlignmentOptions.Top,
                      "Reading Info...");

        // CODE: center, dominant
        ConfigureText(codeDisplay, "CodeDisplay",
                      new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                      new Vector2(0f, 10f),
                      new Vector2(canvasWidth - 2f * p, codeFontSize + 20f),
                      codeFontSize,
                      HorizontalAlignmentOptions.Center, VerticalAlignmentOptions.Middle,
                      "_  _  _  _");

        // STATUS: bottom-center
        ConfigureText(statusText, "Status",
                      new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                      new Vector2(0f, bottomY),
                      new Vector2(canvasWidth - 2f * p, statusFontSize + 10f),
                      statusFontSize,
                      HorizontalAlignmentOptions.Center, VerticalAlignmentOptions.Bottom,
                      "");

        Debug.Log($"ScreenDisplay: Auto-layout complete. Canvas = {canvasWidth}x{canvasHeight} @ scale {canvasPixelScale}", this);
        EditorUtility.SetDirty(canvasRT);
    }

    private void ConfigureText(TMP_Text tmp, string label,
                               Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
                               Vector2 anchoredPos, Vector2 size, float fontSize,
                               HorizontalAlignmentOptions h, VerticalAlignmentOptions v,
                               string defaultText)
    {
        if (tmp == null)
        {
            Debug.LogWarning($"ScreenDisplay: {label} field not assigned — skipping.", this);
            return;
        }
        RectTransform rt = tmp.rectTransform;
        Undo.RecordObject(rt, $"Auto-Layout {label} RT");
        Undo.RecordObject(tmp, $"Auto-Layout {label} TMP");

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        tmp.fontSize = fontSize;
        tmp.enableAutoSizing = false;
        tmp.horizontalAlignment = h;
        tmp.verticalAlignment = v;
        tmp.color = normalColor;
        if (!string.IsNullOrEmpty(defaultText)) tmp.text = defaultText;

        EditorUtility.SetDirty(rt);
        EditorUtility.SetDirty(tmp);
    }
#endif
}