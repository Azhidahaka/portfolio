using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using QuantumTek.EncryptedSave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UI_NAME {
    NONE = -1,
    DEFAULT = 0,

    SceneLoading,
    NetworkLoading,

    GET_UI_START,
    
    Logo,
    Home,
    Title,
    MatchBlocks,
    OptionMenu,
    ShopMenu,
    NoSpaceWarning,
    Ranking,
    Achievement,

    AgreementPopup,
    LoginSelectPopup,
    AccountLinkSelectPopup,
    SimpleWarningPopup,
    CommonPopup,
    QuitPopup,
    ShopPopup,
    RemoveBlockMenu,
    AchievementNoticePopup,
    ChallengeGameEnterPopup,
    SelectLanguagePopup,
    MailBoxPopup,
    GetItemListPopup,
    PiggySkinListPopup,
    NicknameChangePopup,
    UserProfilePopup,
    FriendsListPopup,
    FriendsSearchPopup,
	GameLevelPopup,
	ProfileChangePopup,
    MultiGameListPopup,
    LeagueRewardListPopup,
    ChallengeBeforePopup,
    ChallengeFriendListPopup,
    ChallengeResultPopup,
    LeagueResultPopup,

    WaveCountDown,
    CountdownWarning,
    HitEffect,
    ScoreEffect,
    
    ResultPopupPersonal,
    ResultPopupChallenge,
    PiggyBankPopup,
    SelectNationPopup,
    SimpleNoticePopup,

    TutorialSet,

    GET_UI_END,
}

public class UIManager : MonoBehaviour {
    public static UIManager instance;

    public List<Transform> canvasTransforms;

    public NetworkLoading networkLoading;

    private Dictionary<string, GameObject> dicUIPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> dicUIInstances = new Dictionary<string, GameObject>();

    private List<UI_NAME> globalUIs = new List<UI_NAME>() {
    };

    private List<UI_NAME> loadingUIs = new List<UI_NAME>() {
        UI_NAME.NetworkLoading,
        UI_NAME.SceneLoading,
    };

    private SceneLoading sceneLoading;

    private long skinID;

    private void Awake() {
        instance = this;
        networkLoading.SetInstance();

        //로컬에 저장된 스킨아이디가 있다면 가져온다.
        string skinIDPath = Constant.SKIN_ID_PATH;
        if (ES_Save.Exists(skinIDPath))
            skinID = ES_Save.Load<long>(skinIDPath);
    }

