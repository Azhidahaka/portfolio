using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackendQuestion : MonoBehaviour {
    public static BackendQuestion instance;

    private string questionAuthorize;

    private void Awake() {
        instance = this;
    }

    public void ShowQuestion() {
#if !UNITY_EDITOR
        NetworkLoading.instance.Show();
        Backend.Question.GetQuestionAuthorize((callback) => {
            if (callback.IsSuccess()) {
                NetworkLoading.instance.Hide();
                questionAuthorize = callback.GetReturnValuetoJSON()["authorize"].ToString();
                ShowQuestionView();
            }
            else
                NetworkErrorHandler.instance.OnFail(null, ShowQuestion);
        });
#endif
    }

    private void ShowQuestionView() {
        bool isQuestionViewOpen = false;
        string inDate = BackendLogin.instance.UserInDate;
        // margin(빈 여백)이 10인 1대1 문의창을 생성합니다.
#if !UNITY_EDITOR && UNITY_ANDROID
        isQuestionViewOpen = BackEnd.Support.Android.Question.OpenQuestionView(questionAuthorize, inDate);
#elif UNITY_IOS
        isQuestionViewOpen = BackEnd.Support.iOS.Question.OpenQuestionView(questionAuthorize, inDate);
#endif
        if (isQuestionViewOpen) 
            Debug.Log("1대1문의창이 생성되었습니다");
    }
}
