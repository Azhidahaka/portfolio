using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LuckyFlow.EnumDefine;

public class MatchBlocksSound : MonoBehaviour {
    public enum ACTION {
        PICK_UP_BUNDLE = 1,
        PUT = 2,
        WRONG = 3,
        ROTATE = 4,

        USE_PATTERN_CHANGE_ITEM = 5,
        USE_BUNDLE_RESET_ITEM = 6,
    }

    [System.Serializable]
    public class BundleSoundDTO {
        public ACTION action;
        public SOUND_CLIP_EFFECT soundClip;
    }

    public static MatchBlocksSound instance;

    public List<BundleSoundDTO> sounds;

    private Dictionary<ACTION, SOUND_CLIP_EFFECT> dicBundleSound = new Dictionary<ACTION, SOUND_CLIP_EFFECT>();

    private void Awake() {
        instance = this;

        for (int i = 0; i < sounds.Count; i++) {
            dicBundleSound.Add(sounds[i].action, sounds[i].soundClip);
        }
    }

    public void PlayEffect(ACTION action) {
        if (SoundManager.instance == null || 
            dicBundleSound.ContainsKey(action) == false)
            return;

        SoundManager.instance.PlayEffect(dicBundleSound[action]);
    }
}
