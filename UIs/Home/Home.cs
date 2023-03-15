using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserData;

public class Home : UIBase {
    public MaskableGraphic imgBtnPrev;
    public MaskableGraphic imgBtnNext;
    public GameObject objNewAchievement;
    public Text lblNewAchievementCount;

    public MatchBlocksPiggyBank piggyBank;
    public GameObject objNewMail;
    public Text lblNewMailCount;

    public HomeRankingSlot firstRanking;
    public HomeRankingSlot secondRanking;
    public HomeRankingSlot thirdRanking;
    public HomeRankingSlot myRanking;

    public CommonGoods goods;

    public GameObject objNewNotice;
    public Text lblNewNoticeCount;

    public RawImage imgTitle;

    public UserStatus userStatus;
    public Profile profile;

    public GameObject goNewFriend;
    public Text lblNewFriendRequestCount;

    private bool newMessage = false;

    private void Awake() {
        InitTutorialTargets();
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.UserDataUserInfoUpdate, OnUserDataUserInfoUpdate);
        EventManager.Register(EventEnum.UpdateAchievementCleared, OnUpdateAchievementCleared);
        EventManager.Register(EventEnum.UserDataMailInfoUpdate, OnUserDataMailInfoUpdate);
        EventManager.Register(EventEnum.SimpleRankingUpdate, OnSimpleRankingUpdate);
        EventManager.Register(EventEnum.NoticeRead, OnNoticeRead);
        EventManager.Register(EventEnum.NicknameChanged, OnNicknameChanged);

