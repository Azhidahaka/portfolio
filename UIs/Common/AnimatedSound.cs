using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedSound : MonoBehaviour {
    public List<SOUND_CLIP_EFFECT> soundClips = new List<SOUND_CLIP_EFFECT>(){
         SOUND_CLIP_EFFECT.NONE,
         SOUND_CLIP_EFFECT.NONE,
         SOUND_CLIP_EFFECT.NONE,
         SOUND_CLIP_EFFECT.NONE,
         SOUND_CLIP_EFFECT.NONE,
    };

    public void PlaySound0() {
        if (soundClips == null || 
            soundClips.Count < 1 || 
            soundClips[0] == SOUND_CLIP_EFFECT.NONE)
            return;

        if (SoundManager.instance != null)
            SoundManager.instance.PlayEffect(soundClips[0]);
    }

    public void PlaySound1() {
        if (soundClips == null || 
            soundClips.Count < 2 || 
            soundClips[1] == SOUND_CLIP_EFFECT.NONE)
            return;
        if (SoundManager.instance != null)
            SoundManager.instance.PlayEffect(soundClips[1]);
    }

    public void PlaySound2() {
        if (soundClips == null || 
            soundClips.Count < 3 ||
            soundClips[2] == SOUND_CLIP_EFFECT.NONE)
            return;
        if (SoundManager.instance != null)
            SoundManager.instance.PlayEffect(soundClips[2]);
    }

    public void PlaySound3() {
        if (soundClips == null || 
            soundClips.Count < 4 ||
            soundClips[3] == SOUND_CLIP_EFFECT.NONE)
            return;

        if (SoundManager.instance != null)
            SoundManager.instance.PlayEffect(soundClips[3]);
    }

    public void PlaySound4() {
        if (soundClips == null || 
            soundClips.Count < 5 ||
            soundClips[4] == SOUND_CLIP_EFFECT.NONE)
            return;
        if (SoundManager.instance != null)
            SoundManager.instance.PlayEffect(soundClips[4]);
    }
}
