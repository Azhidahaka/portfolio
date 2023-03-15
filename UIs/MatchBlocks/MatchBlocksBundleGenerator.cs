using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchBlocksBundleGenerator : MonoBehaviour {
    public HorizontalLayoutGroup grid;

    private MatchBlocksBundle selectedBundle;

    private GameData.BoardWaveDTO boardWaveData;
    private List<GameData.BundleDTO> bundleDatas;
    private List<GameData.BundleSetDTO> bundleSetDatas;

    private List<MatchBlocksBundle> bundles = new List<MatchBlocksBundle>();

    public void OnEnable() {
        EventManager.Register(EventEnum.MatchBlocksDragStart, OnMatchBlocksDragStart);
        EventManager.Register(EventEnum.MatchBlocksResetBundles, OnMatchBlocksResetBundles);
        EventManager.Register(EventEnum.MatchBlocksChangeBundlePatterns, OnMatchBlocksChangeBundlePatterns);
        EventManager.Register(EventEnum.MatchBlocksChangeBoardWave, OnMatchBlocksChangeBoardWave);
    }

    private void OnMatchBlocksChangeBoardWave(object[] args) {
        long waveID = (long)args[0];
        UserData.RefereeNoteDTO refereeNote = (UserData.RefereeNoteDTO)args[1];

        GameData.StageDTO stageData = GameDataModel.instance.GetStageData(refereeNote.stageLevel);
        boardWaveData = GameDataModel.instance.GetBoardWaveData(stageData.boardID, waveID);
        bundleDatas = GameDataModel.instance.GetBundleDatas(boardWaveData.bundleSetID);
        bundleSetDatas = GameDataModel.instance.GetBundleSetDatas(boardWaveData.bundleSetID);
    }

    private void OnMatchBlocksChangeBundlePatterns(object[] args) {
        StopCoroutine(JobChangeBundlePatterns());
        StartCoroutine(JobChangeBundlePatterns());
    }

    private IEnumerator JobChangeBundlePatterns() {
        float totalWidth = 0;
        for (int i = 0; i < bundles.Count; i++) {
            totalWidth += bundles[i].GetBundleWidth();
        }

        float interval = (Constant.RESET_PATTERN_DURATION - 0.4f) / totalWidth;

        float delay = 0;
        for (int i = 0; i < bundles.Count; i++) {
            MatchBlocksBundle bundle = bundles[i];
            bundle.ResetBlockPatterns(delay, interval);
            delay += interval * bundle.GetBundleWidth();
        }

        yield return new WaitForSeconds(Constant.RESET_PATTERN_DURATION);

        EventManager.Notify(EventEnum.MatchBlocksChangeBundlePatternsEnd);
    }

    private void OnMatchBlocksResetBundles(object[] args) {
        SetData(boardWaveData, true, true);
    }

    private void OnMatchBlocksDragStart(object[] args) {
        selectedBundle = (MatchBlocksBundle)args[0];
    }

    public void OnDisable() {
        EventManager.Remove(EventEnum.MatchBlocksDragStart, OnMatchBlocksDragStart);
        EventManager.Remove(EventEnum.MatchBlocksResetBundles, OnMatchBlocksResetBundles);
        EventManager.Remove(EventEnum.MatchBlocksChangeBundlePatterns, OnMatchBlocksChangeBundlePatterns);
        EventManager.Remove(EventEnum.MatchBlocksChangeBoardWave, OnMatchBlocksChangeBoardWave);
    }

    public void SetData(GameData. BoardWaveDTO boardWaveData, bool useItem = false, bool onlyDataChange = false) {
        this.boardWaveData = boardWaveData;
        bundleDatas = GameDataModel.instance.GetBundleDatas(boardWaveData.bundleSetID);
        bundleSetDatas = GameDataModel.instance.GetBundleSetDatas(boardWaveData.bundleSetID);

        if (onlyDataChange == false) {
            for (int i = 0; i < bundles.Count; i++) {
                if (bundles[i] != null) {
                    Common.ToggleActive(bundles[i].gameObject, false);
                    Destroy(bundles[i].gameObject);
                }
            }
            bundles.Clear();
        }

        int indexRange = Random.Range(0, Constant.MATCH_BLOCK_BUNDLE_COUNT);

        for (int i = 0; i < Constant.MATCH_BLOCK_BUNDLE_COUNT; i++) {
            MatchBlocksBundle bundle = null;
            if (onlyDataChange)
                bundle = bundles[i];

            if (i <= indexRange && useItem)
                LoadPlacableBundle(bundle, i);
            else
                LoadBundle(bundle, i);
        }

        grid.SetLayoutHorizontal();
    }

    public void SetData(GameData.BoardWaveDTO boardWaveData, List<BundleInfoDTO> bundleInfos) {
        if (bundleInfos == null) {
            SetData(boardWaveData);
            return;
        }

        this.boardWaveData = boardWaveData;
        bundleDatas = GameDataModel.instance.GetBundleDatas(boardWaveData.bundleSetID);
        bundleSetDatas = GameDataModel.instance.GetBundleSetDatas(boardWaveData.bundleSetID);

        for (int i = 0; i < bundles.Count; i++) {
            if (bundles[i] != null) {
                Common.ToggleActive(bundles[i].gameObject, false);
                Destroy(bundles[i].gameObject);
            }
        }
        bundles.Clear();

        for (int i = 0; i < bundleInfos.Count; i++) {
            LoadBundle(bundleInfos[i]);
        }

        grid.SetLayoutHorizontal();
    }

    public void AddNewBundle() {
        bundles.Remove(selectedBundle);
        Destroy(selectedBundle.gameObject);

        LoadBundle();

        grid.SetLayoutHorizontal();
    }

    private void LoadBundle(MatchBlocksBundle bundle = null, int bundleIndex = -1) {
        //1 ~ 10000
        int value = Random.Range(1, 10001);

        long accProbability = 0;
        int selectedIndex = Constant.INCORRECT;
        for (int i = 0; i < bundleSetDatas.Count; i++) {
            long min = accProbability;
            long max = accProbability + bundleSetDatas[i].probability;
            if (value > min && value <= max) {
                selectedIndex = i;
                break;
            }

            accProbability += bundleSetDatas[i].probability;
        }

        long difficulty = bundleSetDatas[selectedIndex].bundleDifficulty;
        //첫번째 튜토리얼을 완료하지 않았다면 1개짜리 블록은 만들지 않는다.
        if (UserDataModel.instance.statistics.tutorialCompleteIDs.Contains((long)TUTORIAL_ID.FIRST_MATCHBLOCKS) == false &&
            difficulty == 1)
            difficulty = 2;

        GameData.BundleDTO bundleData = GetBundleData(difficulty);

        if (bundle == null) {
            GameObject bundleObject = ResourceManager.instance.GetMatchBlocksBundle(grid.transform);
            bundle = bundleObject.GetComponentInChildren<MatchBlocksBundle>();
            bundles.Add(bundle);
            bundle.SetData(bundleData);
        }
        else {
            bundle.ResetAction(bundleData, bundleIndex);
        }
    }

    private void LoadBundle(BundleInfoDTO bundleInfo) {
        GameObject bundleObject = ResourceManager.instance.GetMatchBlocksBundle(grid.transform);
        MatchBlocksBundle bundle = bundleObject.GetComponentInChildren<MatchBlocksBundle>();
        bundles.Add(bundle);
        bundle.SetData(bundleInfo);
    }

    private GameData.BundleDTO GetBundleData(long difficulty) {
        List<GameData.BundleDTO> candidates = new List<GameData.BundleDTO>();

        foreach(GameData.BundleDTO bundleData in bundleDatas) {
            if (bundleData.difficulty != difficulty)
                continue;
            candidates.Add(bundleData);
        }

        int index = Random.Range(0, candidates.Count);
        return candidates[index];
    }

    public List<MatchBlocksBundle> GetBundles() { 
        return bundles;
    }

    private void LoadPlacableBundle(MatchBlocksBundle bundle = null, int bundleIndex = -1) {
        List<GameData.BundleDTO> placableBundleDatas = new List<GameData.BundleDTO>();

        for (int i = 0; i < bundleDatas.Count;i++) {
            if (MatchBlocksReferee.instance.IsPlacableOnBoard(bundleDatas[i]) == false)
                continue;
            placableBundleDatas.Add(bundleDatas[i]);
        }

        GameData.BundleDTO bundleData;
        if (placableBundleDatas.Count > 0) {
            int index = Random.Range(0, placableBundleDatas.Count);
            bundleData = placableBundleDatas[index];
        }
        else 
            bundleData = GameDataModel.instance.GetDefaultBundleData();
        
        if (bundle == null) {
            GameObject bundleObject = ResourceManager.instance.GetMatchBlocksBundle(grid.transform);
            bundle = bundleObject.GetComponent<MatchBlocksBundle>();
            bundles.Add(bundle);
            bundle.SetData(bundleData);
        }
        else {
            bundle.ResetAction(bundleData, bundleIndex);
        }
        
    }

    public Transform GetFirstBundleTransform() {
        return bundles[0].transform;
    }

    public List<BundleInfoDTO> GetBundleInfos() {
        List<BundleInfoDTO> bundleInfos = new List<BundleInfoDTO>();
        for (int i = 0; i < bundles.Count; i++) {
            BundleInfoDTO bundleInfo = bundles[i].GetBundleInfo();
            bundleInfos.Add(bundleInfo);
        }

        return bundleInfos;
    }
}
