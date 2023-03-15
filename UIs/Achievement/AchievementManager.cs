using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour {
    public static AchievementManager instance;

    private void Awake() {
        instance = this;
    }

    public void StartCheckDate() {
        StopAllCoroutines();
        StartCoroutine(JobCheckDate());
    }

    private IEnumerator JobCheckDate() {
        while (true) {
            long prevLoginUtcZero = UserDataModel.instance.userProfile.loginUtcZero;
            long loginUtcZero = Common.GetUTCDateZero(Common.GetUTCNow());
            //하루전에 로그인 기록이 있으면 DAILY_CONTINUOUS_LOGIN_COUNT 추가
            if (prevLoginUtcZero + 86400 == loginUtcZero) 
                WebUser.instance.ReqRefreshDate();
            
            yield return new WaitForSeconds(1.0f);
        }
    }

    public void CheckAchievement() {
        for (ACHIEVEMENT_GROUP group = ACHIEVEMENT_GROUP.START + 1; group < ACHIEVEMENT_GROUP.END; group++) {
            List<GameData.AchievementDTO> selectedDatas = AchievementUtil.GetSelectedAchievementDatas(group);
            //이미 완료했으면 패스
            if (UserDataModel.instance.statistics.clearedAchievementIDs.Contains(selectedDatas[0].achievementID) ||
                UserDataModel.instance.statistics.receivedAchievementIDs.Contains(selectedDatas[0].achievementID)) 
                continue;

            if (AchievementUtil.IsCleared(selectedDatas)) {
                long achievementID = selectedDatas[0].achievementID;
                UserDataModel.instance.SetAchievementCleared(achievementID);

                //문제 발생시 삭제(재귀호출)
                if (selectedDatas[0].repeat == (long)ACHIEVEMENT_REPEAT.DAILY) 
                    UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.DAILY_ACHIEVEMENT_CLEAR_COUNT, 1);

                EventManager.Notify(EventEnum.UpdateAchievementCleared, achievementID);
                AchievementNoticePopup popup = UIManager.instance.GetUI<AchievementNoticePopup>(UI_NAME.AchievementNoticePopup);
                popup.SetData(achievementID);
                popup.Show();
            }
        }
    }
}
