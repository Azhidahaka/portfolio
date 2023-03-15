using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectNationPopupSlot : MonoBehaviour {
    public RawImage icoFlag;

    public void SetData(Texture flagTexture) {
        icoFlag.texture = flagTexture;
    }

    public void OnBtnFlagClick() {
        long flagNo;
        long.TryParse(icoFlag.texture.name.Replace("icoFlag", ""), out flagNo);
        EventManager.Notify(EventEnum.SelectNationPopupFlagSelected, flagNo);
    }
}
