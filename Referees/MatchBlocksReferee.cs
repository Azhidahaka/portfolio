using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MatchBlocksReferee : MonoBehaviour {
    public enum STATE {
        LOAD,
        DEPLOY_WAITING,
        CHECK_MATCHED,
        MATCHED_EFFECT_END,

        CHANGE_WAVE,

        USE_ITEM,

        GAME_OVER,
    }

    public static MatchBlocksReferee instance;

    private UserData.RefereeNoteDTO refereeNote;

    private long itemID;

    private MatchBlocks menu;
    private Dictionary<long, GameData.HitDTO> hitDataDic = new Dictionary<long, GameData.HitDTO>();

    private GameData.StageDTO stageData;
    private List<GameData.BoardWaveDTO> boardWaveDatas;

    //난이도 변경시마다 재할당되는 데이터
    private GameData.BoardWaveDTO boardWaveData;
    private List<int> blockTextureIndexes = new List<int>();
    private GameData.DisruptorDTO vineData;
    private GameData.DisruptorDTO wallData;

    private bool bundleDragging = false;
    private bool lastTurnMatched = false;

    private STATE state = STATE.LOAD;

    private HitEffect hitEffect;
    private CountdownWarning countdownWarning;
    private WaveCountDown waveCountDown;

    private NoSpaceWarning noSpaceWarning;

    private int maxGrid;

    public bool BundleDragging {
        get {
            return bundleDragging;
        }
    }

    public void Awake() {
        instance = this;
    }

    public long AvailableGold {
        get {
            return refereeNote.availableGold;
        }
        set {
            refereeNote.availableGold = value;
        }
    }

    public long AvailableGoldMax {
        get {
            return refereeNote.availableGoldMax;
        }
        set {
            refereeNote.availableGoldMax = value;
        }
    }

    public long ShowNoSpaceAdsCount {
        set {
            refereeNote.showNoSpaceAdsCount = value;
        }
        get {
            return refereeNote.showNoSpaceAdsCount;
        }
    }


    public void SetData(GameData.StageDTO stageData, UserData.RefereeNoteDTO continueRefereeNote = null) {
        bundleDragging = false;

        this.stageData = stageData;

        if (continueRefereeNote == null) {
            refereeNote = new UserData.RefereeNoteDTO();
            refereeNote.boardWaveIndex = Constant.INCORRECT;
            refereeNote.stageLevel = stageData.level;
            refereeNote.remainTime = stageData.time;
            refereeNote.availableGold = Constant.MATCH_BLOCKS_AVAILABLE_GOLD;
            refereeNote.availableGoldMax = Constant.MATCH_BLOCKS_AVAILABLE_GOLD;
            refereeNote.showNoSpaceAdsCount = 0;
        }
        else
            refereeNote = continueRefereeNote;

        List<GameData.HitDTO> hitDatas = GameDataModel.instance.hits;
        hitDataDic.Clear();
        for (int i = 0; i < hitDatas.Count; i++) {
            hitDataDic.Add(hitDatas[i].matchCount, hitDatas[i]);
        }

        boardWaveDatas = GameDataModel.instance.GetBoardWaveDatas(stageData.boardID);

        ChangeWaveData(continueRefereeNote != null);
        SetState(STATE.LOAD);
    }

    private void ChangeWaveData(bool continued = false) {
        if (continued == false) {
            refereeNote.boardWaveIndex = refereeNote.nextBoardWaveIndex;
            refereeNote.waveScore = 0;
            refereeNote.nextWaveRemainTurn = Constant.INCORRECT;

            refereeNote.waveCount++;
            refereeNote.waveRewardReceived = false;
        }

        boardWaveData = boardWaveDatas[refereeNote.boardWaveIndex];

        wallData = GameDataModel.instance.GetDisruptorData(boardWaveData.wallID);
        vineData = GameDataModel.instance.GetDisruptorData(boardWaveData.vineID);
        SetBlockInfos();
    }

    private void SetWaveClearAchievement() {
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.WAVE_CLEAR_COUNT, 1);
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ACC_WAVE_CLEAR_COUNT, 1);
    }

    private void SetBlockInfos() {
        blockTextureIndexes.Clear();

        long patternID = boardWaveData.patternID;
        if (refereeNote.waveCount > boardWaveDatas.Count)
            patternID = boardWaveDatas[boardWaveDatas.Count - 1].patternID;

        List<GameData.PatternDTO> patterns = GameDataModel.instance.GetPatternDatas(patternID);
        ResourceManager.instance.LoadBlockTextures(patterns);

        for (int i = 0; i < patterns.Count; i++) {
            blockTextureIndexes.Add(i);
        }
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.MatchBlocksGameOver, OnMatchBlocksGameOver);
        EventManager.Register(EventEnum.MatchBlocksSubmitBlockInfos, OnMatchBlocksSubmitBlockInfos);
        EventManager.Register(EventEnum.MatchBlocksUseItem, OnMatchBlocksUseItem);
        EventManager.Register(EventEnum.MatchBlocksRemoveBlockConfirm, OnMatchBlocksRemoveBlockConfirm);
        EventManager.Register(EventEnum.MatchBlocksAddBundleComplete, OnMatchBlocksAddBundleComplete);

        EventManager.Register(EventEnum.MatchBlocksBundleResetEnd, OnMatchBlocksBundleResetEnd);

        EventManager.Register(EventEnum.MatchBlocksDragStart, OnMatchBlocksDragStart);
        EventManager.Register(EventEnum.MatchBlocksDragEnd, OnMatchBlocksDragEnd);

        EventManager.Register(EventEnum.MatchBlocksEffectEnd, OnMatchBlocksEffectEnd);

        EventManager.Register(EventEnum.MatchBlocksRotateBundle, OnMatchBlocksRotateBundle);

        EventManager.Register(EventEnum.MatchBlocksChangeBundlePatternsEnd, OnMatchBlocksChangeBundlePatternsEnd);
    }

    private void OnMatchBlocksRotateBundle(object[] args) {
        refereeNote.bundleInfos = menu.GetBundleInfos();
        CheckBoardBlocks(true);
    }

    private void OnMatchBlocksEffectEnd(object[] args) {
        if (hitEffect != null)
            hitEffect.Hide();
        SetState(STATE.MATCHED_EFFECT_END);
    }

    private void OnMatchBlocksDragStart(object[] args) {
        bundleDragging = true;
    }

    private void OnMatchBlocksDragEnd(object[] args) {
        bundleDragging = false;
    }

    private void OnMatchBlocksBundleResetEnd(object[] args) {
        Debug.Log("MatchBlocksBundleResetEnd");
        refereeNote.bundleInfos = menu.GetBundleInfos();
        CheckBoardBlocks(true);
    }

    private void OnMatchBlocksAddBundleComplete(object[] args) {
        refereeNote.bundleInfos = (List<BundleInfoDTO>)args[0];

        ActVine();
        CheckBoardBlocks(false);
    }

    private void ActVine() {
        MatchBlocksBoardSpace[] boardSpaces = menu.GetBoardSpaces();

        List<SpaceDTO> candidateInfos = new List<SpaceDTO>();
        bool vineExist = false;

        if (lastTurnMatched == false) {
            foreach (MatchBlocksBoardSpace boardSpace in boardSpaces) {
                SpaceDTO spaceInfo = boardSpace.GetSpaceInfo();
                //덩굴이 아니면 리턴
                if (spaceInfo.disruptorInfo.type != (long)DISRUPTOR_TYPE.VINE)
                    continue;

                vineExist = true;

                if (spaceInfo.disruptorInfo.stem) {
                    spaceInfo.disruptorInfo.stem = false;
                    boardSpace.UpdateDisruptorState();
                }
                else {
                    //퍼뜨리기
                    SpaceDTO leftInfo = GetSpaceInfoByCoordinates(refereeNote.spaceInfos, (int)spaceInfo.coordinates.x - 1, (int)spaceInfo.coordinates.y, boardSpaces);
                    SpaceDTO rightInfo = GetSpaceInfoByCoordinates(refereeNote.spaceInfos, (int)spaceInfo.coordinates.x + 1, (int)spaceInfo.coordinates.y, boardSpaces);
                    SpaceDTO upInfo = GetSpaceInfoByCoordinates(refereeNote.spaceInfos, (int)spaceInfo.coordinates.x, (int)spaceInfo.coordinates.y - 1, boardSpaces);
                    SpaceDTO downInfo = GetSpaceInfoByCoordinates(refereeNote.spaceInfos, (int)spaceInfo.coordinates.x, (int)spaceInfo.coordinates.y + 1, boardSpaces);

                    candidateInfos.Add(leftInfo);
                    candidateInfos.Add(rightInfo);
                    candidateInfos.Add(upInfo);
                    candidateInfos.Add(downInfo);
                }
            }
        }
        List<SpaceDTO> finalCandidates = new List<SpaceDTO>();

        for (int i = 0; i < candidateInfos.Count; i++) {
            SpaceDTO targetSpaceInfo = candidateInfos[i];
            //존재하지 않는경우
            if (targetSpaceInfo == null)
                continue;

            //다른 방해물이 있는경우
            if (targetSpaceInfo.disruptorInfo.type != 0)
                continue;

            //블록이 배치되지 않은경우
            if (targetSpaceInfo.blockTextureIndex == Constant.INCORRECT)
                continue;

            finalCandidates.Add(targetSpaceInfo);
        }

        if (finalCandidates.Count > 0) {
            int index = Random.Range(0, finalCandidates.Count);
            SpaceDTO targetSpaceInfo = finalCandidates[index];
            targetSpaceInfo.disruptorInfo.type = (long)DISRUPTOR_TYPE.VINE;
            targetSpaceInfo.disruptorInfo.stem = true;

            //해당 보드공간에 
            MatchBlocksBoardSpace targetBoardSpace = MatchBlocksUtil.GetBoardSpaceByCoordinates(boardSpaces,
                (int)targetSpaceInfo.coordinates.x,
                (int)targetSpaceInfo.coordinates.y);
            targetBoardSpace.CreateDisruptor(targetSpaceInfo);
        }

        DetermineCreateDisruptor(vineExist);
    }

    private void CheckBoardBlocks(bool checkOnly) {
        List<MatchBlocksBundle> bundles = menu.GetBundles();

        for (int i = 0; i < bundles.Count; i++) {
            GameData.BundleDTO bundleData = bundles[i].GetBundleData();
            bool placable = IsPlacableOnBoard(bundleData);
            if (placable) {
                if (checkOnly == false)
                    DetermineChangeBoardWave();

                refereeNote.noSpace = false;

                if (noSpaceWarning != null && noSpaceWarning.IsActive)
                    noSpaceWarning.Hide();

                WebStage.instance.ReqGetWaveReward();
                WebUser.instance.ReqSavePlayData(refereeNote);

                SetState(STATE.DEPLOY_WAITING);
                return;
            }
        }

        if (refereeNote.noSpace == false) {
            refereeNote.noSpace = true;

            if (noSpaceWarning == null)
                noSpaceWarning = UIManager.instance.GetUI<NoSpaceWarning>(UI_NAME.NoSpaceWarning);
            noSpaceWarning.SetData();
            noSpaceWarning.Show();

            EventManager.Notify(EventEnum.TutorialCheck);
        }

        SetState(STATE.DEPLOY_WAITING);
    }

    private void OnMatchBlocksRemoveBlockConfirm(object[] args) {
        Vector2 coordinates = (Vector2)args[0];

        GameData.ItemDTO itemData = GameDataModel.instance.GetItemData(itemID);

        RemoveBlocksByItem(coordinates, itemData);

        refereeNote.availableGold -= itemData.price;
        WebStage.instance.ReqUseItem(itemID);

        EventManager.Notify(EventEnum.MatchBlocksRefereeDecision, false, refereeNote);

        //아이템으로 확보된 공간까지 체크
        CheckBoardBlocks(false);
    }

    private void RemoveBlocksByItem(Vector2 coordinates, GameData.ItemDTO itemData) {
        MatchBlocksBoardSpace[] boardSpaces = menu.GetBoardSpaces();

        List<MatchBlocksBoardSpace> rangedSpaces = MatchBlocksUtil.GetRangedSpaces(boardSpaces, coordinates, itemData);

        List<MatchBlocksBoardSpace> removedBoardSpaces = new List<MatchBlocksBoardSpace>();

        foreach (MatchBlocksBoardSpace rangedSpace in rangedSpaces) {
            //존재하지 않거나, EMPTY 또는 HIDE상태인경우 아무것도 하지 않음
            if (rangedSpace == null ||
                rangedSpace.IsEmpty() ||
                rangedSpace.gameObject.activeSelf == false)
                return;

            bool blockRemoved;
            if (itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.CLEAR_SELECTED_PATTERNS ||
                itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.HAMMER)
                blockRemoved = RemoveMatchBlocksInfo(rangedSpace.GetCoordinates(), true);
            else
                blockRemoved = RemoveMatchBlocksInfo(rangedSpace.GetCoordinates());

            if (blockRemoved)
                removedBoardSpaces.Add(rangedSpace);

            rangedSpace.RemoveDisruptor();
        }

        SetRemoveScore(removedBoardSpaces);

        StartCoroutine(JobCheckMatched(rangedSpaces, false));
    }

    private long SetRemoveScore(List<MatchBlocksBoardSpace> removedBoardSpaces) {
        long score = Constant.SCORE_PER_REMOVED_BLOCK * removedBoardSpaces.Count;
        refereeNote.totalScore += score;
        refereeNote.waveScore += score;
        SetScoreAchievement(score, removedBoardSpaces.Count);

        GameData.ItemDTO itemData = GameDataModel.instance.GetItemData(itemID);
        foreach (MatchBlocksBoardSpace space in removedBoardSpaces) {
            ShowRemoveEffect(space, itemData.type);
            ShowGainScoreEffect(space, Constant.SCORE_PER_REMOVED_BLOCK);
        }

        return score;
    }

    
    private void ShowRemoveEffect(MatchBlocksBoardSpace boardSpace, long itemType) {
        EFFECT_NAME effectName = EFFECT_NAME.vfxBombClear;
        if (itemType == (long)MATCH_BLOCKS_ITEM_TYPE.CLEAR_SELECTED_PATTERNS)
            effectName = EFFECT_NAME.vfxMagicClear;

        ClearEffect clearEffect = (ClearEffect)EffectManager.instance.LoadEffect(effectName);
        clearEffect.transform.position = boardSpace.transform.position;
        clearEffect.SetData(boardSpace.GetBlockTextureIndex(), boardSpace.GetSizeDelta(), GetBoardScale().x, 1);
        clearEffect.Show();
    }

    private void OnMatchBlocksUseItem(object[] args) {
        if (BundleDragging)
            return;

        long itemID = (long)args[0];

        GameData.ItemDTO itemData = GameDataModel.instance.GetItemData(itemID);
        //시간증가 아이템을 제외하고는 배치가능한 상태에서만 사용가능
        if (itemData.type != (long)MATCH_BLOCKS_ITEM_TYPE.INCREASE_TIME &&
            state != STATE.DEPLOY_WAITING)
            return;

        UseItem(itemID);
    }

    private void UseItem(long itemID) {
        GameData.ItemDTO itemData = GameDataModel.instance.GetItemData(itemID);

        if (MatchBlocksUtil.IsRemoveItem(itemData)) {
            this.itemID = itemID;
            EventManager.Notify(EventEnum.MatchBlocksShowRemoveBlockPopup, itemID);
        }
        else if (itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.RESET_BUNDLES) {
            refereeNote.availableGold -= itemData.price;
            WebStage.instance.ReqUseItem(itemID, () => {
                MatchBlocksSound.instance.PlayEffect(MatchBlocksSound.ACTION.USE_BUNDLE_RESET_ITEM);
                EventManager.Notify(EventEnum.MatchBlocksResetBundles);
                SetState(STATE.USE_ITEM);
                refereeNote.bundleInfos = menu.GetBundleInfos();
            });
        }
        else if (itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.CHANGE_BUNDLE_PATTERNS) {
            refereeNote.availableGold -= itemData.price;
            WebStage.instance.ReqUseItem(itemID, () => {
                MatchBlocksSound.instance.PlayEffect(MatchBlocksSound.ACTION.USE_PATTERN_CHANGE_ITEM);
                SetState(STATE.USE_ITEM);
                EventManager.Notify(EventEnum.MatchBlocksChangeBundlePatterns);
            });
        }
        else if (itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.INCREASE_TIME) {
            refereeNote.availableGold -= itemData.price;
            WebStage.instance.ReqUseItem(itemID, () => {
                IncreaseTime(itemID);
            });
        }
    }

    private void IncreaseTime(long itemID) {
        GameData.ItemDTO itemData = GameDataModel.instance.GetItemData(itemID);
        refereeNote.remainTime += itemData.value;
        if (refereeNote.remainTime >= stageData.time)
            refereeNote.remainTime = stageData.time;
                
        WebUser.instance.ReqSavePlayData(refereeNote);
    }

    public void StartMatchBlocks(MatchBlocks menu) {
        this.menu = menu;

        MatchBlocksBoardSpace[] boardSpaces = menu.GetBoardSpaces();

        //refereeNote.spaceInfos.Clear();
        if (refereeNote.spaceInfos == null || refereeNote.spaceInfos.Count == 0) {
            foreach (MatchBlocksBoardSpace boardSpace in boardSpaces) {
                refereeNote.spaceInfos.Add(boardSpace.GetSpaceInfo());
            }
        }
        else {
            foreach (SpaceDTO spaceInfo in refereeNote.spaceInfos) {
                MatchBlocksBoardSpace space =
                    MatchBlocksUtil.GetBoardSpaceByCoordinates(boardSpaces,
                                                               (int)spaceInfo.coordinates.x,
                                                                (int)spaceInfo.coordinates.y);
                space.SetSpaceInfo(spaceInfo);
            }
        }

        DetermineChangeBoardWave(true);
        SetState(STATE.DEPLOY_WAITING);

        UserDataModel.instance.statistics.dicRoundRecord.Clear();
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ROUND_EARNED_SCORE, refereeNote.totalScore);
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ROUND_ITEM_USECOUNT, refereeNote.itemUsedCount);

        EventManager.Notify(EventEnum.MatchBlocksStart, refereeNote);

        StartCoroutine(JobCheckTimer());
    }

    private void SetState(STATE state) {
        this.state = state;
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.MatchBlocksGameOver, OnMatchBlocksGameOver);
        EventManager.Remove(EventEnum.MatchBlocksSubmitBlockInfos, OnMatchBlocksSubmitBlockInfos);
        EventManager.Remove(EventEnum.MatchBlocksUseItem, OnMatchBlocksUseItem);

        EventManager.Remove(EventEnum.MatchBlocksRemoveBlockConfirm, OnMatchBlocksRemoveBlockConfirm);
        EventManager.Remove(EventEnum.MatchBlocksAddBundleComplete, OnMatchBlocksAddBundleComplete);

        EventManager.Remove(EventEnum.MatchBlocksBundleResetEnd, OnMatchBlocksBundleResetEnd);

        EventManager.Remove(EventEnum.MatchBlocksDragStart, OnMatchBlocksDragStart);
        EventManager.Remove(EventEnum.MatchBlocksDragEnd, OnMatchBlocksDragEnd);

        EventManager.Remove(EventEnum.MatchBlocksEffectEnd, OnMatchBlocksEffectEnd);
        EventManager.Remove(EventEnum.MatchBlocksRotateBundle, OnMatchBlocksRotateBundle);

        EventManager.Remove(EventEnum.MatchBlocksChangeBundlePatternsEnd, OnMatchBlocksChangeBundlePatternsEnd);
    }

    private void OnMatchBlocksChangeBundlePatternsEnd(object[] args) {
        refereeNote.bundleInfos = menu.GetBundleInfos();
        CheckBoardBlocks(true);
    }

    private void OnMatchBlocksGameOver(object[] args) {
        StopAllCoroutines();

        state = STATE.GAME_OVER;

        if (countdownWarning != null)
            countdownWarning.Hide();
        if (waveCountDown != null)
            waveCountDown.Hide();

        WebStage.instance.ReqEndMatchBlocks(stageData.level,
                                            refereeNote,
                                            OnResMatchBlocksEnd);
    }

    private void OnResMatchBlocksEnd() {
        EventManager.Notify(EventEnum.MatchBlocksEnd, refereeNote);
    }

    private void OnMatchBlocksSubmitBlockInfos(object[] args) {
        if (state == STATE.GAME_OVER)
            return;

        List<MatchBlocksBoardSpace> changedBoardSpaces = (List<MatchBlocksBoardSpace>)args[0];
        bool success = true;

        for (int i = 0; i < changedBoardSpaces.Count; i++) {
            if (IsPlacableBlock(changedBoardSpaces[i]) == false)
                success = false;
        }

        if (success) {
            refereeNote.turnCount++;
            UserDataModel.instance.IncreaseGoldByTurn();

            //블록 배치
            for (int i = 0; i < changedBoardSpaces.Count; i++) {
                DeployBlock(changedBoardSpaces[i]);
            }

            MatchBlocksSound.instance.PlayEffect(MatchBlocksSound.ACTION.PUT);
            EventManager.Notify(EventEnum.BundleDeployed);
            StartCoroutine(JobCheckMatched(changedBoardSpaces, true));
        }
        else {
            MatchBlocksSound.instance.PlayEffect(MatchBlocksSound.ACTION.WRONG);
            EventManager.Notify(EventEnum.MatchBlocksRefereeDecision, false, refereeNote);
            SetState(STATE.DEPLOY_WAITING);
        }
    }

    IEnumerator JobCheckMatched(List<MatchBlocksBoardSpace> checkSpaces, bool byDeploy) {
        do {
            SetState(STATE.CHECK_MATCHED);
            checkSpaces = CheckMatched(checkSpaces);
            EventManager.Notify(EventEnum.MatchBlocksRefereeDecision, byDeploy, refereeNote);
            byDeploy = false;
            yield return new WaitUntil(() => state == STATE.MATCHED_EFFECT_END);
        } while (checkSpaces.Count > 0);

        SetState(STATE.DEPLOY_WAITING);
    }

    private void DeployBlock(MatchBlocksBoardSpace boardSpace) {
        for (int i = 0; i < refereeNote.spaceInfos.Count; i++) {
            if (refereeNote.spaceInfos[i].coordinates == boardSpace.GetCoordinates()) {
                refereeNote.spaceInfos[i] = boardSpace.GetSpaceInfo();
                refereeNote.spaceInfos[i].blockTextureIndex = boardSpace.GetTobePlacedBlockTextureIndex();
                return;
            }
        }

        Vector2 coordinates = boardSpace.GetCoordinates();
        Debug.LogError($"DeploayBlock::좌표를 찾을수 없음. X:{(int)coordinates.x}, Y:{(int)coordinates.y}");
    }

    private List<MatchBlocksBoardSpace> CheckMatched(List<MatchBlocksBoardSpace> changedBoardBlocks) {
        List<SpaceDTO> matchedInfos = new List<SpaceDTO>();

        for (int i = 0; i < changedBoardBlocks.Count; i++) {
            SpaceDTO newBlock = new SpaceDTO();
            newBlock.coordinates = changedBoardBlocks[i].GetCoordinates();
            newBlock.blockTextureIndex = changedBoardBlocks[i].GetBlockTextureIndex();

            if (IsContain(matchedInfos, newBlock))
                continue;

            List<SpaceDTO> checkedInfos = new List<SpaceDTO>();
            checkedInfos.Add(newBlock);

            List<SpaceDTO> aroundBlocks = CheckAroundBlocks(newBlock, ref checkedInfos);

            while (aroundBlocks.Count > 0) {
                List<SpaceDTO> nextAroundBlocks = new List<SpaceDTO>();
                foreach (SpaceDTO aroundBlock in aroundBlocks) {
                    List<SpaceDTO> temp = CheckAroundBlocks(aroundBlock, ref checkedInfos);
                    nextAroundBlocks.AddRange(temp);
                }
                aroundBlocks = nextAroundBlocks;
            }

            if (checkedInfos.Count >= Constant.MATCH_BLOCK_MATCH_MIN_COUNT) {
                matchedInfos.AddRange(checkedInfos);
                refereeNote.accumulateHitCount += matchedInfos.Count - Constant.MATCH_BLOCK_MATCH_MIN_COUNT + 1;
                lastTurnMatched = true;
            }
        }

        List<MatchBlocksBoardSpace> disruptorRemovedSpaces = new List<MatchBlocksBoardSpace>();

        if (matchedInfos.Count >= Constant.MATCH_BLOCK_MATCH_MIN_COUNT) {
            MatchBlocksBoardSpace[] boardSpaces = menu.GetBoardSpaces();

            long score = SetScore(matchedInfos);

            for (int i = 0; i < matchedInfos.Count; i++) {
                MatchBlocksBoardSpace boardSpace = MatchBlocksUtil.GetBoardSpaceByCoordinates(boardSpaces,
                    (int)matchedInfos[i].coordinates.x,
                    (int)matchedInfos[i].coordinates.y);
                if (boardSpace.IsStem())
                    boardSpace.RemoveDisruptor();

                ShowClearEffect(boardSpace);
                ShowGainScoreEffect(boardSpace, score / matchedInfos.Count);
                RemoveMatchBlocksInfo(matchedInfos[i].coordinates);
            }
            SoundManager.instance.PlayEffect(SOUND_CLIP_EFFECT.BlockClear);

            disruptorRemovedSpaces = DetermineRemoveDisruptor(boardSpaces, matchedInfos);

            //refereeNote.remainTime += matchedInfos.Count * stageData.time
        }
        else
            SetState(STATE.MATCHED_EFFECT_END);

        return disruptorRemovedSpaces;
    }

    private void ShowClearEffect(MatchBlocksBoardSpace boardSpace) {
        ClearEffect clearEffect = (ClearEffect)EffectManager.instance.LoadEffect(EFFECT_NAME.vfxBlockClear);
        clearEffect.transform.position = boardSpace.transform.position;
        clearEffect.SetData(boardSpace.GetBlockTextureIndex(), boardSpace.GetSizeDelta(), GetBoardScale().x);
        clearEffect.Show();
    }

    private void ShowGainScoreEffect(MatchBlocksBoardSpace boardSpace, long score) {
        GainScoreEffect effect = (GainScoreEffect)EffectManager.instance.LoadEffect(EFFECT_NAME.GainScoreEffect);
        effect.transform.position = boardSpace.transform.position;
        effect.SetData(boardSpace.GetSizeDelta(), GetBoardScale().x, score);
        effect.Show();
    }

    private long SetScore(List<SpaceDTO> matchedInfos) {
        if (matchedInfos.Count < Constant.MATCH_BLOCK_MATCH_MIN_COUNT)
            return 0;

        long key = matchedInfos.Count;
        if (key > Constant.MATCH_BLOCK_HIT_MAX)
            key = Constant.MATCH_BLOCK_HIT_MAX;

        GameData.HitDTO hitData = hitDataDic[key];

        long score = hitData.scorePerBlock * matchedInfos.Count;
        refereeNote.totalScore += score;
        refereeNote.waveScore += score;
        SetScoreAchievement(score, matchedInfos.Count);

        refereeNote.remainTime += hitData.grade;

        if (hitEffect == null)
            hitEffect = UIManager.instance.GetUI<HitEffect>(UI_NAME.HitEffect);
        hitEffect.SetData(matchedInfos.Count, score);
        hitEffect.Show();

        EventManager.Notify(EventEnum.TutorialCheck);

        return score;
    }

    private void SetScoreAchievement(long score, long matchedCount) {
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ROUND_EARNED_SCORE, score);
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ACC_GET_SCORE, score);

        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.BLOCK_CLEAR_COUNT, matchedCount);
        UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ACC_BLOCK_CLEAR_COUNT, matchedCount);

        //최고 Hit 갱신
        long bestHit = UserDataModel.instance.GetStatistics(STATISTICS_TYPE.ETC_BEST_HIT);
        if (matchedCount > bestHit)
            UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.ETC_BEST_HIT, matchedCount);
    }

    private List<MatchBlocksBoardSpace> DetermineRemoveDisruptor(MatchBlocksBoardSpace[] boardSpaces, List<SpaceDTO> matchedInfos) {
        List<MatchBlocksBoardSpace> candidates = new List<MatchBlocksBoardSpace>();
        for (int i = 0; i < matchedInfos.Count; i++) {
            MatchBlocksBoardSpace left = MatchBlocksUtil.GetBoardSpaceByCoordinates(
                boardSpaces,
                (int)matchedInfos[i].coordinates.x - 1,
                (int)matchedInfos[i].coordinates.y);

            if (left != null && candidates.Contains(left) == false)
                candidates.Add(left);

            MatchBlocksBoardSpace right = MatchBlocksUtil.GetBoardSpaceByCoordinates(
                boardSpaces,
                (int)matchedInfos[i].coordinates.x + 1,
                (int)matchedInfos[i].coordinates.y);
            if (right != null && candidates.Contains(right) == false)
                candidates.Add(right);

            MatchBlocksBoardSpace up = MatchBlocksUtil.GetBoardSpaceByCoordinates(
                boardSpaces,
                (int)matchedInfos[i].coordinates.x,
                (int)matchedInfos[i].coordinates.y - 1);
            if (up != null && candidates.Contains(up) == false)
                candidates.Add(up);

            MatchBlocksBoardSpace down = MatchBlocksUtil.GetBoardSpaceByCoordinates(
                boardSpaces,
                (int)matchedInfos[i].coordinates.x,
                (int)matchedInfos[i].coordinates.y + 1);
            if (down != null && candidates.Contains(down) == false)
                candidates.Add(down);
        }

        List<MatchBlocksBoardSpace> disruptorRemovedSpaces =
            new List<MatchBlocksBoardSpace>();

        for (int i = 0; i < candidates.Count; i++) {
            if (candidates[i].IsDisruptor() || candidates[i].IsStem()) {
                //마지막으로 파괴된 장애물인경우에만 업적 체크를 한다.
                SetDisruptorRemoveCount(candidates[i], i == candidates.Count - 1);
                RemoveMatchBlocksInfo(candidates[i].GetCoordinates(), false, true);
                candidates[i].RemoveDisruptor();
                disruptorRemovedSpaces.Add(candidates[i]);
            }
        }

        return disruptorRemovedSpaces;
    }

    private void SetDisruptorRemoveCount(MatchBlocksBoardSpace boardSpace, bool checkAchievement) {
        SpaceDTO spaceInfo = boardSpace.GetSpaceInfo();
        if (spaceInfo.disruptorInfo == null)
            return;

        if (spaceInfo.disruptorInfo.type == (long)DISRUPTOR_TYPE.ICE) 
            UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.REMOVE_ICE_COUNT, 1, checkAchievement);
        else if (spaceInfo.disruptorInfo.type == (long)DISRUPTOR_TYPE.VINE) 
            UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.REMOVE_VINE_COUNT, 1, checkAchievement);
        else if (spaceInfo.disruptorInfo.type == (long)DISRUPTOR_TYPE.WALL) 
            UserDataModel.instance.SetAchievementCount(STATISTICS_TYPE.REMOVE_WALL_COUNT, 1, checkAchievement);
    }

    private bool RemoveMatchBlocksInfo(Vector2 coordinates, bool clearAll = false, bool stemOnly = false) {
        bool blockRemoved = false;

        for (int i = 0; i < refereeNote.spaceInfos.Count; i++) {
            SpaceDTO spaceInfo = refereeNote.spaceInfos[i];
            if (spaceInfo.coordinates != coordinates)
                continue;

            switch ((DISRUPTOR_TYPE)spaceInfo.disruptorInfo.type) {
                case DISRUPTOR_TYPE.NONE: {
                    if (spaceInfo.blockTextureIndex != Constant.INCORRECT)
                        blockRemoved = true;
                    spaceInfo.blockTextureIndex = Constant.INCORRECT;
                    break;
                }

                //덩굴, 얼음
                case DISRUPTOR_TYPE.VINE: {
                    spaceInfo.disruptorInfo.type = (long)DISRUPTOR_TYPE.NONE;
                    //줄기상태인경우에는 블록까지 제거
                    if (clearAll || spaceInfo.disruptorInfo.stem && stemOnly == false) {
                        if (spaceInfo.blockTextureIndex != Constant.INCORRECT)
                            blockRemoved = true;
                        spaceInfo.blockTextureIndex = Constant.INCORRECT;
                    }
                    break;
                }
                case DISRUPTOR_TYPE.ICE: {
                    spaceInfo.disruptorInfo.type = (long)DISRUPTOR_TYPE.NONE;
                    if (clearAll) {
                        if (spaceInfo.blockTextureIndex != Constant.INCORRECT)
                            blockRemoved = true;
                        spaceInfo.blockTextureIndex = Constant.INCORRECT;
                    }
                    break;
                }

                //벽
                case DISRUPTOR_TYPE.WALL: {
                    if (clearAll) {
                        spaceInfo.disruptorInfo.hp = 0;
                        spaceInfo.disruptorInfo.type = (long)DISRUPTOR_TYPE.NONE;
                    }
                    else {
                        spaceInfo.disruptorInfo.hp--;
                        //체력이 0이되면 벽 제거
                        if (spaceInfo.disruptorInfo.hp <= 0)
                            spaceInfo.disruptorInfo.type = (long)DISRUPTOR_TYPE.NONE;
                    }
                    break;
                }
            }

            return blockRemoved;
        }

        return blockRemoved;
    }

    private List<SpaceDTO> CheckAroundBlocks(SpaceDTO pivot, ref List<SpaceDTO> checkedInfos) {
        List<SpaceDTO> aroundInfos = new List<SpaceDTO>();

        MatchBlocksBoardSpace[] boardSpaces = menu.GetBoardSpaces();

        SpaceDTO leftInfo = GetSpaceInfoByCoordinates(refereeNote.spaceInfos, (int)pivot.coordinates.x - 1, (int)pivot.coordinates.y, boardSpaces);
        SpaceDTO rightInfo = GetSpaceInfoByCoordinates(refereeNote.spaceInfos, (int)pivot.coordinates.x + 1, (int)pivot.coordinates.y, boardSpaces);
        SpaceDTO upInfo = GetSpaceInfoByCoordinates(refereeNote.spaceInfos, (int)pivot.coordinates.x, (int)pivot.coordinates.y - 1, boardSpaces);
        SpaceDTO downInfo = GetSpaceInfoByCoordinates(refereeNote.spaceInfos, (int)pivot.coordinates.x, (int)pivot.coordinates.y + 1, boardSpaces);

        if (leftInfo != null &&
            IsSameBlock(pivot.blockTextureIndex, leftInfo.blockTextureIndex) &&
            IsContain(checkedInfos, leftInfo) == false) {
            checkedInfos.Add(leftInfo);
            aroundInfos.Add(leftInfo);
        }
        if (rightInfo != null &&
            IsSameBlock(pivot.blockTextureIndex, rightInfo.blockTextureIndex) &&
            IsContain(checkedInfos, rightInfo) == false) {
            checkedInfos.Add(rightInfo);
            aroundInfos.Add(rightInfo);
        }
        if (upInfo != null &&
            IsSameBlock(pivot.blockTextureIndex, upInfo.blockTextureIndex) &&
            IsContain(checkedInfos, upInfo) == false) {
            checkedInfos.Add(upInfo);
            aroundInfos.Add(upInfo);
        }
        if (downInfo != null &&
            IsSameBlock(pivot.blockTextureIndex, downInfo.blockTextureIndex) &&
            IsContain(checkedInfos, downInfo) == false) {
            checkedInfos.Add(downInfo);
            aroundInfos.Add(downInfo);
        }

        return aroundInfos;
    }

    private bool IsContain(List<SpaceDTO> list, SpaceDTO item) {
        foreach (SpaceDTO check in list) {
            if (check.coordinates == item.coordinates &&
                check.blockTextureIndex == item.blockTextureIndex)
                return true;
        }
        return false;
    }

    private SpaceDTO GetSpaceInfoByCoordinates(List<SpaceDTO> candidates, int x, int y, MatchBlocksBoardSpace[] boardSpaces) {
        for (int i = 0; i < candidates.Count; i++) {
            Vector2 coordinates = candidates[i].coordinates;
            MatchBlocksBoardSpace boardSpace = MatchBlocksUtil.GetBoardSpaceByCoordinates(boardSpaces, (int)coordinates.x, (int)coordinates.y);

            if (boardSpace.IsEmpty() ||
                boardSpace.IsDisruptor() ||
                boardSpace.gameObject.activeSelf == false)
                continue;

            if (coordinates.x == x && coordinates.y == y)
                return candidates[i];
        }

        return null;
    }

    private bool IsSameBlock(int left, int right) {
        if (left == Constant.INCORRECT || right == Constant.INCORRECT)
            return false;

        if (left == right)
            return true;
        return false;
    }

    private bool IsPlacableBlock(MatchBlocksBoardSpace block) {
        Vector2 coordinates = block.GetCoordinates();
        for (int i = 0; i < refereeNote.spaceInfos.Count; i++) {
            if (refereeNote.spaceInfos[i].coordinates == coordinates) {
                //이미 같은자리에 다른 블록이 배치되어있으면 패스    
                if (refereeNote.spaceInfos[i].blockTextureIndex != Constant.INCORRECT)
                    return false;
                return true;
            }
        }

        return true;
    }

    public int GetRandomBlockTextureIndex() {
        int index = Random.Range(0, blockTextureIndexes.Count);
        return blockTextureIndexes[index];
    }

    public int GetTutorialBlockInfo(int index) {
        return blockTextureIndexes[index];
    }

    public bool IsPlacableOnBoard(GameData.BundleDTO bundleData) {
        int width = bundleData.width;
        int height = bundleData.height;
        List<int> bundleBlockTextureIndexes = new List<int>();
        SetBlockInfos(bundleData, ref bundleBlockTextureIndexes);

        MatchBlocksBoardSpace[] boardSpaces = menu.GetBoardSpaces();

        for (int i = 0; i < 4; i++) {
            bool placable = IsPlacable(boardSpaces, width, bundleBlockTextureIndexes);
            if (placable)
                return true;

            RotateClockwise(ref width, ref height, ref bundleBlockTextureIndexes);
        }

        return false;
    }

    private void RotateClockwise(ref int width, ref int height, ref List<int> blockTextureIndexes) {
        int prevWidth = width;
        int prevHeight = height;

        int temp = width;
        width = height;
        height = temp;

        int[] after = new int[height * width];
        for (int i = 0; i < blockTextureIndexes.Count; i++) {
            int beforeX = i % prevWidth;
            int beforeY = i / prevWidth;
            int afterX = prevHeight - 1 - beforeY;
            int afterY = beforeX;
            after[afterY * width + afterX] = blockTextureIndexes[i];
        }

        blockTextureIndexes.Clear();
        blockTextureIndexes.AddRange(after);
    }

    private void SetBlockInfos(GameData.BundleDTO bundleData, ref List<int> blockTextureIndexes) {
        for (int i = 0; i < bundleData.marking.Length; i++) {
            int blockTextureIndex = Constant.INCORRECT;
            if (bundleData.marking[i] != '0') 
                blockTextureIndex = 0;
            blockTextureIndexes.Add(blockTextureIndex);
        }
    }

    private bool IsPlacable(MatchBlocksBoardSpace[] boardSpaces, int width, List<int> bundleBlockTextureIndexes) {
        foreach (MatchBlocksBoardSpace boardSpace in boardSpaces) {
            Vector2 boardBlockCoordinates = boardSpace.GetCoordinates();

            bool placable = true;
            //번들블록들을 차례로 검사
            for (int i = 0; i < bundleBlockTextureIndexes.Count; i++) {
                if (bundleBlockTextureIndexes[i] == Constant.INCORRECT)
                    continue;

                int bundleBlockCoordinatesX = i % width;
                int bundleBlockCoordinatesY = i / width;
                int x = (int)(boardBlockCoordinates.x + bundleBlockCoordinatesX);
                int y = (int)(boardBlockCoordinates.y + bundleBlockCoordinatesY);
                MatchBlocksBoardSpace targetBoardSpace = MatchBlocksUtil.GetBoardSpaceByCoordinates(boardSpaces, x, y);

                //존재하지 않거나 이미 배치된 블록이 있는 영역이 있으면 더이상 검사하지 않음
                if (targetBoardSpace == null ||
                    targetBoardSpace.IsEmpty() ||
                    targetBoardSpace.IsDeployed() ||
                    targetBoardSpace.IsDisruptor() ||
                    targetBoardSpace.gameObject.activeSelf == false) {
                    placable = false;
                    break;
                }
            }

            if (placable)
                return true;   
        }

        return false;
    }

    private void DetermineCreateDisruptor(bool vineExist) {
        if (lastTurnMatched) {
            lastTurnMatched = false;
            return;
        }

        //웨이브 전환중이면 장애물 생성되지 않음
        if (refereeNote.nextWaveRemainTurn > 0)
            return;

        SpaceDTO.DisruptorDTO disruptorInfo = MatchBlocksUtil.GetDisruptorInfo(vineData, wallData);
        //방해물이 생성되지 않았으면 리턴
        if (disruptorInfo == null)
            return;

        //덩굴이 이미 존재하면 새로 생성되지는 않는다.
        if (disruptorInfo.type == (long)DISRUPTOR_TYPE.VINE && vineExist)
            return;

        //벽 생성 최대갯수를 초과했다면 아무것도 하지 않는다.
        long disruptorCount = GetDisruptorCount(disruptorInfo.type);
        if (wallData != null && disruptorCount >= wallData.generateMax)
            return;

        List<MatchBlocksBoardSpace> candidates = new List<MatchBlocksBoardSpace>();

        MatchBlocksBoardSpace[] boardSpaces = menu.GetBoardSpaces();
        for (int i = 0; i < boardSpaces.Length; i++) {
            MatchBlocksBoardSpace boardSpace = boardSpaces[i];

            if (disruptorInfo.type == (long)DISRUPTOR_TYPE.VINE) {
                if (boardSpace.IsDeployed() == false ||
                    boardSpace.IsDisruptor() ||
                    boardSpace.IsStem())
                    continue;
            }
            else if (boardSpace.IsDeployed() ||
                    boardSpace.IsEmpty() ||
                    boardSpace.IsDisruptor() ||
                    boardSpace.gameObject.activeSelf == false) {
                continue;
            }

            candidates.Add(boardSpace);
        }

        //놓을곳이 한군데도 없으면 리턴
        if (candidates.Count == 0)
            return;

        int targetSpaceIndex = Random.Range(0, candidates.Count);
        MatchBlocksBoardSpace targetSpace = candidates[targetSpaceIndex];
        for (int i = 0; i < refereeNote.spaceInfos.Count; i++) {
            if (refereeNote.spaceInfos[i].coordinates == targetSpace.GetCoordinates()) {
                refereeNote.spaceInfos[i].disruptorInfo = disruptorInfo;
                targetSpace.CreateDisruptor(refereeNote.spaceInfos[i]);
                break;
            }
        }

        EventManager.Notify(EventEnum.TutorialCheck);
    }

    private long GetDisruptorCount(long disruptorType) {
        long count = 0;
        foreach (SpaceDTO spaceInfo in refereeNote.spaceInfos) {
            if (spaceInfo.disruptorInfo.type == disruptorType)
                count++;
        }

        return count;
    }

    private void DetermineChangeBoardWave(bool continued = false) {
        //아직 웨이브 전환 조건에 도달하지 못한상태
        if (refereeNote.waveScore < boardWaveData.goal) {
            refereeNote.nextWaveRemainTurn = Constant.INCORRECT;
            return;
        }

        if (continued ||
            refereeNote.nextWaveRemainTurn == Constant.INCORRECT) {

            //다음웨이브에 추가되거나 사라지는 블록 위에 이펙트 표시
            if (continued == false) {
                refereeNote.nextWaveRemainTurn = Constant.NEXT_WAVE_WAIT;
                SetNextWaveIndex();
                SetWaveClearAchievement();
                EventManager.Notify(EventEnum.TutorialCheck);
            }

            SetChangeWatingBoard();
            ShowChangeWaveWarning();
        }
        else
            refereeNote.nextWaveRemainTurn--;

        //전환 카운트다운중이면 리턴
        if (refereeNote.nextWaveRemainTurn > 0) {
            if (waveCountDown == null)
                waveCountDown = UIManager.instance.GetUI<WaveCountDown>(UI_NAME.WaveCountDown);
            waveCountDown.Show();

            EventManager.Notify(EventEnum.MatchBlocksChangeBoardWaveCountDown, refereeNote);
            return;
        }

        if (waveCountDown != null)
            waveCountDown.Hide();

        ChangeWaveData();
        EventManager.Notify(EventEnum.MatchBlocksChangeBoardWave, boardWaveData.waveID, refereeNote);

        EventManager.Notify(EventEnum.TutorialCheck);
    }

    private void SetNextWaveIndex() {
        refereeNote.nextBoardWaveIndex = refereeNote.boardWaveIndex + 1;

        if (refereeNote.nextBoardWaveIndex > boardWaveDatas.Count - 1) {
            List<long> repeatIndexes = new List<long>();
            for (int i = 0; i < boardWaveDatas.Count; i++) {
                if (boardWaveDatas[i].repeat > 0)
                    repeatIndexes.Add(i);
            }

            refereeNote.nextBoardWaveIndex = Random.Range(0, repeatIndexes.Count);
        }
    }

    private void ShowChangeWaveWarning() {
        MatchBlocksBoardSpace[] boardSpaces = menu.GetBoardSpaces();

        GameData.BoardWaveDTO nextBoardWaveData = boardWaveDatas[refereeNote.nextBoardWaveIndex];

        foreach (MatchBlocksBoardSpace boardSpace in boardSpaces) {
            long currentWaveID = boardWaveData.waveID;
            MatchBlocksBoardSpace.SPACE_TYPE currentSpaceType = boardSpace.GetBoardType(currentWaveID);
            long nextWaveID = nextBoardWaveData.waveID;
            MatchBlocksBoardSpace.SPACE_TYPE nextSpaceType = boardSpace.GetBoardType(nextWaveID);

            if (currentSpaceType == nextSpaceType)
                continue;

            if (( currentSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.NORMAL ||
                currentSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.ICE ) &&
                ( nextSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.EMPTY ||
                nextSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.HIDE )) {
                WarningEffect effect = (WarningEffect)EffectManager.instance.LoadEffect(EFFECT_NAME.vfxBlockWarningHide, boardSpace.transform);
                //effect.SetData(boardSpace.GetSizeDelta(), GetBoardScale().x);
                effect.SetData(boardSpace.GetSizeDelta(), 1.0f);
                effect.Show();
                Debug.Log($"vfxBlockWarningHide::{boardSpace.gameObject.name}");
            }
            
            else if (nextSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.ICE) {
                EFFECT_NAME effectName = EFFECT_NAME.vfxBlockWarningGlass;
                WarningEffect effect = (WarningEffect)EffectManager.instance.LoadEffect(effectName, boardSpace.transform);

                //effect.SetData(boardSpace.GetSizeDelta(), GetBoardScale().x);
                effect.SetData(boardSpace.GetSizeDelta(), 1.0f);
                effect.Show();
            }
            else if (
                ( currentSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.EMPTY ||
                currentSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.HIDE ) &&
                nextSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.NORMAL) {

                EFFECT_NAME effectName = EFFECT_NAME.vfxBlockWarningShow;
                WarningEffect effect = (WarningEffect)EffectManager.instance.LoadEffect(effectName, boardSpace.transform);

                //effect.SetData(boardSpace.GetSizeDelta(), GetBoardScale().x);
                effect.SetData(boardSpace.GetSizeDelta(), 1.0f);
                effect.Show();
            }
        }
    }

    private void SetChangeWatingBoard() {
        MatchBlocksBoardSpace[] boardSpaces = menu.GetBoardSpaces();

        GameData.BoardWaveDTO nextBoardWaveData = boardWaveDatas[refereeNote.nextBoardWaveIndex];

        float minX = float.PositiveInfinity;
        float maxX = 0;
        float minY = float.PositiveInfinity;
        float maxY = 0;

        long currentWaveID = boardWaveData.waveID;
        long nextWaveID = nextBoardWaveData.waveID;

        foreach (MatchBlocksBoardSpace boardSpace in boardSpaces) {
            MatchBlocksBoardSpace.SPACE_TYPE currentSpaceType = boardSpace.GetBoardType(currentWaveID);
            MatchBlocksBoardSpace.SPACE_TYPE nextSpaceType = boardSpace.GetBoardType(nextWaveID);

            //이번웨이브에 Normal, Ice, Empty이거나
            //다음웨이브에 Normal, Ice, Empty
            if (currentSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.NORMAL ||
                currentSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.ICE ||
                currentSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.EMPTY ||
                nextSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.NORMAL ||
                nextSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.ICE ||
                nextSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.EMPTY) {
                Vector2 coordinates = boardSpace.GetCoordinates();
                if (coordinates.x < minX)
                    minX = coordinates.x;
                if (coordinates.x > maxX)
                    maxX = coordinates.x;
                if (coordinates.y < minY)
                    minY = coordinates.y;
                if (coordinates.y > maxY)
                    maxY = coordinates.y;
            }
        }

        for (int x = (int)minX; x <= (int)maxX; x++) {
            for (int y = (int)minY; y <= (int)maxY; y++) {
                MatchBlocksBoardSpace boardSpace = MatchBlocksUtil.GetBoardSpaceByCoordinates(boardSpaces, x, y);
                MatchBlocksBoardSpace.SPACE_TYPE currentSpaceType = boardSpace.GetBoardType(currentWaveID);
                if (currentSpaceType == MatchBlocksBoardSpace.SPACE_TYPE.HIDE)
                    boardSpace.SetEmpty(false, false);
            }
        }

        MatchBlocksBoard board = menu.GetBoard();
        maxGrid = Math.Max((int)( maxX - minX ), (int)( maxY - minY )) + 1;
        float scale = GameDataModel.instance.GetBoardScale(maxGrid);
        board.SetConstaintCount(Math.Abs((int)minX - (int)maxX) + 1, scale);
    }

    public Vector3 GetBoardScale() {
        //웨이브 전환중이면 
        if (refereeNote.nextWaveRemainTurn > 0) {
            float scale = GameDataModel.instance.GetBoardScale(maxGrid);
            return Vector3.one * scale;
        }
        else
            return Vector3.one * boardWaveData.scale;
    }

    public GameData.BoardWaveDTO GetBoardWaveData() {
        return boardWaveData;
    }

    public bool IsPlaceableState() {
        if (state == STATE.DEPLOY_WAITING)
            return true;
        Debug.Log("아이템 사용중::번들 배치 불가능");
        return false;
    }

    public UserData.RefereeNoteDTO GetRefereeNote() {
        return refereeNote;
    }

    public void SetWaveRewardReceived() {
        refereeNote.waveRewardReceived = true;
        UserDataModel.instance.SaveRefereeNote(refereeNote);
    }

    public void ForceChangeWave() {
        if (refereeNote.nextWaveRemainTurn != Constant.INCORRECT)
            return;

        refereeNote.waveScore = boardWaveData.goal;

        refereeNote.nextWaveRemainTurn = Constant.NEXT_WAVE_WAIT;
        SetNextWaveIndex();

        SetChangeWatingBoard();
        ShowChangeWaveWarning();

        if (waveCountDown == null)
            waveCountDown = UIManager.instance.GetUI<WaveCountDown>(UI_NAME.WaveCountDown);
        waveCountDown.Show();

        EventManager.Notify(EventEnum.MatchBlocksChangeBoardWaveCountDown, refereeNote);
    }

    public STATE GetState() {
        return state;
    }

    private IEnumerator JobCheckTimer() {
        while (refereeNote.remainTime > 0) {
            float prevTime = Time.time;
            yield return new WaitForEndOfFrame();
            float elapsedTime = Time.time - prevTime;
            refereeNote.remainTime -= elapsedTime;
            if (refereeNote.remainTime < 0)
                refereeNote.remainTime = 0;
            else if (refereeNote.remainTime <= Constant.COUNTDOWN_SECONDS && 
                (countdownWarning == null || countdownWarning.IsActive == false)) {
                if (countdownWarning == null)
                    countdownWarning = UIManager.instance.GetUI<CountdownWarning>(UI_NAME.CountdownWarning);
                countdownWarning.SetData(refereeNote);
                countdownWarning.Show();

                WebUser.instance.ReqSavePlayData(refereeNote);
            }
            else if (refereeNote.remainTime > Constant.COUNTDOWN_SECONDS &&
                countdownWarning != null && countdownWarning.IsActive)
                countdownWarning.Hide();
        }

        //@Todo 게임오버 
        EventManager.Notify(EventEnum.MatchBlocksGameOver);
    }
}
