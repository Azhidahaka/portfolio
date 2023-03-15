using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class FriendsSearchPopup : UIBase {
    public enum POPUP_TYPE {
        SEARCH,
        RECEIVED,
        RECOMMAND,
    }

    public GameObject prefabRecommand;
    public GameObject prefabRequest;

    public LayoutGroup layoutGroup;

    public GameObject goFriendReceives;
    public Text lblFriendReceivesCount;

    public Text lblInputNickname;

    private List<FriendsSearchPopupSlot> listSlot = new List<FriendsSearchPopupSlot>();
    private POPUP_TYPE popupType;

    private void Awake() {
        Common.ToggleActive(prefabRecommand, false);
        Common.ToggleActive(prefabRequest, false);
    }

    private void OnEnable() {
        SetInFrontInCanvas();

        EventManager.Register(EventEnum.FriendsStateUpdate, OnFriendsStateUpdate);
    }

    private void OnFriendsStateUpdate(object[] args) {
        SetData(popupType);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.FriendsStateUpdate, OnFriendsStateUpdate);
    }

    public void SetData(POPUP_TYPE popupType) {
        this.popupType = popupType;

        SetScrollVIew();
        SetReceivesCount();
    }

    private void SetReceivesCount() {
        long receivesCount = UserDataModel.instance.friendReceives.Count;
        lblFriendReceivesCount.text = Common.GetCommaFormat(receivesCount);
        Common.ToggleActive(goFriendReceives, receivesCount > 0);
    }

    private void SetScrollVIew() {
        if (listSlot == null) 
            listSlot = new List<FriendsSearchPopupSlot>();
        foreach (FriendsSearchPopupSlot slot in listSlot) {
            Destroy(slot.gameObject);
        }
        listSlot.Clear();

        List<UserData.SimpleUserDTO> list;
        GameObject prefab;

        if (popupType == POPUP_TYPE.RECOMMAND) {
            list = UserDataModel.instance.friendRecommands;
            prefab = prefabRecommand;
        }
        else if (popupType == POPUP_TYPE.RECEIVED) {
            list = UserDataModel.instance.friendReceives;
            prefab = prefabRequest;
        }
        else {
            list = UserDataModel.instance.findUserInfos;
            prefab = prefabRecommand;
        }

        for (int i = 0; i < list.Count; i++) {
            UserData.SimpleUserDTO simpleUserInfo = list[i];
            UserData.PublicUserDataDTO publicUserData = PublicUserDataManager.instance.GetPublicUserData(simpleUserInfo);

            FriendsSearchPopupSlot slot;
            GameObject slotGO =  Instantiate(prefab, layoutGroup.transform);
            slot = slotGO.GetComponent<FriendsSearchPopupSlot>();

            Common.ToggleActive(slotGO, true);
            slot.SetData(simpleUserInfo, publicUserData);
            listSlot.Add(slot);
        }
    }

    public void OnTabRecommandClick() {
        Callback callback = () => {
            SetData(POPUP_TYPE.RECOMMAND);
        };
        BackendRequest.instance.ReqFriendRecommendList(callback);
    }

    public void OnTabReceivedClick() {
        Callback callback = () => {
            SetData(POPUP_TYPE.RECEIVED);
        };
        BackendRequest.instance.ReqFriendReceivedList(callback);
    }

    public void OnBtnSearchClick() {
        string nickname = lblInputNickname.text;
        if (nickname.Length == 0)
            return;

        lblInputNickname.text = "";

        Callback successCallback = () => SetData(POPUP_TYPE.SEARCH);

        if (nickname[0] == '#') {
            nickname = nickname.Replace("#", string.Empty);
            int no;
            int.TryParse(nickname, out no);
            BackendRequest.instance.ReqFindUserByNumber(no, successCallback);
        }
        else {
            BackendRequest.instance.ReqFindUserByNickName(nickname, successCallback);
        }
    }
}
