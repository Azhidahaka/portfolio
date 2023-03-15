using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementUtil {
    public static List<GameData.AchievementDTO> GetSelectedAchievementDatas(ACHIEVEMENT_GROUP group) {
        List<GameData.AchievementDTO> achievementDatas = GameDataModel.instance.GetAchievementDatasByGroup((long)group);

        long selectedID = 0;

        foreach (GameData.AchievementDTO achievementData in achievementDatas) {
            List<long> clearedIDs = UserDataModel.instance.statistics.clearedAchievementIDs;
            List<long> rewardReceivedIds = UserDataModel.instance.statistics.receivedAchievementIDs;
            //보상까지 수령한 업적인경우 패스
            if (rewardReceivedIds.Contains(achievementData.achievementID))
                continue;

            selectedID = achievementData.achievementID;
            break;
        }

        if (selectedID == 0) 
            selectedID = achievementDatas[achievementDatas.Count - 1].achievementID;            
        List<GameData.AchievementDTO> selectedAchievementDatas = GameDataModel.instance.GetAchievementDatasByID(selectedID);
        return selectedAchievementDatas;
    }

    public static bool IsCleared(List<GameData.AchievementDTO> achievementDatas) {
        for (int i = 0; i < achievementDatas.Count; i++) {
            GameData.AchievementDTO achievementData = achievementDatas[i];
            long comparingValue;
            if (achievementData.repeat == (long)ACHIEVEMENT_REPEAT.IMMEDIATELY_INCREASE) {
                long level = UserDataModel.instance.GetAchievementLevel(achievementData.achievementID);
                //achievementData.value * achievementData.
                comparingValue = achievementData.value + achievementData.incValue * level;
            }
            else 
                comparingValue = achievementData.value;

            long value = UserDataModel.instance.GetStatistics((STATISTICS_TYPE)achievementData.valueType);
            bool matched = OperatorUtil.IsMatchedValue(achievementData.comparingOperator, value, comparingValue);
            if (matched == false)
                return false;
        }

        return true;
    }

    public static long GetCurrentValue(GameData.AchievementDTO achievementData) {
        long value = UserDataModel.instance.GetStatistics((STATISTICS_TYPE)achievementData.valueType);
        return value;
    }

    public static long GetComparingValue(GameData.AchievementDTO achievementData) {
        long comparingValue;
        if (achievementData.repeat == (long)ACHIEVEMENT_REPEAT.IMMEDIATELY_INCREASE) {
            long level = UserDataModel.instance.GetAchievementLevel(achievementData.achievementID);
            comparingValue = achievementData.value + achievementData.incValue * level;
        }
        else 
            comparingValue = achievementData.value;
        return comparingValue;
    }

    public static long GetComparingValue(GameData.AchievementDTO achievementData, long level) {
        long comparingValue;
        if (achievementData.repeat == (long)ACHIEVEMENT_REPEAT.IMMEDIATELY_INCREASE) {
            comparingValue = achievementData.value + achievementData.incValue * level;
        }
        else 
            comparingValue = achievementData.value;
        return comparingValue;
    }

    public static long GetClearedAchivementCount() {
        return UserDataModel.instance.statistics.clearedAchievementIDs.Count;
    }
}
