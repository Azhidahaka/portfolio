using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using QuantumTek.EncryptedSave;
using System.Collections.Generic;
using UnityEngine;
using UserData;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

public class UserDataModel : MonoBehaviour {
    public static UserDataModel instance;

    public Dictionary<string, TableVersionInfoDTO> dicTableVersionInfo = new Dictionary<string, TableVersionInfoDTO>();

    public UserProfileDTO userProfile = new UserProfileDTO();
    public StatisticsDTO statistics = new StatisticsDTO();
    public LastSelectedInfoDTO lastSelectedInfos = new LastSelectedInfoDTO();

    public List<MailInfoDTO> mailInfos = new List<MailInfoDTO>();
    public PublicUserDataDTO publicUserData = new PublicUserDataDTO();

    //로컬에만 저장
    public GameOptionDTO gameOptions = new GameOptionDTO();
    public RefereeNoteDTO refereeNote = new RefereeNoteDTO();

    private bool revisionChanged = false;
    private bool continueGame = false;

    public List<ChallengeRankDTO> leagueRankInfos = new List<ChallengeRankDTO>();
    public List<ChallengeRankDTO> top3RankInfos = new List<ChallengeRankDTO>();

    public ChallengeRankDTO myChallengeRankInfo;
    public List<NoticeDTO> noticeInfos = new List<NoticeDTO>();

    public List<SimpleUserDTO> friendRecommands = new List<SimpleUserDTO>();
    public List<SimpleUserDTO> friends = new List<SimpleUserDTO>();

    public bool getFriendList = false;

    public List<SimpleUserDTO> friendReceives = new List<SimpleUserDTO>();
    public List<SimpleUserDTO> findUserInfos = new List<SimpleUserDTO>();

    public LeagueScoreDTO leagueScoreInfo = new LeagueScoreDTO();
    public List<LeagueDataDTO> leagueInfos = new List<LeagueDataDTO>();

    public long myLeagueUserCount;

    public MyRankInfo myRankInfo;

    public ChallengeMsgDTO challengeMsgInfo = new ChallengeMsgDTO();
    public List<FriendMessageDTO> receivedFriendMessages = new List<FriendMessageDTO>();

    private FriendMessageDTO lastFriendMessage;

    public FriendMessageDTO LastFriendMessage {
        get {
            if (lastFriendMessage == null)
                lastFriendMessage = new FriendMessageDTO();
            return lastFriendMessage;
        }
        set {
            //원본 삭제여부에 영향을 받지 않기위해 Json으로 변환한 뒤 재할당한다.
            string lasgFriendMessageStr = JsonConvert.SerializeObject(value);
            lastFriendMessage = JsonConvert.DeserializeObject<FriendMessageDTO>(lasgFriendMessageStr);
        }
    }

    public bool ContinueGame {
        get {
            return continueGame;
        }
        set {
            continueGame = value;
        }
    }

    public long DailyAdsViewCount {
        get {
            if (userProfile.lastAdsReset == 0 ||
                Common.GetUTCDateZero(userProfile.lastAdsReset) < Common.GetUTCTodayZero()) {
                userProfile.lastAdsReset = Common.GetUTCNow();
                userProfile.dailyAdsViewCount = 0;
            }

            return userProfile.dailyAdsViewCount;
        }
        set {
            userProfile.dailyAdsViewCount = value;
        }
    }

    public long LastSelectedStageLevel {
        set {
            if (value <= 0)
                lastSelectedInfos.stageLevel = GameDataModel.instance.stages[0].level;
            else
                lastSelectedInfos.stageLevel = value;
        }

        get {
            if (lastSelectedInfos.stageLevel <= 0)
                lastSelectedInfos.stageLevel = GameDataModel.instance.stages[0].level;

            return lastSelectedInfos.stageLevel;
        }
    }

    public long SkinID {
        set {
            if (value <= 0)
                lastSelectedInfos.skinID = 1;
            else
                lastSelectedInfos.skinID = value;

            ES_Save.Save(lastSelectedInfos.skinID, Constant.SKIN_ID_PATH);
        }

        get {
            if (lastSelectedInfos.skinID <= 0) {
                string path = Constant.SKIN_ID_PATH;
                if (ES_Save.Exists(path)) {
                    long skinID = ES_Save.Load<long>(path);
                    if (skinID > 0)
                        return skinID;
                }

                lastSelectedInfos.skinID = 1;
            }

            return lastSelectedInfos.skinID;
        }
    }

