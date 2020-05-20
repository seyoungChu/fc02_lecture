using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//루프, 페이드 관련 속성, 오디오 클립 속성들.
public class SoundClip
{
    public SoundPlayType playType = SoundPlayType.None;
    public string clipName = string.Empty;
    public string clipPath = string.Empty;
    public float maxVolume = 1.0f;
    public bool isLoop = false;
    public float[] checkTime = new float[0];
    public float[] setTime = new float[0];
    public int realId = 0;

    private AudioClip clip = null;
    public int currentLoop = 0;
    public float pitch = 1.0f;
    public float dopplerLevel = 1.0f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    public float minDistance = 10000.0f;
    public float maxDistance = 50000.0f;
    public float sparialBlend = 1.0f;

    public float fadeTime1 = 0.0f;
    public float fadeTime2 = 0.0f;
    public Interpolate.Function interpolate_Func;
    public bool isFadeIn = false;
    public bool isFadeOut = false;

    public SoundClip() { }
    public SoundClip(string clipPath, string clipName)
    {
        this.clipPath = clipPath;
        this.clipName = clipName;
    }
    public void PreLoad()
    {
        if(this.clip == null)
        {
            string fullPath = this.clipPath + this.clipName;
            this.clip = ResourceManager.Load(fullPath) as AudioClip;
        }
    }
    
    public void AddLoop()
    {
        this.checkTime = ArrayHelper.Add(0.0f, this.checkTime);
        this.setTime = ArrayHelper.Add(0.0f, this.setTime);
    }

    public void RemoveLoop(int index)
    {
        this.checkTime = ArrayHelper.Remove(index, this.checkTime);
        this.setTime = ArrayHelper.Remove(index, this.setTime);
    }
    public AudioClip GetClip()
    {
        if(this.clip == null)
        {
            PreLoad();
        }
        if(this.clip == null && this.clipName != string.Empty)
        {
            Debug.LogWarning($"Can not load audio clip Resouce {this.clipName}");
            return null;
        }
        return this.clip;
    }
    public void ReleaseClip()
    {
        if(this.clip != null)
        {
            this.clip = null;
        }
    }

    public bool HasLoop()
    {
        return this.checkTime.Length > 0;
    }
    public void NextLoop()
    {
        this.currentLoop++;
        if(this.currentLoop >= this.checkTime.Length)
        {
            this.currentLoop = 0;
        }
    }

    public void CheckLoop(AudioSource source)
    {
        if(HasLoop() && source.time >= this.checkTime[this.currentLoop])
        {
            source.time = this.setTime[this.currentLoop];
            this.NextLoop();
        }
    }

    public void FadeIn(float time, Interpolate.EaseType easeType)
    {
        this.isFadeOut = false;
        this.fadeTime1 = 0.0f;
        this.fadeTime2 = time;
        this.interpolate_Func = Interpolate.Ease(easeType);
        this.isFadeIn = true;

    }
    public void FadeOut(float time, Interpolate.EaseType easeType)
    {
        this.isFadeIn = false;
        this.fadeTime1 = 0.0f;
        this.fadeTime2 = time;
        this.interpolate_Func = Interpolate.Ease(easeType);
        this.isFadeOut = true;
    }

    /// <summary>
    /// 페이드인,아웃 효과 프로세스.
    /// </summary>    
    public void DoFade(float time, AudioSource audio)
    { 
        if(this.isFadeIn == true)
        {
            this.fadeTime1 += time;
            audio.volume = Interpolate.Ease(this.interpolate_Func, 0, maxVolume, 
                fadeTime1, fadeTime2);
            if(this.fadeTime1 >= this.fadeTime2)
            {
                this.isFadeIn = false;
            }
        }
        else if(this.isFadeOut == true)
        {
            this.fadeTime1 += time;
            audio.volume = Interpolate.Ease(this.interpolate_Func, maxVolume, 
                0 - maxVolume, fadeTime1, fadeTime2);
            if(this.fadeTime1 >= this.fadeTime2)
            {
                this.isFadeOut = false;
                audio.Stop();
            }
        }
    }
}
