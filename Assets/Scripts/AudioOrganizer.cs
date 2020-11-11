using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script can be used on GameObjects with any amount of AudioSources in its children 
// It's recommended to keep the GameObject empty besides this script as well as to keep its children empty besides one audiosource each
// Call the function with AudioOrganizer.PlayAudio( [Audio to play], [Minimum pitch], [Maximum pitch] ) from any script
// Make sure to use the Organizer.Audio namespace

namespace Organizer.Audio
{
    public class AudioOrganizer : MonoBehaviour
    {
        public List<AudioSource> audioChannels = new List<AudioSource>();
        [SerializeField] bool printChannel = false;
        [SerializeField] bool scalePitchToTimeScale = true;
        float lastTimeScale = 1f;

        private void Update()
        {
            UpdatePitch();
        }
        void UpdatePitch()
        {
            if (scalePitchToTimeScale && Time.deltaTime != 0)
            {
                for (int i = 0; i < audioChannels.Count; i++)
                {
                    if (audioChannels[i].isPlaying)
                    {
                        float factor = Time.timeScale / lastTimeScale;
                        audioChannels[i].pitch *= factor;
                    }
                }
                lastTimeScale = Time.timeScale;
            }
        }
        void Start()
        {
            foreach (AudioSource audioChannel in GetComponentsInChildren<AudioSource>())
            {
                audioChannels.Add(audioChannel);
            }
        }
        public void PlayAudio(Sound sound)
        {
            float pitch = Random.Range(sound.extremePitches.x, sound.extremePitches.y);
            float volume = sound.volume * GameSettings.effectsVolume;
            if (scalePitchToTimeScale)
                pitch *= Time.timeScale;
            for (int i = 0; i < audioChannels.Count; i++)
            {
                bool isPlaying = audioChannels[i].isPlaying;
                if (!isPlaying)
                {
                    PlayClip(audioChannels[i], sound.clip, pitch, volume);
                    if (printChannel)
                    {
                        Debug.Log("audio channel " + (i + 1) + " playing");
                    }
                    return;
                }
                else
                {
                    if (i == audioChannels.Count - 1)
                    {
                        PlayClip(audioChannels[0], sound.clip, pitch, volume);
                        if (printChannel)
                        {
                            Debug.Log("audio channel 0 overridden");
                        }
                    }
                    continue;
                }
            }
        }
        void PlayClip(AudioSource source, AudioClip clip, float pitch, float volume)
        {
            source.clip = clip;
            source.pitch = pitch;
            source.volume = volume;
            source.Play();
        }
    }
}
