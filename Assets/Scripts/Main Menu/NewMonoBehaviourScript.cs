using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
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

    [Header("Tab Colors")]
    private Color activeTabColor = new Color(0f, 1f, 0.31f, 0.06f);
    private Color inactiveTabColor = new Color(0f, 0f, 0f, 0f);
    private Color activeTextColor = new Color(0f, 1f, 0.31f, 1f);
    private Color inactiveTextColor = new Color(0.16f, 0.47f, 0.26f, 1f);

    [Header("Audio")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public TextMeshProUGUI musicVal;
    public TextMeshProUGUI sfxVal;
    public AudioMixer audioMixer;

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

    // resolution list
    private Resolution[] resolutions;

    // controls data
    private (string action, string key)[] controls = {
        ("MOVE",       "WASD"),
        ("INTERACT",   "E"),
        ("FLASHLIGHT", "F"),
        ("INVENTORY",  "I"),
        ("CROUCH",     "C"),
        ("EXAMINE",    "Q"),
        ("PAUSE",      "ESC"),
    };

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        // start on audio tab
        SwitchTab("audio");

        // setup sliders
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        musicSlider.value = 70;
        sfxSlider.value = 85;

        // setup resolutions
        SetupResolutions();

        // setup fullscreen
        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        // spawn controls
        SpawnControls();

        // tab buttons
        audioTab.onClick.AddListener(() => { PlayClick(); SwitchTab("audio"); });
        displayTab.onClick.AddListener(() => { PlayClick(); SwitchTab("display"); });
        controlsTab.onClick.AddListener(() => { PlayClick(); SwitchTab("controls"); });

        // footer buttons
        backButton.onClick.AddListener(() => { PlayClick(); CloseSettings(); });
        applyButton.onClick.AddListener(() => { PlayClick(); ApplySettings(); });

        // hover sounds on all buttons
        AddHoverSound(audioTab);
        AddHoverSound(displayTab);
        AddHoverSound(controlsTab);
        AddHoverSound(backButton);
        AddHoverSound(applyButton);
    }

    // ── TABS ──────────────────────────────────────────
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

    // ── AUDIO ─────────────────────────────────────────
    void OnMusicChanged(float val)
    {
        musicVal.text = Mathf.RoundToInt(val) + "%";
        if (audioMixer != null)
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(val / 100f) * 20f);
    }

    void OnSFXChanged(float val)
    {
        sfxVal.text = Mathf.RoundToInt(val) + "%";
        if (audioMixer != null)
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(val / 100f) * 20f);
    }

    // ── DISPLAY ───────────────────────────────────────
    void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            if (!options.Contains(option)) options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // ── CONTROLS ──────────────────────────────────────
    void SpawnControls()
    {
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

    // ── APPLY ─────────────────────────────────────────
    void ApplySettings()
    {
        int idx = resolutionDropdown.value;
        if (resolutions != null && idx < resolutions.Length)
        {
            Resolution res = resolutions[idx];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        }

        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.Save();

        CloseSettings();
    }

    // ── OPEN / CLOSE ──────────────────────────────────
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        SwitchTab("audio");
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    // ── SOUNDS ────────────────────────────────────────
    void PlayClick()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    void AddHoverSound(Button btn)
    {
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