using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserData;

public class GetItemListPopupSlot : MonoBehaviour {
    public Text lblItemName;
    
    public GameObject objSkin;
    public GameObject objGoods;
    public RawImage icoGoods;
    public Text lblGoodsAmount;

    private MailRewardDTO rewardInfo;
    private GameData.PackageDTO packageData;

    private GameObject skinObject;
    private GameObject bankObject;

    public void SetData(MailRewardDTO rewardInfo) {
        this.rewardInfo = rewardInfo;

        if (skinObject != null)
            Destroy(skinObject);

        SetItemName();

        if (rewardInfo.type == (long)MAIL_REWARD_TYPE.SKIN) {
            Common.ToggleActive(objSkin, true);
            Common.ToggleActive(objGoods, false);
            skinObject = ResourceManager.instance.GetSkinThumbnail(rewardInfo.count, objSkin.transform);
        }
        else {
            Common.ToggleActive(objGoods, true);
            Common.ToggleActive(icoGoods.gameObject, true);
            Common.ToggleActive(lblGoodsAmount.gameObject, rewardInfo.count > 0);
            Common.ToggleActive(objSkin, false);
            lblGoodsAmount.text = Common.GetCommaFormat(rewardInfo.count);
            icoGoods.texture = ResourceManager.instance.GetMailRewardIco(rewardInfo.type);
        }
    }

    private void SetItemName() {
        switch((MAIL_REWARD_TYPE)rewardInfo.type) {
            case MAIL_REWARD_TYPE.GOLD:
                lblItemName.text = TermModel.instance.GetTerm("format_cost_type_gold");
                break;
            case MAIL_REWARD_TYPE.DIAMOND:
                lblItemName.text = TermModel.instance.GetTerm("format_cost_type_diamond");
                break;
            case MAIL_REWARD_TYPE.CHALLENGE_TICKET:
                lblItemName.text = TermModel.instance.GetTerm("competition_ticket");
                break;
            case MAIL_REWARD_TYPE.SKIN:
                lblItemName.text = TermModel.instance.GetSkinName(rewardInfo.count);
                break;
        }
    }

    public void SetData(GameData.PackageDTO packageData) {
        this.packageData = packageData;

        if (skinObject != null)
            Destroy(skinObject);
        if (bankObject != null)
            Destroy(bankObject);

        SetPackageItemName();

        switch((PACKAGE_TYPE)packageData.type) {
            case PACKAGE_TYPE.DIAMOND:
            case PACKAGE_TYPE.GOLD:
                Common.ToggleActive(objGoods, true);
                Common.ToggleActive(icoGoods.gameObject, true);
                Common.ToggleActive(lblGoodsAmount.gameObject, true);
                Common.ToggleActive(objSkin, false);
                icoGoods.texture = ResourceManager.instance.GetPackageIco(packageData.type);
                lblGoodsAmount.text = Common.GetCommaFormat(packageData.value);
                break;

            case PACKAGE_TYPE.PIGGYBANK_SKIN:
                Common.ToggleActive(objGoods, true);
                Common.ToggleActive(icoGoods.gameObject, false);
                Common.ToggleActive(lblGoodsAmount.gameObject, false);
                Common.ToggleActive(objSkin, false);
                bankObject = ResourceManager.instance.GetPiggyBank(packageData.value, objGoods.transform);
                Bank bank = bankObject.GetComponent<Bank>();
                Destroy(bank.canvasIco);
                break;

            case PACKAGE_TYPE.FRAME:
                Common.ToggleActive(objGoods, true);
                Common.ToggleActive(icoGoods.gameObject, true);
                Common.ToggleActive(lblGoodsAmount.gameObject, false);
                Common.ToggleActive(objSkin, false);
                icoGoods.texture = ResourceManager.instance.GetProfileFrame(packageData.value);
                break;

            case PACKAGE_TYPE.PROFILE:
                Common.ToggleActive(objGoods, true);
                Common.ToggleActive(icoGoods.gameObject, true);
                Common.ToggleActive(lblGoodsAmount.gameObject, false);
                Common.ToggleActive(objSkin, false);
                icoGoods.texture = ResourceManager.instance.GetProfileTexture(packageData.value);
                
                break;
            case PACKAGE_TYPE.SKIN:
                Common.ToggleActive(objSkin, true);
                Common.ToggleActive(objGoods, false);
                skinObject = ResourceManager.instance.GetSkinThumbnail(rewardInfo.count, objSkin.transform);
                break;
        }
    }

    private void SetPackageItemName() {
        switch((PACKAGE_TYPE)packageData.type) {
            case PACKAGE_TYPE.GOLD:
                lblItemName.text = TermModel.instance.GetTerm("format_cost_type_gold");
                break;
            case PACKAGE_TYPE.DIAMOND:
                lblItemName.text = TermModel.instance.GetTerm("format_cost_type_diamond");
                break;
            case PACKAGE_TYPE.CHALLENGE_TICKET:
                lblItemName.text = TermModel.instance.GetTerm("competition_ticket");
                break;
            case PACKAGE_TYPE.SKIN:
                lblItemName.text = TermModel.instance.GetSkinName(packageData.value);
                break;
            case PACKAGE_TYPE.PIGGYBANK_SKIN:
                lblItemName.text = TermModel.instance.GetPiggyBankSkinName(packageData.value);
                break;
            case PACKAGE_TYPE.FRAME:
                lblItemName.text = TermModel.instance.GetFrameName(packageData.value);
                break;
            case PACKAGE_TYPE.PROFILE:
                lblItemName.text = TermModel.instance.GetProfileName(packageData.value);
                break;
        }
    }
}
