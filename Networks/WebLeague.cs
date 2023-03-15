using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuckyFlow.EnumDefine;
using UserData;
using Newtonsoft.Json;

public class WebLeague : MonoBehaviour {
    public static WebLeague instance;

    private bool first = true;

    private void Awake() {
        instance = this;
    }

    public void ReqDetermineLeague(Callback finalCallback, bool forRankingUI = false) {
        LeagueScoreDTO leagueScoreInfo = UserDataModel.instance.leagueScoreInfo;
        bool updateUserData = true;

        //배치된 리그가 없는경우
        if (leagueScoreInfo.leagueID == (int)LEAGUE_LEVEL.NONE) {
            int nextLeagueID = LeagueUtil.GetNextLeagueID();
            LeagueDataDTO leagueInfo = UserDataModel.instance.GetLeagueInfo(nextLeagueID);
            //배치할 리그가 없는경우
            if (leagueInfo == null) {
                //@todo : 뭔가 알려줄 필요가 있다면 내용 작성
                updateUserData = false;
                Debug.Log("배치할 리그 없음");
            }
            //배치할 리그가 있는경우 
            else {
                LeagueUtil.ResetLeagueScoreData();
                UserDataModel.instance.leagueScoreInfo.leagueID = nextLeagueID;
                UserDataModel.instance.leagueScoreInfo.seasonNo = leagueInfo.seasonNo;
                UserDataModel.instance.leagueScoreInfo.uuid = leagueInfo.uuid;

                UserDataModel.instance.publicUserData.currentLeagueID = nextLeagueID;
                UserDataModel.instance.publicUserData.currentLeagueScore = 0;
                UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.PUBLIC_USER_DATAS);

                Debug.Log($"배치된 리그 : {nextLeagueID}");
            }
        }
        //이미 속한 리그가 있는경우
        else {
            LeagueDataDTO leagueInfo = UserDataModel.instance.GetLeagueInfo(leagueScoreInfo.leagueID);
            //진행중이던 리그가 종료된 경우
            if (leagueInfo == null ||
                UserDataModel.instance.leagueScoreInfo.seasonNo != leagueInfo.seasonNo) {
                LeagueUtil.SetPublicUserDataRecord(leagueScoreInfo.leagueID,
                                                   leagueScoreInfo.seasonNo,
                                                   leagueScoreInfo.leagueScore,
                                                   leagueScoreInfo.lastRank);
                DetermineRewardAndLeague(finalCallback);
                Debug.Log($"리그 종료 : {leagueScoreInfo.leagueID}, 보상수령후 재배치");
                return;
            }
            //진행중인경우 현재 순위를 받아온다.
            else {
                Debug.Log($"리그진행중 : {leagueScoreInfo.leagueID}");
                updateUserData = false;
                if (first == false && forRankingUI)
                    finalCallback();
                else {
                    first = false;
                    BackendRequest.instance.ReqMyLeagueRanking(finalCallback);
                }
                return;
            }
        }

        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.LEAGUE_SCORE);
        if (updateUserData)
            BackendRequest.instance.ReqSyncUserData(false, true, finalCallback);
        else if (finalCallback != null)
            finalCallback();
    }

    private void DetermineRewardAndLeague(Callback finalCallback) {
        Callback determineReward = () => {
            //리그에 한번이라도 참여한경우에만 보상 지급

            if (UserDataModel.instance.leagueScoreInfo.lastRank > 0) {
                long rank = UserDataModel.instance.leagueScoreInfo.lastRank;
                long leagueUserCount = UserDataModel.instance.myLeagueUserCount;
                float percent = (float)rank / leagueUserCount * 100;

                DetermineReward(UserDataModel.instance.leagueScoreInfo.leagueID,
                                rank,
                                percent,
                                finalCallback);
            }
            else
                DetermineNextLeague(finalCallback);
        };

        BackendRequest.instance.ReqMyLeagueRanking(determineReward);
    }

    private void DetermineReward(long leagueID, long rank, float percent, Callback finalCallback) {
        //패키지에 따라 별도 처리 
        List<GameData.PackageDTO> packageDatas = GameDataModel.instance.GetLeagueRewardPackageDatas(leagueID, rank, percent);
        foreach (GameData.PackageDTO packageData in packageDatas) {
            switch ((PACKAGE_TYPE)packageData.type) {
                case PACKAGE_TYPE.DIAMOND: {
                    long amount = packageData.value + packageData.bonus;
                    UserDataModel.instance.AddDiamond(amount);
                    break;
                }

                case PACKAGE_TYPE.GOLD: {
                    long amount = packageData.value + packageData.bonus;
                    UserDataModel.instance.AddGold(amount);
                    break;
                }

                case PACKAGE_TYPE.PIGGYBANK_SKIN:
                    UserDataModel.instance.AddBankSkin(packageData.value);
                    break;

                case PACKAGE_TYPE.FRAME:
                    UserDataModel.instance.AddFrame(packageData.value);
                    break;

                case PACKAGE_TYPE.PROFILE:
                    UserDataModel.instance.AddProfile(packageData.value);
                    break;
            }
        }

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.USER_PROFILE);

        Callback showRewardListPopup = () => {
            GetItemListPopup itemListPopup = UIManager.instance.GetUI<GetItemListPopup>(UI_NAME.GetItemListPopup);
            itemListPopup.SetData(packageDatas, finalCallback);
            itemListPopup.Show();
        };

        //종료된 리그정보를 미리 기억해둔다.
        LeagueScoreDTO endLeagueScoreInfo = new LeagueScoreDTO();
        endLeagueScoreInfo.lastRank = UserDataModel.instance.leagueScoreInfo.lastRank;
        endLeagueScoreInfo.leagueID = UserDataModel.instance.leagueScoreInfo.leagueID;
        endLeagueScoreInfo.leagueScore = UserDataModel.instance.leagueScoreInfo.leagueScore;
        endLeagueScoreInfo.seasonNo = UserDataModel.instance.leagueScoreInfo.seasonNo;
        endLeagueScoreInfo.uuid = UserDataModel.instance.leagueScoreInfo.uuid;
        endLeagueScoreInfo.extension = UserDataModel.instance.leagueScoreInfo.extension;

        Callback showLeagueResultPopup = () => {
            LeagueResultPopup leagueResultPopup = UIManager.instance.GetUI<LeagueResultPopup>(UI_NAME.LeagueResultPopup);
            leagueResultPopup.SetData(endLeagueScoreInfo, showRewardListPopup);
            leagueResultPopup.Show();
        };

        DetermineNextLeague(showLeagueResultPopup);
        //보상 결정 -> 다음리그를 결정 -> 리그결과팝업 -> 보상목록 팝업 -> 리그UI 표시 or 랭킹UI 표시
    }

    private void DetermineNextLeague(Callback successCallback) {
        long rank = UserDataModel.instance.leagueScoreInfo.lastRank;
        long leagueUserCount = UserDataModel.instance.myLeagueUserCount;
        float percent = (float)rank / leagueUserCount * 100;

        int leagueID = LeagueUtil.GetNextLeagueID(UserDataModel.instance.leagueScoreInfo.leagueID, rank, percent);
        LeagueDataDTO leagueInfo = UserDataModel.instance.GetLeagueInfo(leagueID);
        LeagueUtil.ResetLeagueScoreData();
        if (leagueInfo != null) {
            UserDataModel.instance.leagueScoreInfo.leagueID = leagueID;
            UserDataModel.instance.leagueScoreInfo.seasonNo = leagueInfo.seasonNo;
            UserDataModel.instance.leagueScoreInfo.uuid = leagueInfo.uuid;
        }
        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.LEAGUE_SCORE);
        BackendRequest.instance.ReqSyncUserData(false, true, successCallback);
    }
}
