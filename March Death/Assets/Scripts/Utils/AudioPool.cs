using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Basic audio pooling system
    /// </summary>
    class AudioPool
    {
        /// <summary>
        ///  The base object where the sources will be attached
        /// </summary>
        private GameObject _baseComponent;
        /// <summary>
        /// Num of audio sources to pool
        /// </summary>
        private int _numSources;
        /// <summary>
        /// array of sources
        /// </summary>
        private AudioSource[] _aSource;

        private float _volume = .7f;

        /// <summary>
        /// Creates an AudioPool, attaches the sources to the gameObject
        /// </summary>
        /// <param name="baseComponent"></param>
        /// <param name="numSources"></param>
        public AudioPool(GameObject gameObject, int numSources = 1)
        {
            _baseComponent = gameObject;
            _numSources = numSources;
            _aSource = new AudioSource[_numSources];
            Setup();
        }

        /// <summary>
        /// Instantiates the sources
        /// </summary>
        private void Setup()
        {
            for (int i = 0; i < _numSources; i++)
            {
                _aSource[i] = _baseComponent.AddComponent<AudioSource>();
                _aSource[i].volume = _volume;
            }
        }


        /// <summary>
        /// Silly poolint, only the first ones to arrive will sound
        /// </summary>
        /// <param name="audio"></param>
        public void Play(AudioClip audio)
        {
            for (int i = 0; i < _numSources; i++)
            {
                if (!_aSource[i].isPlaying)
                {
                    _aSource[i].clip = audio;
                    _aSource[i].Play();
                    return;
                }
            }

        }
    }
}
