using UnityEngine;
using System.Collections;

public class SharedTextureClient : MonoBehaviour {

	/// The name of the application sender.
	[Tooltip("The name of the application sending data.")]
	public string senderAppName = "";

	/// The name of the server defined in the app defined in senderAppName.
	[Tooltip("The name of the server defined in the app defined in senderAppName.")]
	public string senderName = "";

	/// The materials to apply texture shared.
	[Tooltip("The materials to apply texture shared.")]
	public Material[] targetMaterials;

	// Init default values
	void Init(){
		
	}

	void Start () {

		// Init default values
		Init ();

		// Load saved settings
		LoadSettings ();

		// Init Syphon or Spout client
		SetupGraphicClient ();

	}
	
	// Update is called once per frame
	void Update () {
		UpdateGraphicClient ();
	}

	void SetupGraphicClient(){
		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		SetupSyphonClient();
		#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		SetupSpoutClient();
		#endif
	}

	void UpdateGraphicClient(){
		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		//UpdateSyphonClient();
		#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		UpdateSpoutClient();
		#endif
	}

	#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	void SetupSyphonClient(){
		// Instantiate client
		gameObject.AddComponent<Syphon>();
		SyphonClientTexture client = gameObject.AddComponent<SyphonClientTexture>();
		// Init receiver
		client.clientAppName = senderAppName;
		client.clientName = senderName;

		// Apply shared texture
		// Add a mesh renderer because syphon take materials on this mesh renderer
	/*	MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer> ();
		meshRenderer.materials = new Material[targetMaterials.Length];
		for (int i=0; i<targetMaterials.Length; i++) {
			meshRenderer.materials[i] = targetMaterials[i];
		}*/
	}

	#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	void SetupSpoutClient(){
		// Check Direct3D version
		if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Direct3D11) {
			Debug.LogWarning ("If you encounter any issue with Spout client (texture is not received), try to use Direct3D11 graphics API");
		}

		// Instantiate client
		gameObject.AddComponent<Spout.Spout>();
		Spout.SpoutReceiver client = gameObject.AddComponent<Spout.SpoutReceiver>();
		// Init receiver
		if (senderName == "") {
			client.sharingName = "Any";
		} else {
			client.sharingName = senderAppName + " - " + senderName;
		}

		// Apply shared texture 
		foreach (Material mat in targetMaterials) {
			mat.mainTexture = client.texture;
		}
	}

	void UpdateSpoutClient(){
		Spout.SpoutReceiver client = gameObject.GetComponent<Spout.SpoutReceiver> ();
		// Apply shared texture 
		foreach (Material mat in targetMaterials) {
			if (mat != null) {
				mat.mainTexture = client.texture;
			}
		}
	}

	#endif

	void SaveSettings(){
		string prefix = gameObject.name + GetType ().ToString ();

	}

	void LoadSettings(){
		string prefix = gameObject.name + GetType ().ToString ();

	}

	void OnApplicationQuit(){
		SaveSettings ();
	}

}
