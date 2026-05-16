using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles Settings panel logic — Audio, Display, Controls.
/// Attach to the SettingsPanel GameObject.
/// Works from both the Main Menu and the Pause Menu.
/// </summary>
public class Settings_Manager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject audioContent;
    public GameObject displayContent;
    public GameObject controlsContent;

    [Header("Tabs")]
    public Button audioTab;
    public Button displayTab;
    public Button controlsTab;

    [Header("Audio")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public TextMeshProUGUI musicVal;
    public TextMeshProUGUI sfxVal;

    [Header("Display")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("Controls Grid")]
    public GameObject controlsGrid;
    public GameObject controlRowPrefab;

    [Header("Footer")]
    public Button backButton;
    public Button applyButton;

    [Header("Sounds")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;

    // Tab colors
    private Color activeTabColor = new Color(0f, 1f, 0.31f, 0.06f);
    private Color inactiveTabColor = new Color(0f, 0f, 0f, 0f);
    private Color activeTextColor = new Color(0f, 1f, 0.31f, 1f);
    private Color inactiveTextColor = new Color(0.16f, 0.47f, 0.26f, 1f);

    // Resolutions
    private Resolution[] resolutions;

    // Controls data — add/remove entries here only
    private (string action, string key)[] controls =
    {
        ("MOVE",       "WASD"),
        ("INTERACT",   "E"),
        ("FLASHLIGHT", "F"),
        ("INVENTORY",  "I"),
        ("CROUCH",     "C"),
        ("EXAMINE",    "Q"),
        ("PAUSE",      "ESC"),
    };

    // ── Lifecycle ─────────────────────────────────────

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        LoadSettings();
    }

    void Start()
    {
        SwitchTab("audio");
        SetupResolutions();
        SpawnControls();

        // tab listeners
        audioTab.onClick.AddListener(() => { PlayClick(); SwitchTab("audio"); });
        displayTab.onClick.AddListener(() => { PlayClick(); SwitchTab("display"); });
        controlsTab.onClick.AddListener(() => { PlayClick(); SwitchTab("controls"); });

        // slider listeners
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        // display listeners
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        // footer listeners
        backButton.onClick.AddListener(() => { PlayClick(); CloseSettings(); });
        applyButton.onClick.AddListener(() => { PlayClick(); ApplySettings(); });

        // hover sounds
        AddHoverSound(audioTab);
        AddHoverSound(displayTab);
        AddHoverSound(controlsTab);
        AddHoverSound(backButton);
        AddHoverSound(applyButton);
    }

    // ── Load / Save ───────────────────────────────────

    void LoadSettings()
    {
        // audio
        float music = PlayerPrefs.GetFloat("MusicVolume", 70f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 85f);

        if (musicSlider != null)
        {
            musicSlider.value = music;
            if (musicVal != null) musicVal.text = Mathf.RoundToInt(music) + "%";
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfx;
            if (sfxVal != null) sfxVal.text = Mathf.RoundToInt(sfx) + "%";
        }

        AudioListener.volume = music / 100f;

        // display
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    }

    void ApplySettings()
    {
        // resolution
        if (resolutions != null && resolutionDropdown != null)
        {
            int idx = resolutionDropdown.value;
            if (idx < resolutions.Length)
            {
                Resolution res = resolutions[idx];
                Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            }
        }

        // save
        PlayerPrefs.SetFloat("MusicVolume", musicSlider != null ? musicSlider.value : 70f);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider != null ? sfxSlider.value : 85f);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle != null && fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("[Settings] Applied and saved.");
        CloseSettings();
    }

    // ── Tabs ──────────────────────────────────────────

    public void SwitchTab(string tab)
    {
        audioContent.SetActive(tab == "audio");
        displayContent.SetActive(tab == "display");
        controlsContent.SetActive(tab == "controls");

        SetTabStyle(audioTab, tab == "audio");
        SetTabStyle(displayTab, tab == "display");
        SetTabStyle(controlsTab, tab == "controls");
    }

    void SetTabStyle(Button btn, bool active)
    {
        var img = btn.GetComponent<Image>();
        var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (img != null) img.color = active ? activeTabColor : inactiveTabColor;
        if (txt != null) txt.color = active ? activeTextColor : inactiveTextColor;
    }

    // ── Audio ─────────────────────────────────────────

    void OnMusicChanged(float val)
    {
        if (musicVal != null) musicVal.text = Mathf.RoundToInt(val) + "%";
        AudioListener.volume = val / 100f;
        PlayerPrefs.SetFloat("MusicVolume", val);
    }

    void OnSFXChanged(float val)
    {
        if (sfxVal != null) sfxVal.text = Mathf.RoundToInt(val) + "%";
        PlayerPrefs.SetFloat("SFXVolume", val);
    }

    // ── Display ───────────────────────────────────────

    void SetupResolutions()
    {
        if (resolutionDropdown == null) return;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            if (!options.Contains(option)) options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentIndex = options.Count - 1;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    // ── Controls ──────────────────────────────────────

    void SpawnControls()
    {
        Debug.Log("Grid: " + controlsGrid);
        Debug.Log("Prefab: " + controlRowPrefab);
        if (controlsGrid == null || controlRowPrefab == null) return;

        foreach (Transform child in controlsGrid.transform)
            Destroy(child.gameObject);

        foreach (var (action, key) in controls)
        {
            GameObject row = Instantiate(controlRowPrefab, controlsGrid.transform);
            var texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = action;
                texts[1].text = key;
            }
        }
    }

    // ── Open / Close ──────────────────────────────────

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        SwitchTab("audio");
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    // ── Sounds ────────────────────────────────────────

    void PlayClick()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    void AddHoverSound(Button btn)
    {
        if (btn == null) return;
        var trigger = btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entry.callback.AddListener((e) => {
            if (hoverSound != null)
                audioSource.PlayOneShot(hoverSound, 0.5f);
        });
        trigger.triggers.Add(entry);
    }
}