using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SharedTexture : MonoBehaviour
{
    public Camera TargetCamera;
    public bool UseCameraResolution;
	public int outputWidth;
	public int outputHeight;

    private Camera _myCam;

    private int currentWidth, currentHeight;

    [Header("Spout settings")]
    public bool SpoutOutput;
    public string sharingName = "UnitySender";
    public bool alphaSupport = false;

    private RenderTexture texture;


    [Header("Funnel settings")]
    [HideInInspector]
    /// Anti-aliasing (MSAA) option
    public int antiAliasing = 1;
    [HideInInspector]
    /// Discards alpha channel before sending
    public bool discardAlpha = true;
    [HideInInspector]
    /// Determines how to handle the rendered screen
    public Funnel.Funnel.RenderMode renderMode;

    private Klak.Spout.SpoutSender spout;
    private Funnel.Funnel funnel;
    private bool isSendingTexture; 

    public void NewTextureSize(int width, int height)
    {
        if (UseCameraResolution)
        {
            width = TargetCamera.pixelWidth;
            height = TargetCamera.pixelHeight;
			outputHeight = height;
			outputWidth = width;
        }

        if (width == 0 || height == 0)
            return;

        if (width != currentWidth || height != currentHeight)
        {
            //Debug.Log("New texture : " + width + "*" + height);
            enabled = false;
            RenderTexture newText = new RenderTexture(width, height, 24);
			currentWidth = width;
			currentHeight = height;
			outputHeight = height;
			outputWidth = width;
			GetComponent<Camera>().targetTexture = newText;
            GetComponent<Klak.Spout.SpoutSender>().sourceTexture = newText;
            texture = newText;
            enabled = true;
        }
    }

    // Use this for initialization
    void Awake()
    {
        //Get the shared texture camera on this object
        _myCam = GetComponent<Camera>();

        if (!this.isActiveAndEnabled) return;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        this.gameObject.SetActive(false); //to prevent awake method to execute before its variable initialization

        spout = gameObject.AddComponent<Klak.Spout.SpoutSender>();

        spout.useCamera = false;
        spout.sourceTexture = texture;
        spout.senderName = sharingName;
        spout.alphaSupport = alphaSupport;

        NewTextureSize(outputWidth, outputHeight);

#endif
#if UNITY_EDITOR_MAC || UNITY_STANDALONE_OSX
        this.gameObject.SetActive(false);
        funnel = gameObject.AddComponent<Funnel.Funnel>(); //to prevent awake method to execute before its variable initialization

        funnel.antiAliasing = antiAliasing;
        funnel.discardAlpha = discardAlpha;
        funnel.renderMode = renderMode;
#endif
        this.gameObject.SetActive(true);
        isSendingTexture = true;
        this.enabled = SpoutOutput;
    }

    void OnDisable()
    {
        if (spout != null)
            spout.enabled = false;

        if (funnel != null)
            funnel.enabled = false;
    }

    void OnEnable()
    {
        if (spout == null && funnel == null)
            Awake();

        if (spout != null)
            spout.enabled = true;

        if (funnel != null)
            funnel.enabled = true;
    }

    void UpdateCamera()
    {
        _myCam.CopyFrom(TargetCamera);
        _myCam.targetTexture = texture;
    }

    // Update is called once per frame
    void Update()
    {
        if(!SpoutOutput && isSendingTexture)
        {
            OnDisable();
            isSendingTexture = false;
        }
        if(SpoutOutput && !isSendingTexture)
        {
            OnEnable();
            isSendingTexture = true;
        }

        UpdateCamera();
        NewTextureSize(outputWidth, outputHeight) ;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        if (spout == null) return;

        if (spout.sourceTexture != texture)
            spout.sourceTexture = texture;

#endif
#if UNITY_EDITOR_MAC || UNITY_STANDALONE_OSX

        if(funnel.antiAliasing != antiAliasing)
            funnel.antiAliasing = antiAliasing;

        if(funnel.discardAlpha != discardAlpha)
            funnel.discardAlpha = discardAlpha;

        if(funnel.renderMode != renderMode)
            funnel.renderMode = renderMode;
#endif
    }
}
