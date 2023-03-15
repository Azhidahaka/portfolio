using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using UnityEngine.UI;

public class SceneLoading : UIBase {
    public static SceneLoading instance;
    public Slider sldProgress;

    private SCENE_LOADING_PROGRESS progress;
    private AsyncOperation async;

    public void SetInstance() {
        instance = this;
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.SceneLoadingProgressChanged, OnSceneLoadingProgressChanged);

        progress = SCENE_LOADING_PROGRESS.NONE;
        StopAllCoroutines();
        StartCoroutine(JobCheckProgress());
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.SceneLoadingProgressChanged, OnSceneLoadingProgressChanged);
    }

    private void OnSceneLoadingProgressChanged(object[] args) {
        progress = (SCENE_LOADING_PROGRESS)args[0];
        if (args.Length == 2)
            async = (AsyncOperation)args[1];
        SetSlider();
    }

    private IEnumerator JobCheckProgress() {
        if (sldProgress != null)
            sldProgress.value = 0;

        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => progress == SCENE_LOADING_PROGRESS.DONE);

        yield return new WaitForSeconds(0.5f);

        Hide();

        EventManager.Notify(EventEnum.TutorialCheck);
    }

    private void SetSlider() {
        if (sldProgress == null)
            return;

        StopCoroutine(JobSetSlider());
        StartCoroutine(JobSetSlider());
    }

    IEnumerator JobSetSlider() {
        float startValue = sldProgress.value;
        float endValue = (float)progress / 100;
        float duration = 0.1f;
        float startTime = Time.time;
        
        float elapsedTime;
        do {
            elapsedTime = Time.time - startTime;
            if (elapsedTime > duration)
                elapsedTime = duration;

            sldProgress.value = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            yield return new WaitForEndOfFrame();
        } while(elapsedTime < duration);
    }
}
