using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour {
    public static TutorialManager instance;

    private List<GameData.TutorialDTO> tutorials;

    private long selectedTutorialID;
    private TutorialSet tutorialSet;

    private Dictionary<TUTORIAL_STEP_TARGET, Transform> dicTutorialTarget = new Dictionary<TUTORIAL_STEP_TARGET, Transform>();

    bool skip = false;

    private void Awake() {
        instance = this;
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.TutorialCheck, OnTutorialCheck);
        EventManager.Register(EventEnum.TutorialEnd, OnTutorialEnd);
    }

    private void OnPauseByTutorial(object[] args) {
        SetTimeScale(0);
    }

    public void SetTimeScale(float timeScale) {
        Time.timeScale = timeScale;
    }

    private void OnResumeByTutorial(object[] args) {
        SetTimeScale(1);
    }

    private void OnTutorialEnd(object[] args) {
        if (args != null && args.Length > 0)
            skip = (bool)args[0];
        WebTutorial.instance.ReqEndTutorial(selectedTutorialID, OnResEndTutorial);
    }

    private void OnResEndTutorial() {
        tutorialSet.Hide();
        selectedTutorialID = 0;

        skip = false;
        OnTutorialCheck(null);
    }

    private void OnTutorialCheck(object[] args) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Constant.SKIP_TUTORIAL)
            return;
