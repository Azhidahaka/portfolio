using LuckyFlow.Event;
using Newtonsoft.Json;
using QuantumTek.EncryptedSave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicUserDataManager : MonoBehaviour {
    public static PublicUserDataManager instance;

    private Queue<UserData.SimpleUserDTO> queue = new Queue<UserData.SimpleUserDTO>();
    private Dictionary<string, UserData.PublicUserDataDTO> dicPublicUserData = new Dictionary<string, UserData.PublicUserDataDTO>();

    private bool waiting = false;
    private UserData.SimpleUserDTO waitingUserInfo;

    private void Awake() {
        instance = this;
        Load();
    }

    private void Load() {
        if (ES_Save.Exists(Constant.PUBLIC_USER_DATAS_PATH)) {
            string dicStr = ES_Save.Load<string>(Constant.PUBLIC_USER_DATAS_PATH);
            dicPublicUserData = JsonConvert.DeserializeObject<Dictionary<string, UserData.PublicUserDataDTO>>(dicStr);
        }
    }

    private void Start() {
        StartCoroutine(JobGetPublicUserData());
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.GetPublicUserData, OnGetPublicUserData);
    }

    private void OnGetPublicUserData(object[] args) {
        string inDate = (string)args[0];
        UserData.PublicUserDataDTO publicUserData = (UserData.PublicUserDataDTO)args[1];
        publicUserData.lastLogin = waitingUserInfo.lastLogin;
        UserUtil.CheckLastLogin(waitingUserInfo);
        Debug.LogWarning($"WAITION_USER_INFO::LASTLOGIN = {waitingUserInfo.lastLogin}");

        if (dicPublicUserData.ContainsKey(inDate))
            dicPublicUserData.Remove(inDate);
        
        dicPublicUserData.Add(inDate, publicUserData); 
        Save();

        waiting = false;
    }

    private void Save() {
        int removeCount = 0;
        //정해진 개수만큼만 저장하고, 초과하는 데이터는 앞에서부터 버린다.
        if (dicPublicUserData.Count > Constant.PUBLIC_USER_DATA_SAVE_MAX) {
            removeCount = dicPublicUserData.Count - (int)Constant.PUBLIC_USER_DATA_SAVE_MAX;
        }

        List<string> removeKeys = new List<string>();
        foreach (KeyValuePair<string, UserData.PublicUserDataDTO> pair in dicPublicUserData) {
            if (removeCount == 0)
                break;

            removeKeys.Add(pair.Key);
            removeCount--;
        }

        for (int i = 0; i < removeKeys.Count; i++) {
            dicPublicUserData.Remove(removeKeys[i]);
        }

        string dicStr = JsonConvert.SerializeObject(dicPublicUserData);
        ES_Save.Save(dicStr, Constant.PUBLIC_USER_DATAS_PATH);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.GetPublicUserData, OnGetPublicUserData);
    }

    private IEnumerator JobGetPublicUserData() {
        while (true) {
            yield return new WaitWhile(() => queue.Count == 0);

            UserData.SimpleUserDTO simpleUserInfo = queue.Dequeue();
            string inDate = simpleUserInfo.inDate;
            //이미 받아온 데이터가 있으며, 마지막 로그인시각이 같은경우 새로 갱신하지 않는다.

            if (dicPublicUserData.ContainsKey(inDate) && 
                Common.ConvertStringToTimestamp(simpleUserInfo.lastLogin) <= 
                Common.ConvertStringToTimestamp(dicPublicUserData[inDate].lastLogin))
                continue;

            waitingUserInfo = simpleUserInfo;

            BackendRequest.instance.ReqGetPublicUserDataByInDate(inDate);
            waiting = true;
            yield return new WaitWhile(() => waiting);
        }
    }

    public UserData.PublicUserDataDTO GetPublicUserData(UserData.SimpleUserDTO simpleUserInfo) {
        if (TutorialManager.instance.IsTutorialInProgress())
            return TutorialUtil.GetTutorialPublicUserData();

        UserUtil.CheckLastLogin(simpleUserInfo);

        if (dicPublicUserData.ContainsKey(simpleUserInfo.inDate)) {
            //옛날정보인 경우 갱신을 요청한다.
            
            if (string.IsNullOrEmpty(simpleUserInfo.lastLogin) ||simpleUserInfo.lastLogin.Length != 24) {
                simpleUserInfo.lastLogin = simpleUserInfo.inDate;
                dicPublicUserData[simpleUserInfo.inDate].lastLogin = simpleUserInfo.inDate;
            }

            if (Common.ConvertStringToTimestamp(simpleUserInfo.lastLogin) > 
                Common.ConvertStringToTimestamp(dicPublicUserData[simpleUserInfo.inDate].lastLogin))
                queue.Enqueue(simpleUserInfo);
            return dicPublicUserData[simpleUserInfo.inDate];
        }

        //내 inDate라면 굳이 받아올필요가 없다
        if (simpleUserInfo.inDate == BackendLogin.instance.UserInDate)
            return UserDataModel.instance.publicUserData;

        queue.Enqueue(simpleUserInfo);
        return null;
    }

    public void SetPublicUserData(string inDate, UserData.PublicUserDataDTO publicUserData) {
        if (dicPublicUserData.ContainsKey(inDate))
            dicPublicUserData.Remove(inDate);
        
        dicPublicUserData.Add(inDate, publicUserData); 
        Save();
    }
}
