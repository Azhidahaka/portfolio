using LuckyFlow.EnumDefine;
using QuantumTek.EncryptedSave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebOption : MonoBehaviour {
    public static WebOption instance;

    private void Awake() {
        instance = this;
    }

    public void ReqChangeEffectVolume(float effectVolumeRatio, Callback successCallback = null) {
        UserDataModel.instance.gameOptions.effectVolumeRatio = effectVolumeRatio;

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.GAME_OPTIONS);
        if (successCallback != null)
            successCallback();
    }

    public void ReqChangeBGMVolume(float bgmVolumeRatio, Callback successCallback = null) {
        UserDataModel.instance.gameOptions.bgmVolumeRatio = bgmVolumeRatio;

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.GAME_OPTIONS);
        if (successCallback != null)
            successCallback();
    }

    public void ReqMuteBGM(Callback successCallback = null) {
        UserDataModel.instance.gameOptions.bgmVolumeRatio = 0;

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.GAME_OPTIONS);
        if (successCallback != null)
            successCallback();
    }

    public void ReqMuteEffect(Callback successCallback = null) {
        UserDataModel.instance.gameOptions.effectVolumeRatio = 0;

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.GAME_OPTIONS);
        if (successCallback != null)
            successCallback();
    }

    public void ReqSetVibrate(bool enable, Callback successCallback = null) {
        UserDataModel.instance.gameOptions.vibrate = enable;
        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.GAME_OPTIONS);
        if (successCallback != null)
            successCallback();
    }
}
