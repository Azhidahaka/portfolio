using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoveBlockRanged : MonoBehaviour {
    public RawImage ico;
    public void SetData(long itemID) {
        ico.texture = ResourceManager.instance.GetIcoItemTexture(itemID);
    }
}
