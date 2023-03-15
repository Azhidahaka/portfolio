using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSet : UIBase {
    public class EndEventInfo {
        public EndEventInfo(EventEnum eventEnum, ObjectCallback callback) {
            this.eventEnum = eventEnum;
            this.callback = callback;
        }

        public EventEnum eventEnum;
        public ObjectCallback callback;
    }

    public GameObject descCardObject;
    public Transform descParent;

    public TutorialSetFocus focus;
    public TutorialSetFocus rectFocus;
    public TutorialSetFinger finger;

    public GameObject floorRingObject;

    public GameObject backgroundObject;
    public Image bgDesc;
    public GameObject transparentBGObject;

    public GameObject delayObject;

    public TutorialSetNpc npc;

    public GameObject btnSkipObject;

    public GameObject parentLblMessage;
    public List<Text> lblMessageList;

    private List<GameData.TutorialStepDTO> tutorialStepDatas;
    private int stepIndex;
    private GameData.TutorialStepDTO currentStepData;

    private Transform targetTransform;

    private EndEventInfo endEventInfo;

    private GameObject descPageObject;
    private string msg;

    public void SetData(List<GameData.TutorialStepDTO> tutorialStepDatas) {
        this.tutorialStepDatas = tutorialStepDatas;
        stepIndex = 0;
        SetStep();
        DetermineShowBtnSkip();
    }

    private void DetermineShowBtnSkip() {
        if (tutorialStepDatas == null || tutorialStepDatas.Count == 0) {
            Common.ToggleActive(btnSkipObject, false);
            return;
        }

        GameData.TutorialDTO tutorialData = GameDataModel.instance.GetTutorialData(tutorialStepDatas[0].tutorialID);
        if (tutorialData.skip == 0)
            Common.ToggleActive(btnSkipObject, false);
        else
            Common.ToggleActive(btnSkipObject, true);
    }

    private void SetStep() {
        if (stepIndex >= tutorialStepDatas.Count) {
            stepIndex = 0;
            tutorialStepDatas = null;
            EventManager.Notify(EventEnum.TutorialEnd);
            TutorialManager.instance.SetTimeScale(1);
            return;
        }

        currentStepData = tutorialStepDatas[stepIndex];
        switch((TUTORIAL_STEP_TYPE)currentStepData.type) {
            case TUTORIAL_STEP_TYPE.FOCUS: {
                ShowFocus();
                break;
            }
            case TUTORIAL_STEP_TYPE.DESC: {
                ShowDesc();
                break;
            }
            case TUTORIAL_STEP_TYPE.DESC_PAGE: {
                ShowDescPage();
                break;
            }
            case TUTORIAL_STEP_TYPE.TRANSPARENT_BG: {
                ShowTransparentBG();
                break;
            }
            case TUTORIAL_STEP_TYPE.SHOW_OBJECT: {
                ShowObject();
                break; //리턴!!
            }
            case TUTORIAL_STEP_TYPE.HIDE_OBJECT: {
                HideObject();
                break;
            }

            case TUTORIAL_STEP_TYPE.RECT_FOCUS: {
                ShowRectFocus();
                break;
            }
        }

        TutorialManager.instance.SetTimeScale(currentStepData.timeScale);

        DetermineShowMessage();
        DetermineShowDelay();

        Debug.Log(string.Format("Tutorial ID = {0}, Step = {1}", currentStepData.tutorialID, currentStepData.step));
        NotifyStartEvent();     
        DetermineSetNext();
    }

    private void DetermineSetNext() {
        if (currentStepData.type != (long)TUTORIAL_STEP_TYPE.SHOW_OBJECT &&
            currentStepData.type != (long)TUTORIAL_STEP_TYPE.HIDE_OBJECT)
            return;

        SetNext();
    }

    private void ShowRectFocus() {
        SetTarget();
        
        rectFocus.SetTarget(targetTransform);
        rectFocus.transform.localScale = new Vector3(currentStepData.focusScale, currentStepData.extension, 1);
        Common.ToggleActive(rectFocus.gameObject, true);

        if (currentStepData.finger == (long)TUTORIAL_STEP_FINGER.SHOW) {
            finger.SetTarget(rectFocus.posFinger);
            
            StartCoroutine(JobShowFinger(currentStepData.delay));
        }
        else 
            Common.ToggleActive(finger.gameObject, false);
        SetEndEvent();

        Common.ToggleActive(focus.gameObject, false);
        Common.ToggleActive(descCardObject, false);
        Common.ToggleActive(backgroundObject, false);
        Common.ToggleActive(transparentBGObject, false);
    }

    private IEnumerator JobShowFinger(float delay) {
        yield return new WaitForSecondsRealtime(currentStepData.delay);
        ShowFinger();
    }

    private void NotifyStartEvent() {
        if (currentStepData.startEvent == 0)
            return;

        EventEnum eventName = Common.ToEnum<EventEnum>(((TUTORIAL_STEP_EVENT)currentStepData.startEvent).ToString());
        Debug.Log("NotifyStartEvent::" + eventName.ToString());
        EventManager.Notify(eventName);
    }

    private void DetermineShowDelay() {
        if (currentStepData.delay <= 0) 
            return;
        
        Common.ToggleActive(delayObject, true);
        StartCoroutine(JobHideDelay(currentStepData.delay));
    }

    private IEnumerator JobHideDelay(float delay) {
        yield return new WaitForSecondsRealtime(delay);
        HideDelay();
    }

    private void HideDelay() {
        Common.ToggleActive(delayObject, false);
        EventManager.Notify(EventEnum.TutorialDelayEnd);
    }

    private void ShowTransparentBG() {
        SetTarget();
        SetEndEvent();

        Common.ToggleActive(transparentBGObject, true);

        Common.ToggleActive(rectFocus.gameObject, false);
        Common.ToggleActive(focus.gameObject, false);
        Common.ToggleActive(finger.gameObject, false);
        Common.ToggleActive(descCardObject, false);
        Common.ToggleActive(parentLblMessage, false);
        Common.ToggleActive(backgroundObject, false);
    }

    private void ShowObject() {
        SetTarget();

        Common.ToggleActive(targetTransform.gameObject, true);
        
        Common.ToggleActive(rectFocus.gameObject, false);
        Common.ToggleActive(backgroundObject, false);
        Common.ToggleActive(transparentBGObject, false);
        Common.ToggleActive(focus.gameObject, false);
        Common.ToggleActive(finger.gameObject, false);
        Common.ToggleActive(descCardObject, false);
        Common.ToggleActive(parentLblMessage, false);
    }

    private void HideObject() {
        SetTarget();
        Common.ToggleActive(targetTransform.gameObject, false);

        Common.ToggleActive(rectFocus.gameObject, false);
        Common.ToggleActive(backgroundObject, false);
        Common.ToggleActive(transparentBGObject, false);
        Common.ToggleActive(focus.gameObject, false);
        Common.ToggleActive(finger.gameObject, false);
        Common.ToggleActive(descCardObject, false);
        Common.ToggleActive(parentLblMessage, false);    
    }

    private void ShowDescPage() {
        SetTarget();

        if (descPageObject != null)
            Destroy(descPageObject);

        descPageObject = ResourceManager.instance.LoadTutorialDesc(currentStepData.tutorialID, currentStepData.step, descParent);

        Common.ToggleActive(descCardObject, true);

        //배경 표시 여부
        if (currentStepData.extension == 0)
            Common.ToggleActive(backgroundObject, false);
        else {
            Common.ToggleActive(backgroundObject, true);
            Color color = bgDesc.color;
            if (currentStepData.extension == 1) 
                color.a = 216.75f / 255f;
            else if (currentStepData.extension == 2) 
                color.a = 0.005f;
            bgDesc.color = color;
        }

        Common.ToggleActive(rectFocus.gameObject, false);
        Common.ToggleActive(transparentBGObject, false);
        Common.ToggleActive(focus.gameObject, false);
        Common.ToggleActive(finger.gameObject, false);
        Common.ToggleActive(parentLblMessage, false);

        SetEndEvent();
    }

    private void ShowDesc() {
        SetTarget();

        DetermineShowMessage();

        //배경 표시 여부
        if (currentStepData.extension == 0)
            Common.ToggleActive(backgroundObject, false);
        else {
            Common.ToggleActive(backgroundObject, true);
            Color color = bgDesc.color;
            if (currentStepData.extension == 1) 
                color.a = 216.75f / 255f;
            else if (currentStepData.extension == 2) 
                color.a = 0.005f;
            bgDesc.color = color;
        }

        Common.ToggleActive(rectFocus.gameObject, false);
        Common.ToggleActive(focus.gameObject, false);
        Common.ToggleActive(finger.gameObject, false);
        Common.ToggleActive(descCardObject, false);
        Common.ToggleActive(transparentBGObject, false);

        SetEndEvent();
    }

    private void ShowFocus() {
        SetTarget();
        
        focus.SetTarget(targetTransform);
        //focus.gameObject.transform.position = targetTransform.position;
        focus.transform.localScale = Vector3.one * currentStepData.focusScale;
        Common.ToggleActive(focus.gameObject, true);
        if (currentStepData.focusScale != 1)
            Common.ToggleActive(floorRingObject, false);
        else
            Common.ToggleActive(floorRingObject, true);

        if (currentStepData.finger == (long)TUTORIAL_STEP_FINGER.SHOW) {
            finger.SetTarget(focus.posFinger);
            StartCoroutine(JobShowFinger(currentStepData.delay));
        }
        else 
            Common.ToggleActive(finger.gameObject, false);
        SetEndEvent();

        Common.ToggleActive(rectFocus.gameObject, false);
        Common.ToggleActive(descCardObject, false);
        Common.ToggleActive(backgroundObject, false);
        Common.ToggleActive(transparentBGObject, false);
    }

    private void DetermineShowMessage() {
        if (currentStepData.showMsg == (long)TUTORIAL_SHOW_MSG.NONE) {
            Common.ToggleActive(parentLblMessage, false);
            return;
        }

        if (currentStepData.showMsg == (long)TUTORIAL_SHOW_MSG.NEW) 
            msg = TermModel.instance.GetTutorialDesc(currentStepData.tutorialID, currentStepData.step);

        Common.ToggleActive(parentLblMessage.gameObject, true);
        for (int i = 0; i < lblMessageList.Count; i++) {
            if (i + 1 == currentStepData.descPos) {
                Common.ToggleActive(lblMessageList[i].gameObject, true);
                lblMessageList[i].text = msg;
            }
            else
                Common.ToggleActive(lblMessageList[i].gameObject, false);
        }
    }

    private void ShowFinger() {
        finger.SetPosition();
        Common.ToggleActive(finger.gameObject, true);
    }

    private void SetTarget() {
        switch ((TUTORIAL_STEP_TARGET)currentStepData.target) {
            case TUTORIAL_STEP_TARGET.NONE: {
                targetTransform = null;
                break;
            }

            case TUTORIAL_STEP_TARGET.LOCATION_MATCH_BLOCKS_BTN_BOMB_ITEM: {
                MatchBlocks matchBlocks = UIManager.instance.GetUI<MatchBlocks>(UI_NAME.MatchBlocks, false);
                if (matchBlocks == null || matchBlocks.IsActive == false)
                    targetTransform = null;
                else 
                    targetTransform = matchBlocks.GetLocationBtnBombItem();
                break;
            }

            case TUTORIAL_STEP_TARGET.MATCH_BLOCKS_FIRST_BUNDLE: {
                MatchBlocks matchBlocks = UIManager.instance.GetUI<MatchBlocks>(UI_NAME.MatchBlocks, false);
                if (matchBlocks == null || matchBlocks.IsActive == false)
                    targetTransform = null;
                else 
                    targetTransform = matchBlocks.GetFirstBundleTransform();
                break;
            }

            case TUTORIAL_STEP_TARGET.LEAGUE_FIRST_SLOT: {
                MultiGameListPopup multiGameListPopup = UIManager.instance.GetUI<MultiGameListPopup>(UI_NAME.MultiGameListPopup, false);
                if (multiGameListPopup == null || multiGameListPopup.IsActive == false)
                    targetTransform = null;
                else 
                    targetTransform = multiGameListPopup.GetFirstSlotTransform();
                break;
            }

            case TUTORIAL_STEP_TARGET.LEAGUE_FIRST_SLOT_POS_BTN_REWARD: {
                MultiGameListPopup multiGameListPopup = UIManager.instance.GetUI<MultiGameListPopup>(UI_NAME.MultiGameListPopup, false);
                if (multiGameListPopup == null || multiGameListPopup.IsActive == false)
                    targetTransform = null;
                else {
                    MultiGameListPopupSlot firstSlot = multiGameListPopup.GetFirstSlot();
                    targetTransform = firstSlot.goPosBtnReward.transform;
                }
                break;
            }

            case TUTORIAL_STEP_TARGET.LEAGUE_FIRST_SLOT_POS_BTN_START: {
                MultiGameListPopup multiGameListPopup = UIManager.instance.GetUI<MultiGameListPopup>(UI_NAME.MultiGameListPopup, false);
                if (multiGameListPopup == null || multiGameListPopup.IsActive == false)
                    targetTransform = null;
                else {
                    MultiGameListPopupSlot firstSlot = multiGameListPopup.GetFirstSlot();
                    targetTransform = firstSlot.goPosBtnStart.transform;
                }
                break;
            }

            case TUTORIAL_STEP_TARGET.CHALLENGE_FRIEND_FIRST_SLOT_BTN_SELECT: {
                ChallengeFriendListPopup friendListPopup = UIManager.instance.GetUI<ChallengeFriendListPopup>(UI_NAME.ChallengeFriendListPopup, false);
                if (friendListPopup == null || friendListPopup.IsActive == false)
                    targetTransform = null;
                else {
                    ChallengeFriendListPopupSlot firstSlot = friendListPopup.GetFirstSlot();
                    targetTransform = firstSlot.goBtnSelect.transform;
                }
                break;
            }

            case TUTORIAL_STEP_TARGET.CHALLENGE_FRIEND_FIRST_SLOT: {
                ChallengeFriendListPopup friendListPopup = UIManager.instance.GetUI<ChallengeFriendListPopup>(UI_NAME.ChallengeFriendListPopup, false);
                if (friendListPopup == null || friendListPopup.IsActive == false)
                    targetTransform = null;
                else {
                    ChallengeFriendListPopupSlot firstSlot = friendListPopup.GetFirstSlot();
                    targetTransform = firstSlot.transform;
                }
                break;
            }

            case TUTORIAL_STEP_TARGET.MAILBOX_FIRST_SLOT: {
                MailBoxPopup mailBoxPopup = UIManager.instance.GetUI<MailBoxPopup>(UI_NAME.MailBoxPopup, false);
                if (mailBoxPopup == null || mailBoxPopup.IsActive == false)
                    targetTransform = null;
                else {
                    MailBoxPopupItem firstSlot = mailBoxPopup.GetFirstSlot();
                    targetTransform = firstSlot.transform;
                }
                break;
            }

            case TUTORIAL_STEP_TARGET.MAILBOX_FIRST_SLOT_BTN_REWARD: {
                MailBoxPopup mailBoxPopup = UIManager.instance.GetUI<MailBoxPopup>(UI_NAME.MailBoxPopup, false);
                if (mailBoxPopup == null || mailBoxPopup.IsActive == false)
                    targetTransform = null;
                else {
                    MailBoxPopupItem firstSlot = mailBoxPopup.GetFirstSlot();
                    targetTransform = firstSlot.goBtnReceive.transform;
                }
                break;
            }

            default:
                targetTransform = TutorialManager.instance.GetTutorialTarget((TUTORIAL_STEP_TARGET)currentStepData.target);
                break;
        }
    }

    private void SetEndEvent() {
        if (currentStepData.endEvent == 0)
            return;

        endEventInfo = null;
            EventEnum eventName = Common.ToEnum<EventEnum>(((TUTORIAL_STEP_EVENT)currentStepData.endEvent).ToString());
        Debug.Log("SetEndEvent::" + eventName.ToString());
        endEventInfo = new EndEventInfo(eventName, OnEndEvent);
            
        EventManager.Register(endEventInfo.eventEnum, endEventInfo.callback);
    }

    public void OnBtnFocusClick() {
        Debug.Log("OnBtnFocusClick");
        //@todo  : Invoke가 실행이 맞는지 확인
        if (IsButtonTarget())
            targetTransform.GetComponent<Button>().onClick.Invoke();
        else
            OnBackgroundClick();
    }

    private bool IsButtonTarget() {
        if (targetTransform == null)
            return false;

        if (targetTransform.GetComponent<Button>() == null)
            return false;

        return true;
    }

    public void OnBackgroundClick() {
        Debug.Log("OnBackgroundClick");
        if (IsButtonTarget() == false && endEventInfo == null) 
            SetNext();
    }

    private void OnEndEvent(object[] args) {
        if (endEventInfo != null)
            Debug.Log("OnEndEvent::" + endEventInfo.eventEnum.ToString());
        EventManager.Remove(endEventInfo.eventEnum, endEventInfo.callback);
        endEventInfo = null;

        SetNext();
    }

    private void SetNext() {
        stepIndex++;
        SetStep();
    }

    public bool IsInProgress() {
        if (tutorialStepDatas != null && tutorialStepDatas.Count > 0)
            return true;
        return false;
    }

    public void OnBtnSkipClick() {
        SetTutorialSkip();
    }

    private void SetTutorialSkip() {
        stepIndex = 0;
        tutorialStepDatas = null;
        if (endEventInfo != null)
            EventManager.Remove(endEventInfo.eventEnum, endEventInfo.callback);
        endEventInfo = null;
        EventManager.Notify(EventEnum.TutorialEnd, true);
    }
}
