using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

public class NetworkErrorHandler : MonoBehaviour {
    public static NetworkErrorHandler instance;

    private void Awake() {
        instance = this;
    }

    private bool showMessage = false;

    public void OnFail(BackendReturnObject res, Callback retryAction) {
        Debug.Log("온페일");
        if (retryAction == null) {
            Dispatcher.AddAction(() => {
                if (res != null)
                    Debug.LogError($"res::{res.GetMessage()}");
                ShowSessionExpired();
            });
            showMessage = true;
            Debug.Log("온페일 끝1");
            return;
        }

        if (res == null) {
            Dispatcher.AddAction(() => ShowNetworkError(retryAction));
            showMessage = true;
            Debug.Log("온페일 끝2");
            return;
        }

        switch(res.GetStatusCode()) {
            case "401": {
                if (res.GetMessage().Contains("maintenance")) {
                    Dispatcher.AddAction(ShowMaintenanceError);
                }
                else {
                    Dispatcher.AddAction(() => {
                        Debug.LogError($"401::{res.GetErrorCode()}::{res.GetMessage()}");
                        ShowSessionExpired();
                    });
                }
                break;
            }

            case "400":
            case "429":
            case "503": {
                Dispatcher.AddAction(ShowServerError);
                break;
            }

            case "403": {
                Dispatcher.AddAction(ShowTooManyRequest);
                break;
            }

            case "408": 
            default: {
                Dispatcher.AddAction(() => ShowNetworkError(retryAction));
                break;
            }
        }
        
        showMessage = true;
        Debug.Log("온페일 끝3");
    }

    private void ShowNetworkError(Callback retryAction) {
        string msg = TermModel.instance.GetTerm("msg_network_error");
        MessageUtil.ShowNetworkWarning(retryAction);
        NetworkLoading.instance.Hide();
    }
    
    private void ShowTooManyRequest() {
        string msg = TermModel.instance.GetTerm("msg_too_many_request");
        MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.OK, msg, () => Application.Quit());
        NetworkLoading.instance.Hide();
    }

    private void ShowSessionExpired() {
        string msg = TermModel.instance.GetTerm("msg_session_expired");
        MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.OK, msg, () => {
            BackendLogin.instance.ResetUserIndate();
        });
        NetworkLoading.instance.Hide();
    }

    private void ShowServerError() {
        string msg = TermModel.instance.GetTerm("msg_server_error");
        MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.OK, msg, () => Application.Quit());
        NetworkLoading.instance.Hide();
    }

    private void ShowMaintenanceError() {
        string msg = TermModel.instance.GetTerm("msg_server_maintenance");
        MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.OK, msg, () => Application.Quit());
        NetworkLoading.instance.Hide();
    }

    public void Update() {
        if (showMessage == false)
            return;

        showMessage = false;
        Dispatcher.Instance.InvokePending();
    }
}
