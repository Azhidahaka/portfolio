using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuckyFlow.EnumDefine;
using UserData;
using Newtonsoft.Json;

public class LeagueUtil {
    public static int GetNextLeagueID(long currentLeagueID = 0, long rank = 0, float currentLeaguePercent = 100.0f) {
        //브론즈, 최하위리그
        if ((LEAGUE_LEVEL)currentLeagueID == LEAGUE_LEVEL.NONE) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Constant.SHOW_TEST_LEAGUE)
                return (int)LEAGUE_LEVEL.TEST_BRONZE;
            return (int)LEAGUE_LEVEL.BRONZE;
#else
            return (int)LEAGUE_LEVEL.BRONZE;
#endif
        }
        else if ((LEAGUE_LEVEL)currentLeagueID == LEAGUE_LEVEL.TEST_BRONZE ||
            (LEAGUE_LEVEL)currentLeagueID == LEAGUE_LEVEL.BRONZE) {
            if (currentLeaguePercent <= 25.0f || (rank > 0 && rank <= 50)) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (Constant.SHOW_TEST_LEAGUE)
                    return (int)LEAGUE_LEVEL.TEST_SILVER;
                return (int)LEAGUE_LEVEL.SILVER;
#else
                return (int)LEAGUE_LEVEL.SILVER;
#endif
            }
            else {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (Constant.SHOW_TEST_LEAGUE)
                    return (int)LEAGUE_LEVEL.TEST_BRONZE;
                return (int)LEAGUE_LEVEL.BRONZE;
#else
                return (int)LEAGUE_LEVEL.BRONZE;
#endif
            }
        }
        //실버
        else {
            if (rank == 0 || (currentLeaguePercent > 75.0f &&  rank > 50)) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (Constant.SHOW_TEST_LEAGUE)
                    return (int)LEAGUE_LEVEL.TEST_BRONZE;
                return (int)LEAGUE_LEVEL.BRONZE;
#else
                return (int)LEAGUE_LEVEL.BRONZE;
#endif
            }
            else {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (Constant.SHOW_TEST_LEAGUE)
                    return (int)LEAGUE_LEVEL.TEST_SILVER;
                return (int)LEAGUE_LEVEL.SILVER;
#else
                return (int)LEAGUE_LEVEL.SILVER;
#endif
            }
        }
    }

    public static void ResetLeagueScoreData() {
        UserDataModel.instance.leagueScoreInfo.leagueID = 0;
        UserDataModel.instance.leagueScoreInfo.leagueScore = 0;
        UserDataModel.instance.leagueScoreInfo.lastRank = 0;
        UserDataModel.instance.leagueScoreInfo.seasonNo = 0;
        ChallengeRankDTO.ExtensionDTO extension = new ChallengeRankDTO.ExtensionDTO();
        extension.nickname = BackendLogin.instance.nickname;
        extension.flagNo = UserDataModel.instance.userProfile.flagNo;
        extension.profileNo = UserDataModel.instance.publicUserData.profileNo;
        extension.frameNo = UserDataModel.instance.publicUserData.profileFrameNo;
        UserDataModel.instance.leagueScoreInfo.extension = JsonConvert.SerializeObject(extension);
    }

    public static void SetPublicUserDataRecord(long leagueID, long seasonNo, long score, long rank) {
        if (leagueID == 0 ||
            seasonNo == 0 ||
            score == 0 ||
            rank == 0)
            return;

        List<PublicUserDataDTO.LeagueRecordDTO> recordList = UserDataModel.instance.publicUserData.leagueRecordList;
        for (int i = 0; i < recordList.Count; i++) {
            PublicUserDataDTO.LeagueRecordDTO record = recordList[i];
            if (record.leagueID != leagueID || record.seasonNo != seasonNo)
                continue;

            //이전 기록보다 높은경우에만 갱신한다.
            if (score > record.score) {
                record.score = score;
                record.rank = rank;
                UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.PUBLIC_USER_DATAS);
            }
            return;
        }

        //기존 데이터가 없으면 새로 추가한다.
        PublicUserDataDTO.LeagueRecordDTO newRecord = new UserData.PublicUserDataDTO.LeagueRecordDTO();
        newRecord.leagueID = leagueID;
        newRecord.seasonNo = seasonNo;
        newRecord.score = score;
        newRecord.rank = rank;
        recordList.Add(newRecord);
        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.PUBLIC_USER_DATAS);
    }

    public static List<GameData.PackageDTO> GetRewardPackageDatas() {
        if (UserDataModel.instance.leagueScoreInfo.lastRank == 0)
            return new List<GameData.PackageDTO>();

        long leagueID = UserDataModel.instance.leagueScoreInfo.leagueID;
        long rank = UserDataModel.instance.leagueScoreInfo.lastRank;
        long leagueUserCount = UserDataModel.instance.myLeagueUserCount;
        float percent = (float)rank / leagueUserCount * 100;

        List<GameData.PackageDTO> packageDatas = GameDataModel.instance.GetLeagueRewardPackageDatas(leagueID, rank, percent);
        return packageDatas;
    }

    public static string GetUserName(string nickname) {
        string userName = $"{nickname}";
        return userName;
    }

    public static string GetUserNo(long userNo) {
        string noStr = $"#{userNo}";
        return noStr;
    }

    public static string GetLeagueName(long leagueID) {
        string leagueName = TermModel.instance.GetTerm($"league_name_{leagueID}");
        return leagueName;
    }

    public static STAGE_LEVEL GetStageLevel() {
        long leagueID = UserDataModel.instance.leagueScoreInfo.leagueID;
        if (leagueID == (long)LEAGUE_LEVEL.BRONZE ||
            leagueID == (long)LEAGUE_LEVEL.TEST_BRONZE)
            return STAGE_LEVEL.LeagueBronze;
        else
            return STAGE_LEVEL.LeagueSilver;
    }

    public static void ShowLeagueRanking() {
        Callback showRanking = () => {
            Ranking ranking = UIManager.instance.GetUI<Ranking>(UI_NAME.Ranking);
            ranking.SetData(Ranking.POPUP_STATE.CHALLENGE);
            ranking.Show();
        };

        Callback reqRanking = () => {
            BackendRequest.instance.ReqLeagueRanking(showRanking);
        };

        Callback reqTop3Ranking = () => {
            BackendRequest.instance.ReqLeagueRankTop3(reqRanking);
        };

        WebLeague.instance.ReqDetermineLeague(reqTop3Ranking, true);
    }

    public static bool IsAvailableLeague(string leagueTitle) {
        if (Constant.SHOW_TEST_LEAGUE == false) {
            if (leagueTitle.Contains("TEST"))
                return false;
            return true;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (leagueTitle.Contains("TEST"))
            return true;
        return false;
#else
        if (leagueTitle.Contains("TEST"))
            return false;
        return true;
#endif
    }
}
