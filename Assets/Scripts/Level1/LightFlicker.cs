using UnityEngine;
using System.Collections.Generic;

public class FlickeringLight : MonoBehaviour
{
    [Header("Light Settings")]
    public Light lightSource;
    public float minIntensity = 0.2f;
    public float maxIntensity = 1.2f;

    [Header("Flicker Smoothness")]
    [Tooltip("Higher numbers make the flicker smoother/slower.")]
    [Range(1, 50)]
    public int smoothing = 5;

    private Queue<float> smoothQueue;
    private float lastSum = 0;

    void Start()
    {
        smoothQueue = new Queue<float>(smoothing);

        if (lightSource == null)
            lightSource = GetComponent<Light>();
    }

    void Update()
    {
        if (lightSource == null) return;

        while (smoothQueue.Count >= smoothing)
        {
            lastSum -= smoothQueue.Dequeue();
        }

        float newVal = Random.Range(minIntensity, maxIntensity);
        smoothQueue.Enqueue(newVal);
        lastSum += newVal;

        lightSource.intensity = lastSum / (float)smoothQueue.Count;
    }
}