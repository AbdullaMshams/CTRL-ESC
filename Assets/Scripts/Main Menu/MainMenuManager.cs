using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to a MainMenuManager empty GameObject in the Main Menu scene.
///
/// SCENE SETUP:
///   - Create a scene called "MainMenu" — add it to Build Settings index 0
///   - Create a scene called "LevelSelect" — add it to Build Settings index 1
///   - Your game level scenes go after that
///
/// HIERARCHY SETUP:
///   MainMenuManager (this script)
///   Canvas
///     MainMenuPanel       ← the main buttons panel
///     SettingsPanel       ← settings overlay
///     ConfirmPanel        ← confirmation dialog
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmPanel;

    [Header("Scene Names")]
    [SerializeField] private string levelSelectScene = "LevelSelect";

    // Which confirm action is pending
    private System.Action pendingConfirmAction;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        // Make sure cursor is visible and unlocked on the menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Show only the main panel on start
        ShowPanel(mainMenuPanel);
    }

    private void Update()
    {
        // ESC closes settings or confirm if open
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf) CloseSettings();
            else if (confirmPanel.activeSelf) CloseConfirm();
        }
    }

    // ── Button Callbacks ──────────────────────────────────────────────────────

    /// <summary>
    /// Called by the NEW GAME button.
    /// If a save exists, asks for confirmation first.
    /// If no save, goes straight to level select.
    /// </summary>
    public void OnNewGame()
    {
        bool hasSave = PlayerPrefs.HasKey("UnlockedLevel") &&
                       PlayerPrefs.GetInt("UnlockedLevel") > 1;

        if (hasSave)
        {
            // Warn: previous save will be deleted
            ShowConfirm(
                () => {
                    DeleteSave();
                    LoadLevelSelect();
                }
            );
        }
        else
        {
            // No save — just start fresh
            DeleteSave();
            LoadLevelSelect();
        }
    }

    /// <summary>
    /// Called by the CONTINUE button.
    /// Goes to level select (player picks up from where they left off).
    /// </summary>
    public void OnContinue()
    {
        LoadLevelSelect();
    }

    /// <summary>
    /// Called by the SETTINGS button.
    /// </summary>
    public void OnSettings()
    {
        ShowPanel(settingsPanel);
    }

    /// <summary>
    /// Called by the QUIT button.
    /// </summary>
    public void OnQuit()
    {
        ShowConfirm(() => {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        });
    }

    // ── Settings ──────────────────────────────────────────────────────────────

    public void CloseSettings()
    {
        ShowPanel(mainMenuPanel);
    }

    // ── Confirm Dialog ────────────────────────────────────────────────────────

    /// <summary>
    /// Show the confirm panel and store what to do on confirm.
    /// </summary>
    private void ShowConfirm(System.Action onConfirm)
    {
        pendingConfirmAction = onConfirm;
        confirmPanel.SetActive(true);
    }

    /// <summary>
    /// Called by the CONFIRM button in the dialog.
    /// </summary>
    public void OnConfirmYes()
    {
        pendingConfirmAction?.Invoke();
        pendingConfirmAction = null;
        CloseConfirm();
    }

    /// <summary>
    /// Called by the CANCEL button in the dialog.
    /// </summary>
    public void CloseConfirm()
    {
        confirmPanel.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void ShowPanel(GameObject panel)
    {
        mainMenuPanel.SetActive(panel == mainMenuPanel);
        settingsPanel.SetActive(panel == settingsPanel);
        // confirmPanel is toggled separately — not exclusive
    }

    private void DeleteSave()
    {
        PlayerPrefs.SetInt("UnlockedLevel", 1); // only level 1 unlocked
        PlayerPrefs.Save();
        Debug.Log("[MainMenu] Save data reset.");
    }

    private void LoadLevelSelect()
    {
        SceneManager.LoadScene(levelSelectScene);
    }
}
