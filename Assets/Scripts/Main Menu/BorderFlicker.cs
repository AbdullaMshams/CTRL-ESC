using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BorderFlicker : MonoBehaviour
{
    public Image[] borders; // drag all 4 border images here
    public float flickerInterval = 4f;

    void Start()
    {
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            yield return new WaitForSeconds(flickerInterval);

            // SUBTLE — quick double dip
            yield return FlickerSequence(new float[]
            { 0.6f, 1f, 0.3f, 1f }, 0.05f);
        }
    }

    IEnumerator FlickerSequence(float[] alphas, float stepTime)
    {
        foreach (float a in alphas)
        {
            SetAlpha(a);
            yield return new WaitForSeconds(stepTime);
        }
        SetAlpha(1f);
    }

    void SetAlpha(float a)
    {
        foreach (var img in borders)
        {
            Color c = img.color;
            c.a = a;
            img.color = c;
        }
    }
}