using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GainSkinEffect : MatchBlocksEffect {
    public Transform posSkin;
    public void SetData(long skinID) {
        SetRealTime(true);
        ResourceManager.instance.GetSkinThumbnail(skinID, posSkin);
    }
}
