using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroScenePresenter : MonoBehaviour {
    public Reporter reporter;

    private void Start() {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugUI.instance.Show();
        Debug.Log("개발빌드 IntroScenePresenter");
        Common.ToggleActive(reporter.gameObject, true);
#else
        DebugUI.instance.Hide();
        Common.ToggleActive(reporter.gameObject, false);
#endif
    }

    private void OnEnable() {
        StartCoroutine(ShowLogo());
    }

    IEnumerator ShowLogo() {
        yield return new WaitForEndOfFrame();
        Logo logo = UIManager.instance.GetUI<Logo>(UI_NAME.Logo);
        logo.Show();

        yield return new WaitForSeconds(2.0f);
        
        Init();
    }

    private void Init() {
        BackendLogin.instance.Initialize();
    }

    private void InitFirebase() {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }
}
