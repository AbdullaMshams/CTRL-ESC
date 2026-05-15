using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleEffect : MonoBehaviour
{
    [Header("References")]
    public Image scanLine;        // ← changed from RawImage to Image
    public TextMeshProUGUI titleText;

    [Header("Scanline")]
    public float scanSpeed = 1.5f;

    [Header("Glitch")]
    public float glitchInterval = 4f;
    public float glitchDuration = 0.15f;

    private RectTransform scanRect;

    void Start()
    {
        scanRect = scanLine.GetComponent<RectTransform>();
        StartCoroutine(ScanLoop());
        StartCoroutine(GlitchLoop());
    }

    IEnumerator ScanLoop()
    {
        RectTransform parentRect = scanLine.transform.parent.GetComponent<RectTransform>();
        float halfParentH = parentRect.rect.height / 2f;

        while (true)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * scanSpeed;
                float y = Mathf.Lerp(halfParentH, -halfParentH, t);
                scanRect.anchoredPosition = new Vector2(0, y);
                yield return null;
            }
        }
    }

    IEnumerator GlitchLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(glitchInterval);
            float[] offsets = { -4f, 4f, -2f, 2f, 0f };
            foreach (float x in offsets)
            {
                titleText.rectTransform.anchoredPosition += new Vector2(x, 0);
                yield return new WaitForSeconds(glitchDuration / offsets.Length);
                titleText.rectTransform.anchoredPosition -= new Vector2(x, 0);
            }
        }
    }
}