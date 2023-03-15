using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuckyFlow.EnumDefine;
using UnityEngine.UI;

public class MailBoxPopupItem : MonoBehaviour {
    public Text lblTitle;
    public Text lblExpire;

    public GameObject objSkin;
    public GameObject objGoods;
    public RawImage icoGoods;
    public GameObject objBankSkin;

    public GameObject objReceived;
    public Text lblGoodsAmount;
    public Text lblBtnReceive;

    public GameObject goBtnReceive;

    private UserData.MailInfoDTO mailInfo;
    private GameObject skinObject;

    private UserData.FriendMessageDTO messageInfo;

    public void SetData(UserData.MailInfoDTO mailInfo) {
        messageInfo = null;
        if (skinObject != null)
            Destroy(skinObject);

        this.mailInfo = mailInfo;

        if (mailInfo.title == string.Empty)
            lblTitle.text = MailUtil.GetMailTitle(mailInfo.category);
        else
            lblTitle.text = mailInfo.title;
            
        string expireFormat = TermModel.instance.GetTerm("format_mail_expire");
        lblExpire.text = string.Format(
            expireFormat, 
            Common.GetShortTimerFormat(mailInfo.expireTime - Common.GetUnixTimeNow()));

        if (mailInfo.rewards == null || mailInfo.rewards.Count == 0) {
            Common.ToggleActive(objGoods, false);
            Common.ToggleActive(objSkin, false);
            Common.ToggleActive(objBankSkin, false);
        }
        else if (mailInfo.rewards[0].type == (long)MAIL_REWARD_TYPE.SKIN) {
            Common.ToggleActive(objSkin, true);
            Common.ToggleActive(objGoods, false);
            Common.ToggleActive(objBankSkin, false);
            skinObject = ResourceManager.instance.GetSkinThumbnail(mailInfo.rewards[0].count, objSkin.transform);
        }
        else if (mailInfo.rewards[0].type == (long)MAIL_REWARD_TYPE.PIGGYBANK_SKIN) {
            Common.ToggleActive(objBankSkin, true);
            Common.ToggleActive(objSkin, false);
            Common.ToggleActive(objGoods, false);
            skinObject = ResourceManager.instance.GetPiggyBank(mailInfo.rewards[0].count, objBankSkin.transform);
        }
        else {
            Common.ToggleActive(objGoods, true);
            Common.ToggleActive(objSkin, false);
            Common.ToggleActive(objBankSkin, false);
            lblGoodsAmount.text = Common.GetCommaFormat(mailInfo.rewards[0].count);
            Common.ToggleActive(lblGoodsAmount.gameObject, true);
            icoGoods.texture = ResourceManager.instance.GetMailRewardIco(mailInfo.rewards[0].type);
        }
        
        Common.ToggleActive(objReceived, mailInfo.received);
        lblBtnReceive.text = TermModel.instance.GetTerm("mail_get_item");
    }

    public void SetData(UserData.FriendMessageDTO messageInfo) {
        mailInfo = null;
        if (skinObject != null)
            Destroy(skinObject);
        
        this.messageInfo = messageInfo;

        if (TutorialManager.instance.IsTutorialInProgress()) {
            TutorialSetInfo();
            return;
        }

        UserData.SimpleUserDTO friendInfo;
        if (messageInfo.challengeMsgInfo.state == (long)CHALLENGE_STATE.RECEIVE)
            friendInfo = UserDataModel.instance.GetFriendInfo(messageInfo.challengeMsgInfo.senderInDate);
        else
            friendInfo = UserDataModel.instance.GetFriendInfo(messageInfo.challengeMsgInfo.receiverInDate);
        
        if (friendInfo == null) 
            lblTitle.text = MailUtil.GetChallengeTitle("", messageInfo.challengeMsgInfo.state);
        else
            lblTitle.text = MailUtil.GetChallengeTitle(friendInfo.nickname, messageInfo.challengeMsgInfo.state);
        
        //메세지 도착시간을 기록
        double timstamp = Common.ConvertStringToTimestamp(messageInfo.msgInDate);
        lblExpire.text = Common.ConvertTimestampToDateTime(timstamp);

        Common.ToggleActive(objGoods, true);
        Common.ToggleActive(objSkin, false);
        Common.ToggleActive(objBankSkin, false);
        Common.ToggleActive(lblGoodsAmount.gameObject, false);
        icoGoods.texture = ResourceManager.instance.GetChallengeIco();
        Common.ToggleActive(objReceived, false);

        if (messageInfo.challengeMsgInfo.state == (long)CHALLENGE_STATE.REJECT) 
            lblBtnReceive.text = TermModel.instance.GetTerm("btn_ok");
        else
            lblBtnReceive.text = TermModel.instance.GetTerm("btn_go");
    }

    private void TutorialSetInfo() {
        string titleFormat = TermModel.instance.GetTerm("format_challenge_check_result");
        lblTitle.text = string.Format(titleFormat, "LuckyFlow");
        
        //메세지 도착시간을 기록
        lblExpire.text = Common.ConvertTimestampToDateTime(Common.GetUTCNow());

        Common.ToggleActive(objGoods, true);
        Common.ToggleActive(objSkin, false);
        Common.ToggleActive(objBankSkin, false);
        Common.ToggleActive(lblGoodsAmount.gameObject, false);
        icoGoods.texture = ResourceManager.instance.GetChallengeIco();
        Common.ToggleActive(objReceived, false);

        lblBtnReceive.text = TermModel.instance.GetTerm("btn_go");
    }

    public void OnBtnItemClick() {
        WebMail.instance.ReqMailRead(mailInfo, OnResMailReadSuccess);
    }

    private void OnResMailReadSuccess() {
        EventManager.Notify(EventEnum.UserDataMailInfoUpdate);
    }

    public void OnBtnGetRewardClick() {
        if (TutorialManager.instance.IsTutorialInProgress()) {
            TutorialShowResult();
            return;
        }

        if (messageInfo != null)
            UserDataModel.instance.LastFriendMessage = messageInfo;

        //메일보상받기 터치시
        if (mailInfo != null) {
            BackendRequest.instance.ReqGetPostReward(mailInfo, () => {
                Debug.Log("OnBtnGetRewardClick::ReqGetPostReward::Success");
            });
        }
        //도전장 거절우편
        else if (messageInfo.challengeMsgInfo.state == (long)CHALLENGE_STATE.REJECT) {
            WebChallenge.instance.ReqRejectedMessageConfirm(messageInfo);
        }
        //도전장 받음
        else if (messageInfo.challengeMsgInfo.state == (long)CHALLENGE_STATE.RECEIVE) {
            ChallengeBeforePopup challengeBeforePopup = UIManager.instance.GetUI<ChallengeBeforePopup>(UI_NAME.ChallengeBeforePopup);
            challengeBeforePopup.SetData(messageInfo.challengeMsgInfo, messageInfo.msgInDate);
            challengeBeforePopup.Show();
        }
        //도전장 결과 받음
        else {
            //결과우편 삭제 -> 승패에 따라 보상 받기 -> 이겼으면 광고 표시 -> 결과팝업 표시  
            WebChallenge.instance.ReqGetChallengeReward(messageInfo);
        }
    }

    private void TutorialShowResult() {
        ChallengeResultPopup challengeResultPopup = UIManager.instance.GetUI<ChallengeResultPopup>(UI_NAME.ChallengeResultPopup);
        UserData.ChallengeMsgDTO challengeMsgInfo = TutorialUtil.GetTutorialChallengeMsgDTO();
        challengeResultPopup.SetData(challengeMsgInfo);
        challengeResultPopup.Show();
    }
}
