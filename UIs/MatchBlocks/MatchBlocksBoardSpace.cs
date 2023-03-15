using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchBlocksBoardSpace : MonoBehaviour {
    public enum SPACE_TYPE {
        EMPTY = 0,
        NORMAL = 1,
        ICE = 2,
        HIDE = 3,
    }

    [System.Serializable]
    public class WaveInfo {
        public long waveID;
        public SPACE_TYPE spaceType;
    }

    public List<WaveInfo> waveInfos = new List<WaveInfo>();

    public GameObject objBlank;

    public MatchBlocksBlockUnit greenUnit;
    public MatchBlocksBlockUnit redUnit;
    public MatchBlocksBlockUnit placedUnit;

    public Transform posDisruptor;
    public MatchBlocksDisruptor ice;
    public GameObject objChecked;
    private GameObject disruptorObject;
    private MatchBlocksDisruptor disruptor;

    private Dictionary<long, SPACE_TYPE> dicBoardWaveInfo = new Dictionary<long, SPACE_TYPE>();
    private long waveID;

    private SpaceDTO spaceInfo;
    private int toBePlacedBlockTextureIndex;

    public RectTransform rectTransform;

    private void OnEnable() {
        EventManager.Register(EventEnum.MatchBlocksShowRemoveBlockPopup, EnableRaycastTarget);
        EventManager.Register(EventEnum.MatchBlocksRemoveCancel, DisableRaycastTarget);
        EventManager.Register(EventEnum.MatchBlocksRefereeDecision, DisableRaycastTarget);
    }

    private void EnableRaycastTarget(object[] args = null) {
        ToggleRaycastTarget(true);
    }

    private void DisableRaycastTarget(object[] args = null) {
        ToggleRaycastTarget(false);
    }

    private void ToggleRaycastTarget(bool enabled) {
        placedUnit.ico.raycastTarget = enabled;
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.MatchBlocksShowRemoveBlockPopup, EnableRaycastTarget);
        EventManager.Remove(EventEnum.MatchBlocksRemoveCancel, DisableRaycastTarget);
        EventManager.Remove(EventEnum.MatchBlocksRefereeDecision, DisableRaycastTarget);
    }

    public void SetData(int x, int y) {
        ToggleRaycastTarget(false);

        spaceInfo = new SpaceDTO();
        spaceInfo.coordinates = new Vector2(x, y);

        Common.ToggleActive(objBlank, IsEmpty() == false);
        greenUnit.Hide();
        redUnit.Hide();
        placedUnit.Hide();

        dicBoardWaveInfo.Clear();
        foreach (WaveInfo waveInfo in waveInfos) {
            dicBoardWaveInfo.Add(waveInfo.waveID, waveInfo.spaceType);
        }
    }

    public void SetGreenState() {
        if (IsEmpty())
            return;

        Common.ToggleActive(objBlank, false);
        greenUnit.Show();
        redUnit.Hide();
        placedUnit.Hide();
    }

    public void SetRedState() {
        if (IsEmpty())
            return;

        redUnit.Show();
        if (IsDisruptor() == false)
            placedUnit.Show();

        Common.ToggleActive(objBlank, false);
        greenUnit.Hide();
    }

    public void SetDefaultState() {
        if (IsEmpty())
            return;

        toBePlacedBlockTextureIndex = Constant.INCORRECT;

        if (placedUnit.GetBlockTextureIndex() == Constant.INCORRECT) {
            Common.ToggleActive(objBlank, true);

            greenUnit.Hide();
            redUnit.Hide();
            placedUnit.Hide();
        }
        else {
            placedUnit.Show();

            Common.ToggleActive(objBlank, true);
            greenUnit.Hide();
            redUnit.Hide();
        }
    }

    public Vector2 GetCoordinates() {
        return spaceInfo.coordinates;
    }

    public bool IsEmpty() {
        return spaceInfo.empty;
    }

    public void SetBlockInfo(int blockTextureIndex) {
        this.toBePlacedBlockTextureIndex = blockTextureIndex;
    }

    public void UpdateSpaceInfo() {
        placedUnit.SetBlockTextureIndex(spaceInfo.blockTextureIndex);
    }

    public int GetBlockTextureIndex() {
        return spaceInfo.blockTextureIndex;
    }

    public bool IsDeployed() {
        return placedUnit.GetBlockTextureIndex() != Constant.INCORRECT;
    }

    public void OnBtnRemoveClick() {
        if (IsEmpty())
            return;

        EventManager.Notify(EventEnum.MatchBlocksRemoveBlockSelected, spaceInfo.coordinates);
    }

    public bool IsDisruptor() {
        if (spaceInfo.disruptorInfo.type == (long)DISRUPTOR_TYPE.NONE ||
            IsStem())
            return false;
        return true;
    }

    public bool IsStem() {
        if (spaceInfo.disruptorInfo.type == (long)DISRUPTOR_TYPE.VINE &&
            spaceInfo.disruptorInfo.stem)
            return true;
        return false;
    }

    public void RemoveDisruptor(bool editMode = false, bool instant = false) {
        if (editMode) {
            ice.Hide();
            return;
        }

        if (disruptor != null) {
            disruptor.Remove(instant);
            if (instant) 
                //아예 초기화해버린다.
                spaceInfo.disruptorInfo = new SpaceDTO.DisruptorDTO();

            if (spaceInfo.disruptorInfo.hp == 0 || instant)
                disruptor = null;
        }
    }

    public void UpdateDisruptorState() {
        if (spaceInfo.disruptorInfo.type == (long)DISRUPTOR_TYPE.NONE)
            return;

        if (disruptor == null)
            Debug.LogError($"X:{spaceInfo.coordinates.x}, Y:{spaceInfo.coordinates.y}");
            
        disruptor.UpdateState();
    }

    public bool IsIceBlock() {
        return spaceInfo.disruptorInfo.type == (long)DISRUPTOR_TYPE.ICE;
    }

    public SPACE_TYPE GetBoardType(long waveID) {
        if (dicBoardWaveInfo.ContainsKey(waveID) == false)
            return SPACE_TYPE.HIDE;
        return dicBoardWaveInfo[waveID];
    }

    public void SetWave(long waveID, bool setSpace, bool editMode) {
        this.waveID = waveID;

        if (setSpace == false)
            return;

        SPACE_TYPE boardType = GetBoardType(waveID);

        switch(boardType) {
            case SPACE_TYPE.EMPTY:
                SetEmpty(false, editMode);
                break;

            case SPACE_TYPE.ICE:
                SetIce(false, editMode);
                break;

            case SPACE_TYPE.NORMAL:
                SetNormal(false, editMode);
                break;

            case SPACE_TYPE.HIDE:
                SetHide(false, editMode);
                break;
        }
    }

    public void SetIce(bool updateDic, bool editMode) {
        Common.ToggleActive(gameObject, true);
        Common.ToggleActive(objBlank, true);

        spaceInfo.empty = false;
        //다른 장애물이 있다면 그대로 둔다.
        if (editMode)
            spaceInfo.disruptorInfo.type = (long)DISRUPTOR_TYPE.ICE;
        else if (spaceInfo.disruptorInfo.type == (long)DISRUPTOR_TYPE.NONE &&
            (editMode == false && UserDataModel.instance.ContinueGame == false)) 
            spaceInfo.disruptorInfo.type = (long)DISRUPTOR_TYPE.ICE;
        
        if (IsIceBlock()) 
            CreateDisruptor(spaceInfo, editMode);

        if (updateDic) 
            waveInfos[(int)waveID - 1].spaceType = SPACE_TYPE.ICE;
    }

    public void SetNormal(bool updateDic, bool editMode) {
        Common.ToggleActive(gameObject, true);
        Common.ToggleActive(objBlank, true);

        spaceInfo.empty = false;

        if (editMode)
            RemoveDisruptor(editMode);
        else if (IsIceBlock() == false) 
            ice.Hide();

        if (updateDic) 
            waveInfos[(int)waveID - 1].spaceType = SPACE_TYPE.NORMAL;
    }

    public void SetEmpty(bool updateDic, bool editMode) {
        Common.ToggleActive(gameObject, true);
        Common.ToggleActive(objBlank, false);

        RemoveDisruptor(editMode, true);

        spaceInfo.blockTextureIndex = Constant.INCORRECT;
        spaceInfo.disruptorInfo.type = (long)DISRUPTOR_TYPE.NONE;
        spaceInfo.empty = true;

        ice.Hide();
        placedUnit.Hide();

        if (updateDic) 
            waveInfos[(int)waveID - 1].spaceType = SPACE_TYPE.EMPTY;
    }

    public void SetHide(bool updateDic, bool editMode) {
        Common.ToggleActive(gameObject, false);
        Common.ToggleActive(objBlank, false);

        RemoveDisruptor(editMode, true);

        spaceInfo.blockTextureIndex = Constant.INCORRECT;
        spaceInfo.disruptorInfo.type = (long)DISRUPTOR_TYPE.NONE;
        spaceInfo.empty = true;
        ice.Hide();

        if (updateDic) 
            waveInfos[(int)waveID - 1].spaceType = SPACE_TYPE.HIDE;
    }

    public void CreateDisruptor(SpaceDTO spaceInfo, bool editMode = false) {
        if (spaceInfo.disruptorInfo.type == (long)DISRUPTOR_TYPE.NONE)
            return;

        if (editMode == false) {
            if (spaceInfo.disruptorInfo.type == (long)DISRUPTOR_TYPE.ICE)
                UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ROUND_CREATE_GLASS, 1, false);
            else if (spaceInfo.disruptorInfo.type == (long)DISRUPTOR_TYPE.VINE)
                UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ROUND_CREATE_VINE, 1, false);
            else if (spaceInfo.disruptorInfo.type == (long)DISRUPTOR_TYPE.WALL)
                UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ROUND_CREATE_WALL, 1, false);
        }

        if (spaceInfo.disruptorInfo.type == (long)DISRUPTOR_TYPE.ICE) 
            disruptor = ice;
        else {
            disruptorObject = ResourceManager.instance.LoadDisruptor(spaceInfo.disruptorInfo.type, posDisruptor);
            disruptor = disruptorObject.GetComponent<MatchBlocksDisruptor>();
        }
        
        disruptor.SetData(spaceInfo);
        disruptor.Show();
    }

    public SpaceDTO GetSpaceInfo() {
        return spaceInfo;
    }

    public int GetTobePlacedBlockTextureIndex() {
        return toBePlacedBlockTextureIndex;
    }

    //로드시에만 사용.값이 아니라 연결의 개념
    public void SetSpaceInfo(SpaceDTO spaceInfo) {
        this.spaceInfo = spaceInfo;

        CreateDisruptor(spaceInfo);
        UpdateSpaceInfo();
        SetDefaultState();
    }

    public Vector3 GetSizeDelta() {
        return rectTransform.sizeDelta;
    }

    public void SetSpaceInfoInEditMode() {
        if (spaceInfo == null)
            spaceInfo = new SpaceDTO();
    }
}
