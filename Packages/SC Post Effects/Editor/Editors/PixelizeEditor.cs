using UnityEditor;
using UnityEditor.Rendering;
#if UNITY_2022_2_OR_NEWER
using EffectSettingsEditor = UnityEditor.CustomEditor;
#else
using EffectSettingsEditor = UnityEditor.Rendering.VolumeComponentEditorAttribute;
#endif

namespace SCPE
{
    [EffectSettingsEditor(typeof(Pixelize))]
    sealed class PixelizeEditor : VolumeComponentEditor
    {
        SerializedDataParameter amount;
        SerializedDataParameter resolutionPreset;
        SerializedDataParameter resolution;
        
        private bool isSetup;

        public override void OnEnable()
        {
            base.OnEnable();

            var o = new PropertyFetcher<Pixelize>(serializedObject);
            isSetup = AutoSetup.ValidEffectSetup<PixelizeRenderer>();

            amount = Unpack(o.Find(x => x.amount));
            resolutionPreset = Unpack(o.Find(x => x.resolutionPreset));
            resolution = Unpack(o.Find(x => x.resolution));
        }

        public override void OnInspectorGUI()
        {
            SCPE_GUI.DisplayDocumentationButton("pixelize");

            SCPE_GUI.DisplaySetupWarning<PixelizeRenderer>(ref isSetup, serializedObject);

            PropertyField(amount);
            SCPE_GUI.DisplayIntensityWarning(amount);
            
            PropertyField(resolutionPreset);
            if (resolutionPreset.value.intValue == (int)Pixelize.Resolution.Custom)
            {
                EditorGUI.indentLevel++;
                PropertyField(resolution);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
        }
    }
}