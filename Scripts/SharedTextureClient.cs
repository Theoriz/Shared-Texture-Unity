using UnityEngine;
using System.Collections;

public class SharedTextureClient : MonoBehaviour {

	/// The name of the sender.
	[Tooltip("The name of the sender.")]
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
		// TODO
	}

	#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	void SetupSpoutClient(){
		// Instantiate client
		gameObject.AddComponent<Spout.Spout>();
		Spout.SpoutReceiver client = gameObject.AddComponent<Spout.SpoutReceiver>();
		// Init receiver
		if (senderName == "") {
			client.sharingName = "Any";
		} else {
			client.sharingName = senderName;
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
			mat.mainTexture = client.texture;
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
