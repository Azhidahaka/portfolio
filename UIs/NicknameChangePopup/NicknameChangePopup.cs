using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NicknameChangePopup : UIBase {
    public InputField inputNickname;

    public GameObject objBtnCancel;
    public GameObject objCost;
    public Text lblNickname;
    public Text lblCost;

    public GameObject goNotice;

    public Callback callback;

    private void OnEnable() {
        SetInFrontInCanvas();
    }

    public void SetData(Callback callback = null) {
        this.callback = callback;

        if (string.IsNullOrEmpty(BackendLogin.instance.nickname)) {
            Common.ToggleActive(objBtnCancel, false);
            Common.ToggleActive(objCost, false);
        }
        else {
            Common.ToggleActive(objBtnCancel, true);
            if (UserDataModel.instance.userProfile.freeNicknameChangeCount > 0) {
                Common.ToggleActive(objCost, false);
                Common.ToggleActive(goNotice, false);
            }
            else {
                Common.ToggleActive(objCost, true);
                Common.ToggleActive(goNotice, true);
                lblCost.text = Common.GetCostFormat(LuckyFlow.EnumDefine.PRODUCT_COST_TYPE.DIAMOND, 
                                                    Constant.CHANGE_NICKNAME_COST_DIAMOND, 
                                                    Color.red);
            }
        }
    }

    public void OnBtnOkClick() {
        string nickname = lblNickname.text;
        if (lblNickname.text.Contains("#")) {
            string msg = TermModel.instance.GetTerm("msg_nickname_format");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        if (UserDataModel.instance.userProfile.freeNicknameChangeCount < 1 &&
            UserDataModel.instance.userProfile.diamond < Constant.CHANGE_NICKNAME_COST_DIAMOND) {
            string msg = TermModel.instance.GetTerm("msg_not_enough_diamond");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        if (IsDuplicate(nickname)) {
            OnResUpdateNicknameDuplicated();
            return;
        }

        BackendRequest.instance.ReqUpdateNickname(nickname, 
                                                  OnResUpdateNicknameSuccess, 
                                                  OnResUpdateNicknameDuplicated, 
                                                  OnResUpdateNicknameOutOfFormat);
    }

    private bool IsDuplicate(string nickname) {
        List<GameData.RankingNPCDTO> rankingNPCDatas = GameDataModel.instance.rankingNPCs;
        foreach (GameData.RankingNPCDTO npcData in rankingNPCDatas) {
            if (npcData.name == nickname)
                return true;
        }

        return false;
    }

    private void OnResUpdateNicknameSuccess() {
        //성공했습니다.
        string msg = TermModel.instance.GetTerm("msg_nickname_changed");
        MessageUtil.ShowSimpleWarning(msg);
        EventManager.Notify(EventEnum.NicknameChanged);
        EventManager.Notify(EventEnum.GoodsUpdate);
        Hide();
        if (callback != null)
            callback();
    }

    private void OnResUpdateNicknameDuplicated() {
        //이미 존재하는 닉네임입니다.
        string msg = TermModel.instance.GetTerm("msg_nickname_duplicated");
        MessageUtil.ShowSimpleWarning(msg);
    }

    private void OnResUpdateNicknameOutOfFormat() {
        //형식에 맞지않습니다.
        string msg = TermModel.instance.GetTerm("msg_nickname_format");
        MessageUtil.ShowSimpleWarning(msg);
    }

    public void OnInputNicknameSubmit() {
        
    }

    public void OnBtnCancelClick() {
        Hide();
    }

    public override void Hide() {
        base.Hide();
        EventManager.Notify(EventEnum.TutorialCheck);
    }
}
