using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeResultPopup : UIBase {
    public enum RESULT {
        None,
        Win,
        Lose,
        Draw,
    }

    public Animator animator;
    public ChallengeUserStatus sender;
    public ChallengeUserStatus receiver;

    public Text lblGainGold;
    public GameObject goLblDraw;
    public GameObject goLblLose;

    public GameObject goBtnConfirm;
    public GameObject goBtnGetRewardAndConfirm;

    private RESULT result;

    private UserData.ChallengeMsgDTO challengeMsgInfo;

    private void Awake() {
        InitTutorialTargets();
    }

    private void OnEnable() {
        SetInFrontInCanvas();

        if (result == RESULT.None)
            return;

        AnimationUtil.SetTrigger(animator, result.ToString());
        EventManager.Notify(EventEnum.ShowChallengeResult);
    }

    public void SetData(UserData.ChallengeMsgDTO challengeMsgInfo) {
        this.challengeMsgInfo = challengeMsgInfo;

        sender.SetData(challengeMsgInfo, true, false);
        receiver.SetData(challengeMsgInfo, false, false);

        if (TutorialManager.instance.IsTutorialInProgress()) {
            SetTutorial();
            return;
        }

        //내가 도전장을 보낸사람일때
        if (BackendLogin.instance.UserInDate == challengeMsgInfo.senderInDate) {
            if(challengeMsgInfo.senderScore > challengeMsgInfo.receiverScore)
                result = RESULT.Win;
            else if (challengeMsgInfo.senderScore < challengeMsgInfo.receiverScore)
                result = RESULT.Lose;
            else
                result = RESULT.Draw;
        }
        else {
            if(challengeMsgInfo.receiverScore > challengeMsgInfo.senderScore)
                result = RESULT.Win;
            else if (challengeMsgInfo.receiverScore < challengeMsgInfo.senderScore)
                result = RESULT.Lose;
            else
                result = RESULT.Draw;
        }

        //WIN
        if (result == RESULT.Win) {
            Common.ToggleActive(goBtnGetRewardAndConfirm, true);
            Common.ToggleActive(goBtnConfirm, false);
            
            lblGainGold.text = $"+{Common.GetCommaFormat(challengeMsgInfo.bettingGold * 2)}";
            Common.ToggleActive(lblGainGold.gameObject, true);
            Common.ToggleActive(goLblDraw, false);
            Common.ToggleActive(goLblLose, false);
            AnimationUtil.SetTrigger(animator, "Win");
        }
        //LOSE
        else if (result == RESULT.Lose) {
            Common.ToggleActive(goBtnConfirm, true);
            Common.ToggleActive(goBtnGetRewardAndConfirm, false);
            
            Common.ToggleActive(goLblLose, true);
            Common.ToggleActive(lblGainGold.gameObject, false);
            Common.ToggleActive(goLblDraw, false);
            AnimationUtil.SetTrigger(animator, "Lose");
        }
        //DRAW
        else {
            Common.ToggleActive(goBtnGetRewardAndConfirm, true);
            Common.ToggleActive(goBtnConfirm, false);
            
            Common.ToggleActive(goLblDraw, true);
            Common.ToggleActive(goLblLose, false);
            Common.ToggleActive(lblGainGold.gameObject, false);
            AnimationUtil.SetTrigger(animator, "Draw");
        }
    }

    private void SetTutorial() {
        result = RESULT.Win;

        Common.ToggleActive(goBtnGetRewardAndConfirm, true);
        Common.ToggleActive(goBtnConfirm, false);
            
        lblGainGold.text = $"+{Common.GetCommaFormat(challengeMsgInfo.bettingGold * 2)}";
        Common.ToggleActive(lblGainGold.gameObject, true);
        Common.ToggleActive(goLblDraw, false);
        Common.ToggleActive(goLblLose, false);
        AnimationUtil.SetTrigger(animator, "Win");
    }

    public override void Hide() {
        if (TutorialManager.instance.IsTutorialInProgress()) {
            TutorialHide();
            return;
        }

        AdsManager adsInstance = AdsManager.GetLoadedInstance(ADS_PLACEMENT.REWARDED_VIDEO);
        Callback viewAdscallback = () => {
            if (App.instance.GetSceneName() == App.SCENE_NAME.MatchBlocks) {
                base.Hide();
                App.instance.ChangeScene(App.SCENE_NAME.Home);
            }
            else 
                base.Hide();
        };

        //승리시 광고 이후 결과팝업 표시
        if (result == RESULT.Win &&
            adsInstance != null &&
            adsInstance.IsRewardedVideoLoaded()) {
            adsInstance.ShowRewardedAd(false, () => viewAdscallback());
        }
        else
            viewAdscallback();
    }

    private void TutorialHide() {
        base.Hide();
        MailBoxPopup mailBoxPoup = UIManager.instance.GetUI<MailBoxPopup>(UI_NAME.MailBoxPopup, false);
        mailBoxPoup.Hide();
        EventManager.Notify(EventEnum.HideChallengeResult);
    }
}
