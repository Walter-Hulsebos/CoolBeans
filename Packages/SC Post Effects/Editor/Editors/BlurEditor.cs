﻿using UnityEditor;
using UnityEditor.Rendering;
#if UNITY_2022_2_OR_NEWER
using EffectSettingsEditor = UnityEditor.CustomEditor;
#else
using EffectSettingsEditor = UnityEditor.Rendering.VolumeComponentEditorAttribute;
#endif

namespace SCPE
{
    [EffectSettingsEditor(typeof(Blur))]
    sealed class BlurEditor : VolumeComponentEditor
    {
        Blur effect;

        SerializedDataParameter mode;
        SerializedDataParameter highQuality;
        SerializedDataParameter amount;
        SerializedDataParameter iterations;
        SerializedDataParameter downscaling;

        SerializedDataParameter distanceFade;
        SerializedDataParameter startFadeDistance;
        SerializedDataParameter endFadeDistance;
        
        private bool isSetup;

        public override void OnEnable()
        {
            base.OnEnable();

            effect = (Blur)target;
            var o = new PropertyFetcher<Blur>(serializedObject);

            isSetup = AutoSetup.ValidEffectSetup<BlurRenderer>();

            mode = Unpack(o.Find(x => x.mode));
            highQuality = Unpack(o.Find(x => x.highQuality));
            amount = Unpack(o.Find(x => x.amount));
            iterations = Unpack(o.Find(x => x.iterations));
            downscaling = Unpack(o.Find(x => x.downscaling));
            distanceFade = Unpack(o.Find(x => x.distanceFade));
            startFadeDistance = Unpack(o.Find(x => x.startFadeDistance));
            endFadeDistance = Unpack(o.Find(x => x.endFadeDistance));
        }

        public override void OnInspectorGUI()
        {
            SCPE_GUI.DisplayDocumentationButton("blur");

            SCPE_GUI.DisplaySetupWarning<BlurRenderer>(ref isSetup, serializedObject);

            PropertyField(amount);
            SCPE_GUI.DisplayIntensityWarning(amount);
            
            EditorGUILayout.Space();

            PropertyField(mode);
            PropertyField(highQuality);
            PropertyField(iterations);
            PropertyField(downscaling);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Distance Fading");
            PropertyField(distanceFade);
            if (distanceFade.value.boolValue)
            {
                PropertyField(startFadeDistance);
                PropertyField(endFadeDistance);
            }
        }
    }
}