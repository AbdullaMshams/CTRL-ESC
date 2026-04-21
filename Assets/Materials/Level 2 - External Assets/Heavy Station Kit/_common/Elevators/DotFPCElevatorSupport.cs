using UnityEngine;
using System.Collections;

public class DotFPCElevatorSupport : MonoBehaviour {

	private Transform _parent = null;
	private CharacterController _ccontroller= null;
	private float _skinWidth = 0f;

	void Start(){
		_ccontroller = GetComponent<CharacterController>();
	}

	void OnTriggerEnter(Collider collider) {
		if (collider.gameObject.tag == "Elevator2") {
			_parent = gameObject.transform.parent;
			gameObject.transform.parent = collider.gameObject.transform;
			if( _ccontroller != null ) {
				_skinWidth = _ccontroller.skinWidth;
				_ccontroller.skinWidth = 0.001f;
			}
		}
	}

	void OnTriggerExit(Collider collider) {
		if (collider.gameObject.tag == "Elevator2") {
			gameObject.transform.parent = _parent;
			if( _ccontroller != null ) {
				_ccontroller.skinWidth = _skinWidth;
			}
		}
	}

}
