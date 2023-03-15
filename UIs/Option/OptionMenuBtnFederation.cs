using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionMenuBtnFederation : MonoBehaviour {
    public Text lblUserName;
    public FEDERATION_TYPE federationType;

    public void SetData() {
        string userID = BackendLogin.instance.GetUserID();
        lblUserName.text = userID;
    }
}
