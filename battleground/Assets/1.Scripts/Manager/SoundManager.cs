using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : SingletonMonobehaviour<SoundManager>
{
    public const string MasterGroupName = "Master";
    public const string EffectGroupName = "Effect";
    public const string BGMGroupName = "BGM";
    public const string UIGroupName = "UI";
    public const string MixerName = "AudioMixer";
    public const string ContainerName = "SoundContainer";
    public const string FadeA = "FadeA";
    public const string FadeB = "FadeB";
    public const string UI = "UI";
    public const string EffectVolumeParam = "Volume_Effect";
    public const string BGMVolumeParam = "Volume_BGM";
    public const string UIVolumeParam = "Volume_UI";

    public enum MusicPlayingType
    {
        None = 0,
        SourceA = 1,
        SourceB = 2,
        AtoB = 3,
        BtoA = 4
    }
    public AudioMixer mixer = null;
    public Transform audioRoot = null;
    public AudioSource fadeA_audio = null;
    public AudioSource fadeB_audio = null;
    public AudioSource[] effect_audios = null;
    public AudioSource UI_audio = null;

    public float[] effect_PlayStartTime = null;
    private int EffectChannelCount = 5;
    private MusicPlayingType currentPlayingType = MusicPlayingType.None;
    private bool isTicking = false;
    private SoundClip currentSound = null;
    private SoundClip lastSound = null;
    private float minVolume = -80.0f;
    private float maxVolume = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        if(this.mixer == null)
        {
            this.mixer = Resources.Load(MixerName) as AudioMixer;
        }
        if(this.audioRoot == null)
        {
            audioRoot = new GameObject(ContainerName).transform;
            audioRoot.SetParent(transform);
            audioRoot.localPosition = Vector3.zero;
        }
        if(fadeA_audio == null)
        {
            GameObject fadeA = new GameObject(FadeA, typeof(AudioSource));
            fadeA.transform.SetParent(audioRoot);
            this.fadeA_audio = fadeA.GetComponent<AudioSource>();
            this.fadeA_audio.playOnAwake = false;
        }
        if(fadeB_audio == null)
        {
            GameObject fadeB = new GameObject(FadeB, typeof(AudioSource));
            fadeB.transform.SetParent(audioRoot);
            fadeB_audio = fadeB.GetComponent<AudioSource>();
            fadeB_audio.playOnAwake = false;
        }
        if(UI_audio == null)
        {
            GameObject ui = new GameObject(UI, typeof(AudioSource));
            ui.transform.SetParent(audioRoot);
            UI_audio = ui.GetComponent<AudioSource>();
            UI_audio.playOnAwake = false;
        }
        if(this.effect_audios == null || this.effect_audios.Length == 0)
        {
            this.effect_PlayStartTime = new float[EffectChannelCount];
            this.effect_audios = new AudioSource[EffectChannelCount];
            for(int i = 0; i < EffectChannelCount;i++)
            {
                effect_PlayStartTime[i] = 0.0f;
                GameObject effect = new GameObject("Effect" + i.ToString(), typeof(AudioSource));
                effect.transform.SetParent(audioRoot);
                this.effect_audios[i] = effect.GetComponent<AudioSource>();
                this.effect_audios[i].playOnAwake = false;
            }
        }

        if(this.mixer != null)
        {
            this.fadeA_audio.outputAudioMixerGroup = mixer.FindMatchingGroups(BGMGroupName)[0];
            this.fadeB_audio.outputAudioMixerGroup = mixer.FindMatchingGroups(BGMGroupName)[0];
            this.UI_audio.outputAudioMixerGroup = mixer.FindMatchingGroups(UIGroupName)[0];
            for(int i =0; i < this.effect_audios.Length;i++)
            {
                this.effect_audios[i].outputAudioMixerGroup = mixer.FindMatchingGroups(EffectGroupName)[0];
            }
        }

        VolumeInit();
    }

    public void SetBGMVolume(float currentRatio)
    {
        currentRatio = Mathf.Clamp01(currentRatio);
        float volume = Mathf.Lerp(minVolume, maxVolume, currentRatio);
        this.mixer.SetFloat(BGMVolumeParam, volume);
        PlayerPrefs.SetFloat(BGMVolumeParam, volume);
    }
    public float GetBGMVolume()
    { 
        if(PlayerPrefs.HasKey(BGMVolumeParam))
        {
            return Mathf.Lerp(minVolume, maxVolume, PlayerPrefs.GetFloat(BGMVolumeParam));
        }
        else
        {
            return maxVolume;
        }
    }
    public void SetEffectVolume(float currentRatio)
    {
        currentRatio = Mathf.Clamp01(currentRatio);
        float volume = Mathf.Lerp(minVolume, maxVolume, currentRatio);
        this.mixer.SetFloat(EffectVolumeParam, volume);
        PlayerPrefs.SetFloat(EffectVolumeParam, volume);
    }
    public float GetEffectVolume()
    {
        if(PlayerPrefs.HasKey(EffectVolumeParam))
        {
            return Mathf.Lerp(minVolume, maxVolume, PlayerPrefs.GetFloat(EffectVolumeParam));
        }
        else
        {
            return maxVolume;
        }
    }

    public void SetUIVolume(float currentRatio)
    {
        currentRatio = Mathf.Clamp01(currentRatio);
        float volume = Mathf.Lerp(minVolume, maxVolume, currentRatio);
        this.mixer.SetFloat(UIVolumeParam, volume);
        PlayerPrefs.SetFloat(UIVolumeParam, volume);
    }
    public float GetUIVolume()
    {
        if(PlayerPrefs.HasKey(UIVolumeParam))
        {
            return Mathf.Lerp(minVolume, maxVolume, PlayerPrefs.GetFloat(UIVolumeParam));
        }else
        {
            return maxVolume;
        }
    }
    void VolumeInit()
    {
        if(this.mixer != null)
        {
            this.mixer.SetFloat(BGMVolumeParam, GetBGMVolume());
            this.mixer.SetFloat(EffectVolumeParam, GetEffectVolume());
            this.mixer.SetFloat(UIVolumeParam, GetUIVolume());
        }
    }

    void PlayAudioSource(AudioSource source, SoundClip clip, float volume)
    {
        if(source == null || clip == null)
        {
            return;
        }
        source.Stop();
        source.clip = clip.GetClip();
        source.volume = volume;
        source.loop = clip.isLoop;
        source.pitch = clip.pitch;
        source.dopplerLevel = clip.dopplerLevel;
        source.rolloffMode = clip.rolloffMode;
        source.minDistance = clip.minDistance;
        source.maxDistance = clip.maxDistance;
        source.spatialBlend = clip.spartialBlend;
        source.Play();
    }

    void PlayAudioSourceAtPoint(SoundClip clip, Vector3 position, float volume)
    {
        AudioSource.PlayClipAtPoint(clip.GetClip(), position, volume);
    }

    public bool IsPlaying()
    {
        return (int)this.currentPlayingType > 0;
    }

    public bool IsDifferentSound(SoundClip clip)
    {
        if(clip == null)
        {
            return false;
        }
        if(currentSound != null &&  currentSound.realId == clip.realId && 
            IsPlaying() && currentSound.isFadeOut == false)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    private IEnumerator CheckProcess()
    {
        while (this.isTicking == true && IsPlaying() == true)
        {
            yield return new WaitForSeconds(0.05f);
            if(this.currentSound.HasLoop())
            {
                if(currentPlayingType == MusicPlayingType.SourceA)
                {
                    currentSound.CheckLoop(fadeA_audio);
                }
                else if(currentPlayingType == MusicPlayingType.SourceB)
                {
                    currentSound.CheckLoop(fadeB_audio);
                }
                else if(currentPlayingType == MusicPlayingType.AtoB)
                {
                    this.lastSound.CheckLoop(this.fadeA_audio);
                    this.currentSound.CheckLoop(this.fadeB_audio);
                }
                else if(currentPlayingType == MusicPlayingType.BtoA)
                {
                    this.lastSound.CheckLoop(this.fadeB_audio);
                    this.currentSound.CheckLoop(this.fadeA_audio);
                }
            }
                 
        }
    }
    public void DoCheck()
    {
        StartCoroutine(CheckProcess());
    }

    public void FadeIn(SoundClip clip, float time, Interpolate.EaseType ease)
    {
        if(this.IsDifferentSound(clip))
        {
            this.fadeA_audio.Stop();
            this.fadeB_audio.Stop();
            this.lastSound = this.currentSound;
            this.currentSound = clip;
            PlayAudioSource(fadeA_audio, currentSound, 0.0f);
            this.currentSound.FadeIn(time, ease);
            this.currentPlayingType = MusicPlayingType.SourceA;
            if(this.currentSound.HasLoop() == true)
            {
                this.isTicking = true;
                DoCheck();
            }
        }
    }

    public void FadeIn(int index, float time, Interpolate.EaseType ease)
    {
        this.FadeIn(DataManager.SoundData().GetCopy(index), time, ease);
    }
    public void FadeOut(float time, Interpolate.EaseType ease)
    {
        if(this.currentSound != null)
        {
            this.currentSound.FadeOut(time, ease);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentSound == null)
        {
            return;
        }            
        if (currentPlayingType == MusicPlayingType.SourceA)
        {
            currentSound.DoFade(Time.deltaTime, fadeA_audio);
        }
        else if (currentPlayingType == MusicPlayingType.SourceB)
        {
            currentSound.DoFade(Time.deltaTime, fadeB_audio);
        }
        else if (currentPlayingType == MusicPlayingType.AtoB)
        {
            this.lastSound.DoFade(Time.deltaTime, fadeA_audio);
            this.currentSound.DoFade(Time.deltaTime, fadeB_audio);
        }
        else if (currentPlayingType == MusicPlayingType.BtoA)
        {
            this.lastSound.DoFade(Time.deltaTime, fadeB_audio);
            this.currentSound.DoFade(Time.deltaTime, fadeA_audio);
        }
        if(fadeA_audio.isPlaying && this.fadeB_audio.isPlaying == false)
        {
            this.currentPlayingType = MusicPlayingType.SourceA;
        }else if(fadeB_audio.isPlaying && fadeA_audio.isPlaying == false)
        {
            this.currentPlayingType = MusicPlayingType.SourceB;
        }else if(fadeA_audio.isPlaying == false && fadeB_audio.isPlaying == false)
        {
            this.currentPlayingType = MusicPlayingType.None;
        }

    }
    public void FadeTo(SoundClip clip, float time, Interpolate.EaseType ease)
    {
        if(currentPlayingType == MusicPlayingType.None)
        {
            FadeIn(clip, time, ease);
        }else if(this.IsDifferentSound(clip))
        {
            if(this.currentPlayingType == MusicPlayingType.AtoB)
            {
                this.fadeA_audio.Stop();
                this.currentPlayingType = MusicPlayingType.SourceB;
            }
            else if(this.currentPlayingType == MusicPlayingType.BtoA)
            {
                this.fadeB_audio.Stop();
                this.currentPlayingType = MusicPlayingType.SourceA;
            }
            lastSound = currentSound;
            currentSound = clip;
            this.lastSound.FadeOut(time, ease);
            this.currentSound.FadeIn(time, ease);
            if(currentPlayingType == MusicPlayingType.SourceA)
            {
                PlayAudioSource(fadeB_audio, currentSound, 0.0f);
                currentPlayingType = MusicPlayingType.AtoB;
            }
            else if(currentPlayingType == MusicPlayingType.SourceB)
            {
                PlayAudioSource(fadeA_audio, currentSound, 0.0f);
                currentPlayingType = MusicPlayingType.BtoA;
            }
            if(currentSound.HasLoop())
            {
                this.isTicking = true;
                DoCheck();
            }
        }
    }
    public void FadeTo(int index, float time, Interpolate.EaseType ease)
    {
        this.FadeTo(DataManager.SoundData().GetCopy(index), time, ease);
    }
    public void PlayBGM(SoundClip clip)
    {
        if(this.IsDifferentSound(clip))
        {
            this.fadeB_audio.Stop();
            this.lastSound = this.currentSound;
            this.currentSound = clip;
            PlayAudioSource(fadeA_audio, clip, clip.maxVolume);
            if(currentSound.HasLoop())
            {
                this.isTicking = true;
                DoCheck();

            }
        }
    }

    public void PlayBGM(int index)
    {
        SoundClip clip = DataManager.SoundData().GetCopy(index);
        PlayBGM(clip);
    }
    public void PlayUISound(SoundClip clip)
    {
        PlayAudioSource(UI_audio, clip, clip.maxVolume);
    }
    public void PlayEffectSound(SoundClip clip)
    {
        bool isPlaySuccess = false;
        for(int i = 0; i < this.EffectChannelCount; i++)
        {
            if(this.effect_audios[i].isPlaying == false)
            {
                PlayAudioSource(this.effect_audios[i], clip, clip.maxVolume);
                this.effect_PlayStartTime[i] = Time.realtimeSinceStartup;
                isPlaySuccess = true;
                break;
            }
            else if(this.effect_audios[i].clip == clip.GetClip())
            {
                this.effect_audios[i].Stop();
                PlayAudioSource(effect_audios[i], clip, clip.maxVolume);
                this.effect_PlayStartTime[i] = Time.realtimeSinceStartup;
                isPlaySuccess = true;
                break;
            }
        }
        if(isPlaySuccess == false)
        {
            float maxTime = 0.0f;
            int selectIndex = 0;
            for(int i  = 0; i < EffectChannelCount;i++)
            {
                if(this.effect_PlayStartTime[i] >  maxTime)
                {
                    maxTime = this.effect_PlayStartTime[i];
                    selectIndex = i;
                }
            }
            PlayAudioSource(this.effect_audios[selectIndex], clip, clip.maxVolume);

        }
    }

    public void PlayEffectSound(SoundClip clip, Vector3 position, float volume)
    {
        bool isPlaySuccess = false;
        for (int i = 0; i < this.EffectChannelCount; i++)
        {
            if (this.effect_audios[i].isPlaying == false)
            {
                PlayAudioSourceAtPoint(clip, position, volume);
                this.effect_PlayStartTime[i] = Time.realtimeSinceStartup;
                isPlaySuccess = true;
                break;
            }
            else if (this.effect_audios[i].clip == clip.GetClip())
            {
                this.effect_audios[i].Stop();
                PlayAudioSourceAtPoint(clip, position, volume);
                this.effect_PlayStartTime[i] = Time.realtimeSinceStartup;
                isPlaySuccess = true;
                break;
            }
        }
        if (isPlaySuccess == false)
        {            
            PlayAudioSourceAtPoint(clip, position, volume);
        }
    }

    public void PlayOneShotEffect(int index, Vector3 position, float volume)
    {
        if(index == (int)SoundList.None)
        {
            return;
        }

        SoundClip clip = DataManager.SoundData().GetCopy(index);
        if(clip == null)
        {
            return;
        }
        PlayEffectSound(clip, position, volume);
    }
    public void PlayOneShot(SoundClip clip)
    {
        if(clip == null)
        {
            return;
        }
        switch(clip.playType)
        {
            case SoundPlayType.EFFECT:
                PlayEffectSound(clip);
                break;
            case SoundPlayType.BGM:
                PlayBGM(clip);
                break;
            case SoundPlayType.UI:
                PlayUISound(clip);
                break;
        }
    }

    public void Stop(bool allStop = false)
    {
        if(allStop)
        {
            this.fadeA_audio.Stop();
            this.fadeB_audio.Stop();
        }

        this.FadeOut(0.5f, Interpolate.EaseType.Linear);
        this.currentPlayingType = MusicPlayingType.None;
        StopAllCoroutines();
    }
}
