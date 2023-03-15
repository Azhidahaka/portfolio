using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebProduct : MonoBehaviour {
    public static WebProduct instance;

    private void Awake() {
        instance = this;
    }

    public void ReqBuyProduct(long packageID, Callback successCallback = null, bool restore = false) {
        GameData.ProductDTO productData = GameDataModel.instance.GetProductDataByPackageID(packageID);
       
        //비용처리(현금결제인경우 아무것도 안한다.
        switch((PRODUCT_COST_TYPE)productData.costType) {
            case PRODUCT_COST_TYPE.DIAMOND:
                UserDataModel.instance.UseDiamond(productData.cost);
                break;
            case PRODUCT_COST_TYPE.GOLD:
                UserDataModel.instance.UseGold(productData.cost);
                break;
        }

        bool checkBanner = false;

        //패키지에 따라 별도 처리 
        List<GameData.PackageDTO> packageDatas = GameDataModel.instance.GetPackageDatas(packageID);
        foreach (GameData.PackageDTO packageData in packageDatas) {
            switch ((PACKAGE_TYPE)packageData.type) {
                case PACKAGE_TYPE.DIAMOND: {
                    if (restore == false) {
                        long amount = packageData.value + packageData.bonus;
                        UserDataModel.instance.AddDiamond(amount);
                        EffectManager.instance.ShowGetDiamondEffect(amount);
                    }
                    break;
                }

                case PACKAGE_TYPE.GOLD: {
                    if (restore == false) {
                        long amount = packageData.value + packageData.bonus;
                        UserDataModel.instance.AddGold(amount);
                        EffectManager.instance.ShowGetGoldEffect(amount);
                    }
                    break;
                }

                case PACKAGE_TYPE.PIGGYBANK:
                    UserDataModel.instance.userProfile.piggyBankType = packageData.value;
                    UserDataModel.instance.BankSkinID = packageData.value;
                    break;

                case PACKAGE_TYPE.ADD_NON_CONSUMABLE:
                    UserDataModel.instance.AddNonconsumable(packageData.value);
                    break;

                case PACKAGE_TYPE.REMOVE_ADS:
                    checkBanner = false;
                    break;

                case PACKAGE_TYPE.SKIN:
                    break;

                case PACKAGE_TYPE.PIGGYBANK_SKIN:
                    break;
            }
        }        

        if (productData.consumable == 0) 
            UserDataModel.instance.AddNonconsumable(productData.packageID);

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.USER_PROFILE,
                                             USER_DATA_KEY.STATISTICS);

        EventManager.Notify(EventEnum.UserDataUserInfoUpdate);

        if (checkBanner)
            EventManager.Notify(EventEnum.CheckBanner);

        AnalyticsUtil.LogBuyProduct(productData.packageID);

        if (successCallback != null)
            successCallback();
    }

    public void ReqBuyProfile(long profileID, Callback successCallback = null) {
        if (UserDataModel.instance.userProfile.diamond < Constant.BUY_PROFILE_COST_DIAMOND) {
            string msg = TermModel.instance.GetTerm("msg_not_enough_diamond");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        UserDataModel.instance.UseDiamond(Constant.BUY_PROFILE_COST_DIAMOND);
        UserDataModel.instance.AddProfile(profileID);

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.USER_PROFILE);

        if (successCallback == null) {
            successCallback = () => {
                EventManager.Notify(EventEnum.UserDataUserInfoUpdate);
            };
        }
        BackendRequest.instance.ReqSyncUserData(false, true, successCallback);
    }

    public void ReqBuyFrame(long frameID, Callback successCallback = null) {
        if (UserDataModel.instance.userProfile.diamond < Constant.BUY_PROFILE_COST_DIAMOND) {
            string msg = TermModel.instance.GetTerm("msg_not_enough_diamond");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        UserDataModel.instance.UseDiamond(Constant.BUY_PROFILE_COST_DIAMOND);
        UserDataModel.instance.AddFrame(frameID);

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.USER_PROFILE);

        if (successCallback == null) {
            successCallback = () => {
                EventManager.Notify(EventEnum.UserDataUserInfoUpdate);
            };
        }
        BackendRequest.instance.ReqSyncUserData(false, true, successCallback);
    }
}