    public long BankSkinID {
        set {
            if (value <= 0)
                lastSelectedInfos.bankSkinID = userProfile.piggyBankType;
            else
                lastSelectedInfos.bankSkinID = value;
        }

        get {
            if (lastSelectedInfos.bankSkinID <= 0) {
                lastSelectedInfos.bankSkinID = userProfile.piggyBankType;
            }

            return lastSelectedInfos.bankSkinID;
        }
    }

    public bool RevisionChanged {
        set {
            revisionChanged = value;
        }
        get {
            return revisionChanged;
        }
    }

    private void Awake() {
        instance = this;
    }

    private void OnDestroy() {
        instance = null;
    }


    // revision은 제외
    public void SaveUserDatas(bool updateVersion = true, params USER_DATA_KEY[] keys) {
        //키가 따로 지정되지 않았으면 모든 데이터 저장
        if (keys.Length == 0) {
            for (USER_DATA_KEY key = USER_DATA_KEY.TABLE_VERSION_INFO + 1; key < USER_DATA_KEY.SERVER_SYNC_END; key++) {
                SaveUserData(key);
            }
        }
        else {
            foreach (USER_DATA_KEY key in keys) {
                SaveUserData(key);
            }
        }

        if (updateVersion)
            UpdateRevision(keys);
    }

    private void UpdateRevision(params USER_DATA_KEY[] keys) {
        bool updateUserDatas = false;
        bool updateMailInfos = false;
        bool updatePublicUserDatas = false;
        bool updateLeagueScores = false;
        for (int i = 0; i < keys.Length; i++) {
            switch (keys[i]) {
                case USER_DATA_KEY.LAST_SELECTED_INFOS:
                case USER_DATA_KEY.STATISTICS:
                case USER_DATA_KEY.USER_PROFILE:
                    updateUserDatas = true;
                    break;

                case USER_DATA_KEY.MAIL_INFOS:
                    updateMailInfos = true;
                    break;

                case USER_DATA_KEY.PUBLIC_USER_DATAS:
                    updatePublicUserDatas = true;
                    break;

                case USER_DATA_KEY.LEAGUE_SCORE:
                    updateLeagueScores = true;
                    break;

                default:
                    break;
            }
        }

        if (updateUserDatas) {
            UpdateTableVersionInfo(USER_DATA_TABLE_NAME.userDatas, Common.GetUnixTimeNowDouble());
            revisionChanged = true;
        }

        if (updateMailInfos) {
            UpdateTableVersionInfo(USER_DATA_TABLE_NAME.mails, Common.GetUnixTimeNowDouble());
            revisionChanged = true;
        }

        if (updatePublicUserDatas) {
            UpdateTableVersionInfo(USER_DATA_TABLE_NAME.publicUserDatas, Common.GetUnixTimeNowDouble());
            revisionChanged = true;
        }

        if (updateLeagueScores) {
            UpdateTableVersionInfo(USER_DATA_TABLE_NAME.leagueScores, Common.GetUnixTimeNowDouble());
            revisionChanged = true;
        }

        SaveUserData(USER_DATA_KEY.TABLE_VERSION_INFO);
    }

    public void SaveUserData(USER_DATA_KEY key) {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;


        string path = GetUserDataPath(key);
        switch (key) {
            case USER_DATA_KEY.TABLE_VERSION_INFO:
                //데이터 저장시간 기록
                string tableVersionInfoPath = GetUserDataPath(USER_DATA_KEY.TABLE_VERSION_INFO);
                ES_Save.Save(JsonConvert.SerializeObject(dicTableVersionInfo, settings), tableVersionInfoPath);
                break;

            case USER_DATA_KEY.USER_PROFILE:
                ES_Save.Save(JsonConvert.SerializeObject(userProfile, settings), path);
                break;

            case USER_DATA_KEY.STATISTICS:
                ES_Save.Save(JsonConvert.SerializeObject(statistics, settings), path);
                break;

            case USER_DATA_KEY.LAST_SELECTED_INFOS:
                ES_Save.Save(JsonConvert.SerializeObject(lastSelectedInfos, settings), path);
                break;

            case USER_DATA_KEY.GAME_OPTIONS:
                ES_Save.Save(JsonConvert.SerializeObject(gameOptions, settings), path);
                break;

            case USER_DATA_KEY.NOTICE_INFOS:
                ES_Save.Save(JsonConvert.SerializeObject(noticeInfos, settings), path);
                break;

            case USER_DATA_KEY.REFEREE_NOTE:
                string refereeNoteStr = JsonConvert.SerializeObject(refereeNote, settings);
                ES_Save.Save(refereeNoteStr, path);
                break;

            case USER_DATA_KEY.MAIL_INFOS:
                ES_Save.Save(JsonConvert.SerializeObject(mailInfos, settings), path);
                break;

            case USER_DATA_KEY.PUBLIC_USER_DATAS:
                ES_Save.Save(JsonConvert.SerializeObject(publicUserData, settings), path);
                break;

            case USER_DATA_KEY.LEAGUE_SCORE:
                ES_Save.Save(JsonConvert.SerializeObject(leagueScoreInfo, settings), path);
                break;
        }
    }

