using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileChangePopupSlot : MonoBehaviour {
    public RawImage img;
    public RawImage imgLock;
    public GameObject goSelected;

    private long id;
    private ProfileChangePopup.TAB tab;

    public void SetData(ProfileChangePopup.TAB tab, long id) {
        this.tab = tab;
        this.id = id;
        if (tab == ProfileChangePopup.TAB.PROFILE) {
            GameData.ProfileDTO profileData = GameDataModel.instance.GetProfileData(id);
            img.texture = imgLock.texture = ResourceManager.instance.GetProfileTexture(id);
            Common.ToggleActive(imgLock.gameObject, 
                                profileData.defaultUnlock == 0 && 
                                UserDataModel.instance.IsProfileExist(id) == false &&
                                UserDataModel.instance.IsNonconsumableExist(profileData.requirePackageID) == false);
        }
        else {
            GameData.FrameDTO frameData = GameDataModel.instance.GetFrameData(id);
            img.texture = imgLock.texture = ResourceManager.instance.GetProfileFrame(id);
            Common.ToggleActive(imgLock.gameObject, 
                                frameData.defaultUnlock == 0 && 
                                UserDataModel.instance.IsFrameExist(id) == false &&
                                UserDataModel.instance.IsNonconsumableExist(frameData.requirePackageID) == false);
        }

        UpdateState();
    }

    public void UpdateState() {
        if (tab == ProfileChangePopup.TAB.FRAME)
            Common.ToggleActive(goSelected, UserDataModel.instance.publicUserData.profileFrameNo == id);
        else
            Common.ToggleActive(goSelected, UserDataModel.instance.publicUserData.profileNo == id);
    }

    public void OnSlotClick() {
        if (imgLock.gameObject.activeSelf) {
            CheckAvailableBuy();
            return;
        }

        if (tab == ProfileChangePopup.TAB.FRAME)
            WebUser.instance.ReqChangeFrame(id);
        else
            WebUser.instance.ReqChangeProfile(id);
    }

    private void CheckAvailableBuy() {
        if (tab == ProfileChangePopup.TAB.FRAME) {
            GameData.FrameDTO frameData = GameDataModel.instance.GetFrameData(id);
            //구매불가
            if (frameData.buy == 0) {
                string msg;
                if (frameData.requirePackageID == 0) {
                    msg = TermModel.instance.GetTerm($"get_frame_{frameData.frameID}");
                }
                else {
                    msg = TermModel.instance.GetTerm($"get_require_package_{frameData.requirePackageID}");
                }
                Texture texture = ResourceManager.instance.GetProfileFrame(frameData.frameID);

                MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.OK, msg, null, null, null, "", "", "", false, texture);
            }
            else {
                string format = TermModel.instance.GetTerm("format_buy_profile_confirm");
                string msg = string.Format(format, Constant.BUY_PROFILE_COST_DIAMOND);
                Callback callbackYes = () => WebProduct.instance.ReqBuyFrame(frameData.frameID);
                Texture texture = ResourceManager.instance.GetProfileFrame(frameData.frameID);

                MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.YES_NO, msg, callbackYes, null, null, "", "", "", false, texture);
            }
        }
        else {
            GameData.ProfileDTO profileData = GameDataModel.instance.GetProfileData(id);
            //구매불가
            if (profileData.buy == 0) {
                string msg;
                if (profileData.requirePackageID == 0) {
                    msg = TermModel.instance.GetTerm($"get_profile_{profileData.profileID}");
                }
                else {
                    msg = TermModel.instance.GetTerm($"get_require_package_{profileData.requirePackageID}");
                }
                Texture texture = ResourceManager.instance.GetProfileTexture(profileData.profileID);

                MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.OK, msg, null, null, null, "", "", "", false, texture);
            }
            else {
                string format = TermModel.instance.GetTerm("format_buy_profile_confirm");
                string msg = string.Format(format, Constant.BUY_PROFILE_COST_DIAMOND);
                Callback callbackYes = () => WebProduct.instance.ReqBuyProfile(profileData.profileID);
                Texture texture = ResourceManager.instance.GetProfileTexture(profileData.profileID);

                MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.YES_NO, msg, callbackYes, null, null, "", "", "", false, texture);
            }
        }
    }
}
