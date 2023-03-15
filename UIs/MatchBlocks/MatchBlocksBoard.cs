using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchBlocksBoard : MonoBehaviour {
    public long targetWaveID;

    private MatchBlocksBoardSpace[] boardSpaces;

    private GridLayoutGroup gridLayoutGroup;

    private long iceBlockCount;

    private GameData.BoardWaveDTO boardWaveData;
    private GameData.StageDTO stageData;

    public void SetData(GameData.StageDTO stageData) {
        this.stageData = stageData;
        iceBlockCount = 0;

        if (gridLayoutGroup == null)
            gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();
        
        boardSpaces = GetComponentsInChildren<MatchBlocksBoardSpace>(true);
        for (int i = 0; i < boardSpaces.Length; i++) {
            int x = i % gridLayoutGroup.constraintCount;
            int y = i / gridLayoutGroup.constraintCount;

            boardSpaces[i].SetData(x, y);
            if (boardSpaces[i].IsIceBlock())
                iceBlockCount++;
        }

        gridLayoutGroup.enabled = true;
    }

    public long GetIceBlockCount() {
        return iceBlockCount;
    }

    public MatchBlocksBoardSpace[] GetBoardSpaces() {
        return boardSpaces;
    }

    public void Reset() {
        for (int i = 0; i < boardSpaces.Length; i++) {
            boardSpaces[i].SetDefaultState();
        }
    }

    public MatchBlocksBoardSpace GetBoardSpace(Vector2 targetCoorinates) {
        for (int i = 0; i < boardSpaces.Length; i++) {
            Vector2 coordinates = boardSpaces[i].GetCoordinates();
            if (coordinates == targetCoorinates)
                return boardSpaces[i];
        }

        return null;
    }

    public void UpdateBlockInfos() {
        for (int i = 0; i < boardSpaces.Length; i++) {
            boardSpaces[i].UpdateSpaceInfo();
        }

        Reset();
    }

    public void SetWave(long waveID) {
        targetWaveID = waveID;

        boardWaveData = GameDataModel.instance.GetBoardWaveData(stageData.boardID, targetWaveID);

        gridLayoutGroup.constraintCount = boardWaveData.grid;
        gridLayoutGroup.transform.localScale = Vector3.one * boardWaveData.scale;
        
        SetSpaces(false);
    }

    public void SetWaveInEditMode() {
        if (gridLayoutGroup == null)
            gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();

        boardSpaces = GetComponentsInChildren<MatchBlocksBoardSpace>(true);
        for (int i = 0; i < boardSpaces.Length; i++) {
            int x = i % gridLayoutGroup.constraintCount;
            int y = i / gridLayoutGroup.constraintCount;

            boardSpaces[i].SetData(x, y);

            string nameFormat = "X{0:D2}, Y{1:D2}";
            string name = string.Format(nameFormat, x, y);

            boardSpaces[i].gameObject.name = name;

            if (boardSpaces[i].IsIceBlock())
                iceBlockCount++;
        }

        SetTargetWaveID(targetWaveID, true);
        SetSpaces(true);
    }

    public void SetTargetWaveID(long waveID, bool editMode) {
        targetWaveID = waveID;
        if (editMode)
            boardSpaces = GetComponentsInChildren<MatchBlocksBoardSpace>(true);
        foreach (MatchBlocksBoardSpace space in boardSpaces) {
            space.SetWave(targetWaveID, false, editMode);
        }
    }

    public void SetSpaces(bool editMode) {
        if (editMode)
            boardSpaces = GetComponentsInChildren<MatchBlocksBoardSpace>(true);
        foreach (MatchBlocksBoardSpace space in boardSpaces) {
            space.SetWave(targetWaveID, true, editMode);
        }
    }

    public float GetBoardScale() {
        return boardWaveData.scale;
    }

    public void SetConstaintCount(int countraintCount, float boardScale) {
        gridLayoutGroup.constraintCount = countraintCount;
        gridLayoutGroup.transform.localScale = Vector3.one * boardScale;
    }
}