    private void Start() {
        //로컬에 저장된 스킨아이디가 없는경우, 유저데이터에 저장된 값을 가져온다.
        if (skinID == 0)
            skinID = UserDataModel.instance.SkinID;
        SetSceneLoadingUI();
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.SkinChanged, OnSkinChanged);
    }

    private void OnSkinChanged(object[] args) {
        bool force = false;
        if (args.Length > 0)
            force = (bool)args[0];

        long newSkinID = UserDataModel.instance.SkinID;
        if (force == false && skinID > 0 && skinID == newSkinID)
            return;

        skinID = newSkinID;

        SetSceneLoadingUI();
        ResetUIs();
    }

    private void ResetUIs() {
        dicUIPrefabs.Clear();
        Resources.UnloadUnusedAssets();

        List<string> instanceKeys = new List<string>();
        foreach (KeyValuePair<string, GameObject> pair in dicUIInstances) {
            instanceKeys.Add(pair.Key);
        }

        Dictionary<string, GameObject> oldInstanceDic = new Dictionary<string, GameObject>();

        for (int i = 0; i < instanceKeys.Count; i++) {
            string key = instanceKeys[i];

            oldInstanceDic.Add(key, dicUIInstances[key]);
        }
        dicUIInstances.Clear();

        foreach (KeyValuePair<string, GameObject> pair in oldInstanceDic) {
            UI_NAME uiName = Common.ToEnum<UI_NAME>(pair.Key);

            UIBase oldUI = pair.Value.GetComponent<UIBase>();
            if (oldUI.gameObject.activeSelf == false)
                continue;

            UIBase newUI = GetUI<UIBase>(uiName);
            newUI.OnCopy(oldUI.GetCopyDatas());
            newUI.Show();
        }

        for (int i = 0; i < instanceKeys.Count; i++) {
            string key = instanceKeys[i];

            Destroy(oldInstanceDic[key]);
        }
    }

    private void SetSceneLoadingUI() {
        if (sceneLoading != null)
            Destroy(sceneLoading.gameObject);

        sceneLoading = GetSceneLoadingUI();
        sceneLoading.SetInstance();
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.SkinChanged, OnSkinChanged);
    }

    public static bool IsNotch() {
        if ((float)Screen.height / Screen.width < 2)
            return false;
        return true;
    }

    private void OnDestroy() {
        instance = null;
    }

    public T GetUI<T>(UI_NAME uiName, bool forceLoad = true) where T : UIBase {
        string key = uiName.ToString();
        //이미 생성된 UI가 있으면 불러온다.
        if (dicUIInstances.ContainsKey(key) && dicUIInstances[key] != null)
            return dicUIInstances[key].GetComponent<T>();

        if (forceLoad == false)
            return null;

        GameObject prefab;
        
        //생성된 UI가 없으면 Instantiate
        if (dicUIPrefabs.ContainsKey(key) == false) {
            string path = GetUIPath(uiName);

            prefab = Resources.Load<GameObject>(path);
            dicUIPrefabs.Add(key, prefab);
        }
        else
            prefab = dicUIPrefabs[key];

        if (prefab == null) {
            Debug.LogError("UI 프리팹 없음::"+ uiName.ToString());
            return null;
        }

        Transform canvasTransform = GetCanvasTransform(prefab);
        GameObject uiInstance = Instantiate(prefab, canvasTransform);
           
        uiInstance.name = key;
        uiInstance.SetActive(false);
        dicUIInstances.Add(key, uiInstance);
        return uiInstance.GetComponent<T>();
    }

    private Transform GetCanvasTransform(GameObject prefab) {
        CanvasOrder canvasOrder = prefab.GetComponent<CanvasOrder>();
        CANVAS_ORDER order = CANVAS_ORDER.BASE_10;
        if (canvasOrder != null)
            order = canvasOrder.order;
        else {
            UI_NAME uiName = Common.ToEnum<UI_NAME>(prefab.name);
            GameData.UIOrderDTO uiOrderData = GameDataModel.instance.GetUIOrderData(uiName);
            if (uiOrderData != null)
                order = (CANVAS_ORDER)uiOrderData.order;
        }

        return GetCanvasTransform(order);
    }

    public Transform GetCanvasTransform(CANVAS_ORDER order) {
        switch (order) {
            case CANVAS_ORDER.BASE_10:
                return canvasTransforms[0];

            case CANVAS_ORDER.MENU_20:
                return canvasTransforms[1];

            case CANVAS_ORDER.DECO_30:
                return canvasTransforms[2];

            case CANVAS_ORDER.POPUP_40:
                return canvasTransforms[3];

            case CANVAS_ORDER.POPUP_DECO_45:
                return canvasTransforms[4];

            case CANVAS_ORDER.TUTORIAL_50:
                return canvasTransforms[5];

            case CANVAS_ORDER.LOADING_60:
                return canvasTransforms[6];

            case CANVAS_ORDER.WARNING_70:
                return canvasTransforms[7];
        }

        return canvasTransforms[0];
    }

    private string GetUIPath(UI_NAME uiName) {
        GameData.UIOrderDTO uiOrderData = GameDataModel.instance.GetUIOrderData(uiName);

        if (uiOrderData.variant == (long)UI_VARIANT.NOT_USE) {
            string pathFormat = "Prefabs/UI/{0}";
            string path = string.Format(pathFormat, uiName.ToString());
            return path;
        }
        else {
            string pathFormat = "Prefabs/UI/{0:D4}/{1}";
            string path = string.Format(pathFormat, skinID, uiName.ToString());
            return path;
        }
    }

    public void HideSceneUIs() {
        UIBase[] uis = GetComponentsInChildren<UIBase>();
        for (int i = 0; i < uis.Length; i++) {
            UI_NAME uiName = Common.ToEnum<UI_NAME>(uis[i].name);
            if (IsLoadingUI(uiName) || IsGlobalUI(uiName))
                continue;
            uis[i].Hide();
        }
    }

    private bool IsGlobalUI(UI_NAME uiName) {
        if (globalUIs.Contains(uiName))
            return true;
        return false;
    }

    private bool IsLoadingUI(UI_NAME uiName) {
        if (loadingUIs.Contains(uiName))
            return true;
        return false;
    }

    public void HideGlobalUIs() {
        UIBase[] uis = GetComponentsInChildren<UIBase>();
        for (int i = 0; i < uis.Length; i++) {
            UI_NAME uiName = Common.ToEnum<UI_NAME>(uis[i].name);
            if (IsLoadingUI(uiName) || IsGlobalUI(uiName) == false)
                continue;
            uis[i].Hide();
        }
    }

    public void DestroyUI(UI_NAME uiName) {
        string key = uiName.ToString();
        //이미 생성된 UI가 있으면 불러온다.
        if (dicUIInstances.ContainsKey(key)) {
            Destroy(dicUIInstances[key]);
            dicUIInstances.Remove(key);
        }
    }

    public SceneLoading GetSceneLoadingUI() {
        string path = GetUIPath(UI_NAME.SceneLoading);
        GameObject prefab = Resources.Load<GameObject>(path);
        Transform canvasTransform = GetCanvasTransform(prefab);
        GameObject instance = Instantiate(prefab, canvasTransform);
        instance.name = UI_NAME.SceneLoading.ToString();
        instance.transform.SetAsFirstSibling();

        SceneLoading sceneLoading = instance.GetComponent<SceneLoading>();
        sceneLoading.Hide();

        return sceneLoading;
    }

    public UIBase GetTopUI() {
        List<CANVAS_ORDER> canvasOrders = new List<CANVAS_ORDER>() {
            CANVAS_ORDER.WARNING_70,
            CANVAS_ORDER.POPUP_40,
        };
        
        foreach (CANVAS_ORDER canvasOrder in canvasOrders) {
            Transform canvasTransform = GetCanvasTransform(canvasOrder);
            UIBase[] canvasUIs = canvasTransform.GetComponentsInChildren<UIBase>(false);
            if (canvasUIs == null || canvasUIs.Length == 0)
                continue;
            return canvasUIs[canvasUIs.Length - 1];
        }

        return null;
    }
}
