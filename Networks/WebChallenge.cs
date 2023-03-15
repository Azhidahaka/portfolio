using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebChallenge : MonoBehaviour {
    public static WebChallenge instance;

    private void Awake() {
        instance = this;
    }

    public void ReqGetChallengeReward(UserData.FriendMessageDTO messageInfo) {
        bool sender = messageInfo.challengeMsgInfo.senderInDate == BackendLogin.instance.UserInDate;

        Callback getReward = () => {
            UserData.ChallengeMsgDTO challengeMsgInfo = UserDataModel.instance.LastFriendMessage.challengeMsgInfo;

            if (challengeMsgInfo.senderScore > challengeMsgInfo.receiverScore) {
                UserDataModel.instance.AddGold(challengeMsgInfo.bettingGold * 2);
                UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.USER_PROFILE);
            }
            else if (challengeMsgInfo.senderScore == challengeMsgInfo.receiverScore) {
                UserDataModel.instance.AddGold(challengeMsgInfo.bettingGold);
                UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.USER_PROFILE);
            }

            BackendRequest.instance.ReqSyncUserData(false, false, OnResGetChallengeReward);
        };

        //BackendRequest.instance.
        Callback determineSendResult = () => {
            UserDataModel.instance.RemoveReceivedFriendMessage(messageInfo.msgInDate);
            EventManager.Notify(EventEnum.UserDataMailInfoUpdate);
            UserData.ChallengeMsgDTO challengeMsgInfo = UserDataModel.instance.LastFriendMessage.challengeMsgInfo;
            if (sender) 
                getReward();
            //내가 도전장을 받은입장일경우 결과를 전송해준다.
            else {
                challengeMsgInfo.state = (long)CHALLENGE_STATE.CHECK_RESULT;
                BackendRequest.instance.ReqSendChallengeMessage(challengeMsgInfo, getReward);
            }
        };
        
        UserDataModel.instance.LastFriendMessage = messageInfo;
        //해당 우편을 먼저 삭제한 뒤, 콜백에서 보상 지급 처리
        BackendRequest.instance.ReqDeleteReceivedChallengeMessage(messageInfo.msgInDate, determineSendResult);
    }

    private void OnResGetChallengeReward() {
        UserData.ChallengeMsgDTO challengeMsgInfo = UserDataModel.instance.LastFriendMessage.challengeMsgInfo;

        ChallengeResultPopup challengeResultPopup = UIManager.instance.GetUI<ChallengeResultPopup>(UI_NAME.ChallengeResultPopup);
        challengeResultPopup.SetData(challengeMsgInfo);
        challengeResultPopup.Show();
    }

    public void ReqRejectChallenge() {
        UserData.FriendMessageDTO message = UserDataModel.instance.LastFriendMessage;
        UserDataModel.instance.LastFriendMessage = message;
        BackendRequest.instance.ReqDeleteReceivedChallengeMessage(message.msgInDate, OnResDeleteReceivedChallengeMessage);
    }

    private void OnResDeleteReceivedChallengeMessage() {
        UserData.FriendMessageDTO message = UserDataModel.instance.LastFriendMessage;
        UserDataModel.instance.RemoveReceivedFriendMessage(message.msgInDate);
        message.challengeMsgInfo.state = (long)CHALLENGE_STATE.REJECT;
        //거절우편 보내기
        BackendRequest.instance.ReqSendChallengeMessage(message.challengeMsgInfo, OnResSendRejectMessage);
    }

    private void OnResSendRejectMessage() {
        UserData.FriendMessageDTO message = UserDataModel.instance.LastFriendMessage;
        UserDataModel.instance.LastFriendMessage = message;
        
        EventManager.Notify(EventEnum.UserDataMailInfoUpdate);
    }

    public void ReqRejectedMessageConfirm(UserData.FriendMessageDTO messageInfo) {
        BackendRequest.instance.ReqDeleteReceivedChallengeMessage(messageInfo.msgInDate, OnResRejectedMessageConfirm);
    }

    private void OnResRejectedMessageConfirm() {
        UserData.FriendMessageDTO message = UserDataModel.instance.LastFriendMessage;
        UserDataModel.instance.RemoveReceivedFriendMessage(message.msgInDate);

        long createUTCDateZero = Common.GetUTCDateZero(message.challengeMsgInfo.createDate);
        long todayZero = Common.GetUTCTodayZero();

        //메세지 받은 날짜가 오늘인경우, remainCount + 1
        if (createUTCDateZero == todayZero) {
            UserDataModel.instance.userProfile.challengeSendRemainCount++;
            if (UserDataModel.instance.userProfile.challengeSendRemainCount > Constant.CHALLENGE_DAILY_SEND_MAX)
                UserDataModel.instance.userProfile.challengeSendRemainCount = Constant.CHALLENGE_DAILY_SEND_MAX;
            UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.USER_PROFILE);
            BackendRequest.instance.ReqSyncUserData(false, false);
        }

        EventManager.Notify(EventEnum.UserDataMailInfoUpdate);
    }

    public void ReqSendChallengeMessage(UserData.ChallengeMsgDTO challengeMsgInfo, Callback successcallback) {
        Callback useGold = () => {
            UserDataModel.instance.UseGold(challengeMsgInfo.bettingGold);
            UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.USER_PROFILE);
            BackendRequest.instance.ReqSyncUserData(false, true, successcallback);
        };
        
        BackendRequest.instance.ReqSendChallengeMessage(challengeMsgInfo, useGold);
    }

    public void ReqAcceptChallenge(UserData.ChallengeMsgDTO challengeMsgInfo, Callback successcallback) {
        UserDataModel.instance.UseGold(challengeMsgInfo.bettingGold);
        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.USER_PROFILE);
        BackendRequest.instance.ReqSyncUserData(false, true, successcallback);
    }
}