    public void LoadLocalUserDatas() {
        dicTableVersionInfo = JsonConvert.DeserializeObject<Dictionary<string, TableVersionInfoDTO>>(LoadLocalUserData(USER_DATA_KEY.TABLE_VERSION_INFO));
        if (dicTableVersionInfo == null)
            dicTableVersionInfo = new Dictionary<string, TableVersionInfoDTO>();

        userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(LoadLocalUserData(USER_DATA_KEY.USER_PROFILE));
        if (userProfile == null) {
            userProfile = new UserProfileDTO();
            userProfile.SetGoldChangedTime();
            userProfile.SetLoginUtcZero();
        }

        statistics = JsonConvert.DeserializeObject<StatisticsDTO>(LoadLocalUserData(USER_DATA_KEY.STATISTICS));
        if (statistics == null)
            statistics = new StatisticsDTO();

        lastSelectedInfos = JsonConvert.DeserializeObject<LastSelectedInfoDTO>(LoadLocalUserData(USER_DATA_KEY.LAST_SELECTED_INFOS));
        if (lastSelectedInfos == null)
            lastSelectedInfos = new LastSelectedInfoDTO();
        EventManager.Notify(EventEnum.SkinChanged);

        gameOptions = JsonConvert.DeserializeObject<GameOptionDTO>(LoadLocalUserData(USER_DATA_KEY.GAME_OPTIONS));
        if (gameOptions == null) {
            gameOptions = new GameOptionDTO();
            gameOptions.SetDefaultLanguage();
        }
        EventManager.Notify(EventEnum.UpdateVolume);

        noticeInfos = JsonConvert.DeserializeObject<List<NoticeDTO>>(LoadLocalUserData(USER_DATA_KEY.NOTICE_INFOS));
        if (noticeInfos == null)
            noticeInfos = new List<NoticeDTO>();

        mailInfos = JsonConvert.DeserializeObject<List<MailInfoDTO>>(LoadLocalUserData(USER_DATA_KEY.MAIL_INFOS));
        if (mailInfos == null)
            mailInfos = new List<MailInfoDTO>();

        publicUserData = JsonConvert.DeserializeObject<PublicUserDataDTO>(LoadLocalUserData(USER_DATA_KEY.PUBLIC_USER_DATAS));
        if (publicUserData == null)
            publicUserData = new PublicUserDataDTO();

        leagueScoreInfo = JsonConvert.DeserializeObject<LeagueScoreDTO>(LoadLocalUserData(USER_DATA_KEY.LEAGUE_SCORE));
        if (leagueScoreInfo == null)
            leagueScoreInfo = new LeagueScoreDTO();
    }

    private string LoadLocalUserData(USER_DATA_KEY key) {
        string path = GetUserDataPath(key);
        if (ES_Save.Exists(path)) {
            Debug.Log("LoadLocalUserData::path = " + path);
            return ES_Save.Load<string>(path);
        }
        return "";
    }

    private long LoadLocalUserDataLong(USER_DATA_KEY key) {
        string path = GetUserDataPath(key);
        if (ES_Save.Exists(path))
            return ES_Save.Load<long>(path);
        return 0;
    }

    public string GetUserDataPath(USER_DATA_KEY key) {
        string format = "_UserData_{0}_{1}";
        string inDate = BackendLogin.instance.UserInDate;

        DateTime parsedDate = DateTime.Parse(inDate);
        TimeSpan timeSpan = ( parsedDate - new DateTime(1970, 1, 1, 0, 0, 0) );
        string replacedIndate = Common.ConvertTimestampToString(timeSpan.TotalSeconds);

        string path = string.Format(format, replacedIndate, key.ToString());
        return path;
    }

