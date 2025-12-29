using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    public AudioSource sfxSource;    // 播放音效
    public AudioSource musicSource;  // 播放BGM（可选）

    [Header("SFX Clips")]
    public AudioClip attackClip;
    public AudioClip hitClip;
    public AudioClip rollClip;
    public AudioClip counterClip;
    public AudioClip executeClip;

    [Header("Music Clips")]
    public AudioClip bossMusic;

    
    // ----------- 公共接口 -----------

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayAttack() => PlaySFX(attackClip);
    public void PlayHit() => PlaySFX(hitClip);
    public void PlayRoll() => PlaySFX(rollClip);
    public void PlayCounter() => PlaySFX(counterClip);
    public void PlayExecute() => PlaySFX(executeClip);

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null) return;

        musicSource.loop = loop;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlayBossMusic() => PlayMusic(bossMusic, true);

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }
}
