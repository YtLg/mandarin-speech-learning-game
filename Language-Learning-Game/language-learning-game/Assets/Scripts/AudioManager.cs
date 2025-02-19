using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource voiceSource;
    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip backgroundMusic;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Implementation to start playing background audio if applicable
    }

    // Playing, pausing and swapping out the background audio

    void PlayMusic()
    {
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void ChangeMusic(AudioClip musicInput)
    {
        musicSource.clip = musicInput;
    }

    // For playing and stopping speech and dialogue to avoid interrupt.

    public void PlayDialogue(AudioClip dialogue)
    {
        voiceSource.PlayOneShot(dialogue); // One-off doesn't save audio unlike music.
    }

    public void StopDialogue()
    {
        voiceSource.Stop();
    }

    // For playing and stopping non-speech sound effects to avoid interrupt.

    public void PlaySoundEffect(AudioClip sfx)
    {
        sfxSource.PlayOneShot(sfx);
    }

    public void StopSoundEffect()
    {
        sfxSource.Stop();
    }

}
