using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles Settings panel logic — Audio, Video, Gameplay.
/// Attach to the SettingsPanel GameObject.
/// Works from both the Main Menu and the Pause Menu.
///
/// SETUP:
///   Assign each UI element in the Inspector.
///   Settings are saved to PlayerPrefs automatically.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeValue;

    [Header("Video")]
    [SerializeField] private TextMeshProUGUI displayModeValue;
    [SerializeField] private TextMeshProUGUI resolutionValue;

    [Header("Gameplay")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityValue;

    // ── Data ───────────────────────────────────────────────────────────────────

    private readonly string[] displayModes  = { "FULLSCREEN", "BORDERLESS", "WINDOWED" };
    private readonly string[] resolutions   = { "1280x720", "1600x900", "1920x1080", "2560x1440" };

    private int displayModeIndex;
    private int resolutionIndex;

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        LoadSettings();
    }

    // ── Load / Save ────────────────────────────────────────────────────────────

    private void LoadSettings()
    {
        // Master volume
        float vol = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = vol;
            UpdateVolumeLabel(vol);
        }
        AudioListener.volume = vol;

        // Display mode
        displayModeIndex = PlayerPrefs.GetInt("DisplayMode", 0);
        UpdateDisplayLabel();

        // Resolution
        resolutionIndex = PlayerPrefs.GetInt("Resolution", 2);
        UpdateResolutionLabel();

        // Sensitivity
        float sens = PlayerPrefs.GetFloat("MouseSensitivity", 40f);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = sens;
            UpdateSensitivityLabel(sens);
        }
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume",     masterVolumeSlider  != null ? masterVolumeSlider.value  : 0.8f);
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivitySlider   != null ? sensitivitySlider.value   : 40f);
        PlayerPrefs.SetInt("DisplayMode",   displayModeIndex);
        PlayerPrefs.SetInt("Resolution",    resolutionIndex);
        PlayerPrefs.Save();
        Debug.Log("[Settings] Saved.");
    }

    // ── Audio ─────────────────────────────────────────────────────────────────

    public void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        UpdateVolumeLabel(value);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    private void UpdateVolumeLabel(float value)
    {
        if (masterVolumeValue != null)
            masterVolumeValue.text = Mathf.RoundToInt(value * 100).ToString();
    }

    // ── Video ─────────────────────────────────────────────────────────────────

    public void OnDisplayModePrev() => CycleDisplayMode(-1);
    public void OnDisplayModeNext() => CycleDisplayMode(1);

    private void CycleDisplayMode(int dir)
    {
        displayModeIndex = (displayModeIndex + dir + displayModes.Length) % displayModes.Length;
        UpdateDisplayLabel();
        ApplyDisplayMode();
        PlayerPrefs.SetInt("DisplayMode", displayModeIndex);
    }

    private void UpdateDisplayLabel()
    {
        if (displayModeValue != null)
            displayModeValue.text = displayModes[displayModeIndex];
    }

    private void ApplyDisplayMode()
    {
        switch (displayModeIndex)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.FullScreenWindow;    break;
            case 2: Screen.fullScreenMode = FullScreenMode.Windowed;            break;
        }
    }

    public void OnResolutionPrev() => CycleResolution(-1);
    public void OnResolutionNext() => CycleResolution(1);

    private void CycleResolution(int dir)
    {
        resolutionIndex = (resolutionIndex + dir + resolutions.Length) % resolutions.Length;
        UpdateResolutionLabel();
        ApplyResolution();
        PlayerPrefs.SetInt("Resolution", resolutionIndex);
    }

    private void UpdateResolutionLabel()
    {
        if (resolutionValue != null)
            resolutionValue.text = resolutions[resolutionIndex].Replace("x", "×");
    }

    private void ApplyResolution()
    {
        string[] parts = resolutions[resolutionIndex].Split('x');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int w) &&
            int.TryParse(parts[1], out int h))
        {
            Screen.SetResolution(w, h, Screen.fullScreenMode);
        }
    }

    // ── Gameplay ──────────────────────────────────────────────────────────────

    public void OnSensitivityChanged(float value)
    {
        UpdateSensitivityLabel(value);
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        // Notify PlayerMovement if it exists in scene
        PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
        // We don't set it directly since mouseSensitivity is private —
        // PlayerMovement reads it from PlayerPrefs on Start.
        // For live update, make mouseSensitivity public or add a setter.
    }

    private void UpdateSensitivityLabel(float value)
    {
        if (sensitivityValue != null)
            sensitivityValue.text = Mathf.RoundToInt(value).ToString();
    }
}
