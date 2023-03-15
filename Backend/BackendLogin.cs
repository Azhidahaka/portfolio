using UnityEngine;
using BackEnd;
using GooglePlayGames.BasicApi;
using GooglePlayGames;
using System;
using LitJson;
using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using UnityEngine.UI;
using QuantumTek.EncryptedSave;
using Assets.SimpleEncryption;
using Facebook.Unity;
using System.Collections.Generic;
using AppleAuth;
using AppleAuth.Native;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using System.Text;
using System.Collections;

public class BackendLogin : MonoBehaviour {
    public enum VERSION_INDEX {
        MAJOR = 0,
        MINOR = 1,
        PATCH = 2,
    }

    public enum PlayerID {
        test1,
        test2
    }

    public static BackendLogin instance;

    public double serverTime;
    public double startTime;

    public string nickname;

    private string userInDate;
    private PlayerID playerID;
    private bool init = false;

    //비동기로 가입, 로그인을 할때에는 Update()에서 처리를 해야합니다. 이 값은 Update에서 구현하기 위한 플래그 값 입니다.
    private BackendReturnObject bro = new BackendReturnObject();
    private bool isSuccess = false;
    private string currentVersion = "";

    private string subscriptionType;

    private Callback callback;
    private bool gameCenter = false;

    private AppleAuthManager appleAuthManager;

    private Callback initCallback;
    private string userID;

    private long authorizedTime;
    

    public enum UPDATE_STATE {
        NONE = 0,
        FORCED = 1,
        SELECTABLE = 2,
    }

    public UPDATE_STATE updateState = UPDATE_STATE.NONE;

    public string UserInDate {
        get {
            return userInDate;
        }
    }

    private void Awake() {
        instance = this;
    }

    public void InitPlatforms() {
        InitApple();
        InitGooglePlayGames();
        InitFacebook();
    }

    private void InitApple() {
        if (AppleAuthManager.IsCurrentPlatformSupported) {
            PayloadDeserializer deserializer = new PayloadDeserializer();
            appleAuthManager = new AppleAuthManager(deserializer);
        }
    }

    private void InitGooglePlayGames() {
#if UNITY_ANDROID        
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration
            .Builder()
            .RequestServerAuthCode(false)
            .RequestEmail()
            .RequestIdToken()
            .Build();

        //커스텀된 정보로 GPGS 초기화
        PlayGamesPlatform.InitializeInstance(config);
#if UNITY_ANDROID && DEVELOPMENT_BUILD
        PlayGamesPlatform.DebugLogEnabled = true;
#else
        PlayGamesPlatform.DebugLogEnabled = false;
#endif
        PlayGamesPlatform.Activate();
#endif
    }

    private void InitFacebook() {
        if (FB.IsInitialized == false)
            FB.Init(OnFBInitComplete, OnHideUnity);
    }

    private void OnFBInitComplete() {
        if (FB.IsInitialized)
            FB.ActivateApp();
        else
            Debug.Log("Facebook 초기화 실패");
    }

    public void OnBtnFacebookLoginClick() {
        List<string> permissions = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(permissions, OnFBAuthComplete);
    }

    private void OnFBAuthComplete(ILoginResult result) {
        if (FB.IsLoggedIn) {
            LoginWithFacebookToken();
        }
        else {
            // 로그인 실패
            Debug.LogError($"Facebook 로그인 실패::{result.Error}");
            Dispatcher.AddAction(ShowLoginSelect);
        }
    }

    private void LoginWithFacebookToken() {
        AccessToken accessToken = AccessToken.CurrentAccessToken;
        string facebookToken = accessToken.TokenString;

        BackendReturnObject bro = Backend.BMember.AuthorizeFederation(facebookToken, FederationType.Facebook, "FB Login");
        if (bro.IsSuccess()) {
            FB.API("/me?fields=email", HttpMethod.GET, GetFacebookInfo, new Dictionary<string, string>() { });
            Debug.Log("Facebook 로그인 성공");
        }
        else {
            NetworkErrorHandler.instance.OnFail(bro, LoginWithFacebookToken);
        }
    }

