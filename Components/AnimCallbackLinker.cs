using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimCallbackLinker : MonoBehaviour {
    private Callback callback;

    public void SetCallback(Callback callback) {
        this.callback = callback;
    }

    public void OnAnimFinished() {
        if (callback != null)
            callback();
    }
}
