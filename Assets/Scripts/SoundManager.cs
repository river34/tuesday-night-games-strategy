using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

	public AudioSource background;
	public AudioSource effect;

	private float lowPitchRange = 0.8f;
	private float highPitchRange = 1f;

	public void StartBackground ()
	{
		background.volume = 1;
		background.Play ();
	}

	public void StopBackground ()
	{
		StartCoroutine (FadeOut (background));
	}

	IEnumerator FadeOut (AudioSource source)
	{
		if (!source.isPlaying)
		{
			yield return null;
		}

		while (source.volume > Mathf.Epsilon)
		{
			source.volume -= Time.deltaTime;
			yield return null;
		}

		source.Stop ();
		source.volume = 1;
	}

	public void RandomizeEffect (params AudioClip[] clips)
    {
        int randomIndex = Random.Range (0, clips.Length);
        float randomPitch = Random.Range (lowPitchRange, highPitchRange);
        effect.pitch = randomPitch;
        effect.clip = clips[randomIndex];
        effect.Play ();
    }
}
