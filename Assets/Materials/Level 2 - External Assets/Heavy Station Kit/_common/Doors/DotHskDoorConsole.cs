using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class DotHskDoorConsole : MonoBehaviour {

    public List<DotHskDoorControl> controlledDoors;

    private dotHskDoorMode doorsMode = dotHskDoorMode.active;
    private dotHskDoorMode prevDoorsMode = dotHskDoorMode.active;

    void Start(){
        Init();
    }

    void Update() {
        bool isp = Application.isPlaying;
        if(!isp) { Init(); }
		if(prevDoorsMode != doorsMode) {
            ChangeMode(doorsMode);
            prevDoorsMode = doorsMode;
        }
    }

    void Init() {
        bool first = true;
        foreach(DotHskDoorControl door in controlledDoors) {
			if(door == null) { continue; }
            door.RegisterConsole(this);
            if(first && (door.doorScript != null) ) {
				doorsMode = door.doorScript.mode; first = false;
            }
        }
    }

    public dotHskDoorMode GetMode() {
		if((controlledDoors.Count > 0) && (controlledDoors[0]!=null) && (controlledDoors[0].doorScript != null)) {
            return controlledDoors[0].doorScript.mode;
        }
        return dotHskDoorMode.inactiveClosed;
    }

    public void ChangeMode(dotHskDoorMode doorMode) {
        foreach (DotHskDoorControl door in controlledDoors) {door.SetMode(doorMode);}
    }

	public void SetPowerMode(bool isOn) {
		foreach(DotHskDoorControl door in controlledDoors) {door.SetPowerMode(isOn);}
	}

	public bool IsDoorsAttached(){
		return controlledDoors.Count > 0;
	}
}
