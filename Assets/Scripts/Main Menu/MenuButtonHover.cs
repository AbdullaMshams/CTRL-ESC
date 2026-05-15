using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public TextMeshProUGUI buttonText;
    public Image leftBorder;
    public Image underline;
    public RectTransform textRect;
    public TextMeshProUGUI arrow;

    [Header("Settings")]
    public float animSpeed = 8f;
    public float slideAmount = 18f;

    private readonly Color dimColor = new Color(0.16f, 0.47f, 0.26f);
    private readonly Color brightColor = new Color(0f, 1f, 0.31f);
    private readonly Color transparent = new Color(0f, 1f, 0.31f, 0f);

    private RectTransform underlineRect;
    private RectTransform leftBorderRect;
    private float textWidth;
    private Vector2 normalPos;
    private Coroutine animCoroutine;
    private Material textMat;

    void Awake()
    {
        if (underline != null) underlineRect = underline.GetComponent<RectTransform>();
        if (leftBorder != null) leftBorderRect = leftBorder.GetComponent<RectTransform>();
    }

    void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        yield return null; // wait one frame for layout

        if (textRect != null) textWidth = textRect.rect.width;
        if (textRect != null) normalPos = textRect.anchoredPosition;

        // hide everything at start
        if (underlineRect != null) underlineRect.sizeDelta = new Vector2(0f, 2f);
        if (leftBorder != null) leftBorder.color = transparent;
        if (arrow != null) arrow.color = transparent;
        if (buttonText != null) buttonText.color = dimColor;

        // setup glow
        if (buttonText != null)
        {
            textMat = buttonText.fontMaterial;
            textMat.EnableKeyword("GLOW_ON");
            textMat.SetFloat(ShaderUtilities.ID_GlowPower, 0f);
        }
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(Animate(true));
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(Animate(false));
    }

    IEnumerator Animate(bool hoverIn)
    {
        float t = 0f;

        float startWidth = underlineRect != null ? underlineRect.sizeDelta.x : 0f;
        float targetWidth = hoverIn ? textWidth : 0f;

        Color startBorder = leftBorder != null ? leftBorder.color : transparent;
        Color targetBorder = hoverIn ? brightColor : transparent;

        Color startText = buttonText != null ? buttonText.color : dimColor;
        Color targetText = hoverIn ? brightColor : dimColor;

        Color startArrow = arrow != null ? arrow.color : transparent;
        Color targetArrow = hoverIn ? brightColor : transparent;

        Vector2 startPos = textRect != null ? textRect.anchoredPosition : Vector2.zero;
        Vector2 targetPos = hoverIn ? normalPos + new Vector2(slideAmount, 0) : normalPos;

        float startGlow = textMat != null ? textMat.GetFloat(ShaderUtilities.ID_GlowPower) : 0f;
        float targetGlow = hoverIn ? 0.4f : 0f;

        while (t < 1f)
        {
            t = Mathf.MoveTowards(t, 1f, Time.deltaTime * animSpeed);
            float ease = 1f - Mathf.Pow(1f - t, 3f); // cubic ease out

            if (underlineRect != null) underlineRect.sizeDelta = new Vector2(Mathf.Lerp(startWidth, targetWidth, ease), 2f);
            if (leftBorder != null) leftBorder.color = Color.Lerp(startBorder, targetBorder, ease);
            if (buttonText != null) buttonText.color = Color.Lerp(startText, targetText, ease);
            if (arrow != null) arrow.color = Color.Lerp(startArrow, targetArrow, ease);
            if (textRect != null) textRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
            if (textMat != null) textMat.SetFloat(ShaderUtilities.ID_GlowPower, Mathf.Lerp(startGlow, targetGlow, ease));

            yield return null;
        }
    }
}