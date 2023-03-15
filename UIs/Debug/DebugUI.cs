using QuantumTek.EncryptedSave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour {
    public static DebugUI instance;

    public Toggle toggleTest1;
    public Toggle toggleTest2;

    public void Awake() {
        instance = this;
#if UNITY_EDITOR
        TextAsset loginAsset = Resources.Load("Login") as TextAsset;
        if (loginAsset != null && loginAsset.text == "test2") {
            toggleTest1.isOn = false;
            toggleTest2.isOn = true;
        }
#endif
    }

    public bool IsTest1() {
        return toggleTest1.isOn;
    }

    //@todo 
    public void OnBtnResetClick() {
        try {
            if (ES_Save.Exists(Constant.SKIN_ID_PATH))
                ES_Save.DeleteData(Constant.SKIN_ID_PATH);
            if (ES_Save.Exists(Constant.PATH_AGREE_NIGHT_PUSH))
                ES_Save.DeleteData(Constant.PATH_AGREE_NIGHT_PUSH);
            if (ES_Save.Exists(Constant.PATH_AGREE_PRIVACY))
                ES_Save.DeleteData(Constant.PATH_AGREE_PRIVACY);
            if (ES_Save.Exists(Constant.PATH_AGREE_TERM))
                ES_Save.DeleteData(Constant.PATH_AGREE_TERM);
            if (ES_Save.Exists(Constant.PATH_FORCE_REFRESH_PUSH))
                ES_Save.DeleteData(Constant.PATH_FORCE_REFRESH_PUSH);
            
            UserDataModel.instance.RemoveLocalUserDatas();
            UserDataModel.instance.LoadLocalUserDatas();

#if UNITY_EDITOR
            if (IsTest1()) 
                UserDataModel.instance.userProfile.scoreRowIndate = Constant.TEST_1_SCORE_ROW_INDATE;
            else 
                UserDataModel.instance.userProfile.scoreRowIndate = Constant.TEST_2_SCORE_ROW_INDATE;
#endif

            BackendRequest.instance.ReqSyncUserData(true, true, null);
        }
        catch (System.Exception e) {

        }
    }

    public void Show() {
        Common.ToggleActive(gameObject, true);
    }

    public void Hide() {
        Common.ToggleActive(gameObject, false);
    }
}