    public void GetFacebookInfo(IResult result) {
        if (result.Error == null) {
            //Debug.Log(result.ResultDictionary["id"].ToString());
            //Debug.Log(result.ResultDictionary["name"].ToString());
            if (result.ResultDictionary.ContainsKey("email"))
                userID = result.ResultDictionary["email"].ToString();
            ShowLog($"LoginWithFacebookToken::{userID}");
        }
        else {
            Debug.Log(result.Error);
        }

        OnBackendAuthorized("Facebook");
    }

    private void OnHideUnity(bool isGameShown) {
        if (isGameShown == false)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    public void Initialize() {
        currentVersion = Application.version;
        // ----- 뒤끝 -----
        NetworkLoading.instance.Show(); //프로세싱 팝업 켜기
        try {
            Backend.InitializeAsync(true, (bro) => { // 비동기 
                if (bro.IsSuccess()) {
                    if (Backend.IsInitialized) {
                        init = true;
                        SendQueue.StartSendQueue();
                        Dispatcher.AddAction(InitComplete); //프로세싱 팝업 켜기);

                        // 서버시간 획득
                        BackendReturnObject resServerTime = Backend.Utils.GetServerTime();
                        ResGetServerTime(resServerTime);

                        // Application 버전 확인
                        CheckApplicationVersion();
                    }
                    else {
                        Debug.LogWarning("Backend.IsInitialized == false, 재시도");
                        NetworkErrorHandler.instance.OnFail(null, Initialize);
                    }
                }
                else {
                    Debug.LogWarning("Backend 초기화 실패, 재시도");
                    NetworkErrorHandler.instance.OnFail(bro, Initialize);
                }
            });
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

    public void SetInitCallback(Callback initCallback) {
        this.initCallback = initCallback;
    }

    private void InitComplete() {
        if (initCallback != null) {
            initCallback();
            initCallback = null;
        }

        NetworkLoading.instance.Hide();
        InitPlatforms();
        App.instance.ChangeScene(App.SCENE_NAME.Title);
    }

    private void CheckApplicationVersion() {
        Backend.Utils.GetLatestVersion(versionBRO => {
            if (versionBRO.IsSuccess()) {
                string latest = versionBRO.GetReturnValuetoJSON()["version"].ToString();
                Debug.Log("version info - current: " + currentVersion + " latest: " + latest);
                if (IsOldVersion(currentVersion, latest)) {
                    int type = (int)versionBRO.GetReturnValuetoJSON()["type"];
                    // type = 1 : 선택, type = 2 : 강제
                    bool forced = type == 2;
                    if (forced)
                        updateState = UPDATE_STATE.FORCED;
                    else
                        updateState = UPDATE_STATE.SELECTABLE;
                }
                else 
                    TryLoginWithBackendToken();
            }
            else {
                // 뒷끝 토큰 로그인 시도
                TryLoginWithBackendToken();
            }
        });
    }

    private void TryLoginWithBackendToken() {
        SendQueue.Enqueue(Backend.BMember.RefreshTheBackendToken, res => {
            if (res.IsSuccess()) {
                Dispatcher.AddAction(() => {
                    Debug.Log($"RefreshTheBackendToken::Success");
                    LoginWithTheBackendToken();
                });
            }
            else {
                ResetLoginValues();
                StopCoroutine(JobCheckAuthorizedTime());
                Dispatcher.AddAction(NotifyAuthStateChanged);
            }
        });
    }

    bool IsOldVersion(string current, string latest) {
        string[] currentNumbers = current.Split('.');
        string[] latestNumbers = latest.Split('.');

        long currentLong;
        long latestLong;

        long.TryParse(currentNumbers[(int)VERSION_INDEX.MAJOR], out currentLong);
        long.TryParse(latestNumbers[(int)VERSION_INDEX.MAJOR], out latestLong);

        if (currentLong > latestLong)
            return false;
        else if (currentLong < latestLong)
            return true;

        long.TryParse(currentNumbers[(int)VERSION_INDEX.MINOR], out currentLong);
        long.TryParse(latestNumbers[(int)VERSION_INDEX.MINOR], out latestLong);

        if (currentLong > latestLong)
            return false;
        else if (currentLong < latestLong)
            return true;

        long.TryParse(currentNumbers[(int)VERSION_INDEX.PATCH], out currentLong);
        long.TryParse(latestNumbers[(int)VERSION_INDEX.PATCH], out latestLong);

        if (currentLong >= latestLong)
            return false;
        else if (currentLong < latestLong)
            return true;

        return false;
    }

    private void ResGetServerTime(BackendReturnObject res) {
        string time = res.GetReturnValuetoJSON()["utcTime"].ToString();
        DateTime parsedDate = DateTime.Parse(time);
        TimeSpan timeSpan = ( parsedDate - new DateTime(1970, 1, 1, 0, 0, 0) );
        serverTime = timeSpan.TotalSeconds;
        startTime = Time.realtimeSinceStartup;
    }

    // ##### 로그인 #####
    // 기기에 저장된 뒤끝 AccessToken으로 로그인 (페더레이션, 커스텀 회원가입 또는 로그인 이후에 시도 가능)
    public void LoginWithTheBackendToken() {
        SendQueue.Enqueue(Backend.BMember.LoginWithTheBackendToken, loginBro => // 비동기
        {
            if (loginBro.IsSuccess()) {
                // 뒤끝 토큰 로그인 성공
                isSuccess = true;
                bro = loginBro;
            }
            Dispatcher.AddAction(NotifyAuthStateChanged);
        });
    }

    private void ThreadExceptionEventFunc(string errorMsg) {// 비동기 큐 예외상황 리턴
        Debug.Log("비동기 큐 오류 메시지 : " + errorMsg);
    }

    public void OnBackendAuthorized(string msg = "") {
        if (msg.Length > 0)
            Debug.Log($"OnBackendAuthorized::{msg}");
        ReqGetUserInfo();
    }

    // ##### 유저 정보 #####
    private void ReqGetUserInfo() {
        Backend.BMember.GetUserInfo(ResGetUserInfo);
    }

    private void ResGetUserInfo(BackendReturnObject res) {
        if (res.IsSuccess()) {
            Debug.Log("유저데이터 받아오기 성공");

            JsonData resData = res.GetReturnValuetoJSON();
            JsonData row = resData["row"];
            userInDate =row["inDate"].ToString();
            subscriptionType = row["subscriptionType"].ToString();

            Debug.Log("userInDate::" + userInDate);
            UserDataModel.instance.LoadLocalUserDatas();
            Dispatcher.AddAction(NotifyAuthStateChanged);

            if (userID != null && userID != "")
                UserDataModel.instance.userProfile.userName = userID;

            if (row.ContainsKey("nickname") && row["nickname"] != null) {
                nickname = row["nickname"].ToString();
            }

            authorizedTime = Common.GetUTCNow();
            StopCoroutine(JobCheckAuthorizedTime());
            StartCoroutine(JobCheckAuthorizedTime());

            BackendRequest.instance.ReqSyncUserData(false, true, () => {
                AdsManagerAdmob.instance.DetermineRequestBanner();
            });
        }
        else {
            Debug.Log($"유저데이터 받아오기 실패");
            NetworkErrorHandler.instance.OnFail(res, ReqGetUserInfo);
        }
    }

    private IEnumerator JobCheckAuthorizedTime() {
        while(true) {
            long now = Common.GetUTCNow();
            if (authorizedTime > 0 &&
                now - authorizedTime > 600) {
                CheckAccessToken();
                authorizedTime = now;
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    //@todo : 강제업데이트시 표시
    // 마켓 링크 열기
    public void GetLatestVersion() {
#if UNITY_ANDROID
        Application.OpenURL("market://details?id=" + Application.identifier);
#elif UNITY_IOS
        LANGUAGE language;
        if (Application.systemLanguage == SystemLanguage.Korean)
            language = LANGUAGE.kor;
        else
            language = LANGUAGE.eng;
        Application.OpenURL(Constant.APPLE_APP_STORE_DIC[language]);
#endif
    }

    void Update() {
        Backend.AsyncPoll();

        if (appleAuthManager != null)
            appleAuthManager.Update();

        if (init == false)
            return;

        if (isSuccess) {
            isSuccess = false;
            bro.Clear();
            OnBackendAuthorized("LoginWithBackendToken");
        }

        Dispatcher.Instance.InvokePending();
        SendQueue.Poll();
    }

    public void OnCustomLoginClick() {
        Debug.Log("커스텀로그인");
        if (DebugUI.instance == null ||
            DebugUI.instance.IsTest1())
            playerID = PlayerID.test1;
        else
            playerID = PlayerID.test2;

        CustomLogin(playerID.ToString(), "1", OnCustomLoginClick);
    }

    private void CustomLogin(string id, string password, Callback errorCallback) {
        bro = Backend.BMember.CustomLogin(id, password);
        if (bro.IsSuccess()) {
            // 성공시
            OnBackendAuthorized("Custom");
            Debug.Log("커스텀 로그인 성공");
        }
        else {
            NetworkErrorHandler.instance.OnFail(bro, errorCallback);
        }
    }

    // GPGS 로그인 
    public void OnBtnGoogleLoginClick() { // 로그인으로 해야 될 경우, 동기 형식이 더 편하다
#if UNITY_ANDROID
        // 이미 로그인 된 경우
        if (Social.localUser.authenticated == true) {
            bro = Backend.BMember.AuthorizeFederation(GetGoogleToken(), FederationType.Google, "gpgs");
            if (bro.IsSuccess()) {
                // 성공시
                userID = ( (PlayGamesLocalUser)Social.localUser ).Email;
                ShowLog($"GPGS 로그인 성공::{userID}");
                OnBackendAuthorized("GPGS");
            }
            else {
                NetworkErrorHandler.instance.OnFail(bro, OnBtnGoogleLoginClick);
            }
        }
        else {
            Social.localUser.Authenticate((bool success) => {
                if (success) {
                    // 로그인 성공 -> 뒤끝 서버에 획득한 구글 토큰으로 가입요청
                    bro = Backend.BMember.AuthorizeFederation(GetGoogleToken(), FederationType.Google, "gpgs");

                    if (bro.IsSuccess()) {
                        // 성공시
                        userID = ( (PlayGamesLocalUser)Social.localUser ).Email;
                        ShowLog($"GPGS 로그인 성공::{userID}");
                        OnBackendAuthorized("GPGS");
                    }
                    else {
                        NetworkErrorHandler.instance.OnFail(bro, OnBtnGoogleLoginClick);
                    }
                }
                else {
                    // 로그인 실패
                    Debug.Log("Login failed for some reason");
                    Debug.LogError("GPGS 로그인 실패");
                    Dispatcher.AddAction(ShowLoginSelect);
                }
            });
        }
#endif
    }

    private void ShowLoginSelect() {
        LoginSelectPopup loginSelectPopup = UIManager.instance.GetUI<LoginSelectPopup>(UI_NAME.LoginSelectPopup);
        loginSelectPopup.SetData();
        loginSelectPopup.Show();
    }

    // 구글 토큰 받아옴
    public string GetGoogleToken() {// 딱히 수정할 필요 없음 
#if UNITY_ANDROID
        if (PlayGamesPlatform.Instance.localUser.authenticated) {
            // 유저 토큰 받기
            string userToken = PlayGamesPlatform.Instance.GetIdToken();
            return userToken;
        }
        else {
            Debug.Log("접속되어있지 않습니다. PlayGamesPlatform.Instance.localUser.authenticated :  fail");
        }
#endif
        return null;
    }

    private void OnApplicationPause(bool isPause) {
        if (SendQueue.IsInitialize == false)
            return;

        if (isPause) {
            // 게임이 Pause 되었을 때
            SendQueue.PauseSendQueue();
        }
        else {
            SendQueue.ResumeSendQueue();
            CheckAccessToken();
        }
    }

    public void CheckAccessToken(Callback successCallback = null) {
        if (IsAccessTokenAlive()) {
            if (successCallback != null)
                successCallback();
            return;
        }

        SendQueue.Enqueue(Backend.BMember.RefreshTheBackendToken, res => {
            if (res.IsSuccess()) {
                Dispatcher.AddAction(() => {
                    Debug.Log($"RefreshTheBackendToken::Success");
                    if (successCallback != null)
                        successCallback();
                });
            }
            else {
                NetworkErrorHandler.instance.OnFail(res, null);
            }
        });
    }

    void OnApplicationQuit() {
        // 큐에 처리되지 않는 요청이 남아있는 경우 대기하고 싶은 경우
        // 큐에 몇 개의 함수가 남아있는지 체크
        while (SendQueue.UnprocessedFuncCount > 0) {
            // 처리
        }

        SendQueue.StopSendQueue();
        Backend.Notification.DisConnect();
    }

    public void DiconnectNotificationServer() {
        Backend.Notification.DisConnect();
    }

    public void ReqLogout() {
        Backend.BMember.Logout((res) => {
            if (res.IsSuccess()) {
                Debug.Log("로그아웃 성공");
                ResetLoginValues();
                StopCoroutine(JobCheckAuthorizedTime());
                Dispatcher.AddAction(NotifyAuthStateChanged);
                DiconnectNotificationServer();
            }
            else {
                NetworkErrorHandler.instance.OnFail(res, ReqLogout);
            }
        });
    }

    private void NotifyAuthStateChanged() {
        EventManager.Notify(EventEnum.AuthStateChanged);
    }

    public bool IsAccessTokenAlive() {
        BackendReturnObject res = Backend.BMember.IsAccessTokenAlive();
        if (res.GetMessage() == "Success")
            return true;
        else
            return false;
    }

    public void OnGuestLoginClick() {
        Debug.Log("게스트 로그인");
        ResetLoginValues();

        DeleteGuestInfo();
        bro = Backend.BMember.GuestLogin();
        if (bro.IsSuccess()) {
            Debug.Log("게스트 로그인 성공");
            // 성공시
            OnBackendAuthorized("GUEST");
        }
        else {
            Debug.Log("게스트 로그인 실패");
            NetworkErrorHandler.instance.OnFail(bro, OnGuestLoginClick);
        }
    }

    private void ResetLoginValues() {
        userInDate = "";
        gameCenter = false;
        userID = "";
        nickname = "";
    }

    private void DeleteGuestInfo() {
        try {
            Backend.BMember.DeleteGuestInfo();
        }
        catch (Exception e) {
            Debug.LogWarning($"DeleteGuestInfo::{e.Message}");
        }
    }

    public void ResetUserIndate() {
        userInDate = string.Empty;
        EventManager.Notify(EventEnum.AuthStateChanged);
    }

    public void ChangeCustomToGoogle() {
        NetworkLoading.instance.Show();

        Social.localUser.Authenticate((bool success) => {
            if (success) {
                bro = Backend.BMember.ChangeCustomToFederation(GetGoogleToken(), FederationType.Google);
                //연동성공
                if (bro.IsSuccess()) {
                    ReqLogout();
                    DeleteGuestInfo();
                    ShowAccountLinkSuccess();
                }
                else {
                    string statusCode = bro.GetStatusCode();
                    switch (statusCode) {
                        case "409":
                            Dispatcher.AddAction(ShowDuplicatedFederation);
                            break;

                        default:
                            NetworkErrorHandler.instance.OnFail(bro, ChangeCustomToGoogle);
                            break;
                    }
                }
            }
            else {
                // 로그인 실패
                Debug.Log("Login failed for some reason");
                Debug.LogError("GPGS 로그인 실패");
                Dispatcher.AddAction(ShowAcountLinkSelect);
            }
        });
    }

    private void ShowAcountLinkSelect() {
        AccountLinkSelectPopup accountLinkSelectPopup = UIManager.instance.GetUI<AccountLinkSelectPopup>(UI_NAME.AccountLinkSelectPopup);
        accountLinkSelectPopup.SetData();
        accountLinkSelectPopup.Show();
        NetworkLoading.instance.Hide();
    }

    private void ShowAccountLinkSuccess() {
        string msg = TermModel.instance.GetTerm("msg_account_linkage_success");
        Callback cbOK = () => {
            App.instance.ChangeScene(App.SCENE_NAME.Title);
            //NotifyAuthStateChanged();
        };

        MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.OK, msg, cbOK);
        NetworkLoading.instance.Hide();
    }

    private void ShowDuplicatedFederation() {
        string msg = TermModel.instance.GetTerm("msg_duplicate_federation");
        MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.OK, msg);
        NetworkLoading.instance.Hide();
    }

    private void ShowServerError() {
        string msg = TermModel.instance.GetTerm("msg_server_error");
        MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.OK, msg, () => Application.Quit());
        NetworkLoading.instance.Hide();
    }

    public bool IsCustomSignUp() {
#if UNITY_EDITOR
        return false;
#else
        if (gameCenter == false && subscriptionType == "customSignUp")
            return true;
        return false;
#endif
    }

    // 게임센터 로그인 
    public void OnBtnGameCenterLoginClick() { // 로그인으로 해야 될 경우, 동기 형식이 더 편하다
        // 이미 로그인 된 경우
        if (Social.localUser.authenticated == true) {
            Debug.Log("OnBtnGameCenterLoginClick::authenticated");
            GameCenterSignUp();
        }
        else {
            Debug.Log("OnBtnGameCenterLoginClick::Authenticate");
            Social.localUser.Authenticate((bool success) => {
                if (success) {
                    ShowLog($"OnBtnGameCenterLoginClick::인증 성공::{userID}");
                    GameCenterSignUp();
                }
                else {
                    // 로그인 실패
                    Debug.Log("Login failed for some reason");
                    Debug.LogError("게임센터 로그인 실패");
                    Dispatcher.AddAction(ShowLoginSelect);
                }
            });
        }
    }

    private void GameCenterSignUp() {
        Debug.Log("GameCenterSignUp");

        string id = Social.localUser.id;
        string token = GetGameCenterToken();

        userID = id;

        string format = "id:{0}, name:{1}";
        Debug.Log(string.Format(format, Social.localUser.id, Social.localUser.userName));

        bro = Backend.BMember.CustomSignUp(id, token);
        if (bro.IsSuccess()) {
            Debug.Log("게임센터 가입 완료");
            GameCenterLogin();
        }
        //이미 가입된 경우
        else if (bro.GetStatusCode() == "409" &&
                    bro.GetMessage().ToLower().Contains("customid"))
            GameCenterLogin();
        else
            NetworkErrorHandler.instance.OnFail(bro, GameCenterSignUp);
    }

    private void GameCenterLogin() {
        string id = Social.localUser.id;
        string token = GetGameCenterToken();

        bro = Backend.BMember.CustomLogin(id, token);
        if (bro.IsSuccess()) {
            // 성공시
            gameCenter = true;
            OnBackendAuthorized("GameCenter");
            Debug.Log("게임센터 로그인 성공");
        }
        else {
            NetworkErrorHandler.instance.OnFail(bro, GameCenterLogin);
        }
    }

    private string GetGameCenterToken() {
        string value = AES.Encrypt(Social.localUser.id, Md5.ComputeHash("Azhidahaka"));
        return value;
    }

    public void OnBtnAppleLoginClick() {
        AppleAuthLoginArgs loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential => {
                IAppleIDCredential appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential.IdentityToken != null) {
                    string identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken, 0, appleIdCredential.IdentityToken.Length);
                    userID = appleIdCredential.Email;
                    ShowLog($"Apple 인증 성공::{userID}");
                    OnAppleLoginComplete(identityToken);
                }
                else {
                    Debug.LogError("유효하지 않은 토큰");
                    Dispatcher.AddAction(ShowLoginSelect);
                }
            },
            error => {
                AuthorizationErrorCode authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogWarning("Sign in with Apple failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                // 로그인 실패
                Debug.LogError("Apple 로그인 실패");
                Dispatcher.AddAction(ShowLoginSelect);
            });
    }

    private void OnAppleLoginComplete(string accessToken) {
        BackendReturnObject bro = Backend.BMember.AuthorizeFederation(accessToken, FederationType.Apple, "siwa");
        if (bro.IsSuccess()) {
            // 성공시
            OnBackendAuthorized("Apple");
            Debug.Log("Apple 로그인 성공");
        }
        else {
            Debug.LogError("OnAppLoginComplete::Apple 로그인 실패");
            NetworkErrorHandler.instance.OnFail(bro, OnBtnAppleLoginClick);
        }
    }

    public void ChangeCustomToApple() {
        NetworkLoading.instance.Show();

        AppleAuthLoginArgs loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        appleAuthManager.LoginWithAppleId(loginArgs,
            credential => {
                IAppleIDCredential appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential.IdentityToken != null) {
                    string identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken, 0, appleIdCredential.IdentityToken.Length);
                    OnAppleLoginCompleteToChange(identityToken);
                }

                else {
                    Debug.LogError("유효하지 않은 토큰");
                    Dispatcher.AddAction(ShowAcountLinkSelect);
                }
            },
            error => {
                AuthorizationErrorCode authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogWarning("Sign in with Apple failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                // 로그인 실패
                Debug.LogError("Apple 로그인 실패");
                Dispatcher.AddAction(ShowAcountLinkSelect);
            });
    }

