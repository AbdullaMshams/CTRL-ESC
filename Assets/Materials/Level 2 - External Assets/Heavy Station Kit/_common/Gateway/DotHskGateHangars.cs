using UnityEngine;
using System.Collections;

public class DotHskGateHangars : DotHskGateEventsClass {
	public override void OnInit(){}
	public override void OnChangeMode(bool isOff, bool isBlocked, bool isBroken){}
	// Motion Event: -1 - start motion, -2 - stop motion, 1 - start general motion, 2 - start tuck motion
	public override void OnMotion(int gateID, int motionEvent){}
}
