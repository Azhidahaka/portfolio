using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSetNpc : MonoBehaviour {
    public GameObject topObject;
    public GameObject bottomObject;
    public GameObject topNpcObject;

    public Text lblTopText;
    public Text lblBottomText;
    public GameObject bottomNpcObject;

    private GameData.TutorialStepDTO tutorialData;

    public void SetData(GameData.TutorialStepDTO tutorialData) {
        this.tutorialData = tutorialData;

        if (gameObject.activeSelf == false)
            Common.ToggleActive(gameObject, true);

        //메시지를 띄우지 않는 튜토리얼인경우 모든 오브젝트를 꺼준다.
        if (tutorialData.showMsg == (long)TUTORIAL_SHOW_MSG.NONE) {
            Common.ToggleActive(topObject, false);
            Common.ToggleActive(bottomObject, false);
            return;
        }

        //이전 메시지를 유지하는경우 아무것도 하지 않는다.
        if (tutorialData.showMsg == (long)TUTORIAL_SHOW_MSG.KEEP) 
            return;
        

        if (tutorialData.descPos == (long)TUTORIAL_DESC_POS.TOP) {
            lblTopText.text = TermModel.instance.GetTutorialDesc(tutorialData.tutorialID, tutorialData.step);

            Common.ToggleActive(topObject, true);
            Common.ToggleActive(bottomObject, false);
        }
        else if (tutorialData.descPos == (long)TUTORIAL_DESC_POS.BOTTOM) {
            lblBottomText.text = TermModel.instance.GetTutorialDesc(tutorialData.tutorialID, tutorialData.step);

            Common.ToggleActive(topObject, false);
            Common.ToggleActive(bottomObject, true);
        }

        if (tutorialData.showNpc == 0) {
            Common.ToggleActive(topNpcObject, false);
            Common.ToggleActive(bottomNpcObject, false);
        }
        else {
            Common.ToggleActive(topNpcObject, true);
            Common.ToggleActive(bottomNpcObject, true);
        }

        
    }

    public void Show() {
        Common.ToggleActive(gameObject, true);
    }

    public void Hide() {
        Common.ToggleActive(gameObject, false);
    }
}
