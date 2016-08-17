using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class SyncCamera : MonoBehaviour {

	/*
	 * This script will copy all parameters from reference camera to the camera
	 * attached to this gaemobject, except the target texture
	 * 
	 */

	public Camera refCam = null;
	private Camera thisCam = null;

	// Use this for initialization
	void Start () {
		thisCam = GetComponent<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (refCam != null && thisCam != null) {
			// Save a reference on the target texture of this cam
			RenderTexture targetTexture = thisCam.targetTexture;
			// Apply refCam parameters to this cam
			thisCam.CopyFrom(refCam);
			// Put back target texture that may have been replaced by the one from refCam
			thisCam.targetTexture = targetTexture;
		}
	}
}
