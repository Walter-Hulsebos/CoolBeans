﻿using UnityEditor;
using UnityEditor.Rendering;
#if UNITY_2022_2_OR_NEWER
using EffectSettingsEditor = UnityEditor.CustomEditor;
#else
using EffectSettingsEditor = UnityEditor.Rendering.VolumeComponentEditorAttribute;
#endif

namespace SCPE
{
    [EffectSettingsEditor(typeof(Kaleidoscope))]
    sealed class KaleidoscopeEditor : VolumeComponentEditor
    {
        SerializedDataParameter radialSplits;
        SerializedDataParameter horizontalSplits;
        SerializedDataParameter verticalSplits;
        SerializedDataParameter center;
        SerializedDataParameter maintainAspectRatio;

        private bool isSetup;

        public override void OnEnable()
        {
            base.OnEnable();

            var o = new PropertyFetcher<Kaleidoscope>(serializedObject);
            isSetup = AutoSetup.ValidEffectSetup<KaleidoscopeRenderer>();

            radialSplits = Unpack(o.Find(x => x.radialSplits));
            horizontalSplits = Unpack(o.Find(x => x.horizontalSplits));
            verticalSplits = Unpack(o.Find(x => x.verticalSplits));
            center = Unpack(o.Find(x => x.center));
            maintainAspectRatio = Unpack(o.Find(x => x.maintainAspectRatio));
        }


        public override void OnInspectorGUI()
        {
            SCPE_GUI.DisplayDocumentationButton("kaleidoscope");

            SCPE_GUI.DisplaySetupWarning<KaleidoscopeRenderer>(ref isSetup, serializedObject);

            PropertyField(radialSplits);
            SCPE_GUI.DisplayIntensityWarning(radialSplits);
            
            PropertyField(horizontalSplits);
            PropertyField(verticalSplits);

            EditorGUILayout.Space();
            
            PropertyField(center);
            PropertyField(maintainAspectRatio);
        }
    }
}