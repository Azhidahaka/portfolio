using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkLoading : UIBase {
    public static NetworkLoading instance;

    public void SetInstance() {
        instance = this;
    }
}
