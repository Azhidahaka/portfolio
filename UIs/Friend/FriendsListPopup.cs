using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendsListPopup : UIBase {
    public ScrollRect scrollRect;
    public LayoutGroup layoutGroup;
    public GameObject prefabFriendSlot;

    public Text lblFriendNone;
    public Text lblBtnFriendBreak;

    private List<FriendsListPopupSlot> listSlot = new List<FriendsListPopupSlot>();
    private List<FriendsListPopupSlot> reusableListSlot = new List<FriendsListPopupSlot>();

    private bool friendBreak = false;

    private void Awake() {
        Common.ToggleActive(prefabFriendSlot, false);
    }

    private void OnEnable() {
        SetInFrontInCanvas();

        EventManager.Register(EventEnum.FriendListPopupBtnFriendBreakClick, OnFriendListPopupBtnFriendBreakClick);
        EventManager.Register(EventEnum.FriendsStateUpdate, OnFriendsStateUpdate);
        EventManager.Register(EventEnum.NotifiedNewFriend, OnFriendsStateUpdate);
    }

    private void OnFriendsStateUpdate(object[] args) {
        SetData(friendBreak);
    }

    private void OnFriendListPopupBtnFriendBreakClick(object[] args) {
        string inDate = (string)args[0];
        Callback successCallback = () => SetData(false);
        BackendRequest.instance.ReqFriendBreak(inDate, successCallback);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.FriendListPopupBtnFriendBreakClick, OnFriendListPopupBtnFriendBreakClick);
        EventManager.Remove(EventEnum.FriendsStateUpdate, OnFriendsStateUpdate);
        EventManager.Remove(EventEnum.NotifiedNewFriend, OnFriendsStateUpdate);
    }

    public void SetData(bool friendBreak = false) {
        this.friendBreak = friendBreak;
        SetScrollView();
        SetBtnBreak();
        
    }

    private void SetScrollView() {
        if (listSlot == null) {
            listSlot = new List<FriendsListPopupSlot>();
            reusableListSlot = new List<FriendsListPopupSlot>();
        }
        else {
            foreach (FriendsListPopupSlot scrollViewItem in listSlot) {
                if (reusableListSlot.Contains(scrollViewItem) == false)
                    reusableListSlot.Add(scrollViewItem);
                Common.ToggleActive(scrollViewItem.gameObject, false);
            }
            listSlot.Clear();
        }

        GameObject scrollViewGO;

        List<UserData.SimpleUserDTO> friends = UserDataModel.instance.friends;

        for (int i = 0; i < friends.Count; i++) {
            UserData.SimpleUserDTO friendInfo = friends[i];
            UserData.PublicUserDataDTO publicUserData = PublicUserDataManager.instance.GetPublicUserData(friendInfo);

            FriendsListPopupSlot scrollViewItem;

            if (i >= reusableListSlot.Count) {
                scrollViewGO =  Instantiate(prefabFriendSlot, layoutGroup.transform);
                scrollViewItem = scrollViewGO.GetComponent<FriendsListPopupSlot>();
                reusableListSlot.Add(scrollViewItem);
            }
            else {
                scrollViewItem = reusableListSlot[i];
                scrollViewGO = scrollViewItem.gameObject;
            }

            Common.ToggleActive(scrollViewGO, true);
            scrollViewItem = scrollViewGO.GetComponent<FriendsListPopupSlot>();
            scrollViewItem.SetData(friendInfo, publicUserData);
            listSlot.Add(scrollViewItem);
        }

        Common.ToggleActive(lblFriendNone.gameObject, friends.Count == 0);
    }

    public void OnBtnBreakClick() {
        friendBreak = !friendBreak;
        SetBtnBreak();
        
        EventManager.Notify(EventEnum.FriendListPopupShowBtnBreak, friendBreak);
    }

    private void SetBtnBreak() {
        if (friendBreak) 
            lblBtnFriendBreak.text = TermModel.instance.GetTerm("btn_cancel");
        else
            lblBtnFriendBreak.text = TermModel.instance.GetTerm("friend_break");
    }

    public void OnBtnAddFriendClick() {
        Callback showSearchPopup = () => {
            FriendsSearchPopup searchPopup = UIManager.instance.GetUI<FriendsSearchPopup>(UI_NAME.FriendsSearchPopup);
            searchPopup.SetData(FriendsSearchPopup.POPUP_TYPE.RECOMMAND);
            searchPopup.Show();
        };

        Callback reqFriendReceives = () => {
            BackendRequest.instance.ReqFriendReceivedList(showSearchPopup);
        };

        BackendRequest.instance.ReqFriendRecommendList(reqFriendReceives);
    }
}
