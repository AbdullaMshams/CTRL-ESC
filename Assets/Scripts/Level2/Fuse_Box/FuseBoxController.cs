using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class FuseBoxController : MonoBehaviour
{
    [Header("Slots — drag all FuseSlot GameObjects here")]
    [SerializeField] private FuseSlot[] slots;

    [Header("On Solve")]
    [Tooltip("GameObject to deactivate when puzzle is solved (e.g. a locked door).")]
    [SerializeField] private GameObject unlockTarget;

    [Tooltip("Delay in seconds before unlockTarget is hidden.")]
    [SerializeField] private float unlockDelay = 0.8f;

    [Tooltip("Fired as soon as all fuses are correctly placed.")]
    public UnityEvent OnPuzzleSolved;


    private bool isSolved = false;


    public void OnSlotChanged()
    {
        if (isSolved) return;

        // Check every slot
        foreach (FuseSlot slot in slots)
        {
            if (!slot.isOccupied || !slot.isCorrect)
                return; // at least one slot wrong or empty — not solved yet
        }

        // All slots occupied and correct!
        isSolved = true;
        Debug.Log("[FuseBox] ✓ Puzzle solved — all fuses correct!");

        OnPuzzleSolved?.Invoke();

        if (unlockTarget != null)
            StartCoroutine(UnlockAfterDelay());
    }

    private IEnumerator UnlockAfterDelay()
    {
        yield return new WaitForSeconds(unlockDelay);
        unlockTarget.SetActive(false);
        Debug.Log($"[FuseBox] Unlocked: {unlockTarget.name}");
    }


    public bool IsSolved() => isSolved;

    public int CorrectCount()
    {
        int n = 0;
        foreach (FuseSlot s in slots)
            if (s.isOccupied && s.isCorrect) n++;
        return n;
    }

    public int TotalSlots() => slots != null ? slots.Length : 0;
}