    public void ResetUserDatas() {
        for (USER_DATA_KEY key = USER_DATA_KEY.USER_PROFILE; key < USER_DATA_KEY.SERVER_SYNC_END; key++) {
            ResetUserData(key);
        }
    }

    private void ResetUserData(USER_DATA_KEY key) {
        string path = GetUserDataPath(key);
        if (ES_Save.Exists(path))
            ES_Save.DeleteData(path);
    }

    public void RemoveLocalUserDatas() {
        for (USER_DATA_KEY key = USER_DATA_KEY.TABLE_VERSION_INFO; key < USER_DATA_KEY.SERVER_SYNC_END; key++) {
            RemoveLocalUserData(key);
        }
    }

    private void RemoveLocalUserData(USER_DATA_KEY key) {
        string path = GetUserDataPath(key);
        if (ES_Save.Exists(path))
            ES_Save.DeleteData(path);
    }

    public void AddTableVersionInfo(USER_DATA_TABLE_NAME tableName, string inDate, double updatedAt) {
        string key = tableName.ToString();
        TableVersionInfoDTO tableVersionInfo = new TableVersionInfoDTO();
        tableVersionInfo.inDate = inDate;
        tableVersionInfo.updatedAt = updatedAt;
        dicTableVersionInfo.Add(key, tableVersionInfo);
    }

    public void UpdateTableVersionInfo(USER_DATA_TABLE_NAME tableName, double updatedAt) {
        string key = tableName.ToString();
        if (dicTableVersionInfo.ContainsKey(key)) {
            dicTableVersionInfo[key].updatedAt = updatedAt;
        }
    }

    public TableVersionInfoDTO GetTableVersionInfo(USER_DATA_TABLE_NAME tableName) {
        string key = tableName.ToString();
        if (dicTableVersionInfo.ContainsKey(key))
            return dicTableVersionInfo[key];
        Debug.LogError("GetTableVersionInfo::" + key);
        return null;
    }

    public string GetUserDataStr(USER_DATA_TABLE_NAME table) {
        switch (table) {
            case USER_DATA_TABLE_NAME.userDatas:
                return GetUserDataStr();

            case USER_DATA_TABLE_NAME.mails: {
                return JsonConvert.SerializeObject(mailInfos);
            }
        }

        return string.Empty;
    }

    private string GetUserDataStr() {
        Dictionary<string, object> userDataDic = new Dictionary<string, object>();
        userDataDic.Add("userProfile", userProfile);
        userDataDic.Add("statistics", statistics);
        userDataDic.Add("lastSelectedInfos", lastSelectedInfos);

        string userDataStr = JsonConvert.SerializeObject(userDataDic);

        return userDataStr;
    }