    private void OnAppleLoginCompleteToChange(string accessToken) {
        BackendReturnObject bro = Backend.BMember.ChangeCustomToFederation(accessToken, FederationType.Apple);
        if (bro.IsSuccess()) {
            ReqLogout();
            DeleteGuestInfo();
            ShowAccountLinkSuccess();
        }
        else {
            string statusCode = bro.GetStatusCode();
            switch (statusCode) {
                case "409":
                    Dispatcher.AddAction(ShowDuplicatedFederation);
                    break;

                default:
                    NetworkErrorHandler.instance.OnFail(bro, ChangeCustomToApple);
                    break;
            }
        }
    }

    public void ChangeCustomToFacebook() {
        NetworkLoading.instance.Show();

        List<string> permissions = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(permissions, OnFBAuthCompleteToChange);
    }

    private void OnFBAuthCompleteToChange(ILoginResult result) {
        if (FB.IsLoggedIn) {
            AccessToken accessToken = AccessToken.CurrentAccessToken;
            string facebookToken = accessToken.TokenString;
            BackendReturnObject bro = Backend.BMember.ChangeCustomToFederation(facebookToken, FederationType.Facebook);
            if (bro.IsSuccess()) {
                ReqLogout();
                DeleteGuestInfo();
                ShowAccountLinkSuccess();
            }
            else {
                string statusCode = bro.GetStatusCode();
                switch (statusCode) {
                    case "409":
                        Dispatcher.AddAction(ShowDuplicatedFederation);
                        break;

                    default:
                        NetworkErrorHandler.instance.OnFail(bro, ChangeCustomToFacebook);
                        break;
                }
            }
        }
        else {
            // 로그인 실패
            Debug.Log("Login failed for some reason");
            Debug.LogError("Facebook 로그인 실패");
            Dispatcher.AddAction(ShowAcountLinkSelect);
        }
    }

