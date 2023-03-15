using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public enum SOUND_PRESET {
        DEFAULT,
        PRESET_1,
        PRESET_2,
        PRESET_3,
        PRESET_4,
    }

    public enum SOUND_TYPE {
        EFFECT,
        BGM,
    }

    public static SoundManager instance;

    public List<AudioSource> bgmPresets;
    public List<AudioSource> effectPresets;

    public Transform bgmParent;
    public Transform effectParent;

    public ReusableAudioSource prefabAudioSource;

    public bool fadeBGM = true;

    private Queue<ReusableAudioSource> bgmQueue = new Queue<ReusableAudioSource>();
    private Queue<ReusableAudioSource> bgmPlayingQueue = new Queue<ReusableAudioSource>();

    private Queue<ReusableAudioSource> effectQueue = new Queue<ReusableAudioSource>();
    private Queue<ReusableAudioSource> effectPlayingQueue = new Queue<ReusableAudioSource>();

    private List<AudioClip> bgmClips = new List<AudioClip>();
    private List<AudioClip> effectClips = new List<AudioClip>();

    private void Awake() {
        instance = this;

        CreateQueue();
        LoadClips();
    }

    private void OnDestroy() {
        instance = null;
    }

    private void LoadClips() {
        //폴더에 있는 클립들을 다 불러온다.
        bgmClips.AddRange(Resources.LoadAll<AudioClip>("Audios/BGM"));
        effectClips.AddRange(Resources.LoadAll<AudioClip>("Audios/Effect"));
    }

    private void CreateQueue() {
        ReusableAudioSource bgmSource = Instantiate(prefabAudioSource, bgmParent);
        bgmSource.Init(SOUND_TYPE.BGM);
        bgmSource.gameObject.SetActive(false);
        bgmQueue.Enqueue(bgmSource);

        for (int i = 0; i < Constant.MULTIPLE_EFFECT_PLAY_MAX; i++) {
            ReusableAudioSource effectSource = Instantiate(prefabAudioSource, effectParent);
            effectSource.Init(SOUND_TYPE.EFFECT);
            effectSource.gameObject.SetActive(false);
            effectQueue.Enqueue(effectSource);
        }
    }

    public void PlayBGM(SOUND_CLIP_BGM clipName, SOUND_PRESET presetName = SOUND_PRESET.DEFAULT) {
        if (clipName == SOUND_CLIP_BGM.NONE)
            return;

        ReusableAudioSource source = GetSource(SOUND_TYPE.BGM);

        AudioSource preset = GetBGMPreset(presetName);
        AudioClip clip = GetBGMClip(clipName);
        source.SetData(clip, preset);

        bgmPlayingQueue.Enqueue(source);

        source.Play();
    }

    public void PlayEffect(SOUND_CLIP_EFFECT clipName, SOUND_PRESET presetName = SOUND_PRESET.DEFAULT) {
        if (clipName == SOUND_CLIP_EFFECT.NONE)
            return;

        ReusableAudioSource source = GetSource(SOUND_TYPE.EFFECT);

        AudioSource preset = GetEffectPreset(presetName);
        AudioClip clip = GetEffectClip(clipName);
        source.SetData(clip, preset);

        effectPlayingQueue.Enqueue(source);

        source.Play();
    }

    private AudioClip GetEffectClip(SOUND_CLIP_EFFECT clipName) {
        for (int i = 0; i < effectClips.Count; i++) {
            if (effectClips[i].name == clipName.ToString())
                return effectClips[i];
        }

        Debug.LogWarning("클립 없음::" + clipName.ToString());
        return null;
    }

    private AudioClip GetBGMClip(SOUND_CLIP_BGM clipName) {
        for (int i = 0; i < bgmClips.Count; i++) {
            if (bgmClips[i].name == clipName.ToString())
                return bgmClips[i];
        }

        Debug.LogWarning("클립 없음::" + clipName.ToString());
        return null;
    }

    private ReusableAudioSource GetSource(SOUND_TYPE soundType) {
        Queue<ReusableAudioSource> queue;
        Queue<ReusableAudioSource> playingQueue;
        if (soundType == SOUND_TYPE.BGM) {
            queue = bgmQueue;
            playingQueue = bgmPlayingQueue;
        }
        else {
            queue = effectQueue;
            playingQueue = effectPlayingQueue;
        }

        //대기중인 녀석이 있으면 리턴
        if (queue.Count > 0)
            return queue.Dequeue();

        //전부다 플레이중이면 제일 오래된 녀석을 중지시키고 리턴
        ReusableAudioSource playingSource = playingQueue.Dequeue();
        playingSource.Stop();
        return playingSource;
    }

    private AudioSource GetEffectPreset(SOUND_PRESET preset) {
        for (int i = 0; i < effectPresets.Count; i++) {
            if (effectPresets[i].name == preset.ToString())
                return effectPresets[i];
        }

        Debug.LogWarning("프리셋이 존재하지 않음::" + preset.ToString());
        return null;
    }

    private AudioSource GetBGMPreset(SOUND_PRESET preset) {
        for (int i = 0; i < bgmPresets.Count; i++) {
            if (bgmPresets[i].name == preset.ToString())
                return bgmPresets[i];
        }

        Debug.LogWarning("프리셋이 존재하지 않음::" + preset.ToString());
        return null;
    }
}
