using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeFriendListPopup : UIBase {
    public ScrollRect scrollRect;
    public LayoutGroup layoutGroup;
    public GameObject prefabFriendSlot;

    public Text lblFriendNone;

    private List<ChallengeFriendListPopupSlot> listSlot = new List<ChallengeFriendListPopupSlot>();
    private List<ChallengeFriendListPopupSlot> reusableListSlot = new List<ChallengeFriendListPopupSlot>();

    private string selectedFriendInDate;

    private void Awake() {
        InitTutorialTargets();
        Common.ToggleActive(prefabFriendSlot, false);
    }

    private void OnEnable() {
        SetInFrontInCanvas();

        EventManager.Register(EventEnum.ChallengeFriendListPopupSelectFriend, OnChallengeFriendListPopupSelectFriend);
        EventManager.Register(EventEnum.ChallengeMessageSend, OnChallengeMessageSend);

        if (listSlot.Count == 0)
            return;

        EventManager.Notify(EventEnum.ChallengeFriendListShow);
    }

    private void OnChallengeMessageSend(object[] args) {
        base.Hide();
    }

    private void OnChallengeFriendListPopupSelectFriend(object[] args) {
        selectedFriendInDate = (string)args[0];
        for (int i = 0; i < listSlot.Count; i++) {
            listSlot[i].SetState(selectedFriendInDate);
        }
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.ChallengeFriendListPopupSelectFriend, OnChallengeFriendListPopupSelectFriend);
        EventManager.Remove(EventEnum.ChallengeMessageSend, OnChallengeMessageSend);
    }

    public void SetData() {
        selectedFriendInDate = "";
        SetScrollView();
    }

    private void SetScrollView() {
        if (listSlot == null) {
            listSlot = new List<ChallengeFriendListPopupSlot>();
            reusableListSlot = new List<ChallengeFriendListPopupSlot>();
        }
        else {
            foreach (ChallengeFriendListPopupSlot scrollViewItem in listSlot) {
                if (reusableListSlot.Contains(scrollViewItem) == false)
                    reusableListSlot.Add(scrollViewItem);
                Common.ToggleActive(scrollViewItem.gameObject, false);
            }
            listSlot.Clear();
        }

        GameObject scrollViewGO;

        List<UserData.SimpleUserDTO> friends;

        if (TutorialManager.instance.IsTutorialInProgress())
            friends = TutorialUtil.GetTutorialFriendInfos();
        else 
            friends = UserDataModel.instance.friends;

        for (int i = 0; i < friends.Count; i++) {
            UserData.SimpleUserDTO friendInfo = friends[i];
            UserData.PublicUserDataDTO publicUserData = PublicUserDataManager.instance.GetPublicUserData(friendInfo);

            ChallengeFriendListPopupSlot scrollViewItem;

            if (i >= reusableListSlot.Count) {
                scrollViewGO =  Instantiate(prefabFriendSlot, layoutGroup.transform);
                scrollViewItem = scrollViewGO.GetComponent<ChallengeFriendListPopupSlot>();
                reusableListSlot.Add(scrollViewItem);
            }
            else {
                scrollViewItem = reusableListSlot[i];
                scrollViewGO = scrollViewItem.gameObject;
            }

            Common.ToggleActive(scrollViewGO, true);
            scrollViewItem = scrollViewGO.GetComponent<ChallengeFriendListPopupSlot>();
            scrollViewItem.SetData(friendInfo, publicUserData);
            scrollViewItem.SetState(selectedFriendInDate);
            listSlot.Add(scrollViewItem);
        }

        Common.ToggleActive(lblFriendNone.gameObject, friends.Count == 0);
    }

    public void OnBtnConfirmClick() {
        if (TutorialManager.instance.IsTutorialInProgress()) {
            TutorialConfirm();
            return;
        }

        if (string.IsNullOrEmpty(selectedFriendInDate)) {
            string msg = TermModel.instance.GetTerm("msg_select_friend");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        //점수와 보낼 친구 선택
        UserData.RefereeNoteDTO refereeNote = MatchBlocksReferee.instance.GetRefereeNote();
        ChallengeUtil.CreateChallengeMsgInfo(selectedFriendInDate, refereeNote.totalScore);

        //도전장 팝업 표시
        ChallengeBeforePopup challengeBeforePopup = UIManager.instance.GetUI<ChallengeBeforePopup>(UI_NAME.ChallengeBeforePopup);
        challengeBeforePopup.SetData();
        challengeBeforePopup.Show();
    }

    private void TutorialConfirm() {
        //도전장 팝업 표시
        ChallengeBeforePopup challengeBeforePopup = UIManager.instance.GetUI<ChallengeBeforePopup>(UI_NAME.ChallengeBeforePopup);
        challengeBeforePopup.SetData();
        challengeBeforePopup.Show();
        EventManager.Notify(EventEnum.ChallengeBeforePopupShow);
        Hide();
    }

    public override void Hide() {
        UserDataModel.instance.challengeMsgInfo.receiverInDate = null;
        base.Hide();
    }

    public ChallengeFriendListPopupSlot GetFirstSlot() {
        return listSlot[0];
    }
}
