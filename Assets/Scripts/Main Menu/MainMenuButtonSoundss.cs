using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound, 0.5f);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound, 1f);
    }
}