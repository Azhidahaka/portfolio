using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchBlocks : UIBase {
    public MatchBlocksBundleGenerator generator;

    public RectTransform boardRectTransform;
    public Transform parentBoard;

    public HorizontalLayoutGroup itemLayoutGroup;
    public GameObject prefabItemSlot;

    public CommonGoods goods;
    public Slider sldTimer;

    public Text lblTotalScore;
    public Text lblNextWaveRemain;
    public LayoutGroup waveLayoutGroup;

    public TweenNumber tweenTotalScore;

    public MatchBlocksPiggyBank piggyBank;
    public MatchBlocksTimer timer;

    public Text lblWaveCount;

    private List<MatchBlocksItem> itemSlots = new List<MatchBlocksItem>();

    private MatchBlocksBundle selectedBundle;

    private GameObject boardObject;
    private MatchBlocksBoard board;
    private RemoveBlockMenu removeBlockPopup;
    
    private GameData.StageDTO stageData;
    private GameData.BoardWaveDTO boardWaveData;
    private GameData.ItemDTO usingItemData;

    private List<MatchBlocksBoardSpace> changedBoardSpaces = new List<MatchBlocksBoardSpace>();

    private string nextWaveStr;
    private string pointStr;
    private string waveStr;
    private UserData.RefereeNoteDTO refereeNote;

    private void Awake() {
        InitTutorialTargets();
        Common.ToggleActive(prefabItemSlot, false);
    }

    public void SetData(GameData.StageDTO stageData, List<BundleInfoDTO> bundleInfos = null, int boardWaveIndex = 0, long totalScore = 0) {
        nextWaveStr = TermModel.instance.GetTerm("next_wave");
        pointStr = TermModel.instance.GetTerm("point");
        waveStr = TermModel.instance.GetTerm("format_wave");

        refereeNote = null;

        this.stageData = stageData;

        if (boardObject != null) {
            Destroy(boardObject);
        }
        
        SetWaveCount(1);

        boardObject = ResourceManager.instance.GetBoard(stageData.level, parentBoard);
        board = boardObject.GetComponent<MatchBlocksBoard>();
        board.SetData(stageData);

        List<GameData.BoardWaveDTO> boardWaveDatas = GameDataModel.instance.GetBoardWaveDatas(stageData.boardID);

        boardWaveData = boardWaveDatas[boardWaveIndex];

        board.SetWave(boardWaveData.waveID);

        generator.SetData(boardWaveData, bundleInfos);

        SetScore(totalScore, false);
        SetItemSlots();

        HideRemoveBlockUI();

        goods.SetData(true);

        piggyBank.SetData();

        timer.SetData(stageData);
    }

    private void SetWaveCount(long count) {
        lblWaveCount.text = string.Format(waveStr, count);
    }

    private void HideRemoveBlockUI() {
        if (removeBlockPopup == null)
            return;

        removeBlockPopup.Hide();
    }

    private void SetItemSlots() {
        List<GameData.ItemDTO> itemDatas = GameDataModel.instance.GetItemDatasByItemBundleID(stageData.itemBundleID);

        //이미 추가된 슬롯이 있다면 꺼준다.
        foreach (MatchBlocksItem itemSlot in itemSlots) {
            Common.ToggleActive(itemSlot.gameObject, false);
        }

        for (int i = 0; i < itemDatas.Count; i++) {
            MatchBlocksItem itemSlot;
            //추가하려는 슬롯오브젝트가 없는경우, 새로운 슬롯 생성
            if (i > itemSlots.Count - 1) {
                GameObject slotObject = Instantiate(prefabItemSlot, itemLayoutGroup.transform);
                itemSlot = slotObject.GetComponent<MatchBlocksItem>();
                itemSlots.Add(itemSlot);
            }
            //슬롯 오브젝트가 이미 있다면 데이터만 추가
            else 
                itemSlot = itemSlots[i];

            itemSlot.SetData(itemDatas[i].itemID);
            Common.ToggleActive(itemSlot.gameObject, true);
        }

        itemLayoutGroup.SetLayoutHorizontal();
    }

    private void SetScore(long totalScore = 0, bool reposition = true) {
        long waveScore = 0;
        long nextWaveRemainTurn = Constant.INCORRECT;
        if (refereeNote != null) {
            totalScore = refereeNote.totalScore;
            waveScore = refereeNote.waveScore;
            nextWaveRemainTurn = refereeNote.nextWaveRemainTurn;
        }

        tweenTotalScore.SetData(totalScore, false);

        if (nextWaveRemainTurn <= 0) {
            long nextWaveRemainScore = boardWaveData.goal - waveScore;
            if (nextWaveRemainScore < 0)
                nextWaveRemainScore = 0;
            string nextWaveRemainStr = Common.GetCommaFormat(nextWaveRemainScore);
            lblNextWaveRemain.text = $"{nextWaveStr} : {nextWaveRemainStr}{pointStr}";
        }
        else {
            string format = TermModel.instance.GetTerm("format_next_wave");
            lblNextWaveRemain.text = string.Format(format, nextWaveRemainTurn);
        }

        /*
        if (reposition)
            StartCoroutine(Reposition());
            */
    }

    private IEnumerator Reposition() {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)waveLayoutGroup.transform);

        Common.ToggleActive(waveLayoutGroup.gameObject, false);
        Common.ToggleActive(waveLayoutGroup.gameObject, true);
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.MatchBlocksStart, OnMatchBlocksStart);
        EventManager.Register(EventEnum.MatchBlocksDragStart, OnMatchBlocksDragStart);
        EventManager.Register(EventEnum.MatchBlocksDragEnd, OnMatchBlocksDragEnd);
        EventManager.Register(EventEnum.MatchBlocksRefereeDecision, OnMatchBlocksRefereeDecision);
        EventManager.Register(EventEnum.MatchBlocksShowRemoveBlockPopup, OnMatchBlocksShowRemoveBlockPopup);
        EventManager.Register(EventEnum.MatchBlocksShowEndNotice, OnMatchBlocksShowEndNotice);
        EventManager.Register(EventEnum.MatchBlocksGameOver, OnMatchBlocksGameOver);

        EventManager.Register(EventEnum.MatchBlocksChangeBoardWave, OnMatchBlocksChangeBoardWave);

        EventManager.Register(EventEnum.GoodsUpdate, OnGoodsUpdate);
        EventManager.Register(EventEnum.UserDataUserInfoUpdate, OnGoodsUpdate);

        EventManager.Register(EventEnum.MatchBlocksRemoveBlockSelected, OnMatchBlocksRemoveBlockSelected);

        EventManager.Register(EventEnum.MatchBlocksChangeBoardWaveCountDown, OnMatchBlocksChangeBoardWaveCountDown);

        //StartCoroutine(Reposition());
    }

    private void OnMatchBlocksStart(object[] args) {
        UserData.RefereeNoteDTO refereeNote = (UserData.RefereeNoteDTO)args[0];
        SetWaveCount(refereeNote.waveCount);
    }

    private void OnMatchBlocksChangeBoardWaveCountDown(object[] args) {
        refereeNote = (UserData.RefereeNoteDTO)args[0];
        SetScore();
    }

    private void OnMatchBlocksRemoveBlockSelected(object[] args) {
        if (removeBlockPopup == null || removeBlockPopup.IsActive == false)
            return;

        Vector2 coordinates = (Vector2)args[0];

        MatchBlocksBoardSpace boardSpace = MatchBlocksUtil.GetBoardSpaceByCoordinates(GetBoardSpaces(), 
                                                   (int)coordinates.x, 
                                                   (int)coordinates.y);

        //블록 1개 없애는 아이템인경우, 장애물이나 블록이 없는곳은 선택할수 없다.
        if (usingItemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.HAMMER &&
            boardSpace.IsDeployed() == false &&
            boardSpace.IsDisruptor() == false)
            return;

        //패턴 없애기 아이템인경우, 블록이 없는곳은 선택할수 없다.
        if (usingItemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.CLEAR_SELECTED_PATTERNS &&
            boardSpace.IsDeployed() == false)
            return;

        removeBlockPopup.SetRemoveTarget(boardSpace);
    }

    private void OnGoodsUpdate(object[] args) {
        foreach (MatchBlocksItem itemSlot in itemSlots) {
            itemSlot.UpdateCost();
        }
    }

    private void OnMatchBlocksChangeBoardWave(object[] args) {
        long waveID = (long)args[0];
        refereeNote = (UserData.RefereeNoteDTO)args[1];

        boardWaveData = GameDataModel.instance.GetBoardWaveData(stageData.boardID, waveID);
        board.SetWave(waveID);
        
        SetWaveCount(refereeNote.waveCount);

        SetScore();
    }

    private void OnMatchBlocksGameOver(object[] args) {
        //Common.ToggleActive(topUI, false);
        if (removeBlockPopup != null)
            removeBlockPopup.Hide();
    }

    private void OnMatchBlocksShowEndNotice(object[] args) {
        bool noSpace = (bool)args[0];
        //endNoticeUI.SetData(noSpace);
        //endNoticeUI.Show();
    }

    private void OnMatchBlocksShowRemoveBlockPopup(object[] args) {
        long itemID = (long)args[0];
        usingItemData = GameDataModel.instance.GetItemData(itemID);
        ShowRemoveBlockPopup(itemID);
    }

    private void ShowRemoveBlockPopup(long itemID) {
        if (removeBlockPopup == null)
            removeBlockPopup = UIManager.instance.GetUI<RemoveBlockMenu>(UI_NAME.RemoveBlockMenu);

        usingItemData = GameDataModel.instance.GetItemData(itemID);
        removeBlockPopup.SetData(GetBoardSpaces(), usingItemData);
        removeBlockPopup.Show();
    }

    private void OnMatchBlocksDragStart(object[] args) {
        selectedBundle = (MatchBlocksBundle)args[0];

        StartCoroutine("JobCheckBlocks");
    }

    private void OnMatchBlocksDragEnd(object[] args) {
        StopCoroutine("JobCheckBlocks");

        if (selectedBundle == null)
            return;

        List<MatchBlocksBlockUnit> bundleBlocks = selectedBundle.GetBlocks();
        long blankCount = 0;
        for (int i = 0; i < bundleBlocks.Count; i++) {
            if (bundleBlocks[i].IsBlank())
                blankCount++;
        }

        if (changedBoardSpaces.Count == 0 ||
            changedBoardSpaces.Count < bundleBlocks.Count - blankCount) {
            board.Reset();
            MatchBlocksSound.instance.PlayEffect(MatchBlocksSound.ACTION.WRONG);
            return;
        }

        for (int i = 0; i < changedBoardSpaces.Count; i++) {
            MatchBlocksBoardSpace changedBoardSpace = changedBoardSpaces[i];
            if (changedBoardSpace.IsDeployed() || 
                changedBoardSpace.IsEmpty() ||
                changedBoardSpace.IsDisruptor() ||
                changedBoardSpace.gameObject.activeSelf == false) {
                board.Reset();
                MatchBlocksSound.instance.PlayEffect(MatchBlocksSound.ACTION.WRONG);
                return;
            }
        }

        EventManager.Notify(EventEnum.MatchBlocksSubmitBlockInfos, changedBoardSpaces);
    }

    private void OnMatchBlocksRefereeDecision(object[] args) {
        bool success = (bool)args[0];
        refereeNote = (UserData.RefereeNoteDTO)args[1];

        SetScore();

        board.UpdateBlockInfos();
        
        if (success) {
            generator.AddNewBundle();

            List<BundleInfoDTO> bundleInfos = generator.GetBundleInfos();
            EventManager.Notify(EventEnum.MatchBlocksAddBundleComplete, bundleInfos);
        }
    }

    private IEnumerator JobCheckBlocks() {
        MatchBlocksBoardSpace[] boardSpaces = board.GetBoardSpaces();
        List<MatchBlocksBlockUnit> bundleBlocks = selectedBundle.GetBlocks();

        while(true) {
            changedBoardSpaces.Clear();

            if (IsInBoundary(bundleBlocks[0].transform) == false) {
                board.Reset();
                yield return new WaitForEndOfFrame();
                continue;
            }

            //0,0 블록과 가장 가까운 보드블록을 받아온다.
            MatchBlocksBoardSpace nearestBoardBlock = 
                MatchBlocksUtil.GetNearestBoardBlock(GetBoardSpaces(), bundleBlocks[0].transform.position);
            for (int i = 0; i < bundleBlocks.Count; i++) {
                MatchBlocksBoardSpace boardSpace = board.GetBoardSpace(nearestBoardBlock.GetCoordinates() + bundleBlocks[i].GetCoordinates());
                if (boardSpace == null || boardSpace.IsEmpty()) 
                    continue;

                if (bundleBlocks[i].IsBlank() == false) {
                    boardSpace.SetBlockInfo(bundleBlocks[i].GetBlockTextureIndex());
                    changedBoardSpaces.Add(boardSpace);
                }
            }

            for (int i = 0; i < changedBoardSpaces.Count; i++) {
                MatchBlocksBoardSpace changedBoardBlock = changedBoardSpaces[i];
                //이미 배치되었거나 방해블록이 있는곳이면 붉은색 표시
                if (changedBoardBlock.IsDeployed() || changedBoardBlock.IsDisruptor())
                    changedBoardBlock.SetRedState();
                else {
                    changedBoardBlock.SetGreenState();
                }
            }

            for (int i = 0; i < boardSpaces.Length; i++) {
                if (changedBoardSpaces.Contains(boardSpaces[i]))
                    continue;

                boardSpaces[i].SetDefaultState();
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private bool IsPlacable(MatchBlocksBoardSpace boardSpace) {
        if (boardSpace == null || 
            boardSpace.IsEmpty()) {
            return false;
        }
        return true;
    }

    private bool IsInBoundary(Transform blockTransform) {
        return RectTransformUtility.RectangleContainsScreenPoint(boardRectTransform, blockTransform.position);
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.MatchBlocksStart, OnMatchBlocksStart);
        EventManager.Remove(EventEnum.MatchBlocksDragStart, OnMatchBlocksDragStart);
        EventManager.Remove(EventEnum.MatchBlocksDragEnd, OnMatchBlocksDragEnd);
        EventManager.Remove(EventEnum.MatchBlocksRefereeDecision, OnMatchBlocksRefereeDecision);
        EventManager.Remove(EventEnum.MatchBlocksShowRemoveBlockPopup, OnMatchBlocksShowRemoveBlockPopup);
        EventManager.Remove(EventEnum.MatchBlocksShowEndNotice, OnMatchBlocksShowEndNotice);
        EventManager.Remove(EventEnum.MatchBlocksGameOver, OnMatchBlocksGameOver);
        EventManager.Remove(EventEnum.MatchBlocksChangeBoardWave, OnMatchBlocksChangeBoardWave);

        EventManager.Remove(EventEnum.GoodsUpdate, OnGoodsUpdate);
        EventManager.Remove(EventEnum.UserDataUserInfoUpdate, OnGoodsUpdate);

        EventManager.Remove(EventEnum.MatchBlocksRemoveBlockSelected, OnMatchBlocksRemoveBlockSelected);

        EventManager.Remove(EventEnum.MatchBlocksChangeBoardWaveCountDown, OnMatchBlocksChangeBoardWaveCountDown);
    }

    public MatchBlocksBoardSpace[] GetBoardSpaces() {
        return board.GetBoardSpaces();
    }

    public List<MatchBlocksBundle> GetBundles() {
        return generator.GetBundles();
    } 

    public long GetIceBlockCount() {
        return board.GetIceBlockCount();
    }

    public Transform GetFirstBundleTransform() {
        return generator.GetFirstBundleTransform();
    }

    public void OnBtnBackClick() {
        Callback callback = () => {
            MatchBlocksReferee.instance.StopAllCoroutines();
            App.instance.ChangeScene(App.SCENE_NAME.Home);
        };

        if (stageData.level == (long)STAGE_LEVEL.LeagueBronze ||
            stageData.level == (long)STAGE_LEVEL.LeagueSilver) {
            string msg = TermModel.instance.GetTerm("msg_challenge_exit_warning");
            MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.YES_NO, msg, callback);
        }
        else
            callback();
    }

    public void OnBtnSettingClick() {
        OptionMenu optionMenu = UIManager.instance.GetUI<OptionMenu>(UI_NAME.OptionMenu);
        optionMenu.SetData();
        optionMenu.Show();
    }

    public override void OnCopy(List<object> datas) {
        SetData(datas[0] as GameData.StageDTO);
    }

    public override List<object> GetCopyDatas() {
        List<object> datas = new List<object>();
        datas.Add(stageData);
        return datas;
    }

    public float GetBoardScale() {
        return board.GetBoardScale();
    }

    public MatchBlocksBoard GetBoard() {
        return board;
    }

    public List<BundleInfoDTO> GetBundleInfos() {
        return generator.GetBundleInfos();
    }

    public void OnBtnForceChangeWaveClick() {
        MatchBlocksReferee.instance.ForceChangeWave();
    }

    public Transform GetLocationBtnBombItem() {
        foreach (MatchBlocksItem itemSlot in itemSlots) {
            GameData.ItemDTO itemData = itemSlot.GetItemData();
            if (itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.REMOVE_DIAMOND_RANGED_BLOCK)
                return itemSlot.icoItem.transform;
        }

        return null;
    }
}
