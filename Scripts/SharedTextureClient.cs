using UnityEngine;
using System.Collections;

public class SharedTextureClient : MonoBehaviour {

	/// The name of the sender.
	[Tooltip("The name of the sender.")]
	public string senderName = "";

	// Init default values
	void Init(){
		senderName = "";
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
	
	}

	void SetupGraphicClient(){
		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
		SetupSyphonClient();
		#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		SetupSpoutClient();
		#endif
	}

	#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	void SetupSyphonClient(){
		// TODO
	}

	#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	void SetupSpoutClient(){
		// Instantiate client
		Spout.SpoutReceiver client = gameObject.AddComponent<Spout.SpoutReceiver>();
		// Init receiver
		if (senderName == "") {
			client.sharingName = "Any";
		} else {
			client.sharingName = senderName;
		}
	}

	#endif

	void SaveSettings(){
		string prefix = gameObject.name + GetType ().ToString ();

		PlayerPrefs.SetString (prefix+"senderName", senderName);
	}

	void LoadSettings(){
		string prefix = gameObject.name + GetType ().ToString ();

		senderName = PlayerPrefs.GetString (prefix+"senderName", senderName);
	}

	void OnApplicationQuit(){
		SaveSettings ();
	}

}
