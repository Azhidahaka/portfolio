using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebMail : MonoBehaviour {
    public static WebMail instance;

    private void Awake() {
        instance = this;
    }

    public void ReqMailRead(UserData.MailInfoDTO mailInfo, Callback successCallback = null) {
        UserDataModel.instance.ConfirmMail(mailInfo.no, mailInfo.partNo);
        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.MAIL_INFOS);

        if (successCallback != null)
            successCallback();
    }

    private void ResMailRead(params object[] args) {
        
    }

    public void ReqDeleteReadMails(Callback successCallback = null) {
        List<UserData.MailInfoDTO> deleteMailInfos = new List<UserData.MailInfoDTO>();
        List<UserData.MailInfoDTO> mailInfos = UserDataModel.instance.mailInfos;
        for (int i = 0; i < mailInfos.Count; i++) {
            UserData.MailInfoDTO mailInfo = mailInfos[i];
            if (mailInfo.read && mailInfo.received)
                deleteMailInfos.Add(mailInfo);
        }
        
        if (deleteMailInfos.Count == 0) {
            string msg = TermModel.instance.GetTerm("msg_no_delete_mail");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        for (int i = 0; i < deleteMailInfos.Count; i++) {
            UserDataModel.instance.RemoveMail(deleteMailInfos[i]);
        }

        UserDataModel.instance.SaveUserDatas(true,
                                             USER_DATA_KEY.MAIL_INFOS);

        if (successCallback != null)
            successCallback();
    }
}
