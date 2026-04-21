using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class DotHskVentColony : MonoBehaviour {

	public bool isGrateOpen = false;
	public Animator ventGrate;
	public Texture2D bannerTip;
	public AudioSource audioSource;

	private bool touch = false;
	[HideInInspector][SerializeField]
	private bool isOpen = false;

    private DotControlCenter ccInstance = null;
    private KeyCode interactShortcut = KeyCode.E;

    private void Start() {
        if (DotControlCenter.instance != null) {
            if (DotControlCenter.instance.trackChangesSettings) { ccInstance = DotControlCenter.instance; };
            UpdateConfig(DotControlCenter.instance);
        }
    }

    void Update(){
		if (ventGrate != null) {
            // Update Configuration Changes
            if (ccInstance != null) { UpdateConfig(ccInstance); }
            if (Application.isPlaying) {
				bool keye = touch && Input.GetKeyDown (interactShortcut);
				if (keye || (isGrateOpen!=isOpen)) {
					AnimatorStateInfo stateInfo = ventGrate.GetCurrentAnimatorStateInfo (0);
					if (stateInfo.IsName ("idle")) {
						if (keye) {
							isGrateOpen = isOpen = !isOpen;
						} else {
							isOpen = isGrateOpen;
						}
						ventGrate.Play (isOpen ? "open" : "close", -1, 0);
						if (audioSource != null) {
							audioSource.Play ();
						}
					}
				}
			} else {
				#if UNITY_EDITOR
				if(isGrateOpen!=isOpen){
					isOpen = isGrateOpen;
					if (isOpen) {
						ventGrate.transform.localPosition = new Vector3 (-4.77f, 2.09f, 0f);
						ventGrate.transform.localEulerAngles = new Vector3(0f, 0f, 12.167f);
					} else {
						ventGrate.transform.localPosition = new Vector3 (-4.43f, 1f, 0f);
						ventGrate.transform.localEulerAngles = Vector3.zero;
					}
				}
				#endif
			}
		}
	}

	void OnGUI(){
		if( touch && (bannerTip != null) ){
			float _tw = bannerTip.width;
			float _th = bannerTip.height;
			GUI.DrawTexture (new Rect ((Screen.width - _tw) / 2, Screen.height - 36 - _th, _tw, _th), bannerTip, ScaleMode.ScaleToFit, true); 
		}
	}

	void OnTriggerEnter(Collider other){
		if (other.tag == "Player") { touch = true; }
	}

	void OnTriggerExit(Collider other){
		if (other.tag == "Player") { touch = false; }
	}

    void UpdateConfig(DotControlCenter c) {
        interactShortcut = c.interactShortcut;
    }

}
