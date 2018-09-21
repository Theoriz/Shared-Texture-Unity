// KlakSpout - Spout video frame sharing plugin for Unity
// https://github.com/keijiro/KlakSpout

using UnityEngine;
using UnityEditor;

namespace Klak.Spout
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SpoutSender))]
    sealed class SpoutSenderEditor : Editor
    {
        SerializedProperty _sourceTexture;
        SerializedProperty _senderName;
        SerializedProperty _useCamera;
        SerializedProperty _alphaSupport;

        void OnEnable()
        {
            _sourceTexture = serializedObject.FindProperty("_sourceTexture");
            _senderName = serializedObject.FindProperty("_senderName");
            _useCamera = serializedObject.FindProperty("_useCamera");
            _alphaSupport = serializedObject.FindProperty("_alphaSupport");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (targets.Length == 1)
            {
                var sender = (SpoutSender)target;

                if (_useCamera.boolValue)
                {
                    EditorGUILayout.HelpBox(
                        "Spout Sender is running in camera capture mode.",
                        MessageType.None
                    );

                    // Hide the source texture property.
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Spout Sender is running in render texture mode.",
                        MessageType.None
                    );

                    EditorGUILayout.PropertyField(_sourceTexture);
                }
            }
            else
                EditorGUILayout.PropertyField(_sourceTexture);

            EditorGUILayout.PropertyField(_senderName);
            EditorGUILayout.PropertyField(_useCamera);
            EditorGUILayout.PropertyField(_alphaSupport);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