        EventManager.Register(EventEnum.NotifiedNewMessage, OnNotifiedNewMessage);
        EventManager.Register(EventEnum.NotifiedNewFriend, OnNotifiedNewFriend);
        EventManager.Register(EventEnum.AchievementGetRewardAllComplete, OnUpdateAchievementCleared);
        EventManager.Register(EventEnum.AchievementGetRewardComplete, OnUpdateAchievementCleared);
        EventManager.Register(EventEnum.FriendsStateUpdate, OnFriendsStateUpdate);
    }

    private void OnFriendsStateUpdate(object[] args) {
        DetermineShowNewFriend();
    }

    private void OnNotifiedNewMessage(object[] args) {
        Common.ToggleActive(objNewMail, true);
        newMessage = true;
    }

    private void OnNotifiedNewFriend(object[] args) {
        Common.ToggleActive(goNewFriend, true);
    }

    private void OnNicknameChanged(object[] args) {
        userStatus.SetData();
    }

    private void OnNoticeRead(object[] args) {
        UpdateNewNoticeCount();
    }

    private void OnSimpleRankingUpdate(object[] args) {
        SetRanking();
    }

    private void SetRanking() {
        ChallengeRankDTO firstInfo = null;
        ChallengeRankDTO secondInfo = null;
        ChallengeRankDTO thirdInfo = null;
            
        if (UserDataModel.instance.top3RankInfos.Count >= 1)
            firstInfo = UserDataModel.instance.top3RankInfos[0];
        if (UserDataModel.instance.top3RankInfos.Count >= 2)
            secondInfo = UserDataModel.instance.top3RankInfos[1];
        if (UserDataModel.instance.top3RankInfos.Count >= 3)
            thirdInfo = UserDataModel.instance.top3RankInfos[2];

        bool first = firstRanking.SetData(firstInfo);
        bool second = secondRanking.SetData(secondInfo);
        bool third = thirdRanking.SetData(thirdInfo);

        if (first || second || third || UserDataModel.instance.myChallengeRankInfo == null)
            Common.ToggleActive(myRanking.gameObject, false);
        else {
            Common.ToggleActive(myRanking.gameObject, true);
            myRanking.SetData(UserDataModel.instance.myChallengeRankInfo);
        }
    }

    private void OnUserDataMailInfoUpdate(object[] args) {
        DetermineShowNewMail();
    }

    private void OnUserDataUserInfoUpdate(object[] args) {
        piggyBank.SetData();
    }

    private void OnUpdateAchievementCleared(object[] args) {
        DetermineShowNewAchievement();
    }

    private void DetermineShowNewAchievement() {
        long newCount = AchievementUtil.GetClearedAchivementCount();
        if (newCount > 0) {
            lblNewAchievementCount.text = Common.GetCommaFormat(newCount);
            Common.ToggleActive(objNewAchievement, true);
        }
        else
            Common.ToggleActive(objNewAchievement, false);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.UserDataUserInfoUpdate, OnUserDataUserInfoUpdate);
        EventManager.Remove(EventEnum.UpdateAchievementCleared, OnUpdateAchievementCleared);
        EventManager.Remove(EventEnum.UserDataMailInfoUpdate, OnUserDataMailInfoUpdate);
        EventManager.Remove(EventEnum.SimpleRankingUpdate, OnSimpleRankingUpdate);
        EventManager.Remove(EventEnum.NoticeRead, OnNoticeRead);
        EventManager.Remove(EventEnum.NicknameChanged, OnNicknameChanged);

        EventManager.Remove(EventEnum.NotifiedNewMessage, OnNotifiedNewMessage);
        EventManager.Remove(EventEnum.NotifiedNewFriend, OnNotifiedNewFriend);
        EventManager.Remove(EventEnum.AchievementGetRewardAllComplete, OnUpdateAchievementCleared);
        EventManager.Remove(EventEnum.AchievementGetRewardComplete, OnUpdateAchievementCleared);
        EventManager.Remove(EventEnum.FriendsStateUpdate, OnFriendsStateUpdate);
    }

    public void SetData(bool byTitle) {
        lblNewMailCount.text = "N";
        lblNewFriendRequestCount.text = "N";
        

        imgTitle.texture = ResourceManager.instance.GetGameTitleTexture();

        DetermineShowNewMail();
        DetermineShowNewAchievement();
        DetermineShowNewFriend();

        piggyBank.SetData();

        if (byTitle) {
            BackendLogin.instance.ConnectNotificationServer();
            BackendRequest.instance.ReqGetNotice(OnResGetNotice);
            if (UserDataModel.instance.statistics.getStartDiamonds == 0)
                WebUser.instance.ReqGetStartDiamond();
        }
        else 
            UpdateNewNoticeCount();

        SetRanking();
        goods.SetData();
        userStatus.SetData();
        profile.SetData(UserDataModel.instance.publicUserData);
    }

    private void DetermineShowNewFriend() {
        Common.ToggleActive(goNewFriend, UserDataModel.instance.friendReceives.Count > 0);
    }

    private void OnResGetNotice() {
        string lastNoticeIndate = UserDataModel.instance.statistics.lastNoticeIndate;
        List<UserData.NoticeDTO> noticeInfos = UserDataModel.instance.noticeInfos;
        //공지 내용이 있고, 받아온적 없는 공지일때 표시
        if (noticeInfos.Count > 0 &&
            lastNoticeIndate != noticeInfos[0].inDate) {
            //공지팝업 표시
            SimpleNoticePopup noticePopup = UIManager.instance.GetUI<SimpleNoticePopup>(UI_NAME.SimpleNoticePopup);
            noticePopup.SetData(noticeInfos);
            noticePopup.Show();

            WebUser.instance.ReqSetNoticeIndate();
        }
        UpdateNewNoticeCount();
    }

    private void UpdateNewNoticeCount() {
        long newNoticeCount = GetNewNoticeCount();
        lblNewNoticeCount.text = Common.GetCommaFormat(newNoticeCount);
        Common.ToggleActive(objNewNotice, newNoticeCount > 0);
    }

    private long GetNewNoticeCount() {
        long count = 0;
        foreach (NoticeDTO noticeInfo in UserDataModel.instance.noticeInfos) {
            if (noticeInfo.read == false)
                count++;
        }
        return count;
    }

    public void OnBtnChallengeClick() {
        ChallengeGameEnterPopup popup = UIManager.instance.GetUI<ChallengeGameEnterPopup>(UI_NAME.ChallengeGameEnterPopup);
        popup.SetData();
        popup.Show();
    }

    public void OnBtnShopClick() {
        ShopMenu shopMenu = UIManager.instance.GetUI<ShopMenu>(UI_NAME.ShopMenu);
        shopMenu.SetData(PRODUCT_CATEGORY.GOODS);
        shopMenu.Show();

        WebUser.instance.ReqSetStatistics(1, 
                                                STATISTICS_TYPE.ACC_VISIT_SHOP,
                                                STATISTICS_TYPE.DAILY_VISIT_SHOP);
    }

    public void OnBtnSettingClick() {
        OptionMenu optionMenu = UIManager.instance.GetUI<OptionMenu>(UI_NAME.OptionMenu);
        optionMenu.SetData();
        optionMenu.Show();
    }

    public void OnBtnRankingClick() {
        LeagueUtil.ShowLeagueRanking();
    }

    public void OnBtnAchievmentClick() {
        Achievement achievement = UIManager.instance.GetUI<Achievement>(UI_NAME.Achievement);
        achievement.SetData();
        achievement.Show();
    }

    public void OnBtnSingleClick() {
        Callback newGame = () => {
            UserDataModel.instance.ContinueGame = false;
            GameLevelPopup gameLevelPopup = UIManager.instance.GetUI<GameLevelPopup>(UI_NAME.GameLevelPopup);
            gameLevelPopup.SetData();
            gameLevelPopup.Show();
        };

        RefereeNoteDTO refereeNote = UserDataModel.instance.LoadRefereeNote();
        if (refereeNote != null && TutorialManager.instance.IsTutorialInProgress() == false) {
            string msg = TermModel.instance.GetTerm("msg_continue");
            Callback callbackYes = () => {
                UserDataModel.instance.ContinueGame = true;
                App.instance.ChangeScene(App.SCENE_NAME.MatchBlocks);
            };

            MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.YES_NO, msg, callbackYes, newGame);
        }
        else 
            newGame();
    }

    
    public void OnBtnContinueClick() {
        App.instance.ChangeScene(App.SCENE_NAME.MatchBlocks);
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ACC_PLAYED_COUNT, 1, false);
    }

    public override void OnCopy(List<object> datas) {
        SetData(false);
    }

    public override List<object> GetCopyDatas() {
        return null;
    }

    public void OnBtnMailClick() {
        BackendRequest.instance.ReqPost(ResPost);
    }

    private void ResPost() {
        MailBoxPopup mailBoxPopup = UIManager.instance.GetUI<MailBoxPopup>(UI_NAME.MailBoxPopup);
        mailBoxPopup.SetData(MailBoxPopup.TAB.NORMAL, newMessage);
        mailBoxPopup.Show();
        Common.ToggleActive(objNewMail, false);
        EventManager.Notify(EventEnum.TutorialCheck);
    }

    private void DetermineShowNewMail() {
        List<UserData.MailInfoDTO> mailInfos = UserDataModel.instance.mailInfos;
        if (mailInfos == null || mailInfos.Count == 0) {
            Common.ToggleActive(objNewMail, false);
            return;
        }

        long newCount = 0;
        for (int i = 0; i < mailInfos.Count; i++) {
            if (mailInfos[i].read == false) {
                newCount++;
            }
        }

        lblNewMailCount.text = newCount.ToString();
        Common.ToggleActive(objNewMail, newCount > 0);
    }

    public void OnBtnNoticeClick() {
        Callback callback = () => {
            UpdateNewNoticeCount();
            SimpleNoticePopup noticePopup = UIManager.instance.GetUI<SimpleNoticePopup>(UI_NAME.SimpleNoticePopup);
            noticePopup.SetData(UserDataModel.instance.noticeInfos);
            noticePopup.Show();
        };

        BackendRequest.instance.ReqGetNotice(callback);
    }

    public void OnBtnProfileClick() {
        UserProfilePopup userProfilePopup = UIManager.instance.GetUI<UserProfilePopup>(UI_NAME.UserProfilePopup);
        userProfilePopup.SetData();
        userProfilePopup.Show();
    }

    public void OnBtnFriendClick() {
        Callback successCallback = () => {
            DetermineShowNewFriend();
            FriendsListPopup popup = UIManager.instance.GetUI<FriendsListPopup>(UI_NAME.FriendsListPopup);
            popup.SetData();
            popup.Show();
        };

        BackendRequest.instance.ReqFriendList(successCallback);
    }

    public void OnBtnLeagueClick() {
        BackendRequest.instance.ReqGetCurrentLeague(DetermineLeague);
    }

    private void DetermineLeague() {
        WebLeague.instance.ReqDetermineLeague(ShowMultiGameListPopup);
    }

    private void ShowMultiGameListPopup() {
        MultiGameListPopup multiGameListPopup = UIManager.instance.GetUI<MultiGameListPopup>(UI_NAME.MultiGameListPopup);
        multiGameListPopup.SetData();
        multiGameListPopup.Show();

        EventManager.Notify(EventEnum.TutorialCheck);
    }
}
