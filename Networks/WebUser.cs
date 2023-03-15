using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using QuantumTek.EncryptedSave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebUser : MonoBehaviour {
    public static WebUser instance;

    private void Awake() {
        instance = this;
    }

    public void ReqChangeSkin(long skinID, Callback successCallback = null) {
        if (UserDataModel.instance.SkinID != skinID)
            UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ACC_SKIN_CHANGE_COUNT, 1, true);

        UserDataModel.instance.SkinID = skinID;

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.LAST_SELECTED_INFOS,
                                             USER_DATA_KEY.STATISTICS);

        EventManager.Notify(EventEnum.SkinChanged);

        AnalyticsUtil.LogApplySkin(skinID);

        if (successCallback != null)
            successCallback();
    }

    public void ReqChangePiggyBankSkin(long bankSkinID, Callback successCallback = null) {
        UserDataModel.instance.BankSkinID = bankSkinID;

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.LAST_SELECTED_INFOS,
                                             USER_DATA_KEY.STATISTICS);

        EventManager.Notify(EventEnum.BankSkinChanged);

        if (successCallback != null)
            successCallback();
    }

    public void ReqSavePlayData(UserData.RefereeNoteDTO refereeNote) {
        UserDataModel.instance.SaveRefereeNote(refereeNote);

        UserDataModel.instance.SaveUserDatas(true,
                                            USER_DATA_KEY.REFEREE_NOTE,
                                            USER_DATA_KEY.USER_PROFILE);
    }

    //MatchBlocks -> 받기 버튼 클릭시 동작
    public void ReqWithdrawPiggyBank(Callback successCallback = null) {
        //UserDataModel.instance.userProfile.collectedGold
        long piggyBankType = UserDataModel.instance.userProfile.piggyBankType;
        GameData.PiggyBankDTO piggyBankData = GameDataModel.instance.GetPiggyBankData(piggyBankType);

        long collectedGold = (long)PiggyBankUtil.GetCurrentGold(piggyBankData);
        UserDataModel.instance.AddGold(collectedGold);
        EffectManager.instance.ShowGetGoldEffect(collectedGold);
        UserDataModel.instance.userProfile.collectedGold = 0;
        UserDataModel.instance.userProfile.collectedChangedTime = Common.GetUnixTimeNow();

        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ACC_GET_PIGGY_BANK_REWARD_COUNT, 1);
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.GET_PIGGY_BANK_REWARD_COUNT, 1);

        UserDataModel.instance.SaveUserDatas(true,
                                            USER_DATA_KEY.USER_PROFILE,
                                            USER_DATA_KEY.STATISTICS);

        EventManager.Notify(EventEnum.GoodsUpdate);

        BackendRequest.instance.ReqSyncUserData(false, true, successCallback);
    }

    public void ReqChangeNickname(string nickname, Callback successCallback) {
        UserDataModel.instance.userProfile.nickname = nickname;

        UserDataModel.instance.SaveUserDatas(true,
                                            USER_DATA_KEY.USER_PROFILE);
        BackendRequest.instance.ReqSyncUserData(false, true, successCallback);
    }

    public void ReqSetStatistics(long value, params STATISTICS_TYPE[] valueTypes) {
        for (int i = 0; i < valueTypes.Length; i++) {
            UserDataModel.instance.SetAchievementCount(valueTypes[i], value);
        }

        UserDataModel.instance.SaveUserDatas(true,
                                            USER_DATA_KEY.STATISTICS);
    }

    public void ReqGetAchievementReward(List<GameData.AchievementDTO> achievementDatas, Callback successCallback) {
        if (achievementDatas[0].rewardType == (long)ACHIEVEMENT_REWARD_TYPE.GOLD) {
            UserDataModel.instance.AddGold(achievementDatas[0].rewardValue);
            EffectManager.instance.ShowGetGoldEffect(achievementDatas[0].rewardValue);
        }
        else if (achievementDatas[0].rewardType == (long)ACHIEVEMENT_REWARD_TYPE.DIAMOND) {
            UserDataModel.instance.AddDiamond(achievementDatas[0].rewardValue);
            EffectManager.instance.ShowGetDiamondEffect(achievementDatas[0].rewardValue);
        }

        UserDataModel.instance.statistics.clearedAchievementIDs.Remove(achievementDatas[0].achievementID);

        ACHIEVEMENT_REPEAT repeat = (ACHIEVEMENT_REPEAT)achievementDatas[0].repeat;
        if (repeat == ACHIEVEMENT_REPEAT.NONE ||
            repeat == ACHIEVEMENT_REPEAT.DAILY) {
            UserDataModel.instance.statistics.receivedAchievementIDs.Add(achievementDatas[0].achievementID);
        }
        //반복퀘스트인경우 보상수령 완료 목록에 등록하지 않는다.
        else if (repeat == ACHIEVEMENT_REPEAT.IMMEDIATELY_INCREASE) {
            long nextLevel = UserDataModel.instance.GetAchievementLevel(achievementDatas[0].achievementID) + 1;
            long nextLevelValue = AchievementUtil.GetComparingValue(achievementDatas[0], nextLevel);
            if (nextLevelValue <= achievementDatas[0].valueMax)
                UserDataModel.instance.IncreaseAchievementLevel(achievementDatas[0].achievementID);
            //업적 최대레벨에 도달한 경우 완료처리 
            else
                UserDataModel.instance.statistics.receivedAchievementIDs.Add(achievementDatas[0].achievementID);
        }

        //INSTANT valueType인 경우 업적 완료 즉시 리셋
        foreach (GameData.AchievementDTO achievementData in achievementDatas) {
            if (achievementData.valueType > (long)STATISTICS_TYPE.INSTANT_START &&
                achievementData.valueType < (long)STATISTICS_TYPE.INSTANT_END)
                UserDataModel.instance.ResetInstantAchievementCount(achievementData.valueType);
        }

        //일괄수령인경우 모든 보상이 수령된 직후 따로 처리
        UserDataModel.instance.SaveUserDatas(true,
                                        USER_DATA_KEY.USER_PROFILE,
                                        USER_DATA_KEY.STATISTICS);

        EventManager.Notify(EventEnum.GoodsUpdate);

        
        AchievementManager.instance.CheckAchievement();
        

        //@todo : 보상 획득연출 표시, Callback에서 보상연출 꺼주기
        BackendRequest.instance.ReqSyncUserData(false, false,
            () => {
                AnalyticsUtil.LogGetAchievementReward(achievementDatas[0].achievementID);
                successCallback();
                EventManager.Notify(EventEnum.AchievementGetRewardComplete);
            });
    }

    public void ReqSetFlagNo(long flagNo, Callback successCallback) {
        UserDataModel.instance.userProfile.flagNo = flagNo;

        UserDataModel.instance.SaveUserDatas(true,
                                            USER_DATA_KEY.USER_PROFILE);

        EventManager.Notify(EventEnum.UserDataFlagUpdate);

        BackendRequest.instance.ReqSyncUserData(false, true, successCallback);
    }

    public void ReqRefreshDate() {
        UserDataModel.instance.userProfile.loginUtcZero = Common.GetUTCDateZero(Common.GetUTCNow());
        UserDataModel.instance.statistics.dicDailyRecord.Clear();

        List<GameData.AchievementDTO> achievements = GameDataModel.instance.achievements;
        foreach (GameData.AchievementDTO achievement in achievements) {
            if (achievement.repeat != (long)ACHIEVEMENT_REPEAT.DAILY)
                continue;

            List<long> clearedIDs = UserDataModel.instance.statistics.clearedAchievementIDs;
            if (clearedIDs.Contains(achievement.achievementID))
                clearedIDs.Remove(achievement.achievementID);
            List<long> receivedIDs = UserDataModel.instance.statistics.receivedAchievementIDs;
            if (receivedIDs.Contains(achievement.achievementID))
                receivedIDs.Remove(achievement.achievementID);
        }

        UserDataModel.instance.userProfile.challengeTicket = Constant.CHALLENGE_TICKET_MAX;
        UserDataModel.instance.userProfile.challengeTicktBuyCount = 0;
        UserDataModel.instance.userProfile.challengeSendRemainCount = Constant.CHALLENGE_DAILY_SEND_MAX;    //도전장 횟수 초기화

        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.DAILY_CONTINUOUS_LOGIN_COUNT, 1);

        EventManager.Notify(EventEnum.RefreshDate);

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.USER_PROFILE,
                                             USER_DATA_KEY.STATISTICS);
    }

    public void ReqChangeLanguage(LANGUAGE language, Callback successCallback = null) {
        if (language == (LANGUAGE)UserDataModel.instance.gameOptions.language)
            return;

        UserDataModel.instance.gameOptions.language = (long)language;
        ES_Save.Save((long)language, Constant.LANGUAGE_PATH);

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.USER_PROFILE,
                                             USER_DATA_KEY.GAME_OPTIONS);

        if (successCallback != null)
            successCallback();
    }

    public void ReqBuyChallengeTicket(long price, Callback successCallback = null) {
        UserDataModel.instance.UseDiamond(price);
        UserDataModel.instance.AddChallengeTicket(1);
        UserDataModel.instance.userProfile.challengeTicktBuyCount += 1;

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.USER_PROFILE,
                                             USER_DATA_KEY.STATISTICS);

        EventManager.Notify(EventEnum.GoodsUpdate);

        AnalyticsUtil.LogBuyChallengeTicket();

        if (successCallback != null)
            successCallback();
    }

    public void ReqAgreePolicy(Callback successCallback = null) {
        long newAgreeTerm = (long)POLICY_AGREE_STATE.AGREE;
        if (ES_Save.Exists(Constant.PATH_AGREE_TERM)) {
            newAgreeTerm = ES_Save.Load<long>(Constant.PATH_AGREE_TERM);
            UserDataModel.instance.userProfile.agreeTerm = newAgreeTerm;
        }

        long newAgreePrivacy = (long)POLICY_AGREE_STATE.AGREE;
        if (ES_Save.Exists(Constant.PATH_AGREE_PRIVACY)) {
            newAgreePrivacy = ES_Save.Load<long>(Constant.PATH_AGREE_PRIVACY);
            UserDataModel.instance.userProfile.agreePrivacy = newAgreePrivacy;
        }

        long newAgreeNightPush = (long)POLICY_AGREE_STATE.AGREE;
        if (ES_Save.Exists(Constant.PATH_AGREE_NIGHT_PUSH)) {
            newAgreeNightPush = ES_Save.Load<long>(Constant.PATH_AGREE_NIGHT_PUSH);
            UserDataModel.instance.userProfile.agreeNightPush = newAgreeNightPush;
        }

        long forceRefreshPush = 1;
        if (ES_Save.Exists(Constant.PATH_FORCE_REFRESH_PUSH))
            forceRefreshPush = ES_Save.Load<long>(Constant.PATH_FORCE_REFRESH_PUSH);

        if (forceRefreshPush == 1) {
            forceRefreshPush = 0;
            ES_Save.Save(forceRefreshPush, Constant.PATH_FORCE_REFRESH_PUSH);
            if (UserDataModel.instance.userProfile.agreeNightPush == (long)POLICY_AGREE_STATE.DISAGREE)
                BackendRequest.instance.ReqPushOff(null);
            else 
                BackendRequest.instance.ReqPushOn(null);    

            UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.USER_PROFILE);
        }
        
        if (successCallback != null)
            successCallback();
    }

    public void ReqSetNoticeIndate(Callback successCallback = null) {
        if (UserDataModel.instance.noticeInfos.Count == 0)
            return;

        UserDataModel.instance.statistics.lastNoticeIndate = UserDataModel.instance.noticeInfos[0].inDate;
        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.STATISTICS);
        if (successCallback != null)
            successCallback();
    }

    public void ReqGetAchievementRewardAll() {
        long getGoldAmount = 0;
        long getDiamondAmount = 0;
        List<long> achievementIDs = new List<long>();

        for (ACHIEVEMENT_GROUP group = ACHIEVEMENT_GROUP.START + 1; group < ACHIEVEMENT_GROUP.END; group++) {
            List<GameData.AchievementDTO> achievementDatas = AchievementUtil.GetSelectedAchievementDatas(group);
            if (UserDataModel.instance.statistics.receivedAchievementIDs.Contains(achievementDatas[0].achievementID) ||
                UserDataModel.instance.statistics.clearedAchievementIDs.Contains(achievementDatas[0].achievementID) == false)
                continue;

            achievementIDs.Add(achievementDatas[0].achievementID);

            if (achievementDatas[0].rewardType == (long)ACHIEVEMENT_REWARD_TYPE.GOLD) {
                long amount = achievementDatas[0].rewardValue;
                UserDataModel.instance.AddGold(amount);
                getGoldAmount += amount;
            }
            
            else if (achievementDatas[0].rewardType == (long)ACHIEVEMENT_REWARD_TYPE.DIAMOND) {
                long amount = achievementDatas[0].rewardValue;
                UserDataModel.instance.AddDiamond(achievementDatas[0].rewardValue);
                getDiamondAmount += amount;
            }

            UserDataModel.instance.statistics.clearedAchievementIDs.Remove(achievementDatas[0].achievementID);

            ACHIEVEMENT_REPEAT repeat = (ACHIEVEMENT_REPEAT)achievementDatas[0].repeat;
            if (repeat == ACHIEVEMENT_REPEAT.NONE ||
                repeat == ACHIEVEMENT_REPEAT.DAILY) {
                UserDataModel.instance.statistics.receivedAchievementIDs.Add(achievementDatas[0].achievementID);
            }
            //반복퀘스트인경우 보상수령 완료 목록에 등록하지 않는다.
            else if (repeat == ACHIEVEMENT_REPEAT.IMMEDIATELY_INCREASE) {
                long nextLevel = UserDataModel.instance.GetAchievementLevel(achievementDatas[0].achievementID) + 1;
                long nextLevelValue = AchievementUtil.GetComparingValue(achievementDatas[0], nextLevel);
                if (nextLevelValue <= achievementDatas[0].valueMax)
                    UserDataModel.instance.IncreaseAchievementLevel(achievementDatas[0].achievementID);
                //업적 최대레벨에 도달한 경우 완료처리 
                else
                    UserDataModel.instance.statistics.receivedAchievementIDs.Add(achievementDatas[0].achievementID);
            }

            //INSTANT valueType인 경우 업적 완료 즉시 리셋
            foreach (GameData.AchievementDTO achievementData in achievementDatas) {
                if (achievementData.valueType > (long)STATISTICS_TYPE.INSTANT_START &&
                    achievementData.valueType < (long)STATISTICS_TYPE.INSTANT_END)
                    UserDataModel.instance.ResetInstantAchievementCount(achievementData.valueType);
            }
        }

        if (getGoldAmount == 0 && getDiamondAmount == 0)
            return;

        if (getGoldAmount > 0 && getDiamondAmount == 0) 
            EffectManager.instance.ShowGetGoldEffect(getGoldAmount);
        else if (getGoldAmount == 0 && getDiamondAmount > 0) 
            EffectManager.instance.ShowGetDiamondEffect(getDiamondAmount);
        else 
            EffectManager.instance.ShowGetMultipleGoodsEffect(getGoldAmount, getDiamondAmount);

        UserDataModel.instance.SaveUserDatas(true,
                                            USER_DATA_KEY.USER_PROFILE,
                                            USER_DATA_KEY.STATISTICS);

        AchievementManager.instance.CheckAchievement();
        EventManager.Notify(EventEnum.GoodsUpdate);

        //@todo : 보상 획득연출 표시, Callback에서 보상연출 꺼주기
        BackendRequest.instance.ReqSyncUserData(false, false,
            () => {
                foreach(long achievementID in achievementIDs) {
                    AnalyticsUtil.LogGetAchievementReward(achievementID);
                }
                EventManager.Notify(EventEnum.AchievementGetRewardAllComplete);
            });
    }

    public void ReqGetStartDiamond() {
        UserDataModel.instance.statistics.getStartDiamonds = 1;
        UserDataModel.instance.AddDiamond(Constant.START_DIAMOND);

        UserDataModel.instance.SaveUserDatas(true,
                                            USER_DATA_KEY.USER_PROFILE,
                                            USER_DATA_KEY.STATISTICS);

        EventManager.Notify(EventEnum.GoodsUpdate);
    }

    public void ReqAgreePush(long agreePush, Callback successCallback = null) {
        UserDataModel.instance.userProfile.agreeNightPush = agreePush;

        Callback callback = () => {
            string msg;
            if (agreePush == (long)POLICY_AGREE_STATE.DISAGREE)
                msg = TermModel.instance.GetTerm("msg_push_disabled");
            else
                msg = TermModel.instance.GetTerm("msg_push_enabled");
            MessageUtil.ShowSimpleWarning(msg);
        };

        if (agreePush == (long)POLICY_AGREE_STATE.DISAGREE) 
            BackendRequest.instance.ReqPushOff(callback);
        else 
            BackendRequest.instance.ReqPushOn(callback);

        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.USER_PROFILE);

        if (successCallback != null)
            successCallback();
    }

    public void ReqChangeFrame(long frameID, Callback successCallback = null) {
        if (UserDataModel.instance.publicUserData.profileFrameNo == frameID)
            return;

        UserDataModel.instance.publicUserData.profileFrameNo = frameID;
        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.PUBLIC_USER_DATAS);
        EventManager.Notify(EventEnum.ProfileSkinChanged);
        if (UserDataModel.instance.leagueScoreInfo.leagueID > 0 && UserDataModel.instance.leagueScoreInfo.leagueScore > 0)
            BackendRequest.instance.ReqUpdateLeagueScore(UserDataModel.instance.leagueScoreInfo.leagueScore, successCallback);
        else if (successCallback != null)
            successCallback();
    }

    public void ReqChangeProfile(long profileID, Callback successCallback = null) {
        if (UserDataModel.instance.publicUserData.profileNo == profileID)
            return;

        UserDataModel.instance.publicUserData.profileNo = profileID;
        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.PUBLIC_USER_DATAS);

        EventManager.Notify(EventEnum.ProfileSkinChanged);
        if (UserDataModel.instance.leagueScoreInfo.leagueID > 0 && UserDataModel.instance.leagueScoreInfo.leagueScore > 0)
            BackendRequest.instance.ReqUpdateLeagueScore(UserDataModel.instance.leagueScoreInfo.leagueScore, successCallback);
        else if (successCallback != null)
            successCallback();
    }
}