    public void UpdateUserData(USER_DATA_TABLE_NAME tableName, object data) {
        switch (tableName) {
            case USER_DATA_TABLE_NAME.userDatas: {
                string dataStr = data.ToString();
                Dictionary<string, object> userDataDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataStr);

                string userProfileKey = "userProfile";
                string statisticsKey = "statistics";
                string lastSelectedInfosKey = "lastSelectedInfos";

                if (userDataDic.ContainsKey(userProfileKey)) {
                    userProfile = JsonConvert.DeserializeObject<UserProfileDTO>(userDataDic[userProfileKey].ToString());
                }

                if (userDataDic.ContainsKey(statisticsKey)) {
                    statistics = JsonConvert.DeserializeObject<StatisticsDTO>(userDataDic[statisticsKey].ToString());
                }

                if (userDataDic.ContainsKey(lastSelectedInfosKey)) {
                    lastSelectedInfos = JsonConvert.DeserializeObject<LastSelectedInfoDTO>(userDataDic[lastSelectedInfosKey].ToString());
                }
                return;
            }
            case USER_DATA_TABLE_NAME.mails: {
                string dataStr = data.ToString();
                mailInfos = JsonConvert.DeserializeObject<List<MailInfoDTO>>(dataStr);
                return;
            }
            case USER_DATA_TABLE_NAME.publicUserDatas: {
                publicUserData = data as PublicUserDataDTO;
                return;
            }
            case USER_DATA_TABLE_NAME.leagueScores: {
                leagueScoreInfo = data as LeagueScoreDTO;
                return;
            }
        }
    }

    public void AddGold(long increaseAmount) {
        userProfile.gold += increaseAmount;
        SetAchievementCount(STATISTICS_TYPE.ACC_GET_GOLD, increaseAmount);
    }

    public void UseGold(long useAmount) {
        userProfile.gold -= useAmount;
        if (userProfile.gold < 0)
            userProfile.gold = 0;
        SetAchievementCount(STATISTICS_TYPE.ACC_USE_GOLD, useAmount);
    }

    public void AddDiamond(long increaseAmount) {
        userProfile.diamond += increaseAmount;
    }

    public void UseDiamond(long useAmount) {
        userProfile.diamond -= useAmount;
        if (userProfile.diamond < 0)
            userProfile.diamond = 0;
        SetAchievementCount(STATISTICS_TYPE.ACC_USE_DIAMOND, useAmount);
        SetAchievementCount(STATISTICS_TYPE.USE_DIAMOND, useAmount);
    }

    public bool IsSkinExist(long skinID) {
        GameData.ProductDTO productData = GameDataModel.instance.GetProductDataBySkinID(skinID);

        if (productData == null)
            Debug.LogError($"패키지 없음. 스킨ID = {skinID}");

        if (IsNonconsumableExist(productData.packageID) ||
            userProfile.skinIDs.Contains(skinID))
            return true;
        return false;
    }

    public bool IsPiggyBankSkinExist(long bankSkinID) {
        GameData.ProductDTO productData = GameDataModel.instance.GetProductDataByBankSkinID(bankSkinID);

        if (productData == null)
            Debug.LogError($"패키지 없음. 저금통 스킨ID = {bankSkinID}");

        if (IsNonconsumableExist(productData.packageID) ||
            userProfile.bankSkinIDs.Contains(bankSkinID))
            return true;
        return false;
    }

    public void AddNonconsumable(long packageID) {
        if (IsNonconsumableExist(packageID))
            return;
        userProfile.nonconsumableIDs.Add(packageID);
    }

    public void AddSkin(long skinID) {
        if (userProfile.skinIDs.Contains(skinID) == false)
            userProfile.skinIDs.Add(skinID);
    }

    public void AddBankSkin(long bankSkinID) {
        if (userProfile.bankSkinIDs.Contains(bankSkinID) == false)
            userProfile.bankSkinIDs.Add(bankSkinID);
    }

    public bool IsNonconsumableExist(long packageID) {
        if (userProfile.nonconsumableIDs.Contains(packageID))
            return true;
        return false;
    }

    public bool IsAdsRemoved() {
        GameData.PackageDTO packageData = GameDataModel.instance.GetRemoveAdsPackageData();
        if (packageData == null)
            return false;

        return IsNonconsumableExist(packageData.packageID);
    }

    public void SaveRefereeNote(RefereeNoteDTO refereeNote) {
        //리그전일때는 데이터를 저장하지 않는다.
        if (refereeNote.stageLevel == (long)STAGE_LEVEL.LeagueBronze ||
            refereeNote.stageLevel == (long)STAGE_LEVEL.LeagueSilver)
            return;
        this.refereeNote = refereeNote;
    }

    public RefereeNoteDTO LoadRefereeNote() {
        string refereeNoteStr = LoadLocalUserData(USER_DATA_KEY.REFEREE_NOTE);
        refereeNote = JsonConvert.DeserializeObject<RefereeNoteDTO>(refereeNoteStr);
        if (refereeNote != null && refereeNote.stageLevel == 0)
            return null;
        return refereeNote;
    }

    public void RemoveRefereeNote() {
        string path = GetUserDataPath(USER_DATA_KEY.REFEREE_NOTE);
        if (ES_Save.Exists(path)) {
            Debug.Log("RemoveRefereeNote::path = " + path);
            ES_Save.DeleteData(path);
        }
    }

    public void IncreaseGoldByTurn() {
        //userProfile.collectedGold
        GameData.PiggyBankDTO piggyBankData = GameDataModel.instance.GetPiggyBankData(userProfile.piggyBankType);
        userProfile.collectedGold = PiggyBankUtil.GetCurrentGold(piggyBankData) + piggyBankData.turnValue;
        if (userProfile.collectedGold > piggyBankData.capacity)
            userProfile.collectedGold = piggyBankData.capacity;

        userProfile.collectedChangedTime = Common.GetUnixTimeNow();
    }

    public List<SingleRankDTO> GetSingleRankInfos(long stageLevel) {
        if (statistics.dicSingleRanks.ContainsKey(stageLevel))
            return statistics.dicSingleRanks[stageLevel];

        List<SingleRankDTO> singleRanks = new List<SingleRankDTO>();
        statistics.dicSingleRanks.Add(stageLevel, singleRanks);

        return singleRanks;
    }

    public void SetSingleRanks(long stageLevel, List<SingleRankDTO> singleRanks) {
        if (statistics.dicSingleRanks.ContainsKey(stageLevel))
            statistics.dicSingleRanks[stageLevel] = singleRanks;
        else
            statistics.dicSingleRanks.Add(stageLevel, singleRanks);
    }

    public void SetAchievementCount(STATISTICS_TYPE valueType, long value, bool checkAchievement = true) {
        //ETC이면 value를 셋팅
        if (valueType > STATISTICS_TYPE.ETC_START &&
            valueType < STATISTICS_TYPE.ETC_END) {
            DictionaryUtil.SetValue(DictionaryUtil.SET_TYPE.SET, statistics.dicEtcRecord, (long)valueType, value);
        }
        else if (valueType > STATISTICS_TYPE.INSTANT_START &&
                 valueType < STATISTICS_TYPE.INSTANT_END) {
            DictionaryUtil.SetValue(DictionaryUtil.SET_TYPE.INCREASE, statistics.dicInstantRecord, (long)valueType, value);
        }
        else if (valueType > STATISTICS_TYPE.ACC_START &&
                 valueType < STATISTICS_TYPE.ACC_END) {
            DictionaryUtil.SetValue(DictionaryUtil.SET_TYPE.INCREASE, statistics.dicAccRecord, (long)valueType, value);
        }
        else if (valueType > STATISTICS_TYPE.DAILY_START &&
                 valueType < STATISTICS_TYPE.DAILY_END) {
            DictionaryUtil.SetValue(DictionaryUtil.SET_TYPE.INCREASE, statistics.dicDailyRecord, (long)valueType, value);
        }
        else if (valueType > STATISTICS_TYPE.ROUND_START &&
                 valueType < STATISTICS_TYPE.ROUND_END) {
            DictionaryUtil.SetValue(DictionaryUtil.SET_TYPE.INCREASE, statistics.dicRoundRecord, (long)valueType, value);
        }
        else {
            Debug.LogError($"알수없는 타입::{valueType}");
        }

        if (checkAchievement)
            AchievementManager.instance.CheckAchievement();
    }

    public void ResetInstantAchievementCount(long valueType) {
        if (statistics.dicInstantRecord.ContainsKey(valueType) &&
            valueType > (long)STATISTICS_TYPE.INSTANT_START &&
            valueType < (long)STATISTICS_TYPE.INSTANT_END) {
            statistics.dicInstantRecord.Remove(valueType);
        }
    }

    public long GetStatistics(STATISTICS_TYPE valueType) {
        //ETC이면 value를 셋팅
        if (valueType > STATISTICS_TYPE.ETC_START &&
            valueType < STATISTICS_TYPE.ETC_END) {
            return DictionaryUtil.GetValue(statistics.dicEtcRecord, (long)valueType);
        }
        else if (valueType > STATISTICS_TYPE.INSTANT_START &&
                 valueType < STATISTICS_TYPE.INSTANT_END) {
            return DictionaryUtil.GetValue(statistics.dicInstantRecord, (long)valueType);
        }
        else if (valueType > STATISTICS_TYPE.ACC_START &&
                 valueType < STATISTICS_TYPE.ACC_END) {
            return DictionaryUtil.GetValue(statistics.dicAccRecord, (long)valueType);
        }
        else if (valueType > STATISTICS_TYPE.DAILY_START &&
                 valueType < STATISTICS_TYPE.DAILY_END) {
            return DictionaryUtil.GetValue(statistics.dicDailyRecord, (long)valueType);
        }
        else if (valueType > STATISTICS_TYPE.ROUND_START &&
                 valueType < STATISTICS_TYPE.ROUND_END) {
            return DictionaryUtil.GetValue(statistics.dicRoundRecord, (long)valueType);
        }
        else {
            Debug.LogError($"알수없는 타입::{valueType}");
            return 0;
        }
    }

    public void IncreaseAchievementLevel(long achievementID) {
        if (statistics.dicAchievementLevel.ContainsKey(achievementID))
            statistics.dicAchievementLevel[achievementID]++;
        else
            statistics.dicAchievementLevel.Add(achievementID, 1);
    }

    public long GetAchievementLevel(long achievementID) {
        if (statistics.dicAchievementLevel.ContainsKey(achievementID))
            return statistics.dicAchievementLevel[achievementID];

        return 0;
    }

    public void SetAchievementCleared(long achievementID) {
        if (statistics.clearedAchievementIDs.Contains(achievementID))
            return;
        statistics.clearedAchievementIDs.Add(achievementID);
    }

    public void AddChallengeTicket(long count) {
        userProfile.challengeTicket += count;
        //if (userProfile.challengeTicket > Constant.CHALLENGE_TICKET_MAX)
        //userProfile.challengeTicket = Constant.CHALLENGE_TICKET_MAX;
    }

    public void UseChallengeTicket(long count) {
        userProfile.challengeTicket -= count;
        if (userProfile.challengeTicket < 0)
            userProfile.challengeTicket = 0;
    }

    public bool AddMail(MailInfoDTO mailInfo) {
        bool changed = false;

        if (mailInfo.no == 0)
            mailInfo.no = Common.GetUnixTimeNowDouble();

        for (int i = 0; i < mailInfos.Count; i++) {
            //이미 존재하는 no이면 추가하지 않음
            if (mailInfos[i].no == mailInfo.no)
                return changed;
        }

        if (mailInfo.rewards != null &&
            mailInfo.rewards.Count > 0 &&
            mailInfo.rewards[0].type == (long)MAIL_REWARD_TYPE.PACKAGE) {
            MailRewardDTO rewardInfo = mailInfo.rewards[0];

            List<GameData.PackageDTO> packageDatas = GameDataModel.instance.GetPackageDatas(rewardInfo.count);
            for (int i = 0; i < packageDatas.Count; i++) {
                GameData.PackageDTO packageData = packageDatas[i];

                MailInfoDTO newMailInfo = new MailInfoDTO(mailInfo);
                MailRewardDTO newRewardInfo = new MailRewardDTO();
                if (packageDatas[i].type == (long)PACKAGE_TYPE.GOLD) 
                    newRewardInfo.type = (long)MAIL_REWARD_TYPE.GOLD;
                else if (packageDatas[i].type == (long)PACKAGE_TYPE.DIAMOND) 
                    newRewardInfo.type = (long)MAIL_REWARD_TYPE.DIAMOND;
                else if (packageDatas[i].type == (long)PACKAGE_TYPE.SKIN) 
                    newRewardInfo.type = (long)MAIL_REWARD_TYPE.SKIN;
                else if (packageDatas[i].type == (long)PACKAGE_TYPE.CHALLENGE_TICKET) 
                    newRewardInfo.type = (long)MAIL_REWARD_TYPE.CHALLENGE_TICKET;
                else if (packageDatas[i].type == (long)PACKAGE_TYPE.PIGGYBANK_SKIN) 
                    newRewardInfo.type = (long)MAIL_REWARD_TYPE.PIGGYBANK_SKIN;

                newMailInfo.partNo = i;
                newRewardInfo.count = packageData.value;
                newMailInfo.rewards.Add(newRewardInfo);
                mailInfos.Add(newMailInfo);
            }
        }
        else
            mailInfos.Add(mailInfo);
        changed = true;
        return changed;
    }

    public void ConfirmMail(double mailNo, long partNo, bool getItem = false) {
        if (mailInfos == null || mailInfos.Count == 0)
            return;

        for (int i = 0; i < mailInfos.Count; i++) {
            if (mailInfos[i].no == mailNo && mailInfos[i].partNo == partNo) {
                mailInfos[i].read = true;
                if (getItem)
                    mailInfos[i].received = true;

                break;
            }
        }
    }

    public MailInfoDTO GetMailInfo(double mailNo) {
        if (mailInfos == null || mailInfos.Count == 0)
            return null;

        for (int i = 0; i < mailInfos.Count; i++) {
            if (mailInfos[i].no == mailNo)
                return mailInfos[i];
        }

        return null;
    }

    public void RemoveMail(double mailNo) {
        if (mailInfos == null || mailInfos.Count == 0)
            return;

        for (int i = 0; i < mailInfos.Count; i++) {
            if (mailInfos[i].no == mailNo) {
                mailInfos.Remove(mailInfos[i]);
                break;
            }
        }
    }

    public void RemoveMail(MailInfoDTO mailInfo) {
        if (mailInfos == null || mailInfos.Count == 0)
            return;

        for (int i = 0; i < mailInfos.Count; i++) {
            if (mailInfos[i].no == mailInfo.no && mailInfos[i].partNo == mailInfo.partNo) {
                mailInfos.Remove(mailInfos[i]);
                break;
            }
        }
    }

    public void ConfirmAllMails() {
        if (mailInfos == null || mailInfos.Count == 0)
            return;

        for (int i = 0; i < mailInfos.Count; i++) {
            ConfirmMail(mailInfos[i].no, mailInfos[i].partNo, true);
        }
    }

    public List<string> RemoveNotices(List<string> exceptUUIDs) {
        List<string> removedUUIDS = new List<string>();
        int index = 0;

        while (index < noticeInfos.Count) {
            if (exceptUUIDs.Contains(noticeInfos[index].uuid) == false) {
                string uuid = noticeInfos[index].uuid;
                noticeInfos.RemoveAt(index);
                removedUUIDS.Add(uuid);
            }
            else
                ++index;
        }

        return removedUUIDS;
    }

    public NoticeDTO GetNoticeInfo(string uuid) {
        if (noticeInfos == null || noticeInfos.Count == 0)
            return null;

        for (int i = 0; i < noticeInfos.Count; i++) {
            if (noticeInfos[i].uuid == uuid)
                return noticeInfos[i];
        }

        return null;
    }

    public LeagueDataDTO GetLeagueInfo(long leagueID) {
        if (leagueInfos == null || leagueInfos.Count == 0)
            return null;

        //LeagueID로 속해있는 리그의 이름을 가져와야한다.
        LEAGUE_LEVEL leagueLevel = (LEAGUE_LEVEL)leagueID;

        for (int i = 0; i < leagueInfos.Count; i++) {
            LeagueDataDTO leagueInfo = leagueInfos[i];
             if (leagueInfo.title.Contains(leagueLevel.ToString())) {
                return leagueInfo;
            }
        }

        return null;
    }

    public bool IsFrameExist(long frameID) {
        if (userProfile.frameIDs.Contains(frameID))
            return true;
        return false;
    }

    public bool IsProfileExist(long profileID) {
        if (userProfile.profileIDs.Contains(profileID))
            return true;
        return false;
    }

    public void AddProfile(long profileID) {
        if (userProfile.profileIDs.Contains(profileID) == false)
            userProfile.profileIDs.Add(profileID);
    }

    public void AddFrame(long frameID) {
        if (userProfile.frameIDs.Contains(frameID) == false)
            userProfile.frameIDs.Add(frameID);
    }

    public void RemoveFriend(string inDate) {
        for (int i = 0; i < friends.Count; i++) {
            if (friends[i].inDate == inDate) {
                friends.RemoveAt(i);
                return;
            }
        }
    }

    public SimpleUserDTO GetFriendInfo(string inDate) {
        for (int i = 0; i < friends.Count; i++) {
            SimpleUserDTO friendInfo = friends[0];
            if (friendInfo.inDate == inDate)
                return friendInfo;
        }

        return null;
    }

    public void RemoveReceivedFriendMessage(string msgInDate) {
        for (int i = 0; i < receivedFriendMessages.Count; i++) {
            if (receivedFriendMessages[i].msgInDate == msgInDate) {
                receivedFriendMessages.RemoveAt(i);
                return;
            }
        }
    }

    public void RemoveFriendReceives(string friendInDate) {
        for (int i = 0; i < friendReceives.Count; i++) {
            if (friendReceives[i].inDate == friendInDate) {
                friendReceives.RemoveAt(i);
                return;
            }
        }
    }

    public void RemoveFindUserInfoAfterInvite(string friendInDate) {
        for (int i = 0; i < findUserInfos.Count; i++) {
            if (findUserInfos[i].inDate == friendInDate) {
                findUserInfos.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; i < friendRecommands.Count; i++) {
            if (friendRecommands[i].inDate == friendInDate) {
                friendRecommands.RemoveAt(i);
                return;
            }
        }
    }
}
