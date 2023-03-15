using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailBoxPopup : UIBase {
    public enum TAB {
        NORMAL,
        CHALLENGE,
    }

    public LayoutGroup layoutGroup;
    public ScrollRect scrollView;
    public GameObject prefabMailItem;

    public Text lblNoMail;
    public GameObject btnGetAllItemObject;
    public GameObject btnRemoveAllObject;
    public CommonGoods goods;

    public GameObject goTabNormalBg;
    public GameObject goTabChallengeBg;

    public GameObject goChallengeNew;

    private List<MailBoxPopupItem> listItem = new List<MailBoxPopupItem>();
    private List<MailBoxPopupItem> listReusableItem = new List<MailBoxPopupItem>();

    private List<double> mailNos;

    private TAB tab = TAB.NORMAL;
    private bool newMessage;

    private void Awake() {
        InitTutorialTargets();
        Common.ToggleActive(prefabMailItem.gameObject, false);
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.UserDataMailInfoUpdate, OnUserDataMailInfoUpdate);
        EventManager.Register(EventEnum.NotifiedNewMessage, OnNotifiedNewMessage);
    }

    private void OnNotifiedNewMessage(object[] args) {
        if (tab == TAB.CHALLENGE)
            SetData(tab);
    }

    private void OnUserDataMailInfoUpdate(object[] args) {
        SetData(tab);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.UserDataMailInfoUpdate, OnUserDataMailInfoUpdate);
    }

    public void SetData(TAB tab, bool newMessage = false) {
        this.tab = tab;
        this.newMessage = newMessage;

        if (tab == TAB.NORMAL) {
            Common.ToggleActive(goTabNormalBg, true);
            Common.ToggleActive(goTabChallengeBg, false);
            SetNormalScrollVIew();
            Common.ToggleActive(goChallengeNew, newMessage);
        }
        else {
            Common.ToggleActive(goTabNormalBg, false);
            Common.ToggleActive(goTabChallengeBg, true);
            SetChallengeScrollVIew();
            EventManager.Notify(EventEnum.MailboxChallengeTabSelected);
            Common.ToggleActive(goChallengeNew, false);
            this.newMessage = false;
        }
        goods.SetData();
    }

    private void SetChallengeScrollVIew() {
        if (listItem == null) {
            listItem = new List<MailBoxPopupItem>();
            listReusableItem = new List<MailBoxPopupItem>();
        }
        else {
            foreach (MailBoxPopupItem scrollViewItem in listItem) {
                if (listReusableItem.Contains(scrollViewItem) == false)
                    listReusableItem.Add(scrollViewItem);
                Common.ToggleActive(scrollViewItem.gameObject, false);
            }
            listItem.Clear();
        }

        GameObject scrollViewGO;



        List<UserData.FriendMessageDTO> messages;
        if (TutorialManager.instance.IsTutorialInProgress())
            messages = TutorialUtil.GetTutorialFriendMessages();
        else
            messages = UserDataModel.instance.receivedFriendMessages;
        messages.Sort(new SortMessageByCreateTime());

        //우편이 없으면 없음 라벨 표시하고 버튼 숨기기
        if (messages == null || messages.Count == 0) 
            Common.ToggleActive(lblNoMail.gameObject, true);
        else 
            Common.ToggleActive(lblNoMail.gameObject, false);

        Common.ToggleActive(btnGetAllItemObject, false);
        Common.ToggleActive(btnRemoveAllObject, false);    

        for (int i = 0; i < messages.Count; i++) {
            UserData.FriendMessageDTO message = messages[i];

            MailBoxPopupItem scrollViewItem;

            if (i >= listReusableItem.Count) {
                scrollViewGO =  Instantiate(prefabMailItem, layoutGroup.transform);
                scrollViewItem = scrollViewGO.GetComponent<MailBoxPopupItem>();
                listReusableItem.Add(scrollViewItem);
            }
            else {
                scrollViewItem = listReusableItem[i];
                scrollViewGO = scrollViewItem.gameObject;
            }

            Common.ToggleActive(scrollViewGO, true);
            scrollViewItem = scrollViewGO.GetComponent<MailBoxPopupItem>();
            if (message.challengeMsgInfo.state == 0)
                message.challengeMsgInfo.state = (long)CHALLENGE_STATE.RECEIVE;
            scrollViewItem.SetData(message);
            listItem.Add(scrollViewItem);
        }
    }

    private void SetNormalScrollVIew() {
        if (listItem == null) {
            listItem = new List<MailBoxPopupItem>();
            listReusableItem = new List<MailBoxPopupItem>();
        }
        else {
            foreach (MailBoxPopupItem scrollViewItem in listItem) {
                if (listReusableItem.Contains(scrollViewItem) == false)
                    listReusableItem.Add(scrollViewItem);
                Common.ToggleActive(scrollViewItem.gameObject, false);
            }
            listItem.Clear();
        }

        GameObject scrollViewGO;

        List<UserData.MailInfoDTO> mailInfos = UserDataModel.instance.mailInfos;
        mailInfos.Sort(new SortByCreateTime());

        //우편이 없으면 없음 라벨 표시하고 버튼 숨기기
        if (mailInfos == null || mailInfos.Count == 0) {
            Common.ToggleActive(lblNoMail.gameObject, true);
            Common.ToggleActive(btnGetAllItemObject, false);
            Common.ToggleActive(btnRemoveAllObject, false);
        }
        else {
            Common.ToggleActive(btnRemoveAllObject, true);
            Common.ToggleActive(lblNoMail.gameObject, false);

            bool newMailExist = false;
            for (int i = 0; i < mailInfos.Count; i++) {
                if (mailInfos[i].received == false) {
                    newMailExist = true;
                    break;
                }
            }
            Common.ToggleActive(btnGetAllItemObject, newMailExist);
        }

        for (int i = 0; i < mailInfos.Count; i++) {
            UserData.MailInfoDTO mailInfo = mailInfos[i];

            MailBoxPopupItem scrollViewItem;

            if (i >= listReusableItem.Count) {
                scrollViewGO =  Instantiate(prefabMailItem, layoutGroup.transform);
                scrollViewItem = scrollViewGO.GetComponent<MailBoxPopupItem>();
                listReusableItem.Add(scrollViewItem);
            }
            else {
                scrollViewItem = listReusableItem[i];
                scrollViewGO = scrollViewItem.gameObject;
            }

            Common.ToggleActive(scrollViewGO, true);
            scrollViewItem = scrollViewGO.GetComponent<MailBoxPopupItem>();
            scrollViewItem.SetData(mailInfo);
            listItem.Add(scrollViewItem);
        }
    }

    public class SortByCreateTime : IComparer<UserData.MailInfoDTO>{
        public int Compare(UserData.MailInfoDTO left, UserData.MailInfoDTO right) {
            if (left.createTIme < right.createTIme)
                return 1;
            else if (left.createTIme > right.createTIme)
                return -1;
            return 0;
        }
    }

    public class SortMessageByCreateTime : IComparer<UserData.FriendMessageDTO>{
        public int Compare(UserData.FriendMessageDTO left, UserData.FriendMessageDTO right) {
            double leftTime = Common.ConvertStringToTimestamp(left.msgInDate);
            double rightTime = Common.ConvertStringToTimestamp(right.msgInDate);
            if (leftTime < rightTime)
                return 1;
            else if (leftTime > rightTime)
                return -1;
            return 0;
        }
    }

    public void OnBtnDeleteReadMailClick() {
        WebMail.instance.ReqDeleteReadMails(OnResDeleteReadMailsSuccess);
    }

    private void OnResDeleteReadMailsSuccess() {
        string msg = TermModel.instance.GetTerm("msg_delete_mail_complete");
        MessageUtil.ShowSimpleWarning(msg);

        EventManager.Notify(EventEnum.UserDataMailInfoUpdate);
    }

    public void OnBtnGetAllItemClick() {
        mailNos = new List<double>();
        List<UserData.MailInfoDTO> mailInfos = UserDataModel.instance.mailInfos;
        if (mailInfos == null || mailInfos.Count == 0) 
            return;

        BackendRequest.instance.ReqGetPostRewardsAll(OnResGetAdminPostRewards);
    }

    private void OnResGetAdminPostRewards() {
        EventManager.Notify(EventEnum.UserDataMailInfoUpdate);
    }

    public void OnTabNormalClick() {
        BackendRequest.instance.ReqPost(() => SetData(TAB.NORMAL));
    }

    public void OnTabChallengeClick() {
        if (TutorialManager.instance.IsTutorialInProgress()) {
            TutorialTabChallengeClick();
            return;
        }

        BackendRequest.instance.ReqReceiveChallengeMessage(() => SetData(TAB.CHALLENGE));
    }

    private void TutorialTabChallengeClick() {
        SetData(TAB.CHALLENGE);
    }

    public MailBoxPopupItem GetFirstSlot() {
        if (listItem == null || listItem.Count == 0)
            return null;
        return listItem[0];
    }
}
