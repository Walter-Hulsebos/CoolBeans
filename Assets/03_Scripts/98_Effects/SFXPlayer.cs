using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoolBeans
{
    
    [RequireComponent(typeof(AudioSource))]
    public sealed class SFXPlayer : MonoBehaviour
    {
        [SerializeField] private SoundEffect soundEffect;

        [SerializeField] private AudioSource audioSource;

        private void Reset()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void Play()
        {
            soundEffect.Play(audioSource);
        }
    }
}
