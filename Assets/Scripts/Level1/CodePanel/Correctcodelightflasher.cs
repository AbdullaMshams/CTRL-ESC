using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Flashes a set of lights GREEN ON/OFF when triggered. Use for the EXIT sign
/// and the side LED lights after the player enters the correct code.
///
/// Affects three things on each target:
///   1. Light component color   (Point Light / Spot Light)
///   2. Material emission color (the glowing red bulb meshes)
///   3. Material base/main color (the lit-up surface)
///
/// Setup:
///   - Attach to any empty GameObject (e.g. a "FlashController" on the door).
///   - Add the EXIT sign GameObject and the two red light GameObjects to "targets".
///     (Include the parent if you want all children's lights + renderers to react.)
///   - Wire CodeEntryPanel.OnCodeCorrect → StartFlashing().
/// </summary>
public class CorrectCodeLightFlasher : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Root GameObjects to affect. All Light components and Renderers in their children will be flashed.")]
    [SerializeField] private List<GameObject> targets = new List<GameObject>();

    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = new Color(0.2f, 1f, 0.3f);
    [Tooltip("Brightness of the flash. Higher = more bloom on the bulb.")]
    [SerializeField] private float emissionIntensity = 4f;
    [Tooltip("Brightness of the Light component.")]
    [SerializeField] private float lightIntensity = 3f;
    [Tooltip("Time the light is on each cycle (seconds).")]
    [SerializeField] private float onDuration = 0.35f;
    [Tooltip("Time the light is off each cycle (seconds).")]
    [SerializeField] private float offDuration = 0.35f;
    [Tooltip("How many on/off cycles to play. Set to 0 for infinite (e.g. permanent green).")]
    [SerializeField] private int flashCycles = 6;
    [Tooltip("If true, ends in the ON state (stays green). If false, ends OFF.")]
    [SerializeField] private bool endOnGreen = true;

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flashSound;
    [Range(0f, 1f)][SerializeField] private float volume = 0.4f;

    // ── Cached components ────────────────────────────────────────────────────
    private List<Light> lights = new List<Light>();
    private List<Renderer> renderers = new List<Renderer>();
    private List<Material> instanceMaterials = new List<Material>();
    private bool cached = false;
    private bool isFlashing = false;

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        CacheTargets();
    }

    private void CacheTargets()
    {
        if (cached) return;

        foreach (GameObject t in targets)
        {
            if (t == null) continue;

            lights.AddRange(t.GetComponentsInChildren<Light>(true));

            Renderer[] rends = t.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in rends)
            {
                renderers.Add(r);
                // Use .material (creates an instance) so we don't modify the shared asset.
                instanceMaterials.Add(r.material);
            }
        }
        cached = true;
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary> Hook this into CodeEntryPanel.OnCodeCorrect UnityEvent. </summary>
    public void StartFlashing()
    {
        if (isFlashing) return;
        CacheTargets();
        StartCoroutine(FlashRoutine());
    }

    /// <summary> Stops flashing and turns everything off. </summary>
    public void StopFlashing()
    {
        StopAllCoroutines();
        isFlashing = false;
        SetState(false);
    }

    // ── Routine ──────────────────────────────────────────────────────────────

    private IEnumerator FlashRoutine()
    {
        isFlashing = true;

        if (flashSound != null && audioSource != null)
            audioSource.PlayOneShot(flashSound, volume);

        if (flashCycles <= 0)
        {
            // Infinite — solid green ON forever.
            SetState(true);
            yield break;
        }

        for (int i = 0; i < flashCycles; i++)
        {
            SetState(true);
            yield return new WaitForSeconds(onDuration);
            SetState(false);
            yield return new WaitForSeconds(offDuration);
        }

        SetState(endOnGreen);
        isFlashing = false;
    }

    private void SetState(bool on)
    {
        // Lights
        foreach (Light l in lights)
        {
            if (l == null) continue;
            l.color = flashColor;
            l.intensity = on ? lightIntensity : 0f;
        }

        // Materials
        foreach (Material m in instanceMaterials)
        {
            if (m == null) continue;

            if (on)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", flashColor * emissionIntensity);
                if (m.HasProperty("_BaseColor"))
                    m.SetColor("_BaseColor", flashColor);
                else if (m.HasProperty("_Color"))
                    m.SetColor("_Color", flashColor);
            }
            else
            {
                m.SetColor("_EmissionColor", Color.black);
            }
        }
    }
}