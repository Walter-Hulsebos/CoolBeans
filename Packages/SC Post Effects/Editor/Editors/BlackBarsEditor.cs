using UnityEngine.Rendering.Universal;
using UnityEditor.Rendering;
using UnityEditor;
using UnityEngine;
#if UNITY_2022_2_OR_NEWER
using EffectSettingsEditor = UnityEditor.CustomEditor;
#else
using EffectSettingsEditor = UnityEditor.Rendering.VolumeComponentEditorAttribute;
#endif

namespace SCPE
{
    [EffectSettingsEditor(typeof(BlackBars))]
    sealed class BlackBarsEditor : VolumeComponentEditor
    {
        SerializedDataParameter mode;
        SerializedDataParameter size;
        SerializedDataParameter maxSize;

        private bool isSetup;

        public override void OnEnable()
        {
            base.OnEnable();

            var o = new PropertyFetcher<BlackBars>(serializedObject);
            isSetup = AutoSetup.ValidEffectSetup<BlackBarsRenderer>();

            mode = Unpack(o.Find(x => x.mode));
            size = Unpack(o.Find(x => x.size));
            maxSize = Unpack(o.Find(x => x.maxSize));
        }
        
        public override void OnInspectorGUI()
        {
            SCPE_GUI.DisplayDocumentationButton("black-bars");

            SCPE_GUI.DisplaySetupWarning<BlackBarsRenderer>(ref isSetup, serializedObject);

            PropertyField(mode);
            SCPE_GUI.DisplayIntensityWarning(size);
            
            EditorGUILayout.Space();

            PropertyField(size);
            PropertyField(maxSize);
        }
    }
}