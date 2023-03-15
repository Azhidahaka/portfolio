using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LuckyFlow.EnumDefine;
using TMPro;
public class GainItemEffect : MatchBlocksEffect {
    public RawImage icoItem;
    public TextMeshProUGUI lblAmount;

    public void SetData(long itemID, long count, bool isTicket = false) {
        SetRealTime(true);
        if (isTicket == false) 
            icoItem.texture = ResourceManager.instance.GetIcoItemTexture(itemID);
        else  
            icoItem.texture = ResourceManager.instance.GetMailRewardIco((long)MAIL_REWARD_TYPE.CHALLENGE_TICKET);
        
        lblAmount.text = Common.GetCommaFormat(count);
    }
}
