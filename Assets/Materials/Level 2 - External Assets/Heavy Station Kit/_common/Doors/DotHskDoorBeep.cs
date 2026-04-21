using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotHskDoorBeep : MonoBehaviour {

    public AudioSource audioSource;
    public AudioClip beepSound;

    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            if ((audioSource != null) && (beepSound != null)) {
                audioSource.clip = beepSound;
                audioSource.time = 0;
                audioSource.Play();
            }
        }
    }
    
}