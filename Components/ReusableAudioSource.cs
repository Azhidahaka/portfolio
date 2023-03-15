using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReusableAudioSource : MonoBehaviour {
    private AudioSource audioSource;
    private SoundManager.SOUND_TYPE soundType;

    private AudioClip oldClip;
    private AudioClip newClip;

    private AudioSource preset;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.UpdateVolume, OnUpdateVolume);
        EventManager.Register(EventEnum.EffectVolumeChanged, OnEffectVolumeChanged);
        EventManager.Register(EventEnum.BGMVolumeChanged, OnBGMVolumeChanged);
        EventManager.Register(EventEnum.BGMMute, OnBGMMute);
        EventManager.Register(EventEnum.EffectMute, OnEffectMute);

        StartCoroutine(JobCheckFinish());
    }

    private void OnUpdateVolume(object[] args) {
        audioSource.volume = GetVolume();
    }

    private void OnEffectMute(object[] args) {
        if (soundType != SoundManager.SOUND_TYPE.EFFECT)
            return;

        audioSource.volume = 0;
    }

    private void OnBGMMute(object[] args) {
        if (soundType != SoundManager.SOUND_TYPE.BGM)
            return;

        audioSource.volume = 0;
    }

    private void OnEffectVolumeChanged(object[] args) {
        if (soundType != SoundManager.SOUND_TYPE.EFFECT)
            return;

        float volumeRatio = (float)args[0];
        if (audioSource.mute && volumeRatio > 0)
            audioSource.mute = false;

        audioSource.volume = preset.volume * volumeRatio;
    }

    private void OnBGMVolumeChanged(object[] args) {
        if (soundType != SoundManager.SOUND_TYPE.BGM)
            return;

        float volumeRatio = (float)args[0];
        if (audioSource.mute && volumeRatio > 0)
            audioSource.mute = false;
        
        audioSource.volume = preset.volume * volumeRatio;
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.UpdateVolume, OnUpdateVolume);
        EventManager.Remove(EventEnum.EffectVolumeChanged, OnEffectVolumeChanged);
        EventManager.Remove(EventEnum.BGMVolumeChanged, OnBGMVolumeChanged);
        EventManager.Remove(EventEnum.BGMMute, OnBGMMute);
        EventManager.Remove(EventEnum.EffectMute, OnEffectMute);
    }

    //최초 Instantiate할때 호출
    public void Init(SoundManager.SOUND_TYPE soundType) {
        this.soundType = soundType;
    }

    public void SetData(AudioClip clip, AudioSource preset) {
        this.preset = preset;
        audioSource.priority = preset.priority;
        audioSource.volume = GetVolume();
        audioSource.pitch = preset.pitch;
        audioSource.loop = preset.loop;
        //@todo : 프리셋으로 설정할 옵션이 더 있으면 여기에 추가한다.

        audioSource.clip = clip;

        audioSource.mute = IsMute();
    }

    private float GetVolume() {
        float volume = 0;
        if (soundType == SoundManager.SOUND_TYPE.EFFECT) 
            volume = (float)(preset.volume * UserDataModel.instance.gameOptions.effectVolumeRatio);
        else if (soundType == SoundManager.SOUND_TYPE.BGM) 
            volume = (float)(preset.volume * UserDataModel.instance.gameOptions.bgmVolumeRatio);

        return volume;
    }

    private bool IsMute() {
        if (soundType == SoundManager.SOUND_TYPE.EFFECT) 
            return UserDataModel.instance.gameOptions.effectVolumeRatio == 0;
        else 
            return UserDataModel.instance.gameOptions.bgmVolumeRatio == 0;
    }

    public void Play() {
        gameObject.SetActive(true);
        if (soundType == SoundManager.SOUND_TYPE.BGM) 
            audioSource.Play();
        else
            audioSource.PlayOneShot(audioSource.clip);
    }

    public void Stop() {
        StopCoroutine(JobCheckFinish());
        audioSource.Stop();
        gameObject.SetActive(false);
    }

    private IEnumerator JobCheckFinish() {
        //사운드가 모두 플레이될때까지 기다린다.
        yield return new WaitForEndOfFrame();
        yield return new WaitWhile(()=> audioSource.isPlaying);
        gameObject.SetActive(false);
        if (soundType == SoundManager.SOUND_TYPE.BGM)
            EventManager.Notify(EventEnum.FinishedBGM);
    }
}
