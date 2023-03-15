using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UserData;

public class WebStage : MonoBehaviour {
    public static WebStage instance;

    private void Awake() {
        instance = this;
    }

    public void ReqEndMatchBlocks(long stageLevel, UserData.RefereeNoteDTO refereeNote, Callback callback) {
        GameData.StageDTO stageData = GameDataModel.instance.GetStageData(stageLevel);
        refereeNote.rewardGold += stageData.rewardGold;
        UserDataModel.instance.AddGold(refereeNote.rewardGold);
        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.USER_PROFILE);

        UserDataModel.instance.RemoveRefereeNote();
        UserDataModel.instance.statistics.dicRoundRecord.Clear();

        FriendMessageDTO lastFriendMessage = UserDataModel.instance.LastFriendMessage;
        //리그
        if (stageLevel == (long)STAGE_LEVEL.LeagueSilver || 
            stageLevel == (long)STAGE_LEVEL.LeagueBronze) {
            UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.DAILY_CHALLENGE_MODE_CLEAR_COUNT, 1);
            //DetermineShowRegisterPopup(refereeNote);
            UpdateLeagueScore(refereeNote);
        }
        //도전장 보내는쪽
        else if (lastFriendMessage.challengeMsgInfo == null) {
            UserDataModel.instance.challengeMsgInfo.senderScore = refereeNote.totalScore;
            SetSingleEnd(stageLevel, refereeNote, callback);
        }
        //도전장 수락한 쪽
        else if (lastFriendMessage.challengeMsgInfo.senderScore > 0) {
            lastFriendMessage.challengeMsgInfo.receiverScore = refereeNote.totalScore;
            WebChallenge.instance.ReqGetChallengeReward(lastFriendMessage);
        }
        //혼자하기
        else {
            SetSingleEnd(stageLevel, refereeNote, callback);
        }
    }

    private void SetSingleEnd(long stageLevel, RefereeNoteDTO refereeNote, Callback callback) {
        List<SingleRankDTO> singleRanks = UserDataModel.instance.GetSingleRankInfos(stageLevel);

        UserData.SingleRankDTO rankInfo = new UserData.SingleRankDTO();
        rankInfo.score = refereeNote.totalScore;
        rankInfo.time = Common.GetUnixTimeNow();
        singleRanks.Add(rankInfo);

        singleRanks.Sort(new SortByScore());

        List<SingleRankDTO> newRanks = new List<SingleRankDTO>();
        long rankCount = Math.Min(singleRanks.Count, Constant.SHOW_RANKING_COUNT);
        for (int i = 0; i < rankCount; i++) {
            singleRanks[i].rank = i + 1;
            newRanks.Add(singleRanks[i]);
        }

        UserDataModel.instance.SetSingleRanks(stageLevel, newRanks);
        UserDataModel.instance.statistics.lastSingleRank = rankInfo;

        //플레이횟수
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.DAILY_SINGLE_MODE_CLEAR_COUNT, 1);

        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.STATISTICS);

        callback();
    }

    private void UpdateLeagueScore(RefereeNoteDTO refereeNote) {
        Callback showResultPopup = () => {
            ResultPopupChallenge resultPopupChallenge = UIManager.instance.GetUI<ResultPopupChallenge>(UI_NAME.ResultPopupChallenge);
            resultPopupChallenge.SetData(BackendLogin.instance.nickname);
            resultPopupChallenge.Show();
        };

        Callback reqRanking = () => {
            BackendRequest.instance.ReqLeagueRankingForResult(showResultPopup);
        };

        if (refereeNote.totalScore > UserDataModel.instance.leagueScoreInfo.leagueScore)
            BackendRequest.instance.ReqUpdateLeagueScore(refereeNote.totalScore, reqRanking);
        else
            reqRanking();
    }

    public class SortByScore : IComparer<UserData.SingleRankDTO> {
        public int Compare(SingleRankDTO left, SingleRankDTO right) {
            if (left.score > right.score)
                return -1;
            else if (left.score < right.score)
                return 1;
            return 0;
        }
    }

    public void ReqUseItem(long itemID, Callback callback = null) {
        GameData.ItemDTO itemData = GameDataModel.instance.GetItemData(itemID);
        UserDataModel.instance.UseGold(itemData.price);
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ACC_USE_ITEM, 1);
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ROUND_ITEM_USECOUNT, 1);
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.DAILY_USE_ITEM, 1);

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.USER_PROFILE,
                                             USER_DATA_KEY.STATISTICS);

        EventManager.Notify(EventEnum.GoodsUpdate);

        AnalyticsUtil.LogUseItem(itemID);

        if (callback != null)
            callback();
    }

    public void ReqGetWaveReward() {
        UserData.RefereeNoteDTO refereeNote = MatchBlocksReferee.instance.GetRefereeNote();
        GameData.BoardWaveDTO boardWaveData = MatchBlocksReferee.instance.GetBoardWaveData();
        GameData.WaveRewardDTO waveRewardData = GameDataModel.instance.GetWaveRewardData(refereeNote.stageLevel);

        if (refereeNote.waveScore < boardWaveData.goal ||
            refereeNote.waveRewardReceived)
            return;

        long waveAdditionalValue = refereeNote.waveCount / 3;

        long gold = waveRewardData.clear + waveRewardData.clearAdditional * waveAdditionalValue;
        gold = Math.Min(gold, waveRewardData.clearMax);

        if (gold == 0)
            return;

        refereeNote.rewardGold += gold;
        MatchBlocksReferee.instance.SetWaveRewardReceived();

        EventManager.Notify(EventEnum.MatchBlocksGetWaveRewardComplete, refereeNote);
    }

    public void ReqGetNoSpaceGold(Callback callback = null) {
        MatchBlocksReferee.instance.ShowNoSpaceAdsCount++;
        MatchBlocksReferee.instance.AvailableGold += Constant.NO_SPACE_GET_GOLD;
        MatchBlocksReferee.instance.AvailableGoldMax += Constant.NO_SPACE_GET_GOLD;

        UserDataModel.instance.AddGold(Constant.NO_SPACE_GET_GOLD);
        EffectManager.instance.ShowGetGoldEffect(Constant.NO_SPACE_GET_GOLD);
        UserDataModel.instance.SaveUserDatas(true,
                                            USER_DATA_KEY.USER_PROFILE,
                                            USER_DATA_KEY.STATISTICS);

        EventManager.Notify(EventEnum.GoodsUpdate);

        BackendRequest.instance.ReqSyncUserData(false, false, () => {
            if (callback != null)
                callback();
        });
    }

    public void ReqLeagueStart(Callback successCallback) {
        UserDataModel.instance.LastSelectedStageLevel = (long)LeagueUtil.GetStageLevel();
        UserDataModel.instance.ContinueGame = false;
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ACC_PLAYED_COUNT, 1, false);

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.USER_PROFILE,
                                             USER_DATA_KEY.STATISTICS);

        if (successCallback != null)
            successCallback();
    }

    public void ReqNeverShowGoldWarning(Callback successCallback) {
        UserDataModel.instance.userProfile.neverShowGoldWarning = 1;
        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.USER_PROFILE);
        if (successCallback != null)
            successCallback();
    }
}
