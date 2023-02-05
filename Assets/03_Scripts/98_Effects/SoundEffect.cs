using System.Collections.Generic;
using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.Serialization;
using static Unity.Mathematics.math;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

using F32 = System.Single;
using I32 = System.Int32;

namespace CoolBeans
{
    [CreateAssetMenu(fileName = "SFX.asset", menuName = "CoolBeans/Effects/SFX", order = 0)]
    public class SoundEffect : ScriptableObject 
    {
        [SerializeField] private I32 playIndex = 0;
        
        #if UNITY_EDITOR
        [MinMaxSlider(minValue: 0, maxValue: 1)]
        #endif
        public Vector2 volume = new(0.5f, 0.5f);
        
        #if UNITY_EDITOR
        [MinMaxSlider(0, 3)]
        #endif
        public Vector2 pitch  = new(1, 1);
        
        [SerializeField] private List<AudioClip> audioClips;

        public SoundEffectPlayStyle playBackStyle;
        
        #if UNITY_EDITOR
        private AudioSource _previewer;

        private void OnEnable()
        {
            _previewer = UnityEditor.EditorUtility
                .CreateGameObjectWithHideFlags(name: "AudioPreview", HideFlags.HideAndDontSave,
                    typeof(AudioSource))
                .GetComponent<AudioSource>();
        }

        private void OnDisable()
        {
            DestroyImmediate(_previewer.gameObject);
        }


        [ButtonGroup("previewControls")]
        private void PlayPreview()
        {
            Play(_previewer);
        }

        [ButtonGroup("previewControls")]
        [EnableIf("@" + nameof(_previewer) + ".isPlaying")]
        private void StopPreview()
        {
            _previewer.Stop();
        }
        #endif

        public AudioClip GetClip()
        {
            // get current clip
            AudioClip clip = audioClips[playIndex >= audioClips.Count ? 0 : playIndex];

            // find next clip
            playIndex = playBackStyle switch
            {
                SoundEffectPlayStyle.Sequential => (playIndex + 1) % audioClips.Count,
                SoundEffectPlayStyle.Random     => Random.Range(0, audioClips.Count),
                SoundEffectPlayStyle.Reversed   => (playIndex + audioClips.Count - 1) % audioClips.Count,
                _ => playIndex
            };
            
            return clip;
        }

        [PublicAPI]
        public AudioSource Play(AudioSource source = null) 
        {
            if (audioClips.Count == 0)
            {
                Debug.LogWarning($"Missing sound clips for {name}");
                return null;
            }

            AudioSource __audioSource = source;
            if (__audioSource == null)
            {
                GameObject __obj = new(name: "Sound", typeof(AudioSource));
                __audioSource = __obj.GetComponent<AudioSource>();
            }

            // set source config:
            __audioSource.clip   = GetClip();
            __audioSource.volume = Random.Range(volume.x, volume.y);
            __audioSource.pitch  = Random.Range(pitch.x, pitch.y);

            __audioSource.Play();

            #if UNITY_EDITOR
            if (__audioSource != _previewer)
            {
                Destroy(__audioSource.gameObject, __audioSource.clip.length / __audioSource.pitch);
            }
            #else
            Destroy(__audioSource.gameObject, __audioSource.clip.length / __audioSource.pitch);
            #endif

            return __audioSource;
        }
    }
    
    public enum SoundEffectPlayStyle 
    {
        Random,
        Sequential,
        Reversed,
    }

    
}
