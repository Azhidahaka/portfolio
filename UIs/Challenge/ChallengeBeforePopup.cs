using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeBeforePopup : UIBase {
    public ChallengeUserStatus sender;
    public ChallengeUserStatus receiver;

    public List<ChallengeBtnGold> btnGolds;

    public Text lblDesc;
    public Text lblBtnSend;
    public GameObject goBtnSend;
    public GameObject goBtnAccept;
    public GameObject goBtnConfirm;
    public Text lblBtnSendTimeLeft;

    public Text lblBtnCancel;

    private string resetTimeFormat;
    private string btnSendFormat;

    private UserData.ChallengeMsgDTO challengeMsgInfo;
    private string messageInDate;

    private void Awake() {
        InitTutorialTargets();
    }

    private void OnEnable() {
        SetInFrontInCanvas();

        EventManager.Register(EventEnum.ChallengeSelectGold, OnChallengeSelectGold);
        StartCoroutine(JobCheckDate());
    }

    private void OnChallengeSelectGold(object[] args) {
        long gold = (long)args[0];
        UserDataModel.instance.challengeMsgInfo.bettingGold = gold;

        for (int i = 0; i < btnGolds.Count; i++) {
            btnGolds[i].SetState(gold);
        }
        SetDesc(gold);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.ChallengeSelectGold, OnChallengeSelectGold);
    }

    //결과화면에서 도전장보내기를 선택한 경우에만 사용한다.
    public void SetData() {
        SetData(UserDataModel.instance.challengeMsgInfo);
    }

    public void SetData(UserData.ChallengeMsgDTO challengeMsgInfo, string messageInDate = "") {
        UserDataModel.instance.challengeMsgInfo = challengeMsgInfo;
        this.challengeMsgInfo = challengeMsgInfo;
        this.messageInDate = messageInDate;

        sender.SetData(challengeMsgInfo, true, true);
        receiver.SetData(challengeMsgInfo, false, true);

        if (challengeMsgInfo.bettingGold == 0)
            challengeMsgInfo.bettingGold = Constant.CHALLENGE_GOLD_DEFAULT;

        for (int i = 0; i < btnGolds.Count; i++) {
            //내가 도전장을 보내는 입장일때만 변경 가능
            btnGolds[i].SetData(UserDataModel.instance.challengeMsgInfo.senderInDate == BackendLogin.instance.UserInDate);
            btnGolds[i].SetState(challengeMsgInfo.bettingGold);
        }

        SetDesc(challengeMsgInfo.bettingGold);
        SetBtnSend();

        //내가 보내는것이라면 Send버튼 표시
        if (UserDataModel.instance.challengeMsgInfo.senderInDate == BackendLogin.instance.UserInDate) {
            Common.ToggleActive(goBtnSend, true);
            Common.ToggleActive(goBtnAccept, false);
            Common.ToggleActive(goBtnConfirm, false);
            lblBtnCancel.text = TermModel.instance.GetTerm("btn_cancel");
        }
        else {
            Common.ToggleActive(goBtnSend, false);
            Common.ToggleActive(goBtnAccept, true);
            Common.ToggleActive(goBtnConfirm, true);
            lblBtnCancel.text = TermModel.instance.GetTerm("btn_reject");
        }
    }

    private void SetDesc(long gold) {
        string format = TermModel.instance.GetTerm("format_bet_cost");
        lblDesc.text = string.Format(format, Common.GetCommaFormat(gold * 2));
    }

    private void SetBtnSend() {
        if (string.IsNullOrEmpty(resetTimeFormat))
            resetTimeFormat = TermModel.instance.GetTerm("format_reset_time");
        string timeLeft = Common.GetTimerFormat(Common.GetUTCTodayZero() + 86400 - Common.GetUTCNow());

        lblBtnSendTimeLeft.text = string.Format(resetTimeFormat, timeLeft);

        if (string.IsNullOrEmpty(btnSendFormat))
            btnSendFormat = TermModel.instance.GetTerm("format_send");
        lblBtnSend.text = string.Format(btnSendFormat, 
                                        UserDataModel.instance.userProfile.challengeSendRemainCount, 
                                        Constant.CHALLENGE_DAILY_SEND_MAX);
    }

    //도전장을 받은 후 수락했을때
    public void OnBtnAcceptClick() {
        if (UserDataModel.instance.userProfile.gold < challengeMsgInfo.bettingGold) {
            string msg = TermModel.instance.GetTerm("msg_not_enough_gold");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        WebChallenge.instance.ReqAcceptChallenge(challengeMsgInfo, OnResAcceptChallenge);
    }

    private void OnResAcceptChallenge() {
        //게임화면으로 이동
        UserDataModel.instance.LastSelectedStageLevel = challengeMsgInfo.difficulty;
        UserDataModel.instance.RemoveRefereeNote();
        UserDataModel.instance.ContinueGame = false;
        App.instance.ChangeScene(App.SCENE_NAME.MatchBlocks);

        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ACC_PLAYED_COUNT, 1, false);

        AnalyticsUtil.LogPlayStage(UserDataModel.instance.LastSelectedStageLevel);
    }

    public void OnBtnSendClick() {
        EventManager.Notify(EventEnum.ChallengeBeforePopupBtnSendClick);

        if (TutorialManager.instance.IsTutorialInProgress()) {
            Hide();
            return;
        }

        if (UserDataModel.instance.userProfile.challengeSendRemainCount == 0) {
            string msg = TermModel.instance.GetTerm("msg_challenge_send_limit");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        if (UserDataModel.instance.userProfile.gold < challengeMsgInfo.bettingGold) {
            string msg = TermModel.instance.GetTerm("msg_not_enough_gold");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        challengeMsgInfo.state = (long)CHALLENGE_STATE.RECEIVE;
        challengeMsgInfo.createDate = Common.GetUTCTodayZero();
        WebChallenge.instance.ReqSendChallengeMessage(challengeMsgInfo, Hide);
    }

    public void OnBtnRejectClick() {
        //내가 보내는 입장일때는 그냥 닫기
        if (challengeMsgInfo.senderInDate == BackendLogin.instance.UserInDate)
            Hide();
        //받은메시지 삭제
        else {
            Hide();
            WebChallenge.instance.ReqRejectChallenge();
        }
    }

    private IEnumerator JobCheckDate() {
        while (true) {
            SetBtnSend();

            yield return new WaitForSeconds(1.0f);
        }
    }

}
