using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SpoutOverlay : MonoBehaviour
{
    #region Editable properties

    public Klak.Spout.CustomSpoutReceiver receiver;

    public Shader shader;

    #endregion

    #region Private members

    private Material _material;

    #endregion

    #region MonoBehaviour functions

    private void OnEnable() {

        if (!shader)
            shader = Shader.Find("Custom/ImageToCamera");
    }

    void OnDestroy() {
        if (_material != null)
           if (Application.isPlaying)
                Destroy(_material);
            else
                DestroyImmediate(_material);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {

        if (!receiver) return;

        if (_material == null) {
            _material = new Material(shader);
            _material.hideFlags = HideFlags.DontSave;
        }

        _material.SetTexture("_ImageTex", receiver.receivedTexture);

        Graphics.Blit(source, destination, _material);
    }

    #endregion
}
