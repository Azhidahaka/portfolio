using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectNationPopup : UIBase {
    public GameObject prefabSlot;
    public LayoutGroup layoutGroup;

    private List<SelectNationPopupSlot> listSlot = new List<SelectNationPopupSlot>();

    private void Awake() {
        Common.ToggleActive(prefabSlot, false);
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.SelectNationPopupFlagSelected, OnSelectNationPopupFlagSelected);
    }

    private void OnSelectNationPopupFlagSelected(object[] args) {
        long flagNo = (long)args[0];
        WebUser.instance.ReqSetFlagNo(flagNo, OnResSetFlagNo);
    }

    private void OnResSetFlagNo() {
        Hide();
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.SelectNationPopupFlagSelected, OnSelectNationPopupFlagSelected);
    }

    public void SetData() {
        if (listSlot.Count == 0)
            SetScrollView();
    }

    private void SetScrollView() {
        Dictionary<long, Texture> flagTextures = ResourceManager.instance.GetProfileTextures();
        for (int i = 0; i < flagTextures.Count; i++) {
            GameObject go = Instantiate(prefabSlot, layoutGroup.transform);
            SelectNationPopupSlot slot = go.GetComponent<SelectNationPopupSlot>();
            slot.SetData(flagTextures[i]);
            Common.ToggleActive(go, true);
            listSlot.Add(slot);
        }
    }
}
