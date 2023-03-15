using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchBlocksBlockUnit : MonoBehaviour {
    public RawImage ico;

    public Animator resetPatternAnimator;

    private Vector2 coordinates;
    private int blockTextureIndex = Constant.INCORRECT;
    private bool blank;

    public void SetData(int newBlockTextureIndex = Constant.INCORRECT) {
        blank = newBlockTextureIndex == Constant.INCORRECT;
        blockTextureIndex = newBlockTextureIndex;

        if (blank == false) {
            ico.enabled = true;
            ico.raycastTarget = true;
            ico.texture = ResourceManager.instance.GetBlockTexture(blockTextureIndex);
        }
        else {
            ico.enabled = false;
            ico.raycastTarget = false;
            ico.texture = null;
        }
    }

    public void Show() {
        Common.ToggleActive(gameObject, true);
    }

    public void Hide() {
        Common.ToggleActive(gameObject, false);
    }

    public void SetCoordinates(int x, int y) {
        coordinates = new Vector2(x, y);
    }

    public Vector2 GetCoordinates() {
        return coordinates;
    }

    public int GetBlockTextureIndex() {
        return blockTextureIndex;
    }

    public void SetBlockTextureIndex(int blockTextureIndex) {
        if (blank == false) {
            this.blockTextureIndex = blockTextureIndex;
            if (blockTextureIndex == Constant.INCORRECT) 
                ico.texture = null;
            else 
                ico.texture = ResourceManager.instance.GetBlockTexture(blockTextureIndex);
        }
    }

    public bool IsBlank() {
        return blank;
    }

    public void ResetPattern(float delay) {
        if (blank)
            return;

        blockTextureIndex = MatchBlocksReferee.instance.GetRandomBlockTextureIndex();
        Invoke("PlayChangePatternAnim", delay);
    }

    private void PlayChangePatternAnim() {
        AnimationUtil.SetTrigger(resetPatternAnimator, "ColorChange");
    }

    public void ChangePattern() {
        ico.texture = ResourceManager.instance.GetBlockTexture(blockTextureIndex);
    }
}
