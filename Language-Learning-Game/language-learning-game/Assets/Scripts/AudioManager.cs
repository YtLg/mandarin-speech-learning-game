using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource voiceSource;
    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip backgroundMusic;

    private Coroutine segmentStopCoroutine;


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
        voiceSource.PlayOneShot(dialogue);
    }

    public void StopDialogue()
    {
        voiceSource.Stop();
    }

    public void PlaySegment(AudioClip dialogue, float start, float end)
    {
        voiceSource.clip = dialogue;
        voiceSource.time = start;
        voiceSource.Play();

        if (segmentStopCoroutine != null)
        {
            StopCoroutine(segmentStopCoroutine);
        }
        segmentStopCoroutine = StartCoroutine(StopTimer(end - start));
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
    private System.Collections.IEnumerator StopTimer(float delay)
    {
        yield return new WaitForSeconds(delay);
        voiceSource.Stop();
        segmentStopCoroutine = null;
    }

}
