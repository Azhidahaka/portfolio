using LitJson;
using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UserData;

public class MailUtil {
    public static string GetMailTitle(long mailCategory) {
        string result = TermModel.instance.GetTerm("mail_title_" + mailCategory.ToString());
        return result;
    }

    public static string GetMailContent(long mailCategory) {
        string result = TermModel.instance.GetTerm("mail_content_" + mailCategory.ToString());
        return result;
    }

    public static MailInfoDTO GetAdminMailInfo(JsonData mail) {
        MailInfoDTO mailInfo = new MailInfoDTO();

        mailInfo.title = mail["title"]["S"].ToString();
        mailInfo.content = mail["content"]["S"].ToString();

        string inDate = mail["inDate"]["S"].ToString();
        mailInfo.inDate = inDate;
        mailInfo.no = Common.ConvertStringToTimestamp(inDate);
        
        string expirationDate = mail["expirationDate"]["S"].ToString();
        mailInfo.expireTime = (long)Common.ConvertStringToTimestamp(expirationDate);
        string sentDate = mail["sentDate"]["S"].ToString();
        mailInfo.createTIme = (long)Common.ConvertStringToTimestamp(sentDate);
        
        mailInfo.rewards = new List<MailRewardDTO>();
        MailRewardDTO rewardInfo = new MailRewardDTO();
        long.TryParse(mail["item"]["M"]["type"]["S"].ToString(), out rewardInfo.type);
        long.TryParse(mail["itemCount"]["N"].ToString(), out rewardInfo.count);
        mailInfo.rewards.Add(rewardInfo);
        
        return mailInfo;
    }

    public static string GetChallengeTitle(string nickname, long state) {
        string title;
        bool nickNameEmpty = string.IsNullOrEmpty(nickname);

        string format;
        switch((CHALLENGE_STATE)state) {
            case CHALLENGE_STATE.RECEIVE:
                if (nickNameEmpty) 
                    format = TermModel.instance.GetTerm("challenge_receive");
                else
                    format = TermModel.instance.GetTerm("format_challenge_receive");
                break;
            case CHALLENGE_STATE.REJECT:
                if (nickNameEmpty)
                    format = TermModel.instance.GetTerm("challenge_reject");
                else
                    format = TermModel.instance.GetTerm("format_challenge_reject"); 
                break;
            default:
                if (nickNameEmpty)
                    format = TermModel.instance.GetTerm("challenge_result");
                else
                    format = TermModel.instance.GetTerm("format_challenge_check_result");
                break;
        }
        title = string.Format(format, nickname);
        return title;
    }
}
