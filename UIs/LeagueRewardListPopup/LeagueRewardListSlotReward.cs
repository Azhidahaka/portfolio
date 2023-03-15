using GameData;
using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeagueRewardListSlotReward : MonoBehaviour {
    public GameObject objNormal;
    public RawImage icoNormal;
    public Text lblNormalCount;

    public GameObject objSkin;
    public GameObject objBank;
    public Text lblContext;

    private GameObject bankInstance;

    public void SetData(PackageDTO packageData) {
        bool showNormal = false;
        bool showSkin = false;
        bool showBank = false;

        if (bankInstance)
            Destroy(bankInstance);

        switch((PACKAGE_TYPE)packageData.type) {
            case PACKAGE_TYPE.DIAMOND:
            case PACKAGE_TYPE.GOLD:
                showNormal = true;
                icoNormal.texture = ResourceManager.instance.GetPackageIco(packageData.type);
                lblNormalCount.text = Common.GetCommaFormat(packageData.value);
                Common.ToggleActive(lblNormalCount.gameObject, true);
                lblContext.text = TermModel.instance.GetTerm("league_reward_always");
                break;

            case PACKAGE_TYPE.PIGGYBANK_SKIN:
                showBank = true;
                bankInstance = ResourceManager.instance.GetPiggyBank(packageData.value, objBank.transform);
                Bank bank = bankInstance.GetComponent<Bank>();
                Destroy(bank.canvasIco);
                lblContext.text = TermModel.instance.GetTerm("league_reward_first_time");
                break;

            case PACKAGE_TYPE.FRAME:
                showNormal = true;
                icoNormal.texture = ResourceManager.instance.GetProfileFrame(packageData.value);
                lblContext.text = TermModel.instance.GetTerm("league_reward_first_time");
                Common.ToggleActive(lblNormalCount.gameObject, false);
                break;

            case PACKAGE_TYPE.PROFILE:
                showNormal = true;
                icoNormal.texture = ResourceManager.instance.GetProfileTexture(packageData.value);
                lblContext.text = TermModel.instance.GetTerm("league_reward_first_time");
                Common.ToggleActive(lblNormalCount.gameObject, false);
                break;
        }

        Common.ToggleActive(objNormal, showNormal);
        Common.ToggleActive(objSkin, showSkin);
        Common.ToggleActive(objBank, showBank);
    }
}
