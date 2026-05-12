using UnityEngine;
using System.Collections;

/// <summary>
/// Attach this to each clickable 3D button on the keypad.
/// Requires a Collider (BoxCollider is fine).
///
/// Setup:
///   - Drag the parent CodePanel into "panel".
///   - Set Action to NumberKey, ClearKey, or SubmitKey.
///   - For number buttons, set "numberValue" to "0", "1", ... "9".
///
/// On click, this fires the correct method on CodeEntryPanel and plays a
/// small press animation on the button mesh.
/// </summary>
[RequireComponent(typeof(Collider))]
public class KeypadButton : MonoBehaviour
{
    public enum ButtonAction { NumberKey, ClearKey, SubmitKey }

    [Header("Button Config")]
    [SerializeField] private CodeEntryPanellevel1 panel;
    [SerializeField] private ButtonAction action = ButtonAction.NumberKey;
    [Tooltip("Used only when Action = NumberKey. Must be \"0\" through \"9\".")]
    [SerializeField] private string numberValue = "0";

    [Header("Press Animation")]
    [Tooltip("How far the button presses inward (local space).")]
    [SerializeField] private float pressDepth = 0.005f;
    [Tooltip("Axis to press along (usually local -Z or -Y depending on your model).")]
    [SerializeField] private Vector3 pressDirection = new Vector3(0f, 0f, -1f);
    [SerializeField] private float pressDuration = 0.08f;

    private Vector3 restLocalPos;
    private bool isPressing = false;

    private void Awake()
    {
        restLocalPos = transform.localPosition;

        if (panel == null)
        {
            // Try to auto-find on parent.
            panel = GetComponentInParent<CodeEntryPanellevel1>();
        }
    }

    private void OnMouseDown()
    {
        if (panel == null || !panel.IsActive || panel.IsSolved) return;
        if (isPressing) return;

        // Trigger the correct action.
        switch (action)
        {
            case ButtonAction.NumberKey: panel.PressNumber(numberValue); break;
            case ButtonAction.ClearKey: panel.PressClear(); break;
            case ButtonAction.SubmitKey: panel.PressSubmit(); break;
        }

        StartCoroutine(PressAnimation());
    }

    private IEnumerator PressAnimation()
    {
        isPressing = true;

        Vector3 pressedPos = restLocalPos + pressDirection.normalized * pressDepth;
        float t = 0f;

        // Press in.
        while (t < pressDuration)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(restLocalPos, pressedPos, t / pressDuration);
            yield return null;
        }
        transform.localPosition = pressedPos;

        // Pop back out.
        t = 0f;
        while (t < pressDuration)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(pressedPos, restLocalPos, t / pressDuration);
            yield return null;
        }
        transform.localPosition = restLocalPos;

        isPressing = false;
    }
}