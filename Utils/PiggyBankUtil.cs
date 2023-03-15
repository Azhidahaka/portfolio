using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiggyBankUtil {
    public static float GetCurrentGold(GameData.PiggyBankDTO piggyBankData) {
        float prevGold = UserDataModel.instance.userProfile.collectedGold;

        //최대치를 초과하는경우 최대치까지만 모인다.
        if (prevGold > piggyBankData.capacity)
            return prevGold;

        long collectedChangedTime = UserDataModel.instance.userProfile.collectedChangedTime;

        long elapsedTime = Common.GetUnixTimeNow() - collectedChangedTime;

        long recoverGold = 0;
        if (collectedChangedTime > 0)
            recoverGold = 
                (long)(elapsedTime / piggyBankData.timeCycle * piggyBankData.timeValue);
        
        float resultGold = prevGold + recoverGold;
        if (resultGold > piggyBankData.capacity)
            resultGold = piggyBankData.capacity;
        return resultGold;
    }
}