#endif
        if (string.IsNullOrEmpty(BackendLogin.instance.nickname))
            return;

        if (tutorialSet != null && tutorialSet.IsInProgress())
            return;

        long tutorialID = 0;
        List<long> notMatchedIDs = new List<long>();

        tutorials = GameDataModel.instance.tutorials;

        for (int i = 0; i < tutorials.Count; i++) {
            GameData.TutorialDTO tutorialData = tutorials[i];

            if (tutorialData.use == 0)
                continue;

            //이미 완료한 튜토리얼이면 검사하지 않음
            if (UserDataModel.instance.statistics.tutorialCompleteIDs.Contains(tutorialData.tutorialID))
                continue;

            //선행 튜토리얼이 진행되지 않았으면 검사하지 않음
            if (tutorialData.requireTutorialID > 0 &&
                UserDataModel.instance.statistics.tutorialCompleteIDs.Contains(tutorialData.requireTutorialID) == false)
                continue;

            //튜토리얼 발동조건을 하나라도 충족하지 못한 경우 검사하지 않음
            if (notMatchedIDs.Contains(tutorialData.tutorialID))
                continue;

            if (tutorialID > 0 && tutorialID != tutorialData.tutorialID)
                break;

            if (IsMatched(tutorialData))
                tutorialID = tutorialData.tutorialID;
            else {
                tutorialID = 0;
                notMatchedIDs.Add(tutorialData.tutorialID);
            }
        }

        selectedTutorialID = tutorialID;
        
        if (selectedTutorialID > 0)
            StartTutorial();
    }

    private void StartTutorial() {
        Debug.Log("튜토리얼 시작 : " + selectedTutorialID);
        List<GameData.TutorialStepDTO> tutorialStepDatas = GameDataModel.instance.GetTutorialStepDatas(selectedTutorialID);
        if (tutorialSet == null)
            tutorialSet = UIManager.instance.GetUI<TutorialSet>(UI_NAME.TutorialSet);
        tutorialSet.Show();
        tutorialSet.SetData(tutorialStepDatas);

        WebTutorial.instance.ReqStartTutorial(selectedTutorialID);
    }

    private bool IsMatched(GameData.TutorialDTO tutorialData) {
        App.SCENE_NAME sceneName = App.instance.GetSceneName();
        if (sceneName.ToString() != tutorialData.sceneName1 &&
            sceneName.ToString() != tutorialData.sceneName2)
            return false;

        long targetValue = long.MinValue;
        long comparingValue = tutorialData.value;
        long comparingOperator = tutorialData.comparingOperator;
        switch ((TUTORIAL_CONDITION)tutorialData.condition) {
            case TUTORIAL_CONDITION.ACC_BLOCK_CLEAR_COUNT: {
                targetValue = UserDataModel.instance.GetStatistics(STATISTICS_TYPE.ACC_BLOCK_CLEAR_COUNT);
                break;
            }

            case TUTORIAL_CONDITION.NEXT_WAVE_REMAIN_TURN: {
                UserData.RefereeNoteDTO refereeNote = MatchBlocksReferee.instance.GetRefereeNote();
                if (refereeNote == null)
                    targetValue = 0;
                else
                    targetValue = refereeNote.nextWaveRemainTurn;
                break;
            }

            case TUTORIAL_CONDITION.ACC_HIT_COUNT: {
                UserData.RefereeNoteDTO refereeNote = MatchBlocksReferee.instance.GetRefereeNote();
                if (refereeNote == null)
                    targetValue = 0;
                else
                    targetValue = refereeNote.accumulateHitCount;
                break;
            }

            case TUTORIAL_CONDITION.WAVE_SCORE: {
                UserData.RefereeNoteDTO refereeNote = MatchBlocksReferee.instance.GetRefereeNote();
                if (refereeNote == null)
                    targetValue = 0;
                else
                    targetValue = refereeNote.waveScore;
                break;
            }

            case TUTORIAL_CONDITION.LOGIN_COUNT: {
                targetValue = UserDataModel.instance.GetStatistics(STATISTICS_TYPE.ACC_LOGIN_COUNT);
                break;
            }

            case TUTORIAL_CONDITION.NO_SPACE: {
                NoSpaceWarning noSpaceWarning = UIManager.instance.GetUI<NoSpaceWarning>(UI_NAME.NoSpaceWarning, false);
                if (noSpaceWarning == null || noSpaceWarning.IsActive == false)
                    targetValue = 0;
                else
                    targetValue = 1;
                break;
            }

            case TUTORIAL_CONDITION.CREATE_GLASS_COUNT: {
                targetValue = UserDataModel.instance.GetStatistics(STATISTICS_TYPE.ROUND_CREATE_GLASS);
                break;
            }

            case TUTORIAL_CONDITION.CREATE_VINE_COUNT: {
                targetValue = UserDataModel.instance.GetStatistics(STATISTICS_TYPE.ROUND_CREATE_VINE);
                break;
            }

            case TUTORIAL_CONDITION.CREATE_WALL_COUNT: {
                targetValue = UserDataModel.instance.GetStatistics(STATISTICS_TYPE.ROUND_CREATE_WALL);
                break;
            }

            case TUTORIAL_CONDITION.SHOW_RESULT_PERSONAL: {
                ResultPopupPersonal resultPopupPersonal = UIManager.instance.GetUI<ResultPopupPersonal>(UI_NAME.ResultPopupPersonal, false);
                if (resultPopupPersonal == null || resultPopupPersonal.IsActive == false)
                    targetValue = 0;
                else
                    targetValue = 1;
                break;
            }

            case TUTORIAL_CONDITION.ACC_PLAYED_COUNT: {
                targetValue = UserDataModel.instance.GetStatistics(STATISTICS_TYPE.ACC_PLAYED_COUNT);
                break;
            }

            case TUTORIAL_CONDITION.RANKING_ACTIVE: {
                Ranking ranking = UIManager.instance.GetUI<Ranking>(UI_NAME.Ranking, false);
                if (ranking == null || ranking.IsActive == false)
                    targetValue = 0;
                else
                    targetValue = 1;
                break;
            }

            case TUTORIAL_CONDITION.SHOW_LEAGUE_POPUP: {
                MultiGameListPopup multiGameListPopup = UIManager.instance.GetUI<MultiGameListPopup>(UI_NAME.MultiGameListPopup, false);
                if (multiGameListPopup == null || multiGameListPopup.IsActive == false)
                    targetValue = 0;
                else
                    targetValue = 1;
                break;
            }

            case TUTORIAL_CONDITION.LEAGUE_EXIST: {
                MultiGameListPopup multiGameListPopup = UIManager.instance.GetUI<MultiGameListPopup>(UI_NAME.MultiGameListPopup, false);
                if (multiGameListPopup == null || multiGameListPopup.IsActive == false)
                    targetValue = 0;
                else {
                    if (multiGameListPopup.GetFirstSlotTransform() == null)
                        targetValue = 0;
                    else
                        targetValue = 1;
                }
                break;
            }

            case TUTORIAL_CONDITION.FRIEND_COUNT: {
                targetValue = UserDataModel.instance.friends.Count;
                break;
            }

            case TUTORIAL_CONDITION.GOLD_AMOUNT: {
                targetValue = UserDataModel.instance.userProfile.gold;
                break;
            }

            case TUTORIAL_CONDITION.SHOW_MAILBOX: {
                MailBoxPopup mailBoxPopup = UIManager.instance.GetUI<MailBoxPopup>(UI_NAME.MailBoxPopup, false);
                if (mailBoxPopup == null || mailBoxPopup.IsActive == false)
                    targetValue = 0;
                else
                    targetValue = 1;
                break;
            }

            case TUTORIAL_CONDITION.REFEREE_SCORE: {
                if (MatchBlocksReferee.instance == null)
                    targetValue = 0;
                else {
                    UserData.RefereeNoteDTO refereeNote = MatchBlocksReferee.instance.GetRefereeNote();
                    targetValue = refereeNote.totalScore;
                }
                break;
            }

            case TUTORIAL_CONDITION.CHALLENGE_NO_RECEIVER: {
                if (string.IsNullOrEmpty(UserDataModel.instance.challengeMsgInfo.receiverInDate)) 
                    targetValue = 1;
                else
                    targetValue = 0;
                break;
            }

            case TUTORIAL_CONDITION.CHALLENGE_SEND_REMAIN_COUNT: {
                targetValue = UserDataModel.instance.userProfile.challengeSendRemainCount;
                break;
            }
            default: {
                targetValue = 0;
                break;
            }
        }

        return OperatorUtil.IsMatchedValue(comparingOperator, targetValue, comparingValue);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.TutorialCheck, OnTutorialCheck);
        EventManager.Remove(EventEnum.TutorialEnd, OnTutorialEnd);
    }

    public bool IsTutorialInProgress() {
        return selectedTutorialID > 0;
    }
    
    public void SetTutorialTarget(TUTORIAL_STEP_TARGET target, Transform transform) {
        if (dicTutorialTarget.ContainsKey(target)) 
            dicTutorialTarget[target] = transform;
        else {
            dicTutorialTarget.Add(target, transform);
        }
    }

    public Transform GetTutorialTarget(TUTORIAL_STEP_TARGET target) {
        if (dicTutorialTarget.ContainsKey(target))
            return dicTutorialTarget[target];
        return null;
    }

    public long GetTutorialID() {
        return selectedTutorialID;
    }
}
