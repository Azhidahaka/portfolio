using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebNotice : MonoBehaviour {
    public static WebNotice instance;

    private void Awake() {
        instance = this;
    }

    public void ReqSetNoticeRead(UserData.NoticeDTO noticeInfo) {
        //이미 읽은상태면 아무것도 하지 않음
        if (noticeInfo.read)
            return;

        noticeInfo.read = true;

        UserDataModel.instance.SaveUserDatas(true, USER_DATA_KEY.NOTICE_INFOS);
        EventManager.Notify(EventEnum.NoticeRead);
    }
}