    public FEDERATION_TYPE GetFederationType() {
        if (gameCenter)
            return FEDERATION_TYPE.GAMECENTER;
        //subscriptionType

        if (subscriptionType == "google")
            return FEDERATION_TYPE.GOOGLE;

        if (subscriptionType == "apple")
            return FEDERATION_TYPE.APPLE;

        if (subscriptionType == "facebook")
            return FEDERATION_TYPE.FACEBOOK;

        return FEDERATION_TYPE.GUEST;
    }

    public string GetUserID() {
        if (userID != null && userID != "")
            return userID;

        string userName = UserDataModel.instance.userProfile.userName;
        if (userName != null && userName != "")
            return userName;

        FEDERATION_TYPE federationType = GetFederationType();
        switch (federationType) {
            case FEDERATION_TYPE.APPLE:
                return "Apple";
            case FEDERATION_TYPE.FACEBOOK:
                return "Facebook";
            case FEDERATION_TYPE.GAMECENTER:
                return "GameCenter";
            case FEDERATION_TYPE.GOOGLE:
                return "Google";

            default:
                return "Unknown";
        }
    }

    public void ConnectNotificationServer() {
        Backend.Notification.OnAuthorize = (bool Result, string Reason) => {
            Debug.Log("실시간 서버 성공 여부 : " + Result);
            Debug.Log("실패 시 이유 : " + Reason);
        };

        Backend.Notification.OnAcceptedFriendRequest = () => {
            Debug.Log("친구요청 수락됨");
            Dispatcher.AddAction(() => EventManager.Notify(EventEnum.NotifiedNewFriend));
        };

        Backend.Notification.OnReceivedFriendRequest = () => {
            Debug.Log("친구요청 받음");
            Dispatcher.AddAction(() => EventManager.Notify(EventEnum.NotifiedNewFriend));
        };

        Backend.Notification.OnRejectedFriendRequest = () => {
            Debug.Log("친구요청 거절됨");
            Dispatcher.AddAction(() => EventManager.Notify(EventEnum.NotifiedNewFriend));
        };

        Backend.Notification.OnReceivedMessage = () => {
            Debug.Log("새 쪽지가 도착했습니다!");
            Dispatcher.AddAction(() => EventManager.Notify(EventEnum.NotifiedNewMessage));
        };

        Backend.Notification.OnDisConnect = (string Reason) => {
            Debug.Log("해제 이유 : " + Reason);
        };

        // 실시간 알림 서버로 연결
        Backend.Notification.Connect();
    }

    public void ShowLog(string msg) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(msg);
#endif
    }
}
