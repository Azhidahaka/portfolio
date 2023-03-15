using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonPopup : UIBase {
    public enum BUTTON_TYPE {
        OK = 1,
        YES_NO = 2,
        YES_MODIFY_NO = 3,
    }

    public Text lblText;

    public Button btnOk;
    public Button btnCancel;
    public Button btnModify;
    public Text lblBtnOk;
    public Text lblBtnCancel;
    public Text lblBtnModify;

    public GameObject objTextGuestWarning;

    public RawImage ico;

    private BUTTON_TYPE buttonType;

    private Callback callbackOk;
    private Callback callbackModify;
    private Callback callbackCancel;

    private void OnEnable() {
        SetInFrontInCanvas();
    }

    public void SetData(BUTTON_TYPE buttonType, 
                        string msg, 
                        Callback callbackOk, 
                        Callback callbackCancel = null,
                        Callback callbackModify = null,
                        string textOk = "",
                        string textCancel = "",
                        string textModify = "",
                        bool showGuestWarning = false,
                        Texture texture = null) {
        this.buttonType = buttonType;

        lblBtnOk.text = textOk;
        lblBtnCancel.text = textCancel;
        lblBtnModify.text = textModify;

        if (buttonType == BUTTON_TYPE.OK) {
            Common.ToggleActive(btnCancel.gameObject, false);
            Common.ToggleActive(btnModify.gameObject, false);
            if (textOk == "")
                lblBtnOk.text = TermModel.instance.GetTerm("btn_ok");
        }
        else if (buttonType == BUTTON_TYPE.YES_NO) {
            Common.ToggleActive(btnCancel.gameObject, true);
            Common.ToggleActive(btnModify.gameObject, false);
            if (textOk == "")
                lblBtnOk.text = TermModel.instance.GetTerm("btn_yes");
            if (textCancel == "")
                lblBtnCancel.text = TermModel.instance.GetTerm("btn_no");
        }
        else if (buttonType == BUTTON_TYPE.YES_MODIFY_NO) {
            Common.ToggleActive(btnCancel.gameObject, true);
            Common.ToggleActive(btnModify.gameObject, true);
            if (textOk == "")
                lblBtnOk.text = TermModel.instance.GetTerm("btn_yes");
            if (textCancel == "")
                lblBtnCancel.text = TermModel.instance.GetTerm("btn_no");
            if (textModify == "")
                lblBtnModify.text = TermModel.instance.GetTerm("btn_modify");
        }

        this.callbackOk = callbackOk;
        if (this.callbackOk == null) 
            this.callbackOk = Hide;
        
        this.callbackCancel = callbackCancel;
        if (this.callbackCancel == null) 
            this.callbackCancel = Hide;

        this.callbackModify = callbackModify;
        if (this.callbackModify == null) 
            this.callbackModify = Hide;

        Common.ToggleActive(objTextGuestWarning, showGuestWarning);

        if (ico != null) {
            if (texture == null) {
                Common.ToggleActive(ico.gameObject, false);
                lblText.alignment = TextAnchor.MiddleCenter;
            }
            else {
                ico.texture = texture;
                Common.ToggleActive(ico.gameObject, true);
                lblText.alignment = TextAnchor.LowerCenter;
            }
        }

        lblText.text = msg;
    }

    public void OnBtnOkClick() {
        callbackOk();
        Hide();
    }

    public void OnBtnCancelClick() {
        callbackCancel();
        Hide();
    }

    public void OnBtnModifyClick() {
        callbackModify();
        Hide();
    }

    public override void OnCopy(List<object> datas) {
        buttonType = (BUTTON_TYPE)datas[0];
        string msg = datas[1] as string;
        callbackCancel = datas[2] as Callback;
        callbackOk = datas[3] as Callback;
        callbackModify = datas[4] as Callback;
        
        SetData(buttonType, msg, callbackCancel, callbackOk, callbackModify);
    }

    public override List<object> GetCopyDatas() {
        List<object> datas = new List<object>();
        datas.Add(buttonType);
        datas.Add(lblText.text);
        datas.Add(callbackCancel);
        datas.Add(callbackOk);
        datas.Add(callbackModify);
        return datas;
    }
}
