using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoveBlockMenu : UIBase {
    public RemoveBlockSelected selected;

    public Transform rangedObjectsParent;

    public GameObject prefabRangedObject;

    private List<RemoveBlockRanged> rangedObjects = new List<RemoveBlockRanged>();

    private MatchBlocksBoardSpace selectedSpace;
    private GameData.ItemDTO itemData;
    private MatchBlocksBoardSpace[] boardSpaces;

    private void Awake() {
        Common.ToggleActive(prefabRangedObject, false);
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.MatchBlocksRemoveBlockSelectedDrag, OnMatchBlocksRemoveBlockSelectedDrag);
        SetInFrontInCanvas();
    }

    private void OnMatchBlocksRemoveBlockSelectedDrag(object[] args) {
        MatchBlocksBoardSpace space = (MatchBlocksBoardSpace)args[0];
        SetRemoveTarget(space);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.MatchBlocksRemoveBlockSelectedDrag, OnMatchBlocksRemoveBlockSelectedDrag);
    }

    public void SetData(MatchBlocksBoardSpace[] boardSpaces, GameData.ItemDTO itemData) {
        this.itemData = itemData;
        this.boardSpaces = boardSpaces;

        selectedSpace = null;
        Common.ToggleActive(selected.gameObject, false);

        foreach (RemoveBlockRanged ranged in rangedObjects) {
            Common.ToggleActive(ranged.gameObject, false);
        }

        selected.SetData(boardSpaces, itemData);
    }

    public void SetRemoveTarget(MatchBlocksBoardSpace selectedSpace) {
        this.selectedSpace = selectedSpace;

        SetSpacePosition(selected.gameObject, selectedSpace);

        foreach (RemoveBlockRanged ranged in rangedObjects) {
            Common.ToggleActive(ranged.gameObject, false);
        }

        if (itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.HAMMER)
            return;

        List<MatchBlocksBoardSpace> rangedSpaces = MatchBlocksUtil.GetRangedSpaces(boardSpaces, selectedSpace.GetCoordinates(), itemData);

        int index = 0;
        for (int i = 0; i < rangedSpaces.Count; i++) {
            MatchBlocksBoardSpace rangedSpace = rangedSpaces[i];
            if (rangedSpace.GetCoordinates() == selectedSpace.GetCoordinates())
                continue;
            
            
            RemoveBlockRanged ranged;
            //새로 생성
            if (index > rangedObjects.Count - 1) {
                GameObject rangedObject;
                rangedObject = Instantiate(prefabRangedObject, rangedObjectsParent);
                ranged = rangedObject.GetComponent<RemoveBlockRanged>();
                rangedObjects.Add(ranged);
            }
            else 
                ranged = rangedObjects[index];

            ranged.SetData(itemData.itemID);
            //아이콘 셋팅

            SetSpacePosition(ranged.gameObject, rangedSpace);
            index++;
        }
    }

    private void SetSpacePosition(GameObject origin, MatchBlocksBoardSpace target) {
        origin.transform.position = target.transform.position;
        origin.transform.localScale = Vector3.one * MatchBlocksReferee.instance.GetBoardScale().x;
        Common.ToggleActive(origin.gameObject, true);
    }

    public void OnBtnCancelClick() {
        EventManager.Notify(EventEnum.MatchBlocksRemoveCancel);
        Hide();
    }

    public void OnBtnOkClick() {
        if (selectedSpace == null) {
            string msg = TermModel.instance.GetTerm("msg_location_select");
            MessageUtil.ShowSimpleWarning(msg);
            return;
        }

        EventManager.Notify(EventEnum.MatchBlocksRemoveBlockConfirm,
                            selectedSpace.GetCoordinates());
        Hide();
    }
}
