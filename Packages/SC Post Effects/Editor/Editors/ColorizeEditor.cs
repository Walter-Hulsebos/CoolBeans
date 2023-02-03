﻿using UnityEditor;
using UnityEditor.Rendering;
#if UNITY_2022_2_OR_NEWER
using EffectSettingsEditor = UnityEditor.CustomEditor;
#else
using EffectSettingsEditor = UnityEditor.Rendering.VolumeComponentEditorAttribute;
#endif

namespace SCPE
{
    [EffectSettingsEditor(typeof(Colorize))]
    sealed class ColorizeEditor : VolumeComponentEditor
    {
        SerializedDataParameter mode;
        SerializedDataParameter intensity;
        SerializedDataParameter colorRamp;

        private bool isSetup;

        public override void OnEnable()
        {
            base.OnEnable();

            var o = new PropertyFetcher<Colorize>(serializedObject);
            isSetup = AutoSetup.ValidEffectSetup<ColorizeRenderer>();

            mode = Unpack(o.Find(x => x.mode));
            intensity = Unpack(o.Find(x => x.intensity));
            colorRamp = Unpack(o.Find(x => x.colorRamp));
        }

        public override void OnInspectorGUI()
        {
            SCPE_GUI.DisplayDocumentationButton("colorize");

            SCPE_GUI.DisplaySetupWarning<ColorizeRenderer>(ref isSetup, serializedObject);

            PropertyField(intensity);
            SCPE_GUI.DisplayIntensityWarning(intensity);
            
            EditorGUILayout.Space();
            
            PropertyField(mode);
            PropertyField(colorRamp);

            if (colorRamp.value.objectReferenceValue)
            {
                SCPE.CheckGradientImportSettings(colorRamp.value.objectReferenceValue);
            }
        }
    }
}
