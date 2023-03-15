using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using GooglePlayGames.BasicApi;
using GooglePlayGames;
using System;
using LitJson;
using Newtonsoft.Json;
using UserData;
using LuckyFlow.EnumDefine;
using BackEnd.GlobalSupport;
using LuckyFlow.Event;
using System.Text.RegularExpressions;

public class BackendRequest : MonoBehaviour {
    public static BackendRequest instance;

    private BackendReturnObject bro = new BackendReturnObject();
    private bool isSuccess = false;

    private Callback successCallback;
    private Callback failCallback;

    private bool force = false;

    private void Start() {
        instance = this;
    }

    private void InsertUserData(USER_DATA_TABLE_NAME tableName) {
        isSuccess = false;

        Param param = GetUserDataParam(tableName);

        //Debug.Log(tableName + " : " + dataStr);

        SendQueue.Enqueue(Backend.GameData.Insert, tableName.ToString(), param, (res) => {
            //응답
            if (res.IsSuccess()) {
                string inDate = res.GetInDate();
                UserDataModel.instance.AddTableVersionInfo(tableName, inDate, Common.ConvertStringToTimestamp(inDate));

                if (tableName == USER_DATA_TABLE_NAME.leagueScores)
                    UserDataModel.instance.leagueScoreInfo.scoreInDate = inDate;

                isSuccess = true;
                Debug.Log(tableName + " 서버에 데이터 추가 완료");
            }
            else {
                Debug.Log($"InsertUserData::{tableName}::서버에 데이터 추가하는 도중 에러 발생");
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    public Param GetUserDataParam(USER_DATA_TABLE_NAME tableName) {
        Param param = new Param();

        if (tableName == USER_DATA_TABLE_NAME.publicUserDatas) {
            PublicUserDataDTO publicUserData = UserDataModel.instance.publicUserData;
            param.Add("no", publicUserData.no);
            param.Add("bestLeagueScore", publicUserData.bestLeagueScore);
            param.Add("bestLeagueID", publicUserData.bestLeagueID);
            param.Add("currentLeagueID", publicUserData.currentLeagueID);
            param.Add("currentLeagueScore", publicUserData.currentLeagueScore);
            if (string.IsNullOrEmpty(publicUserData.nickname))
                param.Add("nickname", BackendLogin.instance.nickname);
            else
                param.Add("nickname", publicUserData.nickname);
            param.Add("profileNo", publicUserData.profileNo);
            param.Add("profileFrameNo", publicUserData.profileFrameNo);
            param.Add("leagueRecords", JsonConvert.SerializeObject(publicUserData.leagueRecordList));
            param.Add("active", publicUserData.active);
        }
        else if (tableName == USER_DATA_TABLE_NAME.leagueScores) {
            LeagueScoreDTO leagueScoreInfo = UserDataModel.instance.leagueScoreInfo;
            param.Add("leagueID", leagueScoreInfo.leagueID);
            param.Add("leagueScore", leagueScoreInfo.leagueScore);
            if (string.IsNullOrEmpty(leagueScoreInfo.extension))
                param.Add("extension", "");
            else
                param.Add("extension", leagueScoreInfo.extension);
            param.Add("lastRank", leagueScoreInfo.lastRank);
            param.Add("seasonNo", leagueScoreInfo.seasonNo);
            if (string.IsNullOrEmpty(leagueScoreInfo.uuid))
                param.Add("uuid", "");
            else
                param.Add("uuid", leagueScoreInfo.uuid);
        }
        else {
            string dataStr = UserDataModel.instance.GetUserDataStr(tableName);
            param.Add("data", dataStr);
        }

        return param;
    }

    private void InsertScoreData(Callback callback) {
        isSuccess = false;
        string tableName = Constant.SCORE_TABLE_NAME;

        Param param = new Param();
        param.Add("score", 0);
        //Debug.Log(tableName + " : " + dataStr);

        SendQueue.Enqueue(Backend.GameData.Insert, tableName, param, (res) => {
            //응답
            if (res.IsSuccess()) {
                JsonData resData = res.GetReturnValuetoJSON();
                UserDataModel.instance.userProfile.scoreRowIndate = resData["inDate"].ToString();
                isSuccess = true;
                Debug.Log(tableName + " 서버에 데이터 추가 완료");

                callback();
            }
            else {
                Debug.Log(tableName + " 서버에 데이터 추가하는 도중 에러 발생");
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    private IEnumerator JobSyncUserData() {
        Dictionary<string, TableVersionInfoDTO> dicClient = UserDataModel.instance.dicTableVersionInfo;

        for (USER_DATA_TABLE_NAME tableName = USER_DATA_TABLE_NAME.START + 1; tableName < USER_DATA_TABLE_NAME.END; tableName++) {
            GetUserData(tableName);
            yield return new WaitUntil(() => isSuccess);
        }

        UserDataModel.instance.SaveUserDatas(false);
        UserDataModel.instance.SaveUserData(USER_DATA_KEY.TABLE_VERSION_INFO);

        Debug.Log("UpdateUserdata Success");
        ResSyncUserData();
    }


    private void ResSyncUserData() {
        UserDataModel.instance.RevisionChanged = false;
        Dispatcher.AddAction(() => NetworkLoading.instance.Hide());

        if (successCallback != null) {
            successCallback();
            successCallback = null;
        }
    }

    private void UpdateUserData(USER_DATA_TABLE_NAME tableName, string rowInDate = "") {
        isSuccess = false;

        if (rowInDate == "") {
            TableVersionInfoDTO versionInfo = UserDataModel.instance.GetTableVersionInfo(tableName);
            rowInDate = versionInfo.inDate;
        }

        Param param = GetUserDataParam(tableName);

        SendQueue.Enqueue(Backend.GameData.UpdateV2, tableName.ToString(), rowInDate, BackendLogin.instance.UserInDate, param, (res) => {
            //성공시 처리
            if (res.IsSuccess()) {
                isSuccess = true;
                Debug.Log(tableName + " 서버에 저장 완료:" + res.GetReturnValue());
            }
            //실패시 처리
            else {
                string format = "{0} 서버에 저장도중 에러 발생, StatusCode : {1}, Message : {2}";
                Debug.Log(string.Format(format, tableName, res.GetStatusCode(), res.GetMessage()));
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    private void GetUserData(USER_DATA_TABLE_NAME tableName) {
        isSuccess = false;

        Dictionary<string, TableVersionInfoDTO> dicClient = UserDataModel.instance.dicTableVersionInfo;
        string userIndate = BackendLogin.instance.UserInDate;
        Where where = new Where();
        SendQueue.Enqueue(Backend.GameData.GetMyData, tableName.ToString(), where, (res) => {
            //성공시 
            if (res.IsSuccess()) {
                //Debug.Log("GetUserData::" + res.GetReturnValue());
                JsonData resData = res.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];
                //데이터가 있으면 클라 버전과 비교
                if (rows.Count > 0) {
                    string inDate = rows[0]["inDate"]["S"].ToString();
                    object data;
                    if (tableName == USER_DATA_TABLE_NAME.publicUserDatas) {
                        PublicUserDataDTO publicUserData = new PublicUserDataDTO();
                        long.TryParse(rows[0]["no"]["N"].ToString(), out publicUserData.no);
                        long.TryParse(rows[0]["bestLeagueScore"]["N"].ToString(), out publicUserData.bestLeagueScore);
                        long.TryParse(rows[0]["bestLeagueID"]["N"].ToString(), out publicUserData.bestLeagueID);
                        long.TryParse(rows[0]["currentLeagueID"]["N"].ToString(), out publicUserData.currentLeagueID);
                        long.TryParse(rows[0]["currentLeagueScore"]["N"].ToString(), out publicUserData.currentLeagueScore);
                        if (rows[0].ContainsKey("nickname") && rows[0]["nickname"] != null)
                            publicUserData.nickname = rows[0]["nickname"]["S"].ToString();
                        long.TryParse(rows[0]["profileNo"]["N"].ToString(), out publicUserData.profileNo);
                        long.TryParse(rows[0]["profileFrameNo"]["N"].ToString(), out publicUserData.profileFrameNo);
                        publicUserData.leagueRecords = rows[0]["leagueRecords"]["S"].ToString();
                        if (publicUserData.leagueRecords != null)
                            publicUserData.leagueRecordList = JsonConvert.DeserializeObject<List<PublicUserDataDTO.LeagueRecordDTO>>(publicUserData.leagueRecords);
                        if (rows[0].ContainsKey("active"))
                            long.TryParse(rows[0]["active"]["N"].ToString(), out publicUserData.active);
                        data = publicUserData;
                    }
                    else if (tableName == USER_DATA_TABLE_NAME.leagueScores) {
                        LeagueScoreDTO leagueScoreData = new LeagueScoreDTO();
                        long.TryParse(rows[0]["leagueID"]["N"].ToString(), out leagueScoreData.leagueID);
                        long.TryParse(rows[0]["leagueScore"]["N"].ToString(), out leagueScoreData.leagueScore);
                        if (rows[0].ContainsKey("extension") && rows[0]["extension"] != null)
                            leagueScoreData.extension = rows[0]["extension"]["S"].ToString();
                        long.TryParse(rows[0]["lastRank"]["N"].ToString(), out leagueScoreData.lastRank);
                        long.TryParse(rows[0]["seasonNo"]["N"].ToString(), out leagueScoreData.seasonNo);
                        if (rows[0].ContainsKey("uuid"))
                            leagueScoreData.uuid = rows[0]["uuid"]["S"].ToString();
                        leagueScoreData.scoreInDate = rows[0]["inDate"]["S"].ToString();
                        //전체 데이터를 갱신하지 않더라도 이건 별도로 갱신해준다.
                        UserDataModel.instance.leagueScoreInfo.scoreInDate = leagueScoreData.scoreInDate;

                        data = leagueScoreData;
                    }
                    else
                        data = rows[0]["data"]["S"].ToString();

                    string updatedAtStr = rows[0]["updatedAt"]["S"].ToString();
                    double updatedAt = Common.ConvertStringToTimestamp(updatedAtStr);

                    string key = tableName.ToString();

                    if (force) {
                        UpdateUserData(tableName, inDate);
                    }
                    //클라이언트에 테이블의 version정보가 없거나, 서버 버전이 높으면 클라 업데이트
                    else if (dicClient.ContainsKey(key) == false) {
                        UserDataModel.instance.UpdateUserData(tableName, data);
                        UserDataModel.instance.AddTableVersionInfo(tableName, inDate, updatedAt);
                        isSuccess = true;
                        Debug.Log(tableName + " 서버 데이터 가져오기 완료");
                    }
                    else if (updatedAt > dicClient[key].updatedAt) {
                        UserDataModel.instance.UpdateUserData(tableName, data);
                        UserDataModel.instance.UpdateTableVersionInfo(tableName, updatedAt);
                        isSuccess = true;
                        Debug.Log(tableName + " 서버 데이터 가져오기 완료");
                    }
                    //클라에 저장된 version이 더 높으면 클라의 데이터를 서버에 저장한다.
                    else if (updatedAt < dicClient[key].updatedAt) {
                        UpdateUserData(tableName);
                    }
                    //서버와 클라의 버전이 같다면 아무 처리도 하지 않는다.
                    else {
                        isSuccess = true;
                        Debug.Log(key + " 버전이 일치합니다.");
                    }
                }
                //데이터가 없으면 추가
                else if (tableName == USER_DATA_TABLE_NAME.userDatas) {
                    InsertScoreData(() => {
                        InsertUserData(USER_DATA_TABLE_NAME.userDatas);
                    });
                }
                else if (tableName == USER_DATA_TABLE_NAME.leagueScores) {
                    InsertUserData(USER_DATA_TABLE_NAME.leagueScores);
                }
                else {
                    InsertUserData(tableName);
                }
            }
            //실패시 처리
            else {
                Debug.Log(res.GetMessage());
                Debug.Log(tableName + " 서버의 데이터를 가져오는 도중 에러 발생");
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    public void ReqSyncUserData(bool force, bool showWaiting, Callback successCallback = null) {
        this.force = force;
        this.successCallback = successCallback;

        if (showWaiting)
            Dispatcher.AddAction(() => NetworkLoading.instance.Show());

        StartCoroutine(JobSyncUserData());
    }

    public void ReqPushOn(Callback successCallback = null) {
#if !UNITY_EDITOR && UNITY_ANDROID
        Backend.Android.PutDeviceToken(Backend.Android.GetDeviceToken(), (callback) => {
            Dispatcher.AddAction(() => {
                if (successCallback != null)
                    successCallback();
            });
        });

#elif UNITY_IOS && DEVELOPMENT_BUILD
        Backend.iOS.PutDeviceToken(isDevelopment.iosDev, (callback) => {
            Dispatcher.AddAction(() => {
                if (successCallback != null)
                    successCallback();
            });
        });

#elif UNITY_IOS
        Backend.iOS.PutDeviceToken(isDevelopment.iosProd, (callback) => {
            Dispatcher.AddAction(() => {
                if (successCallback != null)
                    successCallback();
            });
        });
#else
        if (successCallback != null)
            successCallback();
#endif
    }

    public void ReqPushOff(Callback successCallback = null) {
#if !UNITY_EDITOR && UNITY_ANDROID
        Backend.Android.DeleteDeviceToken((callback) => {
            Dispatcher.AddAction(() => {
                if (successCallback != null)
                    successCallback();
            });
        });
#elif UNITY_IOS
        Backend.iOS.DeleteDeviceToken((callback) => {
            Dispatcher.AddAction(() => {
                if (successCallback != null)
                    successCallback();
            });
        });
#else
        if (successCallback != null)
            successCallback();
#endif
    }

    public void DetermineNation(Callback successCallback) {
        bool update = false;
        CountryCode countryCode = CountryCode.UnitedStates;

        SendQueue.Enqueue(Backend.BMember.GetCountryCodeByIndate, BackendLogin.instance.UserInDate, (getRes) => {
            //국가코드가 존재하지 않음
            if (getRes.GetStatusCode() == "404") {
                update = true;
                if (Application.systemLanguage == SystemLanguage.Korean)
                    countryCode = CountryCode.SouthKorea;
            }
            //국가코드가 있음
            else {
                JsonData resData = getRes.GetReturnValuetoJSON();

                string country = resData["country"]["S"].ToString();
                if (country == "KR" && Application.systemLanguage != SystemLanguage.Korean)
                    update = true;
                else if (country != "KR" && Application.systemLanguage == SystemLanguage.Korean) {
                    update = true;
                    countryCode = CountryCode.SouthKorea;
                }
            }

            if (update) {
                Backend.BMember.UpdateCountryCode(countryCode, (res) => {
                    Debug.Log("국가코드 적용 완료: " + countryCode);
                    Dispatcher.AddAction(() => successCallback());
                });
            }
            else {
                Dispatcher.AddAction(() => successCallback());
            }
        });
    }

    public void ReqUpdateScore(long score, string nickname, Callback successCallback) {
        NetworkLoading.instance.Show();
        string uUID = Constant.CHALLENGE_RANKING_UUID;

        Param param = new Param();
        param.Add("score", score);
        ChallengeRankDTO.ExtensionDTO extension = new ChallengeRankDTO.ExtensionDTO();
        extension.nickname = nickname;
        extension.flagNo = UserDataModel.instance.userProfile.flagNo;
        param.Add("extension", JsonConvert.SerializeObject(extension));

        //자릿수가 줄어든 경우 0을 채워준다.
        if (UserDataModel.instance.userProfile.scoreRowIndate.Length == 23)
            UserDataModel.instance.userProfile.scoreRowIndate = UserDataModel.instance.userProfile.scoreRowIndate.Replace("Z", "0Z");

        SendQueue.Enqueue(Backend.URank.User.UpdateUserScore,
                          uUID,
                          Constant.SCORE_TABLE_NAME,
                          UserDataModel.instance.userProfile.scoreRowIndate,
                          param, (getRes) => {
                              Dispatcher.AddAction(NetworkLoading.instance.Hide);
                              if (getRes.IsSuccess()) {
                                  Debug.Log("ReqUpdateScore Complete");
                                  Dispatcher.AddAction(() => {
                                      if (successCallback != null)
                                          successCallback();
                                  });
                              }
                              else {
                                  string format = "스코어 등록중 에러 발생, StatusCode : {0}, Message : {1}";
                                  Debug.Log(string.Format(format, getRes.GetStatusCode(), getRes.GetMessage()));
                              }
                          });
    }

    public void ReqGetNotice(Callback successCallback = null) {
        NetworkLoading.instance.Show();

        Backend.Notice.NoticeList(getRes => {
            if (getRes.IsSuccess()) {
                JsonData resData = getRes.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                Dispatcher.AddAction(NetworkLoading.instance.Hide);

                bool noticeInfoChanged = false;
                List<string> noticeUUIDs = new List<string>();
                for (int i = 0; i < rows.Count; i++) {
                    NoticeDTO noticeInfo = new NoticeDTO();

                    string isPublic = rows[i]["isPublic"]["S"].ToString();
                    noticeInfo.isPublic = isPublic.Contains("y") || isPublic.Contains("Y");
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
                    if (noticeInfo.isPublic == false)
                        continue;
#endif  
                    string[] titleStrList = rows[i]["title"]["S"].ToString().Split('#');

                    noticeInfo.title = titleStrList[0];
                    if (titleStrList.Length > 1 && titleStrList[1] == "R")
                        noticeInfo.rightTitle = true;
                    else
                        noticeInfo.rightTitle = false;

                    if (titleStrList.Length > 2)
                        noticeInfo.version = titleStrList[2];

                    noticeInfo.content = rows[i]["content"]["S"].ToString();
                    noticeInfo.postingDate = rows[i]["postingDate"]["S"].ToString();
                    noticeInfo.author = rows[i]["author"]["S"].ToString();
                    noticeInfo.uuid = rows[i]["uuid"]["S"].ToString();
                    noticeInfo.inDate = rows[i]["inDate"]["S"].ToString();

                    noticeUUIDs.Add(noticeInfo.uuid);

                    string imageKey = "imageKey";
                    if (rows[i].ContainsKey(imageKey) &&
                        rows[i][imageKey].ContainsKey("S") &&
                        rows[i][imageKey]["S"] != null) {
                        noticeInfo.imageKey = rows[i][imageKey]["S"].ToString();
                    }

                    NoticeDTO oldNoticeInfo = UserDataModel.instance.GetNoticeInfo(noticeInfo.uuid);
                    if (oldNoticeInfo == null) {
                        UserDataModel.instance.noticeInfos.Add(noticeInfo);
                        noticeInfoChanged = true;
                    }
                    else if (oldNoticeInfo.version != noticeInfo.version) {
                        oldNoticeInfo.version = noticeInfo.version;
                        oldNoticeInfo.title = noticeInfo.title;
                        oldNoticeInfo.content = noticeInfo.content;
                        oldNoticeInfo.imageKey = noticeInfo.imageKey;
                        oldNoticeInfo.rightTitle = noticeInfo.rightTitle;
                        oldNoticeInfo.read = false;
                        noticeInfoChanged = true;
                    }
                    else {
                        //버전이 같은경우 변경하지 않음
                    }
                }

                Dispatcher.AddAction(() => {
                    List<string> removedUUIDS = UserDataModel.instance.RemoveNotices(noticeUUIDs);
                    if (removedUUIDS.Count > 0) {
                        noticeInfoChanged = true;
                        NoticeManager.instance.DeleteImages(removedUUIDS);
                    }

                    if (noticeInfoChanged)
                        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.NOTICE_INFOS);
                    if (successCallback != null)
                        successCallback();
                });
            }
        });
    }

    private string GetUUID() {
        LeagueScoreDTO leagueScoreInfo = UserDataModel.instance.leagueScoreInfo;
        LeagueDataDTO leagueData = UserDataModel.instance.GetLeagueInfo(leagueScoreInfo.leagueID);
        string uuid;
        if (string.IsNullOrEmpty(leagueScoreInfo.uuid) == false)
            uuid = leagueScoreInfo.uuid;
        else if (leagueData == null)
            uuid = Constant.CHALLENGE_RANKING_UUID;
        else
            uuid = leagueData.uuid;
        return uuid;
    }

    public void ReqLeagueRanking(Callback successCallback) {
        LeagueScoreDTO leagueScoreInfo = UserDataModel.instance.leagueScoreInfo;
        string uUID = GetUUID();

        SendQueue.Enqueue(Backend.URank.User.GetMyRank, uUID, Constant.RANKING_LIST_GAP, (getRes) => {
            if (getRes.IsSuccess()) {
                JsonData resData = getRes.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                List<ChallengeRankDTO> leagueRankInfos = new List<ChallengeRankDTO>();

                for (int i = 0; i < rows.Count; i++) {
                    ChallengeRankDTO challengeRankInfo = new ChallengeRankDTO();
                    long.TryParse(rows[i]["rank"]["N"].ToString(), out challengeRankInfo.rank);
                    string extensionStr = rows[i]["extension"]["S"].ToString();
                    challengeRankInfo.extension = JsonConvert.DeserializeObject<ChallengeRankDTO.ExtensionDTO>(extensionStr);
                    long.TryParse(rows[i]["score"]["N"].ToString(), out challengeRankInfo.score);
                    challengeRankInfo.gamerInDate = rows[i]["gamerInDate"]["S"].ToString();

                    //내 랭킹
                    if (challengeRankInfo.gamerInDate == BackendLogin.instance.UserInDate)
                        UserDataModel.instance.myChallengeRankInfo = challengeRankInfo;

                    leagueRankInfos.Add(challengeRankInfo);
                }

                LeagueDataDTO leagueData = UserDataModel.instance.GetLeagueInfo(leagueScoreInfo.leagueID);
                if (leagueData == null) {
                    UserDataModel.instance.leagueRankInfos = leagueRankInfos;
                }
                else {
                    UserDataModel.instance.leagueRankInfos = 
                        GetRankInfos(leagueScoreInfo.seasonNo, leagueScoreInfo.leagueID, leagueRankInfos, true);
                }

                Dispatcher.AddAction(() => {
                    successCallback();
                });
            }
            //내 랭킹이 존재하지 않음
            else {
                string statusCode = getRes.GetStatusCode();
                Debug.Log($"내 랭킹이 없습니다. StatusCode : {statusCode}, Message : {getRes.GetMessage()}");
                Dispatcher.AddAction(() => {
                    if (statusCode == "404") {
                        ReqLeagueRankingWithoutMe(successCallback);
                    }
                    else if (successCallback != null)
                        successCallback();
                });
            }
        });
    }

    private List<ChallengeRankDTO> GetRankInfos(long seasonNo, long leagueID, List<ChallengeRankDTO> rankInfos, bool includeTop3) {
        List<ChallengeRankDTO> resultInfos = new List<ChallengeRankDTO>();
        List<GameData.DummyRankingDTO> dummyRanDatas = GameDataModel.instance.GetDummyRankInfos(seasonNo, leagueID);


        List<ChallengeRankDTO> dummyRankInfos = new List<ChallengeRankDTO>();
        for (int i = 0; i < dummyRanDatas.Count; i++) {
            GameData.DummyRankingDTO dummyRankData = dummyRanDatas[i];
            GameData.RankingNPCDTO npcData = GameDataModel.instance.GetRankingNPCData(dummyRankData.npcID);

            ChallengeRankDTO rankInfo = new ChallengeRankDTO();
            
            rankInfo.gamerInDate = $"{dummyRankData.seasonNo}{dummyRankData.league}{dummyRankData.npcID}{dummyRankData.score}";
            rankInfo.score = dummyRankData.score;
            rankInfo.extension = new ChallengeRankDTO.ExtensionDTO();
            rankInfo.extension.frameNo = npcData.frameNo;
            rankInfo.extension.profileNo = npcData.profileNo;
            rankInfo.extension.nickname = npcData.name;

            dummyRankInfos.Add(rankInfo);
        }
        
        resultInfos.AddRange(dummyRankInfos);
        resultInfos.AddRange(rankInfos);

       long highestRank;

        if (includeTop3) {
            foreach (ChallengeRankDTO top3RankInfo in UserDataModel.instance.top3RankInfos) {
                bool exist = false;
                foreach (ChallengeRankDTO resultInfo in resultInfos) {
                    //이미 존재하는경우 패스
                    if (resultInfo.gamerInDate == top3RankInfo.gamerInDate) {
                        exist = true;
                        break;
                    }
                }

                if (exist == false) 
                    resultInfos.Add(top3RankInfo);
            }

             //Top3를 제외하고 가장 높은 랭킹
            highestRank = 4;

            if (rankInfos.Count > 0) {
                rankInfos.Sort(new SortByScore());

                for (int i = 0; i < rankInfos.Count; i++) {
                    if (rankInfos[i].rank > 3) {
                        highestRank = rankInfos[i].rank;
                        break;
                    }
                }
            }
        }
        else {
            highestRank = long.MaxValue;
            if (rankInfos.Count > 0) {
                rankInfos.Sort(new SortByScore());

                for (int i = 0; i < rankInfos.Count; i++) {
                    if (rankInfos[i].rank < highestRank) {
                        highestRank = rankInfos[i].rank;
                        break;
                    }
                }
            }
        }

        resultInfos.Sort(new SortByScore());

        if (includeTop3) {
            UserDataModel.instance.top3RankInfos = new List<ChallengeRankDTO>(){null, null, null};

            int count = Math.Min(3, resultInfos.Count);
            long top3Rank = 1;
            for (int i = 0; i < count; i++) {               
                ChallengeRankDTO rankInfo = UserDataModel.instance.top3RankInfos[i] = resultInfos[i];
                if (i == 0) 
                    rankInfo.rank = top3Rank;
                else if (rankInfo.score == resultInfos[i - 1].score)
                    rankInfo.rank = resultInfos[i - 1].rank;
                else {
                    top3Rank++;
                    rankInfo.rank = top3Rank;
                }
            }

            resultInfos.Remove(UserDataModel.instance.top3RankInfos[0]);
            resultInfos.Remove(UserDataModel.instance.top3RankInfos[1]);
            resultInfos.Remove(UserDataModel.instance.top3RankInfos[2]);
        }

        long rank = highestRank;
        for (int i = 0; i < resultInfos.Count; i++) {
            ChallengeRankDTO rankInfo = resultInfos[i];
            if (i == 0) 
                rankInfo.rank = highestRank;
            else if (rankInfo.score == resultInfos[i - 1].score)
                rankInfo.rank = resultInfos[i - 1].rank;
            else {
                rank++;
                rankInfo.rank = rank;
            }
        }

        return resultInfos;
    }

    private long GetMyRank(long myRank, long myScore) {
        LeagueScoreDTO leagueScoreInfo = UserDataModel.instance.leagueScoreInfo;

        List<ChallengeRankDTO> dummyRankInfos = new List<ChallengeRankDTO>();
        List<GameData.DummyRankingDTO> dummyRanDatas = GameDataModel.instance.GetDummyRankInfos(leagueScoreInfo.seasonNo, leagueScoreInfo.leagueID);
        for (int i = 0; i < dummyRanDatas.Count; i++) {
            GameData.DummyRankingDTO dummyRankData = dummyRanDatas[i];
            GameData.RankingNPCDTO npcData = GameDataModel.instance.GetRankingNPCData(dummyRankData.npcID);

            ChallengeRankDTO rankInfo = new ChallengeRankDTO();
            
            rankInfo.gamerInDate = $"{dummyRankData.seasonNo}{dummyRankData.league}{dummyRankData.npcID}{dummyRankData.score}";
            rankInfo.score = dummyRankData.score;
            rankInfo.extension = new ChallengeRankDTO.ExtensionDTO();
            rankInfo.extension.frameNo = npcData.frameNo;
            rankInfo.extension.profileNo = npcData.profileNo;
            rankInfo.extension.nickname = npcData.name;

            dummyRankInfos.Add(rankInfo);
        }

        for (int i = 0; i < dummyRankInfos.Count; i++) {
            ChallengeRankDTO rankInfo = dummyRankInfos[i];
            if (rankInfo.score > myScore)
                myRank++;
        }

        return myRank;
    }

    public class SortByScore : IComparer<ChallengeRankDTO> {
        public int Compare(ChallengeRankDTO left, ChallengeRankDTO right) {
            
            if (left.score > right.score)
                return -1;

            if (left.score < right.score)
                return 1;

            return 0;
        }
    }

    public void ReqLeagueRankTop3(Callback successCallback) {
        string uUID = GetUUID();
        SendQueue.Enqueue(Backend.URank.User.GetRankList, uUID, 3, (getRes) => {
            if (getRes.IsSuccess()) {
                JsonData resData = getRes.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                UserDataModel.instance.top3RankInfos.Clear();
                for (int i = 0; i < rows.Count; i++) {
                    ChallengeRankDTO rankInfo = new ChallengeRankDTO();
                    long.TryParse(rows[i]["rank"]["N"].ToString(), out rankInfo.rank);
                    string extensionStr = rows[i]["extension"]["S"].ToString();
                    rankInfo.extension = JsonConvert.DeserializeObject<ChallengeRankDTO.ExtensionDTO>(extensionStr);
                    long.TryParse(rows[i]["score"]["N"].ToString(), out rankInfo.score);
                    rankInfo.gamerInDate = rows[i]["gamerInDate"]["S"].ToString();

                    UserDataModel.instance.top3RankInfos.Add(rankInfo);
                }

                Dispatcher.AddAction(() => {
                    successCallback();
                });
            }
            //내 랭킹이 존재하지 않음
            else {
                string statusCode = getRes.GetStatusCode();
                Debug.Log($"Top3 랭킹 조회 실패 StatusCode : {statusCode}, Message : {getRes.GetMessage()}");
                Dispatcher.AddAction(() => {
                    successCallback();
                });
            }
        });
    }

    private void ReqLeagueRankingWithoutMe(Callback successCallback) {
        string uUID = GetUUID();
        int limit = Constant.SHOW_CHALLENGE_RANKING_COUNT;
        
        SendQueue.Enqueue(Backend.URank.User.GetRankList, uUID, limit, (getRes) => {
            if (getRes.IsSuccess()) {
                JsonData resData = getRes.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                List<ChallengeRankDTO> leagueRankInfos = new List<ChallengeRankDTO>();
                for (int i = 0; i < rows.Count; i++) {
                    ChallengeRankDTO challengeRankInfo = new ChallengeRankDTO();
                    long.TryParse(rows[i]["rank"]["N"].ToString(), out challengeRankInfo.rank);
                    string extensionStr = rows[i]["extension"]["S"].ToString();
                    challengeRankInfo.extension = JsonConvert.DeserializeObject<ChallengeRankDTO.ExtensionDTO>(extensionStr);
                    long.TryParse(rows[i]["score"]["N"].ToString(), out challengeRankInfo.score);
                    challengeRankInfo.gamerInDate = rows[i]["gamerInDate"]["S"].ToString();

                    leagueRankInfos.Add(challengeRankInfo);
                }

                LeagueScoreDTO leagueScoreInfo = UserDataModel.instance.leagueScoreInfo;
                LeagueDataDTO leagueData = UserDataModel.instance.GetLeagueInfo(leagueScoreInfo.leagueID);
                if (leagueData == null) {
                    UserDataModel.instance.leagueRankInfos = leagueRankInfos;
                }
                else {
                    UserDataModel.instance.leagueRankInfos = 
                        GetRankInfos(leagueScoreInfo.seasonNo, leagueScoreInfo.leagueID, leagueRankInfos, true);
                }
                
                Dispatcher.AddAction(() => {
                    successCallback();
                });
            }
            //내 랭킹이 존재하지 않음
            else {
                string statusCode = getRes.GetStatusCode();
                Debug.Log($"랭킹 조회 실패 StatusCode : {statusCode}, Message : {getRes.GetMessage()}");
                Dispatcher.AddAction(() => {
                    successCallback();
                });
            }
        });
    }

    public void ReqPost(Callback successCallback = null) {
        NetworkLoading.instance.Show();

        Backend.Social.Post.GetPostListV2((res) => {
            if (res.IsSuccess()) {
                JsonData resData = res.GetReturnValuetoJSON();
                JsonData fromAdmin = resData["fromAdmin"];
                //데이터가 있으면 클라 버전과 비교
                if (fromAdmin.Count > 0) {
                    Debug.Log("관리자 우편 갯수:" + fromAdmin.Count);
                }

                List<double> adminMailNos = new List<double>();

                bool mailInfosChanged = false;

                //서버로부터 받아온 관리자우편을 mailInfos에 저장(중복제외)
                for (int i = 0; i < fromAdmin.Count; i++) {
                    MailInfoDTO mailInfo = MailUtil.GetAdminMailInfo(fromAdmin[i]);
                    bool changed = UserDataModel.instance.AddMail(mailInfo);
                    adminMailNos.Add(mailInfo.no);
                    if (changed)
                        mailInfosChanged = true;
                }

                List<double> removeMailNos = new List<double>();
                List<MailInfoDTO> mailInfos = UserDataModel.instance.mailInfos;
                //기한이 만료된 우편을 삭제목록에 추가
                foreach (MailInfoDTO mailInfo in mailInfos) {
                    if (mailInfo.expireTime < Common.GetUnixTimeNow()) {
                        removeMailNos.Add(mailInfo.no);
                    }
                }

                //삭제목록에 있는 우편은 삭제
                for (int i = 0; i < removeMailNos.Count; i++) {
                    UserDataModel.instance.RemoveMail(removeMailNos[i]);
                    mailInfosChanged = true;
                }
                //<--

                if (mailInfosChanged) {
                    UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.MAIL_INFOS);
                }

                Dispatcher.AddAction(() => {
                    EventManager.Notify(EventEnum.UserDataMailInfoUpdate);
                    NetworkLoading.instance.Hide();
                    if (successCallback != null)
                        successCallback();
                });
            }
            else {
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    public void ReqGetPostReward(MailInfoDTO mailInfo, Callback successCallback = null) {
        if (mailInfo.partNo > 0) {
            if (mailInfo.rewards == null || mailInfo.rewards.Count == 0)
                return;

            int index = (int)mailInfo.partNo;
            MailRewardDTO rewardInfo = mailInfo.rewards[0];
            if (rewardInfo.type == (long)MAIL_REWARD_TYPE.GOLD)
                UserDataModel.instance.AddGold(rewardInfo.count);
            else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.DIAMOND)
                UserDataModel.instance.AddDiamond(rewardInfo.count);
            else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.SKIN)
                UserDataModel.instance.AddSkin(rewardInfo.count);
            else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.CHALLENGE_TICKET)
                UserDataModel.instance.AddChallengeTicket(rewardInfo.count);
            else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.PIGGYBANK_SKIN)
                UserDataModel.instance.AddBankSkin(rewardInfo.count);

            UserDataModel.instance.ConfirmMail(mailInfo.no, mailInfo.partNo, true);
            UserDataModel.instance.SaveUserDatas(true,
                                                    USER_DATA_KEY.USER_PROFILE,
                                                    USER_DATA_KEY.MAIL_INFOS);

            if (rewardInfo.type == (long)MAIL_REWARD_TYPE.GOLD)
                EffectManager.instance.ShowGetGoldEffect(rewardInfo.count);
            else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.DIAMOND)
                EffectManager.instance.ShowGetDiamondEffect(rewardInfo.count);
            else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.SKIN)
                EffectManager.instance.ShowGetSkinEffect(rewardInfo.count);
            else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.CHALLENGE_TICKET)
                EffectManager.instance.ShowGetItemEffect(0, rewardInfo.count, true);

            EventManager.Notify(EventEnum.UserDataMailInfoUpdate);
            ReqSyncUserData(false, false, successCallback);
            return;
        }

        NetworkLoading.instance.Show();

        Backend.Social.Post.ReceiveAdminPostItemV2(mailInfo.inDate, (res) => {
            Dispatcher.AddAction(NetworkLoading.instance.Hide);
            if (res.IsSuccess()) {
                JsonData resData = res.GetReturnValuetoJSON();

                MailRewardDTO rewardInfo = new MailRewardDTO();
                long.TryParse(resData["item"]["M"]["type"]["S"].ToString(), out rewardInfo.type);
                long.TryParse(resData["itemCount"]["N"].ToString(), out rewardInfo.count);
                //골드
                if (rewardInfo.type == (long)MAIL_REWARD_TYPE.GOLD)
                    UserDataModel.instance.AddGold(rewardInfo.count);
                //다이아
                else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.DIAMOND)
                    UserDataModel.instance.AddDiamond(rewardInfo.count);
                //스킨
                else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.SKIN)
                    UserDataModel.instance.AddSkin(rewardInfo.count);
                //티켓
                else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.CHALLENGE_TICKET)
                    UserDataModel.instance.AddChallengeTicket(rewardInfo.count);
                //저금통 스킨
                else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.PIGGYBANK_SKIN)
                    UserDataModel.instance.AddBankSkin(rewardInfo.count);
                else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.PACKAGE)
                    GetPackageReward(rewardInfo);

                UserDataModel.instance.ConfirmMail(mailInfo.no, mailInfo.partNo, true);
                UserDataModel.instance.SaveUserDatas(true,
                                                     USER_DATA_KEY.USER_PROFILE,
                                                     USER_DATA_KEY.MAIL_INFOS);

                Dispatcher.AddAction(() => {
                    if (rewardInfo.type == (long)MAIL_REWARD_TYPE.GOLD)
                        EffectManager.instance.ShowGetGoldEffect(rewardInfo.count);
                    else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.DIAMOND)
                        EffectManager.instance.ShowGetDiamondEffect(rewardInfo.count);
                    else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.SKIN)
                        EffectManager.instance.ShowGetSkinEffect(rewardInfo.count);
                    else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.CHALLENGE_TICKET)
                        EffectManager.instance.ShowGetItemEffect(0, rewardInfo.count, true);

                    EventManager.Notify(EventEnum.UserDataMailInfoUpdate);
                    ReqSyncUserData(false, false, successCallback);
                });
            }
            else {
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    private void GetPackageReward(MailRewardDTO rewardInfo) {
        List<GameData.PackageDTO> packageDatas = GameDataModel.instance.GetPackageDatas(rewardInfo.count);
        GameData.PackageDTO packageData = packageDatas[0];
        if (packageData.type == (long)PACKAGE_TYPE.GOLD)
            UserDataModel.instance.AddGold(rewardInfo.count);
        else if (packageData.type == (long)PACKAGE_TYPE.DIAMOND)
            UserDataModel.instance.AddDiamond(rewardInfo.count);
        else if (packageData.type == (long)PACKAGE_TYPE.SKIN)
            UserDataModel.instance.AddSkin(rewardInfo.count);
        else if (packageData.type == (long)PACKAGE_TYPE.CHALLENGE_TICKET)
            UserDataModel.instance.AddChallengeTicket(rewardInfo.count);
        else if (packageData.type == (long)PACKAGE_TYPE.PIGGYBANK_SKIN)
            UserDataModel.instance.AddBankSkin(rewardInfo.count);
    }

    public void ReqGetPostRewardsAll(Callback successCallback = null) {
        NetworkLoading.instance.Show();

        Backend.Social.Post.ReceiveAdminPostAllV2((res) => {
            Dispatcher.AddAction(NetworkLoading.instance.Hide);

            if (res.IsSuccess()) {
                JsonData resData = res.GetReturnValuetoJSON();
                JsonData items = resData["items"];
                //데이터가 있으면 클라 버전과 비교
                if (items.Count > 0) {
                    Debug.Log("수령 아이템 갯수:" + items.Count);
                }

                Dictionary<long, MailRewardDTO> dicReward = new Dictionary<long, MailRewardDTO>();
                List<MailRewardDTO> rewards = new List<MailRewardDTO>();

                for (int i = 0; i < items.Count; i++) {
                    MailRewardDTO rewardInfo = new MailRewardDTO();
                    long.TryParse(items[i]["item"]["M"]["type"]["S"].ToString(), out rewardInfo.type);
                    long.TryParse(items[i]["itemCount"]["N"].ToString(), out rewardInfo.count);

                    long key = rewardInfo.type;
                    if ((MAIL_REWARD_TYPE)rewardInfo.type == MAIL_REWARD_TYPE.SKIN)
                        rewards.Add(rewardInfo);
                    else if (dicReward.ContainsKey(key))
                        dicReward[key].count += rewardInfo.count;
                    else
                        dicReward.Add(key, rewardInfo);

                    //골드
                    if (rewardInfo.type == (long)MAIL_REWARD_TYPE.GOLD)
                        UserDataModel.instance.AddGold(rewardInfo.count);
                    //다이아
                    else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.DIAMOND)
                        UserDataModel.instance.AddDiamond(rewardInfo.count);
                    //스킨
                    else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.SKIN)
                        UserDataModel.instance.AddSkin(rewardInfo.count);
                    //티켓
                    else if (rewardInfo.type == (long)MAIL_REWARD_TYPE.CHALLENGE_TICKET)
                        UserDataModel.instance.AddChallengeTicket(rewardInfo.count);
                }

                foreach (KeyValuePair<long, MailRewardDTO> pair in dicReward) {
                    rewards.Add(pair.Value);
                }

                UserDataModel.instance.ConfirmAllMails();
                UserDataModel.instance.SaveUserDatas(true,
                                                     USER_DATA_KEY.USER_PROFILE,
                                                     USER_DATA_KEY.MAIL_INFOS);

                Dispatcher.AddAction(() => {
                    GetItemListPopup itemListPopup = UIManager.instance.GetUI<GetItemListPopup>(UI_NAME.GetItemListPopup);
                    itemListPopup.SetData(rewards);
                    itemListPopup.Show();

                    EventManager.Notify(EventEnum.UserDataMailInfoUpdate);
                    ReqSyncUserData(false, false, successCallback);
                });
            }
            else {
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    public void ReqSimpleRanking(Callback successCallback) {
        string uUID = GetUUID();
        SendQueue.Enqueue(Backend.URank.User.GetMyRank, uUID, 0, (getRes) => {
            if (getRes.IsSuccess()) {
                JsonData resData = getRes.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                if (rows.Count > 0) {
                    ChallengeRankDTO challengeRankInfo = new ChallengeRankDTO();
                    long.TryParse(rows[0]["rank"]["N"].ToString(), out challengeRankInfo.rank);
                    string extensionStr = rows[0]["extension"]["S"].ToString();
                    challengeRankInfo.extension = JsonConvert.DeserializeObject<ChallengeRankDTO.ExtensionDTO>(extensionStr);
                    long.TryParse(rows[0]["score"]["N"].ToString(), out challengeRankInfo.score);
                    challengeRankInfo.gamerInDate = rows[0]["gamerInDate"]["S"].ToString();
                    UserDataModel.instance.myChallengeRankInfo = challengeRankInfo;
                }

                ReqLeagueRankTop3(successCallback);
            }
            //내 랭킹이 존재하지 않음
            else {
                string statusCode = getRes.GetStatusCode();
                Debug.Log($"내 랭킹이 없습니다. StatusCode : {statusCode}, Message : {getRes.GetMessage()}");
                Dispatcher.AddAction(() => {
                    if (statusCode == "404") {
                        ReqLeagueRankTop3(successCallback);
                    }
                    else if (successCallback != null)
                        successCallback();
                });
            }
        });
    }

    public void ReqUserNumber(Callback successCallback) {
        if (UserDataModel.instance.userProfile.userNumber > 0) {
            if (UserDataModel.instance.publicUserData.no == 0 ||
                UserDataModel.instance.publicUserData.active == 0) {
                UserDataModel.instance.publicUserData.no = UserDataModel.instance.userProfile.userNumber;
                UserDataModel.instance.publicUserData.active = 1;
                UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.PUBLIC_USER_DATAS);
                ReqSyncUserData(false, false, successCallback);
                return;
            }
            else {
                successCallback();
                return;
            }
        }

        NetworkLoading.instance.Show();
        string rowInDate = "2021-11-08T11:45:57.146Z";
        string ownerInDate = "2021-04-16T12:22:34.255Z";

        Backend.GameData.GetV2("userNumber", rowInDate, ownerInDate, res => {
            NetworkLoading.instance.Hide();
            if (res.IsSuccess()) {
                JsonData resData = res.GetReturnValuetoJSON();
                JsonData row = resData["row"];

                long.TryParse(row["number"]["N"].ToString(), out UserDataModel.instance.userProfile.userNumber);
                UserDataModel.instance.publicUserData.no = UserDataModel.instance.userProfile.userNumber;
                UserDataModel.instance.publicUserData.active = 1;
                UserDataModel.instance.SaveUserDatas(true,
                                                    USER_DATA_KEY.USER_PROFILE,
                                                    USER_DATA_KEY.PUBLIC_USER_DATAS);

                ReqIncreaseUserNumber(UserDataModel.instance.userProfile.userNumber, successCallback);
            }
        });
    }

    private void ReqIncreaseUserNumber(long userNumber, Callback successCallback) {
        NetworkLoading.instance.Show();
        string rowInDate = "2021-11-08T11:45:57.146Z";
        string ownerInDate = "2021-04-16T12:22:34.255Z";
        Param param = new Param();
        param.Add("number", userNumber + 1);

        Backend.GameData.UpdateV2("userNumber", rowInDate, ownerInDate, param, res => {
            NetworkLoading.instance.Hide();
            if (res.IsSuccess()) {
                JsonData resData = res.GetReturnValuetoJSON();

                Dispatcher.AddAction(() => {
                    ReqSyncUserData(false, false, successCallback);
                });
            }
        });
    }

    public void ReqUpdateNickname(string inputNickname, Callback successCallback, Callback duplicatedCallback, Callback failCallback) {
        NetworkLoading.instance.Show();
        SendQueue.Enqueue(Backend.BMember.UpdateNickname, inputNickname, (res) => {
            Dispatcher.AddAction(() => NetworkLoading.instance.Hide());

            if (res.IsSuccess()) {
                BackendLogin.instance.nickname = inputNickname;
                UserDataModel.instance.publicUserData.nickname = inputNickname;
                if (UserDataModel.instance.userProfile.freeNicknameChangeCount > 0)
                    UserDataModel.instance.userProfile.freeNicknameChangeCount--;
                else
                    UserDataModel.instance.UseDiamond(Constant.CHANGE_NICKNAME_COST_DIAMOND);
                UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.USER_PROFILE, USER_DATA_KEY.PUBLIC_USER_DATAS);

                if (UserDataModel.instance.leagueScoreInfo.leagueID > 0 && UserDataModel.instance.leagueScoreInfo.leagueScore > 0)
                    ReqUpdateLeagueScore(UserDataModel.instance.leagueScoreInfo.leagueScore, successCallback);
                else
                    successCallback();
            }
            //중복된 닉네임이 있는 경우 FailCallback
            else if (res.GetStatusCode() == "409") {
                Dispatcher.AddAction(() => {
                    duplicatedCallback();
                });
            }
            //닉네임 작성 규칙에 맞지 않는경우
            else if (res.GetStatusCode() == "400") {
                Dispatcher.AddAction(() => {
                    failCallback();
                });
            }
            //기타 네트워크 에러
            else {
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    //친구추천목록 받아오기
    public void ReqFriendRecommendList(Callback successCallback) {
        NetworkLoading.instance.Show();
        string tableName = USER_DATA_TABLE_NAME.publicUserDatas.ToString();
        string column = "active";

        SendQueue.Enqueue(Backend.Social.GetRandomUserInfo, tableName, column, 1, 0, Constant.FRIEND_RECOMMAND_COUNT, (res) => {
            Dispatcher.AddAction(() => NetworkLoading.instance.Hide());
            if (res.IsSuccess()) {
                JsonData resData = res.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                List<SimpleUserDTO> friendRecommands = new List<SimpleUserDTO>();
                for (int i = 0; i < rows.Count; i++) {
                    JsonData row = rows[i];

                    SimpleUserDTO simpleUserInfo = new SimpleUserDTO();
                    simpleUserInfo.inDate = row["inDate"].ToString();
                    if (row.ContainsKey("nickname") && row["nickname"] != null)
                        simpleUserInfo.nickname = row["nickname"].ToString();
                    else
                        simpleUserInfo.nickname = simpleUserInfo.inDate.Substring(0, 8);

                    simpleUserInfo.lastLogin = row["lastLogin"].ToString();
                    UserUtil.CheckLastLogin(simpleUserInfo);
                    friendRecommands.Add(simpleUserInfo);

                    JsonData table = row["table"];
                    PublicUserDataDTO publicUserData = new PublicUserDataDTO();
                    long.TryParse(table["no"].ToString(), out publicUserData.no);
                    long.TryParse(table["bestLeagueID"].ToString(), out publicUserData.bestLeagueID);
                    long.TryParse(table["bestLeagueScore"].ToString(), out publicUserData.bestLeagueScore);
                    long.TryParse(table["currentLeagueID"].ToString(), out publicUserData.currentLeagueID);
                    long.TryParse(table["currentLeagueScore"].ToString(), out publicUserData.currentLeagueScore);
                    publicUserData.nickname = table["nickname"].ToString();
                    long.TryParse(table["profileNo"].ToString(), out publicUserData.profileNo);
                    long.TryParse(table["profileFrameNo"].ToString(), out publicUserData.profileFrameNo);
                    publicUserData.leagueRecordList = JsonConvert.DeserializeObject<List<PublicUserDataDTO.LeagueRecordDTO>>(table["leagueRecords"].ToString());
                    publicUserData.lastLogin = simpleUserInfo.lastLogin;

                    PublicUserDataManager.instance.SetPublicUserData(simpleUserInfo.inDate, publicUserData);
                }
                UserDataModel.instance.friendRecommands = friendRecommands;
                Dispatcher.AddAction(() => successCallback());
            }
            else {
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    public void ReqFindUserByNickName(string nickname, Callback successCallback) {
        SendQueue.Enqueue(Backend.Social.GetUserInfoByNickName, nickname, (res) => {
            UserDataModel.instance.findUserInfos.Clear();

            if (res.IsSuccess()) {
                JsonData resData = res.GetReturnValuetoJSON();
                if (resData.Count > 0) {
                    JsonData row = resData["row"];

                    SimpleUserDTO findUserInfo = new SimpleUserDTO();
                    findUserInfo.nickname = row["nickname"].ToString();
                    findUserInfo.inDate = row["inDate"].ToString();
                    findUserInfo.lastLogin = row["lastLogin"].ToString();
                    UserUtil.CheckLastLogin(findUserInfo);
                    UserDataModel.instance.findUserInfos.Add(findUserInfo);
                }
                Dispatcher.AddAction(() => successCallback());
            }
            else {
                if (res.GetStatusCode() == "404") {
                    Dispatcher.AddAction(() => successCallback());
                }
                else
                    NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    public void ReqFindUserByNumber(int number, Callback successCallback) {
        NetworkLoading.instance.Show();
        string tableName = USER_DATA_TABLE_NAME.publicUserDatas.ToString();
        string column = "no";

        SendQueue.Enqueue(Backend.Social.GetRandomUserInfo, tableName, column, number, 0, Constant.FRIEND_RECOMMAND_COUNT, (res) => {
            Dispatcher.AddAction(() => NetworkLoading.instance.Hide());
            if (res.IsSuccess()) {
                JsonData resData = res.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                UserDataModel.instance.findUserInfos.Clear();

                if (rows.Count > 0) {
                    JsonData row = rows[0];

                    SimpleUserDTO findUserInfo = new SimpleUserDTO();
                    findUserInfo.inDate = row["inDate"].ToString();
                    if (row.ContainsKey("nickname") && row["nickname"] != null)
                        findUserInfo.nickname = row["nickname"].ToString();
                    else
                        findUserInfo.nickname = findUserInfo.inDate.Substring(0, 8);

                    findUserInfo.lastLogin = row["lastLogin"].ToString();
                    UserDataModel.instance.findUserInfos.Add(findUserInfo);

                    JsonData table = row["table"];
                    PublicUserDataDTO publicUserData = new PublicUserDataDTO();
                    long.TryParse(table["no"].ToString(), out publicUserData.no);
                    long.TryParse(table["bestLeagueID"].ToString(), out publicUserData.bestLeagueID);
                    long.TryParse(table["bestLeagueScore"].ToString(), out publicUserData.bestLeagueScore);
                    long.TryParse(table["currentLeagueID"].ToString(), out publicUserData.currentLeagueID);
                    long.TryParse(table["currentLeagueScore"].ToString(), out publicUserData.currentLeagueScore);
                    publicUserData.nickname = table["nickname"].ToString();
                    long.TryParse(table["profileNo"].ToString(), out publicUserData.profileNo);
                    long.TryParse(table["profileFrameNo"].ToString(), out publicUserData.profileFrameNo);
                    publicUserData.leagueRecordList = JsonConvert.DeserializeObject<List<PublicUserDataDTO.LeagueRecordDTO>>(table["leagueRecords"].ToString());
                    publicUserData.lastLogin = findUserInfo.lastLogin;

                    PublicUserDataManager.instance.SetPublicUserData(findUserInfo.inDate, publicUserData);
                }

                Dispatcher.AddAction(() => successCallback());
            }
            else {
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    public void ReqFriendList(Callback successCallback) {
        NetworkLoading.instance.Show();
        SendQueue.Enqueue(Backend.Social.Friend.GetFriendList, Constant.FRIEND_COUNT_LIMIT, (res) => {
            Dispatcher.AddAction(() => NetworkLoading.instance.Hide());
            if (res.IsSuccess()) {
                JsonData resData = res.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                UserDataModel.instance.getFriendList = true;

                List<SimpleUserDTO> friends = new List<SimpleUserDTO>();
                for (int i = 0; i < rows.Count; i++) {
                    JsonData row = rows[i];
                    SimpleUserDTO simpleUserInfo = new SimpleUserDTO();
                    simpleUserInfo.nickname = row["nickname"]["S"].ToString();
                    simpleUserInfo.inDate = row["inDate"]["S"].ToString();
                    simpleUserInfo.lastLogin = row["lastLogin"]["S"].ToString();
                    UserUtil.CheckLastLogin(simpleUserInfo);
                    friends.Add(simpleUserInfo);
                }
                UserDataModel.instance.friends = friends;
                Dispatcher.AddAction(() => {
                    if (successCallback != null)
                        successCallback();
                });
            }
            else {
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    public void ReqFriendInvite(string friendInDate, Callback successCallback = null) {
        NetworkLoading.instance.Show();
        SendQueue.Enqueue(Backend.Social.Friend.RequestFriend, friendInDate, (res) => {
            Dispatcher.AddAction(() => NetworkLoading.instance.Hide());
            if (res.IsSuccess()) {
                UserDataModel.instance.RemoveFindUserInfoAfterInvite(friendInDate);
                string msg = TermModel.instance.GetTerm("msg_send_friend_request");
                Dispatcher.AddAction(() => {
                    MessageUtil.ShowSimpleWarning(msg);
                });
            }
            else {
                switch (res.GetStatusCode()) {
                    case "412": {//request 가 꽉 찬 경우
                        string msg;
                        if (res.GetMessage().Contains("Send"))
                            msg = TermModel.instance.GetTerm("msg_user_friend_count_limit");
                        else
                            msg = TermModel.instance.GetTerm("msg_friend_count_limit");
                        Dispatcher.AddAction(() => {
                            MessageUtil.ShowSimpleWarning(msg);
                        });
                        break;
                    }
                    case "409": {//중복요청인경우
                        string msg = TermModel.instance.GetTerm("msg_friend_already_send");
                        Dispatcher.AddAction(() => {
                            MessageUtil.ShowSimpleWarning(msg);
                        });
                        break;
                    }
                    default:
                        NetworkErrorHandler.instance.OnFail(res, null);
                        break;
                }
            }
            if (successCallback != null)
                successCallback();
        });
    }

    public void ReqFriendAccept(UserData.SimpleUserDTO simpleUserInfo, Callback successCallback = null) {
        NetworkLoading.instance.Show();
        SendQueue.Enqueue(Backend.Social.Friend.AcceptFriend, simpleUserInfo.inDate, (res) => {
            Dispatcher.AddAction(() => NetworkLoading.instance.Hide());
            if (res.IsSuccess()) {
                UserDataModel.instance.RemoveFriendReceives(simpleUserInfo.inDate);
                UserDataModel.instance.friends.Add(simpleUserInfo);
                string msg = TermModel.instance.GetTerm("msg_accept_friend_request");
                Dispatcher.AddAction(() => {
                    EventManager.Notify(EventEnum.FriendsStateUpdate);
                    MessageUtil.ShowSimpleWarning(msg);
                });
            }
            else {
                switch (res.GetStatusCode()) {
                    case "412": {//request 가 꽉 찬 경우
                        string msg;
                        if (res.GetMessage().Contains("Requested"))
                            msg = TermModel.instance.GetTerm("msg_friend_count_limit");
                        else
                            msg = TermModel.instance.GetTerm("msg_user_friend_count_limit");
                        Dispatcher.AddAction(() => {
                            MessageUtil.ShowSimpleWarning(msg);
                        });
                        break;
                    }
                    default:
                        NetworkErrorHandler.instance.OnFail(res, null);
                        break;
                }
            }
            if (successCallback != null)
                successCallback();
        });
    }

    public void ReqFriendReject(string friendInDate, Callback successCallback = null) {
        NetworkLoading.instance.Show();
        SendQueue.Enqueue(Backend.Social.Friend.RejectFriend, friendInDate, (res) => {
            Dispatcher.AddAction(() => NetworkLoading.instance.Hide());
            if (res.IsSuccess()) {
                UserDataModel.instance.RemoveFriendReceives(friendInDate);
                string msg = TermModel.instance.GetTerm("msg_reject_friend_request");
                Dispatcher.AddAction(() => {
                    MessageUtil.ShowSimpleWarning(msg);
                });
            }
            else {
                switch (res.GetStatusCode()) {
                    case "412": {//request 가 꽉 찬 경우
                        string msg = TermModel.instance.GetTerm("msg_user_friend_count_limit");
                        Dispatcher.AddAction(() => {
                            MessageUtil.ShowSimpleWarning(msg);
                        });
                        break;
                    }
                    default:
                        NetworkErrorHandler.instance.OnFail(res, null);
                        break;
                }
            }
            if (successCallback != null)
                successCallback();
        });
    }

    public void ReqFriendReceivedList(Callback successCallback = null) {
        NetworkLoading.instance.Show();
        SendQueue.Enqueue(Backend.Social.Friend.GetReceivedRequestList, (res) => {
            Dispatcher.AddAction(() => NetworkLoading.instance.Hide());
            if (res.IsSuccess()) {
                JsonData resData = res.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                List<SimpleUserDTO> friendReceives = new List<SimpleUserDTO>();
                for (int i = 0; i < rows.Count; i++) {
                    JsonData row = rows[i];
                    SimpleUserDTO simpleUserInfo = new SimpleUserDTO();
                    simpleUserInfo.nickname = row["nickname"]["S"].ToString();
                    simpleUserInfo.inDate = row["inDate"]["S"].ToString();
                    simpleUserInfo.lastLogin = row["inDate"]["S"].ToString();
                    friendReceives.Add(simpleUserInfo);
                }
                UserDataModel.instance.friendReceives = friendReceives;
                Dispatcher.AddAction(() => {
                    if (successCallback != null)
                        successCallback();
                });
            }
            else {
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    //활성화된 리그정보를 받아온다.
    public void ReqGetCurrentLeague(Callback successCallback) {
        NetworkLoading.instance.Show();
        SendQueue.Enqueue(Backend.URank.User.GetRankTableList, (callback) => {
            Dispatcher.AddAction(() => NetworkLoading.instance.Hide());
            if (callback.IsSuccess()) {
                JsonData resData = callback.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                Debug.Log($"총 리그 수:{rows.Count}");

                List<LeagueDataDTO> leagueInfos = new List<LeagueDataDTO>();
                for (int i = 0; i < rows.Count; i++) {
                    JsonData row = rows[i];
                    if (row.ContainsKey("rankEndDateAndTime") == false)
                        continue;
                    LeagueDataDTO leagueInfo = new LeagueDataDTO();
                    string rankEndDateAndTimeStr = row["rankEndDateAndTime"]["S"].ToString();
                    leagueInfo.rankEndDateAndTime = Common.ConvertStringToTimestampUTC(rankEndDateAndTimeStr);

                    string rankStartDateAndTimeStr = row["rankStartDateAndTime"]["S"].ToString();
                    leagueInfo.rankStartDateAndTime = Common.ConvertStringToTimestampUTC(rankStartDateAndTimeStr);

                    //진행중인 리그가 아니면 리스트에 추가하지 않는다.
                    long now = Common.GetUTCNow();
                    if (now < leagueInfo.rankStartDateAndTime ||
                        now >= leagueInfo.rankEndDateAndTime) {
                        Debug.Log($"now:{now}, startTime:{leagueInfo.rankStartDateAndTime}, endTime:{leagueInfo.rankEndDateAndTime}");
                        continue;
                    }

                    leagueInfo.title = row["title"]["S"].ToString();
                    leagueInfo.uuid = row["uuid"]["S"].ToString();

                    if (LeagueUtil.IsAvailableLeague(leagueInfo.title) == false)
                        continue;

                    //seasonNo
                    Regex regex = new Regex("[0-9]+");
                    Match match = regex.Match(leagueInfo.title);
                    long.TryParse(match.Value, out leagueInfo.seasonNo);

                    leagueInfos.Add(leagueInfo);
                }
                UserDataModel.instance.leagueInfos = leagueInfos;
                Debug.Log($"받아온 리그 갯수:{leagueInfos.Count}");
                Dispatcher.AddAction(() => {
                    if (successCallback != null)
                        successCallback();
                });
            }
            else {
                NetworkErrorHandler.instance.OnFail(callback, null);
            }
        });
    }

    public void ReqUpdateLeagueScore(long score, Callback successCallback) {
        NetworkLoading.instance.Show();

        LeagueScoreDTO leagueScoreInfo = UserDataModel.instance.leagueScoreInfo;
        LeagueDataDTO leagueData = UserDataModel.instance.GetLeagueInfo(leagueScoreInfo.leagueID);
        if (leagueData == null)
            return;

        Param param = new Param();
        param.Add("leagueScore", score);
        ChallengeRankDTO.ExtensionDTO extension = new ChallengeRankDTO.ExtensionDTO();
        extension.nickname = BackendLogin.instance.nickname;
        extension.frameNo = UserDataModel.instance.publicUserData.profileFrameNo;
        extension.profileNo = UserDataModel.instance.publicUserData.profileNo;
        param.Add("extension", JsonConvert.SerializeObject(extension));

        string uuid = leagueData.uuid;

        //자릿수가 줄어든 경우 0을 채워준다.
        if (UserDataModel.instance.leagueScoreInfo.scoreInDate.Length == 23)
            UserDataModel.instance.leagueScoreInfo.scoreInDate = UserDataModel.instance.leagueScoreInfo.scoreInDate.Replace("Z", "0Z");

        SendQueue.Enqueue(Backend.URank.User.UpdateUserScore,
                          uuid,
                          Constant.LEAGUE_SCORE_TABLE_NAME,
                          UserDataModel.instance.leagueScoreInfo.scoreInDate,
                          param, (getRes) => {
                              Dispatcher.AddAction(NetworkLoading.instance.Hide);
                              if (getRes.IsSuccess()) {
                                  UserDataModel.instance.publicUserData.currentLeagueScore = score;
                                  UserDataModel.instance.leagueScoreInfo.leagueScore = score;

                                  UserDataModel.instance.SaveUserDatas(true,
                                                                       USER_DATA_KEY.PUBLIC_USER_DATAS,
                                                                       USER_DATA_KEY.LEAGUE_SCORE);

                                  Debug.Log("ReqUpdateLeagueScore Complete");
                                  Dispatcher.AddAction(() => {
                                      if (successCallback != null)
                                          successCallback();
                                  });
                              }
                              else {
                                  string format = "리그 스코어 등록중 에러 발생, StatusCode : {0}, Message : {1} , uuid : {2}";
                                  Debug.Log(string.Format(format, getRes.GetStatusCode(), getRes.GetMessage(), uuid));
                                  string statusCode = getRes.GetStatusCode();
                                  if (statusCode == "428") {
                                      Dispatcher.AddAction(ShowUpdateLeagueScoreError);
                                  }
                                  else
                                      NetworkErrorHandler.instance.OnFail(getRes, null);
                              }
                          });
    }

    private void ShowUpdateLeagueScoreError() {
        long now = Common.GetUTCNow();
        long startTime = Common.GetUTCTodayZero() + 18 * 3600 + 1800;
        long endTime = Common.GetUTCTodayZero() + 20 * 3600;

        Callback callback = () => {
            App.instance.ChangeScene(App.SCENE_NAME.Home);
        };

        if (now >= startTime && now <= endTime) {
            string startTimeStr = Common.ConvertTimestampToHM(startTime);
            string endTimeStr = Common.ConvertTimestampToHM(endTime);
            string warningFormat = TermModel.instance.GetTerm("format_league_time_warning");
            string msg = string.Format(warningFormat, startTimeStr, endTimeStr);

            MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.OK, msg, callback);
        }
        else {
            string msg = TermModel.instance.GetTerm("msg_league_end");
            MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.OK, msg, callback);
        }
    }

    public void ReqMyLeagueRanking(Callback successCallback) {
        long leagueID = UserDataModel.instance.leagueScoreInfo.leagueID;
        if (leagueID == (long)LEAGUE_LEVEL.NONE)
            return;

        string uuid = GetUUID();

        SendQueue.Enqueue(Backend.URank.User.GetMyRank, uuid, 0, (getRes) => {
            if (getRes.IsSuccess()) {
                JsonData resData = getRes.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                long rank = 0;
                long score = 0;
                if (rows.Count > 0) {
                    long.TryParse(rows[0]["rank"]["N"].ToString(), out rank);
                    long.TryParse(rows[0]["score"]["N"].ToString(), out score);
                }

                UserDataModel.instance.leagueScoreInfo.lastRank = GetMyRank(rank, score);

                long totalCount;
                long.TryParse(resData["totalCount"].ToString(), out totalCount);
                UserDataModel.instance.myLeagueUserCount = totalCount + Constant.DUMMY_RANK_COUNT;
                successCallback();
            }
            //내 랭킹이 존재하지 않음
            else {
                string statusCode = getRes.GetStatusCode();
                if (statusCode == "404") {
                    Debug.Log($"내 랭킹이 없습니다. StatusCode : {statusCode}, Message : {getRes.GetMessage()}");
                    successCallback();
                }
                else
                    NetworkErrorHandler.instance.OnFail(getRes, null);
            }
        });
    }

    public void ReqLeagueRankingForResult(Callback successCallback) {
        LeagueDataDTO leagueInfo = UserDataModel.instance.GetLeagueInfo(UserDataModel.instance.leagueScoreInfo.leagueID);
        string uUID = leagueInfo.uuid;
        SendQueue.Enqueue(Backend.URank.User.GetMyRank, uUID, Constant.RANKING_LIST_GAP, (getRes) => {
            if (getRes.IsSuccess()) {
                JsonData resData = getRes.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                List<ChallengeRankDTO> leagueRankInfos = new List<ChallengeRankDTO>();

                for (int i = 0; i < rows.Count; i++) {
                    ChallengeRankDTO challengeRankInfo = new ChallengeRankDTO();
                    long.TryParse(rows[i]["rank"]["N"].ToString(), out challengeRankInfo.rank);
                    string extensionStr = rows[i]["extension"]["S"].ToString();
                    challengeRankInfo.extension = JsonConvert.DeserializeObject<ChallengeRankDTO.ExtensionDTO>(extensionStr);
                    long.TryParse(rows[i]["score"]["N"].ToString(), out challengeRankInfo.score);
                    challengeRankInfo.gamerInDate = rows[i]["gamerInDate"]["S"].ToString();

                    leagueRankInfos.Add(challengeRankInfo);
                }

                LeagueScoreDTO leagueScoreInfo = UserDataModel.instance.leagueScoreInfo;
                LeagueDataDTO leagueData = UserDataModel.instance.GetLeagueInfo(leagueScoreInfo.leagueID);
                if (leagueData == null) {
                    UserDataModel.instance.leagueRankInfos = leagueRankInfos;
                }
                else {
                    UserDataModel.instance.leagueRankInfos = 
                        GetRankInfos(leagueScoreInfo.seasonNo, leagueScoreInfo.leagueID, leagueRankInfos, false);
                }

                foreach (var challengeRankInfo in leagueRankInfos) {
                    //내 랭킹
                    if (challengeRankInfo.gamerInDate == BackendLogin.instance.UserInDate) {
                        UserDataModel.instance.myChallengeRankInfo = challengeRankInfo;
                        UserDataModel.instance.leagueScoreInfo.leagueScore = (int)challengeRankInfo.score;
                        UserDataModel.instance.leagueScoreInfo.lastRank = (int)challengeRankInfo.rank;

                        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.LEAGUE_SCORE);
                        break;
                    }
                }

                Dispatcher.AddAction(() => {
                    successCallback();
                });
            }
            //내 랭킹이 존재하지 않음
            else {
                string statusCode = getRes.GetStatusCode();
                Debug.Log($"내 랭킹이 없습니다. StatusCode : {statusCode}, Message : {getRes.GetMessage()}");
                Dispatcher.AddAction(() => {
                    successCallback();
                });
            }
        });
    }

    public void ReqFriendBreak(string friendInDate, Callback successCallback = null) {
        NetworkLoading.instance.Show();
        SendQueue.Enqueue(Backend.Social.Friend.BreakFriend, friendInDate, (res) => {
            Dispatcher.AddAction(() => NetworkLoading.instance.Hide());
            if (res.IsSuccess()) {
                UserDataModel.instance.RemoveFriend(friendInDate);
                string msg = TermModel.instance.GetTerm("msg_break_friend");
                Dispatcher.AddAction(() => {
                    MessageUtil.ShowSimpleWarning(msg);
                });
            }
            else {
                switch (res.GetStatusCode()) {
                    case "404": {//중복요청인경우
                        string msg = TermModel.instance.GetTerm("msg_no_friend");
                        Dispatcher.AddAction(() => {
                            MessageUtil.ShowSimpleWarning(msg);
                        });
                        break;
                    }
                    default:
                        NetworkErrorHandler.instance.OnFail(res, null);
                        break;
                }
            }
            if (successCallback != null)
                successCallback();
        });
    }

    public void ReqSendChallengeMessage(ChallengeMsgDTO challengeMsgInfo, Callback successCallback) {
        List<string> infoList = new List<string>();
        infoList.Add(challengeMsgInfo.difficulty.ToString());
        infoList.Add(challengeMsgInfo.bettingGold.ToString());
        infoList.Add(challengeMsgInfo.senderInDate);
        infoList.Add(challengeMsgInfo.senderScore.ToString());
        infoList.Add(challengeMsgInfo.receiverInDate);
        infoList.Add(challengeMsgInfo.receiverScore.ToString());
        infoList.Add(challengeMsgInfo.state.ToString());
        infoList.Add(challengeMsgInfo.createDate.ToString());
        string msgStr = JsonConvert.SerializeObject(infoList);
        Debug.Log($"도전장을 보냅니다. 메세지 길이 = {msgStr.Length}");

        string ownerInDate;
        if (challengeMsgInfo.state == (long)CHALLENGE_STATE.RECEIVE) {

            ownerInDate = challengeMsgInfo.receiverInDate;
        }
        else
            ownerInDate = challengeMsgInfo.senderInDate;

        NetworkLoading.instance.Show();
        SendQueue.Enqueue(Backend.Social.Message.SendMessage, ownerInDate, msgStr, (res) => {
            Dispatcher.AddAction(() => NetworkLoading.instance.Hide());
            if (res.IsSuccess()) {
                if (challengeMsgInfo.state == (long)CHALLENGE_STATE.RECEIVE &&
                    UserDataModel.instance.userProfile.challengeSendRemainCount > 0)
                    UserDataModel.instance.userProfile.challengeSendRemainCount--;

                UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.USER_PROFILE);

                //@todo : 도전장 보냄 정보에 추가
                string msg = TermModel.instance.GetTerm($"msg_send_challenge_{challengeMsgInfo.state}");
                Dispatcher.AddAction(() => {
                    MessageUtil.ShowSimpleWarning(msg);
                    EventManager.Notify(EventEnum.ChallengeMessageSend);
                });
            }
            else {
                switch (res.GetStatusCode()) {
                    case "405": {//메세지가 꽉찬경우
                        string msg = TermModel.instance.GetTerm("msg_receiver_message_is_full");
                        Dispatcher.AddAction(() => {
                            MessageUtil.ShowSimpleWarning(msg);
                        });
                        break;
                    }
                    default:
                        NetworkErrorHandler.instance.OnFail(res, null);
                        break;
                }
            }
            if (successCallback != null)
                successCallback();
        });
    }

    public void ReqReceiveChallengeMessage(Callback successCallback) {
        ChallengeMsgDTO challengeMsgInfo = UserDataModel.instance.challengeMsgInfo;

        string msgStr = JsonConvert.SerializeObject(challengeMsgInfo);
        NetworkLoading.instance.Show();
        SendQueue.Enqueue(Backend.Social.Message.GetReceivedMessageList, (res) => {
            Dispatcher.AddAction(() => NetworkLoading.instance.Hide());
            if (res.IsSuccess()) {
                JsonData resData = res.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                UserDataModel.instance.receivedFriendMessages.Clear();
                for (int i = 0; i < rows.Count; i++) {
                    JsonData row = rows[i];
                    FriendMessageDTO messageInfo = new FriendMessageDTO();
                    messageInfo.msgInDate = row["inDate"]["S"].ToString();

                    bool.TryParse(row["isRead"]["BOOL"].ToString(), out messageInfo.isRead);
                    bool.TryParse(row["isReceiverDelete"]["BOOL"].ToString(), out messageInfo.isReceiverDelete);
                    bool.TryParse(row["isSenderDelete"]["BOOL"].ToString(), out messageInfo.isSenderDelete);
                    messageInfo.content = row["content"]["S"].ToString();
                    List<string> challengeMsgInfoList = JsonConvert.DeserializeObject<List<string>>(messageInfo.content);
                    messageInfo.challengeMsgInfo = new ChallengeMsgDTO();
                    long.TryParse(challengeMsgInfoList[(int)CHALLENGE_MSG_INDEX.DIFFICULTY],
                                  out messageInfo.challengeMsgInfo.difficulty);
                    long.TryParse(challengeMsgInfoList[(int)CHALLENGE_MSG_INDEX.BETTING_GOLD],
                                  out messageInfo.challengeMsgInfo.bettingGold);
                    messageInfo.challengeMsgInfo.senderInDate = challengeMsgInfoList[(int)CHALLENGE_MSG_INDEX.SENDER_IN_DATE];
                    long.TryParse(challengeMsgInfoList[(int)CHALLENGE_MSG_INDEX.SENDER_SCORE],
                                  out messageInfo.challengeMsgInfo.senderScore);
                    messageInfo.challengeMsgInfo.receiverInDate = challengeMsgInfoList[(int)CHALLENGE_MSG_INDEX.RECEIVER_IN_DATE];
                    long.TryParse(challengeMsgInfoList[(int)CHALLENGE_MSG_INDEX.RECEIVER_SCORE],
                                  out messageInfo.challengeMsgInfo.receiverScore);
                    long.TryParse(challengeMsgInfoList[(int)CHALLENGE_MSG_INDEX.STATE],
                                    out messageInfo.challengeMsgInfo.state);

                    if (challengeMsgInfoList.Count > 7) {
                        long.TryParse(challengeMsgInfoList[(int)CHALLENGE_MSG_INDEX.CREATE_DATE],
                                    out messageInfo.challengeMsgInfo.createDate);
                    }

                    UserDataModel.instance.receivedFriendMessages.Add(messageInfo);
                }

                Dispatcher.AddAction(() => {
                    if (UserDataModel.instance.receivedFriendMessages.Count > 0)
                        EventManager.Notify(EventEnum.NotifiedNewMessage);
                    if (successCallback != null)
                        successCallback();
                });
            }
            else {
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    public void ReqDeleteReceivedChallengeMessage(string msgInDate, Callback successCallback) {
        ChallengeMsgDTO challengeMsgInfo = UserDataModel.instance.challengeMsgInfo;

        string msgStr = JsonConvert.SerializeObject(challengeMsgInfo);
        NetworkLoading.instance.Show();
        SendQueue.Enqueue(Backend.Social.Message.DeleteReceivedMessage, msgInDate, (res) => {
            Dispatcher.AddAction(() => NetworkLoading.instance.Hide());
            if (res.IsSuccess()) {
                Dispatcher.AddAction(() => {
                    if (successCallback != null)
                        successCallback();
                });
            }
            else {
                switch (res.GetStatusCode()) {
                    case "404":
                        Dispatcher.AddAction(() => {
                            if (successCallback != null)
                                successCallback();
                        });
                        break;
                    default:
                        NetworkErrorHandler.instance.OnFail(res, null);
                        break;
                }
            }
        });
    }

    public void ReqGetPublicUserDataByInDate(string inDate) {
        string tableName = USER_DATA_TABLE_NAME.publicUserDatas.ToString();
        Where where = new Where();
        where.Equal("owner_inDate", inDate);

        SendQueue.Enqueue(Backend.GameData.Get, tableName, where, (res) => {
            if (res.IsSuccess()) {
                JsonData resData = res.GetReturnValuetoJSON();
                JsonData rows = resData["rows"];

                if (rows.Count > 0) {
                    JsonData row = rows[0];

                    PublicUserDataDTO publicUserData = new PublicUserDataDTO();
                    long.TryParse(row["no"]["N"].ToString(), out publicUserData.no);
                    long.TryParse(row["bestLeagueID"]["N"].ToString(), out publicUserData.bestLeagueID);
                    long.TryParse(row["bestLeagueScore"]["N"].ToString(), out publicUserData.bestLeagueScore);
                    long.TryParse(row["currentLeagueID"]["N"].ToString(), out publicUserData.currentLeagueID);
                    long.TryParse(row["currentLeagueScore"]["N"].ToString(), out publicUserData.currentLeagueScore);
                    publicUserData.nickname = row["nickname"]["S"].ToString();
                    long.TryParse(row["profileNo"]["N"].ToString(), out publicUserData.profileNo);
                    long.TryParse(row["profileFrameNo"]["N"].ToString(), out publicUserData.profileFrameNo);
                    publicUserData.leagueRecordList = JsonConvert.DeserializeObject<List<PublicUserDataDTO.LeagueRecordDTO>>(row["leagueRecords"]["S"].ToString());

                    EventManager.Notify(EventEnum.GetPublicUserData, inDate, publicUserData);
                }
            }
        });
    }
}
