using UnityEngine;
using TMPro;
using System;

/// <summary>
/// Reusable confirmation dialog.
/// Attach to the ConfirmPanel GameObject.
///
/// SETUP:
///   ConfirmPanel
///     TitleText       ← TextMeshProUGUI
///     BodyText        ← TextMeshProUGUI
///     ConfirmButton   → OnClick: ConfirmDialog.OnConfirm()
///     CancelButton    → OnClick: ConfirmDialog.OnCancel()
/// </summary>
public class ConfirmDialog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI confirmButtonText;

    private Action onConfirm;
    private Action onCancel;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Show the dialog with custom text and callbacks.
    /// </summary>
    public void Show(string title, string body, string confirmLabel,
                     Action onConfirmCallback, Action onCancelCallback = null)
    {
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;
        if (confirmButtonText != null) confirmButtonText.text = confirmLabel;

        onConfirm = onConfirmCallback;
        onCancel = onCancelCallback;

        gameObject.SetActive(true);
    }

    // ── Button Callbacks ──────────────────────────────────────────────────────

    public void OnConfirm()
    {
        onConfirm?.Invoke();
        gameObject.SetActive(false);
    }

    public void OnCancel()
    {
        onCancel?.Invoke();
        gameObject.SetActive(false);
    }
}