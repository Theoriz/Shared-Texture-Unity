using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Camera))]
public class SharedTextureServer : MonoBehaviour {

	/*
	 * This class creates the appropriate component for texture sharing according 
	 * to the target platform of the app (Syphon for OSX, Spout for Windows)
	 */ 

	public bool autoResolution;
	public bool lockAspectRatio;
	private float aspectRatio;

	public int renderWidth;
	public int renderHeight;
	private int minRenderSize = 300;
	private bool forceRenderSizeUpdate = true;

	private int oldRenderWidth;
	private int oldRenderHeight;

	private int lastManualRenderWidth;
	private int lastManualRenderHeight;
	private bool wasAutoResolution;	// auResolution bool state on last frame

	/// Boolean indicating if the graphic server has already been setup
	private bool isSetup = false;
	/// Boolean indicating if the graphic server has been initialized
	private bool isInitialized = false;

	/// The render texture used by the camera
	private RenderTexture renderTexture;

	#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	private enum GraphicServer{
	SYPHON,
	FUNNEL
	};

	private GraphicServer graphicServer = GraphicServer.SYPHON;
	#endif

	// Init default values
	void Init(){
		autoResolution = true;
		lockAspectRatio = false;
		renderWidth = 1920;
		renderHeight = 1080;
		lastManualRenderWidth = renderWidth;
		lastManualRenderHeight = renderHeight;
		isSetup = false;
		renderTexture = GetComponent<Camera> ().targetTexture;
	}

	void Start () {

		// Init default values
		Init ();

		// Load saved settings
		LoadSettings ();
		Debug.Log ("Render size after load settings : " + renderWidth + "x" + renderHeight);

		// Init other values depending on previously loaded settings 
		oldRenderWidth = renderWidth;
		oldRenderHeight = renderHeight;
		aspectRatio = (float)lastManualRenderWidth/(float)lastManualRenderHeight;

		// The graphic server can now be set up
		isInitialized = true;

		// Init Syphon or Spout server
		SetupGraphicServer ();

	}

	public void SetResolution(int x, int y){
		// Allow a change from other scripts only if the resolution is in AUTO mode
		if (autoResolution && x >= minRenderSize && y >= minRenderSize) {
			renderWidth = x;
			renderHeight = y;
		}
	}

	void Update(){

		// If auto resolution mode just changed
		if (!autoResolution && wasAutoResolution) {
			// If current resolution differ from the last manual resolution saved
			if (renderWidth != lastManualRenderWidth || renderHeight != lastManualRenderHeight) {
				renderWidth = lastManualRenderWidth;
				renderHeight = lastManualRenderHeight;
			}
		}

		// Update the fbo if the render size has changed
		if (forceRenderSizeUpdate || (renderWidth != oldRenderWidth || renderHeight != oldRenderHeight && renderWidth > 100 && renderHeight > 100)) {
			forceRenderSizeUpdate = false;
			StartCoroutine(UpdateRender());
		}

		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		// Safety check to make sure that the syphon texture is reset correctly when changing window size
		RenderTexture rt = gameObject.GetComponent<Camera>().targetTexture;
		if(rt == null || (rt != null && (rt.width != renderWidth || rt.height != renderHeight) ) ){
			switch(graphicServer){
			case GraphicServer.SYPHON:
				ResizeSyphonRenderTexture ();
				break;

			case GraphicServer.FUNNEL:
				ResizeFunnelRenderTexture();
				break;

			default:
				break;
			}
		}
		#endif

		// If not in auto resolution mode
		if (!autoResolution) {
			// Not in auto mode, so save resolution as the last manually saved
			lastManualRenderWidth = renderWidth;
			lastManualRenderHeight = renderHeight;
		}

		// Save autoResolution bool state this frame for the next one
		wasAutoResolution = autoResolution;
	}

	void OnEnable(){
		SetupGraphicServer ();
	}

	void OnDisable(){
		DestroyGraphicServer ();
	}

	IEnumerator UpdateRender () {
		// If aspect ratio locked
		if (!autoResolution && lockAspectRatio) {
			// If user modified width, change height to match aspect ratio
			if (renderWidth != oldRenderWidth) {
				renderHeight = Mathf.RoundToInt ((float)renderWidth / aspectRatio);
			}
			// If user modified height, change width to match aspect ratio
			else if (renderHeight != oldRenderHeight) {
				renderWidth = Mathf.RoundToInt (renderHeight * aspectRatio);
			}
		}

		// If aspect ratio not locked, compute it 
		if (!autoResolution && !lockAspectRatio) {
			aspectRatio = (float)renderWidth / (float)renderHeight;
		}

		// Resize window
		// A resolution switch does not happen immediately; it will actually happen when the current frame is finished
		UpdateWindowSize ();
		// Changing the window resolution has some side effects on render textures (disallocate), so we will wait that Unity 
		// change the resolution, and only then we will resize the render textures, so if a render texture has been disallocated, 
		// it will be reallocated in all cases.
		yield return new WaitForEndOfFrame();

		ResizeRenderTexture ();

		// Save current values as old values to be able to compare them to new values next time they are changed in UI
		oldRenderWidth = renderWidth;
		oldRenderHeight = renderHeight;

	}

	void UpdateWindowSize(){
		int sw = Screen.currentResolution.width;
		int sh = Screen.currentResolution.height;
		if (renderWidth < minRenderSize || renderHeight < minRenderSize) {
			// Do nothing
		} else if (renderWidth < 0.9f * sw && renderHeight < 0.9 * sh) {
			Screen.SetResolution (renderWidth, renderHeight, false);
		} else {
			float renderRatio = (float)renderWidth / (float)renderHeight;
			float screenRatio = sw / sh;
			if (renderRatio > screenRatio) {
				int newWidth = (int)(0.9f * sw);
				Screen.SetResolution (newWidth, (int)(newWidth / renderRatio), false);
			} else {
				int newHeight = (int)(0.9f * sh);
				//Debug.LogError ("sh :" + sh + " / newheight : " + newHeight + " / calc width : " + (int)(newHeight * renderRatio));
				Screen.SetResolution ((int)((float)newHeight * (float)renderRatio), newHeight, false);
			}
		}

	}

	void SetupGraphicServer(){
		if (!isSetup && isInitialized) {
			#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

			// Check current Unity version
			// < 5.3
			if(float.Parse( Application.unityVersion.Substring(0,3))<5.3){ //test the current Unity version. if it's oldest than 5.3 use syphon, else use funnel
				// Use Syphon (because have some issues on version >5.2 when writing this code)
				graphicServer = GraphicServer.SYPHON;
			}
			// >= 5.3
			else{
				// Use funnel, compatible with 5.3+ and latest OpenGL backend
				graphicServer = GraphicServer.FUNNEL;
			}

			switch(graphicServer){
			case GraphicServer.SYPHON:
				SetupSyphonServer();
				break;

			case GraphicServer.FUNNEL:
				SetupFunnelServer();
				break;

			default:
				break;
			}

			#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

			SetupSpoutServer ();
			#endif

			isSetup = true;
		}
	}

	void DestroyGraphicServer(){
		if(isSetup){
			#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			switch(graphicServer){
			case GraphicServer.SYPHON:
				StartCoroutine(DestroySyphonServer());
				break;
	
			case GraphicServer.FUNNEL:
				DestroyFunnelServer();
				break;
	
			default:
				break;
			}
	
			#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	
			DestroySpoutServer();
			#endif
	
			isSetup = false;
		}
	}

	void ResizeRenderTexture(){
		
		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		// Apply changes to syphon server
		switch(graphicServer){
		case GraphicServer.SYPHON:
			ResizeSyphonRenderTexture();
			break;

		case GraphicServer.FUNNEL:
			ResizeFunnelRenderTexture();
			break;

		default:
			break;
		}
		#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		// Apply changes to spout server
		ResizeSpoutRenderTexture();
		#endif
	}

	/// If true, preview in game view, else send only visuals rendered 
	public void ShowEditorView(bool show){
		if (show) {

			#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			switch(graphicServer){
			case GraphicServer.SYPHON:
				gameObject.GetComponent<SyphonServerTextureCustomResolution> ().drawScene = true;
				break;

			case GraphicServer.FUNNEL:
				//#if UNITY_EDITOR
				gameObject.GetComponent<Funnel.Funnel>().renderMode = Funnel.Funnel.RenderMode.PreviewOnGameView;
				//#else
				//gameObject.GetComponent<Funnel.Funnel>().renderMode = Funnel.Funnel.RenderMode.RenderToTarget;
				//#endif
				break;

			default:
				break;
			}
			#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			gameObject.GetComponent<Camera>().enabled = true;
			#endif

		} else {

			#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			switch(graphicServer){
			case GraphicServer.SYPHON:
			gameObject.GetComponent<SyphonServerTextureCustomResolution> ().drawScene = false;
			break;

			case GraphicServer.FUNNEL:
			gameObject.GetComponent<Funnel.Funnel> ().renderMode = Funnel.Funnel.RenderMode.SendOnly;
			break;

			default:
			break;
			}
			#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			gameObject.GetComponent<Camera>().enabled = false;
			#endif

		}
	}

	#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	void SetupSyphonServer(){
		// Add syphon components
		gameObject.AddComponent<Syphon>();
		gameObject.AddComponent<SyphonServerTextureCustomResolution>();
	}

	void SetupFunnelServer(){
		// Add funnel components
		Funnel.Funnel server = gameObject.AddComponent<Funnel.Funnel>();
		// Setup funnel component
		//#if UNITY_EDITOR
		server.renderMode = Funnel.Funnel.RenderMode.PreviewOnGameView;
		//#else
		//server.renderMode = Funnel.Funnel.RenderMode.RenderToTarget;
		//#endif
		server.screenWidth = renderWidth;
		server.screenHeight = renderHeight;
	}

	void ResizeSyphonRenderTexture(){
		gameObject.GetComponent<SyphonServerTextureCustomResolution> ().renderWidth = renderWidth;
		gameObject.GetComponent<SyphonServerTextureCustomResolution> ().renderHeight = renderHeight;
		gameObject.GetComponent<SyphonServerTextureCustomResolution> ().createOrResizeRenderTexture ();
	}

	void ResizeFunnelRenderTexture(){
		gameObject.GetComponent<Funnel.Funnel> ().screenWidth = renderWidth;
		gameObject.GetComponent<Funnel.Funnel> ().screenHeight = renderHeight;
		gameObject.GetComponent<Funnel.Funnel> ().ApplyRenderTextureToCamera ();
	}

	IEnumerator DestroySyphonServer(){
		// Get reference to camera before destroy
		Camera syphonCam = gameObject.GetComponent<SyphonServerTextureCustomResolution> ().cameraInstance;
		// Remove syphon components
		Destroy(gameObject.GetComponent<SyphonServerTextureCustomResolution>());
		// Wait end of frame because we need to destroy a render texture, 
		// and destroying syphon at same time as SyphonServerTextureCustomResolution cause crash
		yield return new WaitForEndOfFrame ();

		Destroy(gameObject.GetComponent<Syphon>());
		// Remove render texture generated by syphon
		syphonCam.targetTexture.Release ();
		syphonCam.targetTexture = null;
		syphonCam.enabled = true;
		// Put back the original renderTexture to the camera if it had one before Syphon was added
		// (because Syphon overrides the target texture on enable to put its own)
		syphonCam.targetTexture = renderTexture;
	}

	void DestroyFunnelServer(){
		// Get funnel component
		Funnel.Funnel server = GetComponent<Funnel.Funnel>();
		// Disable it
		server.enabled = false;

		// Release render texture generated by server
		Destroy(server.renderTexture);
		// Then destroy server
		Destroy(server);
		// Put back the original renderTexture to the camera if it had one before Funnel was added
		// (because Funnel overrides the target texture on enable to put its own)
		GetComponent<Camera>().targetTexture = renderTexture;
	}

	#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	void SetupSpoutServer(){
		// Create a new camera for Spout, based on this camera
		// because Spout can't send a render texture and show the scene in app window with the same camera
		// This instantiation is made in Start function, to prevent cloned camera 
		// to replicate itself through this script before it is destroyed.
		GameObject spoutCamera = Instantiate(gameObject);
		DontDestroyOnLoad(spoutCamera);

		// Remove useless cloned components
		Destroy(spoutCamera.GetComponent<SharedTextureServer>());
		Destroy(spoutCamera.GetComponent<AudioListener>());
		Destroy(spoutCamera.GetComponent<GUILayer>());

		// Set name and tag 
		spoutCamera.name = "Camera Spout Sender";
		spoutCamera.tag = "SpoutCamera";

		// Make spout sender camera child of main camera
		spoutCamera.transform.parent = this.transform;

		// Add spout components 
		gameObject.AddComponent<Spout.Spout>();	// Instantiate Spout script on main camera to prevent crash when exiting app
		Spout.SpoutSender sender = spoutCamera.AddComponent<Spout.SpoutSender>();
		spoutCamera.AddComponent<Spout.InvertCamera>();
		// Add component to sync spout camera and UI camera (which are 2 different cameras, unlike Syphon)
		SyncCamera sync = spoutCamera.AddComponent<SyncCamera>();
		sync.refCam = GetComponent<Camera> ();

		// Setup spout components
		//sender.sharingName = Application.productName;
		sender.sharingName = gameObject.name + " - " + System.IO.Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName); // same way as Syphon
		// Create a new render texture
		RenderTexture targetTexture = new RenderTexture(renderWidth, renderHeight, 24);
		// Set it to the camera 
		spoutCamera.GetComponent<Camera>().targetTexture = targetTexture;
		// And set it to Spout
		sender.texture = targetTexture;
	}

	void ResizeSpoutRenderTexture(){
		GameObject spoutChild = null;
		foreach (Transform child in gameObject.transform) {
			if(child.CompareTag("SpoutCamera")){
				spoutChild = child.gameObject;
				break;
			}
		}
		Spout.SpoutSender spoutSender = spoutChild.GetComponent<Spout.SpoutSender>();
		Camera camera = spoutChild.GetComponent<Camera>();

		// Disable Spout while updating targettexture to prevent crash
		spoutSender.enabled = false;

		// Create a new render texture because we can't reallocate an already existing one
		// Store camera target texture in temp variable to be able to release it while not in use by the camera
		RenderTexture targetTexture = camera.targetTexture;
		camera.targetTexture = null;
		targetTexture.Release();
		targetTexture = new RenderTexture(renderWidth, renderHeight, 24);
		// Set it to the camera 
		camera.targetTexture = targetTexture;
		// Set it to spout sender
		spoutSender.texture = targetTexture;

		// Enable spout sender
		spoutSender.enabled = true;
	}

	void DestroySpoutServer(){
		// Find the gameobject hosting the spout camera
		GameObject spoutChild = null;
		foreach (Transform child in gameObject.transform) {
			if(child.CompareTag("SpoutCamera")){
				spoutChild = child.gameObject;
				break;
			}
		}

		// Destroy it
		Destroy(spoutChild);

		// Then destroy spout component
		Destroy (gameObject.GetComponent<Spout.Spout> ());
	}
	#endif

	void SaveSettings(){
		string prefix = gameObject.name + GetType ().ToString ();

		PlayerPrefs.SetInt (prefix+"renderWidth", renderWidth);
		PlayerPrefs.SetInt (prefix+"renderHeight", renderHeight);
		PlayerPrefs.SetInt (prefix+"autoResolution", autoResolution?1:0);
		PlayerPrefs.SetInt (prefix+"lastManualRenderWidth", lastManualRenderWidth);
		PlayerPrefs.SetInt (prefix+"lastManualRenderHeight", lastManualRenderHeight);
	}

	void LoadSettings(){
		string prefix = gameObject.name + GetType ().ToString ();

		renderWidth = PlayerPrefs.GetInt (prefix+"renderWidth", renderWidth);
		renderHeight = PlayerPrefs.GetInt(prefix+"renderHeight", renderHeight);
		autoResolution = PlayerPrefs.GetInt (prefix+"autoResolution", autoResolution?1:0) == 1 ? true : false;
		lastManualRenderWidth = PlayerPrefs.GetInt (prefix+"lastManualRenderWidth", lastManualRenderWidth);
		lastManualRenderHeight = PlayerPrefs.GetInt (prefix+"lastManualRenderHeight", lastManualRenderHeight);
	}

	void OnApplicationQuit(){
		SaveSettings ();
	}

}