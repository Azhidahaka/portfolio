using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeScenePresenter : MonoBehaviour {
    public static HomeScenePresenter instance;

    private void Awake() {
        instance = this;
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.MatchBlocksBtnPiggyBankClick, OnMatchBlocksBtnPiggyBankClick);
    }

    private void OnMatchBlocksBtnPiggyBankClick(object[] args) {
        GameData.PiggyBankDTO piggyBankData = GameDataModel.instance.GetPiggyBankData(UserDataModel.instance.userProfile.piggyBankType);

        float collectedGold = PiggyBankUtil.GetCurrentGold(piggyBankData);
        //@todo : 골드가 최소량을 만족하지 못하면 메시지 출력
        if (collectedGold < 
            piggyBankData.capacity * (Constant.MIN_DEBIT_GOLD_PERCENT / 100)) {
            //메세지 표시
            return;
        }
        
        //골드가 충분히 모였다면 골드 획득후 상태 갱신
        WebUser.instance.ReqWithdrawPiggyBank(() => {
            EventManager.Notify(EventEnum.WithdrawPiggyGoldComplete);
        });

    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.MatchBlocksBtnPiggyBankClick, OnMatchBlocksBtnPiggyBankClick);
    }

    public void SetData(bool byTitle) {
        Home homeMenu = UIManager.instance.GetUI<Home>(UI_NAME.Home);
        homeMenu.SetData(byTitle);
        homeMenu.Show();

        

        DetermineInputNickname();
    }

    private void DetermineInputNickname() {
        Callback nickNameCallback = () => {
            BackendRequest.instance.ReqPost(() => BackendRequest.instance.ReqReceiveChallengeMessage(null));
            if (UserDataModel.instance.getFriendList == false) {
                BackendRequest.instance.ReqFriendList(null);
                BackendRequest.instance.ReqFriendReceivedList(null);
                BackendRequest.instance.ReqGetCurrentLeague(null);
            }
        };

        if (string.IsNullOrEmpty(BackendLogin.instance.nickname) == false) {
            EventManager.Notify(EventEnum.TutorialCheck);
            nickNameCallback();
            return;
        }

        NicknameChangePopup nicknameChangePopup = UIManager.instance.GetUI<NicknameChangePopup>(UI_NAME.NicknameChangePopup);
        nicknameChangePopup.SetData(nickNameCallback);
        nicknameChangePopup.Show();
    }

    private void OnResSimpleRanking() {
        EventManager.Notify(EventEnum.SimpleRankingUpdate);
    }
}
