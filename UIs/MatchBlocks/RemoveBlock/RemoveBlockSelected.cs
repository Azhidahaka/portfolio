using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RemoveBlockSelected : MonoBehaviour, IDragHandler {
    public RawImage icoItem;

    private MatchBlocksBoardSpace[] boardSpaces;
    private GameData.ItemDTO itemData;

    public void SetData(MatchBlocksBoardSpace[] boardSpaces, GameData.ItemDTO itemData) {
        this.boardSpaces = boardSpaces;
        this.itemData = itemData;

        icoItem.texture = ResourceManager.instance.GetIcoItemTexture
            (itemData.itemID);
    }

    public void OnDrag(PointerEventData eventData) {
        Vector3 pos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform, eventData.position, UICamera.instance.cam, out pos);
        //transform.position = pos;

        MatchBlocksBoardSpace targetBlock = MatchBlocksUtil.GetNearestBoardBlock(boardSpaces, pos);

        if (targetBlock == null || targetBlock.IsEmpty()) {
            return;
        }

        if (itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.HAMMER) {
            if (targetBlock.IsDeployed() == false &&
                targetBlock.IsDisruptor() == false) 
                return;
        }

        if (itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.CLEAR_SELECTED_PATTERNS) {
            if (targetBlock.IsDeployed() == false) 
                return;
        }

        transform.position = targetBlock.transform.position;
        EventManager.Notify(EventEnum.MatchBlocksRemoveBlockSelectedDrag, targetBlock);
    }
}
