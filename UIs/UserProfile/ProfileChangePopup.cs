using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileChangePopup : UIBase {
    public enum TAB {
        PROFILE,
        FRAME,
    }

    public ScrollRect scrollRect;
    public LayoutGroup layoutGroup;
    public GameObject prefabSlot;

    public GameObject goTabProfileBg;
    public GameObject goTabFrameBg;

    private List<ProfileChangePopupSlot> listSlot = new List<ProfileChangePopupSlot>();
    private List<ProfileChangePopupSlot> listReusableSlot = new List<ProfileChangePopupSlot>();

    private TAB selectedTab = TAB.PROFILE;

    private void Awake() {
        Common.ToggleActive(prefabSlot, false);
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.ProfileSkinChanged, OnProfileSkinChanged);
        EventManager.Register(EventEnum.UserDataUserInfoUpdate, OnUserDataUserInfoUpdate);
    }

    private void OnUserDataUserInfoUpdate(object[] args) {
        SetData(selectedTab);
    }

    private void OnProfileSkinChanged(object[] args) {
        for(int i = 0; i < listSlot.Count; i++) {
            listSlot[i].UpdateState();
        }
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.ProfileSkinChanged, OnProfileSkinChanged);
        EventManager.Remove(EventEnum.UserDataUserInfoUpdate, OnUserDataUserInfoUpdate);
    }

    public void SetData(TAB selectedTab) {
        this.selectedTab = selectedTab;
        SetScrollView();
    }

    private void SetScrollView() {
        if (selectedTab == TAB.PROFILE) {
            SetProfile();
        }
        else {
            SetFrame();
        }
    }

    private void SetProfile() {
        Common.ToggleActive(goTabProfileBg, true);
        Common.ToggleActive(goTabFrameBg, false);

        if (listSlot == null) {
            listSlot = new List<ProfileChangePopupSlot>();
            listReusableSlot = new List<ProfileChangePopupSlot>();
        }
        else {
            foreach (ProfileChangePopupSlot scrollViewItem in listSlot) {
                if (listReusableSlot.Contains(scrollViewItem) == false)
                    listReusableSlot.Add(scrollViewItem);
                Common.ToggleActive(scrollViewItem.gameObject, false);
            }
            listSlot.Clear();
        }

        GameObject scrollViewGO;
        List<GameData.ProfileDTO> profileDatas = GameDataModel.instance.profiles;
        for (int i = 0; i < profileDatas.Count; i++) {
            GameData.ProfileDTO profileData = profileDatas[i];
            ProfileChangePopupSlot scrollViewItem;

            if (i >= listReusableSlot.Count) {
                scrollViewGO =  Instantiate(prefabSlot, layoutGroup.transform);
                scrollViewItem = scrollViewGO.GetComponent<ProfileChangePopupSlot>();
                listReusableSlot.Add(scrollViewItem);
            }
            else {
                scrollViewItem = listReusableSlot[i];
                scrollViewGO = scrollViewItem.gameObject;
            }

            Common.ToggleActive(scrollViewGO, true);
            scrollViewItem = scrollViewGO.GetComponent<ProfileChangePopupSlot>();
            scrollViewItem.SetData(selectedTab, profileData.profileID);
            listSlot.Add(scrollViewItem);
        }
    }

    private void SetFrame() {
        Common.ToggleActive(goTabFrameBg, true);
        Common.ToggleActive(goTabProfileBg, false);

        if (listSlot == null) {
            listSlot = new List<ProfileChangePopupSlot>();
            listReusableSlot = new List<ProfileChangePopupSlot>();
        }
        else {
            foreach (ProfileChangePopupSlot scrollViewItem in listSlot) {
                if (listReusableSlot.Contains(scrollViewItem) == false)
                    listReusableSlot.Add(scrollViewItem);
                Common.ToggleActive(scrollViewItem.gameObject, false);
            }
            listSlot.Clear();
        }

        GameObject scrollViewGO;
        List<GameData.FrameDTO> frameDatas = GameDataModel.instance.frames;
        for (int i = 0; i < frameDatas.Count; i++) {
            GameData.FrameDTO frameData = frameDatas[i];
            ProfileChangePopupSlot scrollViewItem;

            if (i >= listReusableSlot.Count) {
                scrollViewGO =  Instantiate(prefabSlot, layoutGroup.transform);
                scrollViewItem = scrollViewGO.GetComponent<ProfileChangePopupSlot>();
                listReusableSlot.Add(scrollViewItem);
            }
            else {
                scrollViewItem = listReusableSlot[i];
                scrollViewGO = scrollViewItem.gameObject;
            }

            Common.ToggleActive(scrollViewGO, true);
            scrollViewItem = scrollViewGO.GetComponent<ProfileChangePopupSlot>();
            scrollViewItem.SetData(selectedTab, frameData.frameID);
            listSlot.Add(scrollViewItem);
        }
    }

    public void OnTabProfileClick() {
        SetData(TAB.PROFILE);
    }

    public void OnTabFrameClick() {
        SetData(TAB.FRAME);
    }
}
