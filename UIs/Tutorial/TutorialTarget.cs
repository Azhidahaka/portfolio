using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuckyFlow.EnumDefine;

public class TutorialTarget : MonoBehaviour {
    public TUTORIAL_STEP_TARGET target;

    public void Init() {
        TutorialManager.instance.SetTutorialTarget(target, transform);
    }
}
