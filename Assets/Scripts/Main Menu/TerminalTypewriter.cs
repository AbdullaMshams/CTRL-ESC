using System.Collections;
using TMPro;
using UnityEngine;

public class TerminalTypewriter : MonoBehaviour
{
    public TextMeshProUGUI terminalText;
    public float typeSpeed = 0.04f;
    public float blinkSpeed = 0.5f;

    private string[] lines = {
        "> INITIALIZING SYSTEM-7...",
        "> LOADING EXPERIMENT DATA...",
        "> SUBJECT STATUS: UNKNOWN",
        "> WELCOME BACK, SUBJECT."
    };

    void Start()
    {
        StartCoroutine(TypeLines());
    }

    IEnumerator TypeLines()
    {
        terminalText.text = "";

        foreach (string line in lines)
        {
            foreach (char c in line)
            {
                terminalText.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }
            terminalText.text += "\n";
            yield return new WaitForSeconds(0.3f);
        }

        StartCoroutine(BlinkCursor());
    }

    IEnumerator BlinkCursor()
    {
        string finalText = terminalText.text;
        bool show = true;

        while (true)
        {
            terminalText.text = finalText + (show ? "<color=#00ff50>█</color>" : " ");
            show = !show;
            yield return new WaitForSeconds(blinkSpeed);
        }
    }
}