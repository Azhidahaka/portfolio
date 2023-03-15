using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using GameData;
using LuckyFlow.EnumDefine;
using System.Collections;

public class GameDataModel : MonoBehaviour {
    public static GameDataModel instance;

    public List<ItemDTO> items;
    public List<ItemBundleDTO> itemBundles;
    
    public List<StageDTO> stages;

    public List<BundleDTO> bundles;
    public List<PatternDTO> patterns;
    public List<BundleSetDTO> bundleSets;
    public List<HitDTO> hits;
    public List<DisruptorDTO> disruptors;
    public List<BoardWaveDTO> boardWaves;
    public List<ProductDTO> products;
    public List<PackageDTO> packages;
    public List<SkinDTO> skins;
    public List<UIOrderDTO> uiOrders;
    public List<PiggyBankDTO> piggyBanks;
    public List<BoardScaleDTO> boardScales;
    public List<WaveRewardDTO> waveRewards;
    public List<AchievementDTO> achievements;

    public List<TutorialDTO> tutorials;
    public List<TutorialStepDTO> tutorialSteps;

    public List<PiggyBankSkinDTO> piggyBankSkins;

    public List<LeagueRewardDTO> leagueRewards;
    public List<FrameDTO> frames;
    public List<ProfileDTO> profiles;
    public List<RankingNPCDTO> rankingNPCs;
    public List<DummyRankingDTO> dummyRankings;

    void Awake() {
        instance = this;

        LoadGameData();
    }

    private void OnDestroy() {
        instance = null;
    }

    private void LoadGameData() {
        TextAsset[] textAssets = Resources.LoadAll<TextAsset>("Datas/");
        for (int i = 0; i < textAssets.Length; i++) {
            TextAsset asset = textAssets[i];
            
            if (asset.name == "Item")
                items = JsonConvert.DeserializeObject<List<ItemDTO>>(asset.text);

            if (asset.name == "ItemBundle")
                itemBundles = JsonConvert.DeserializeObject<List<ItemBundleDTO>>(asset.text);

            if (asset.name == "Stage")
                stages = JsonConvert.DeserializeObject<List<StageDTO>>(asset.text);

            if (asset.name == "Bundle")
                bundles = JsonConvert.DeserializeObject<List<BundleDTO>>(asset.text);

            if (asset.name == "BundleSet")
                bundleSets = JsonConvert.DeserializeObject<List<BundleSetDTO>>(asset.text);

            if (asset.name == "Pattern")
                patterns = JsonConvert.DeserializeObject<List<PatternDTO>>(asset.text);

            if (asset.name == "Hit")
                hits = JsonConvert.DeserializeObject<List<HitDTO>>(asset.text);

            if (asset.name == "Disruptor")
                disruptors = JsonConvert.DeserializeObject<List<DisruptorDTO>>(asset.text);

            if (asset.name == "BoardWave")
                boardWaves = JsonConvert.DeserializeObject<List<BoardWaveDTO>>(asset.text);

            if (asset.name == "Product")
                products = JsonConvert.DeserializeObject<List<ProductDTO>>(asset.text);

            if (asset.name == "Package")
                packages = JsonConvert.DeserializeObject<List<PackageDTO>>(asset.text);

            if (asset.name == "Skin")
                skins = JsonConvert.DeserializeObject<List<SkinDTO>>(asset.text);

            if (asset.name == "UIOrder") 
                uiOrders = JsonConvert.DeserializeObject<List<UIOrderDTO>>(asset.text);

            if (asset.name == "PiggyBank")
                piggyBanks = JsonConvert.DeserializeObject<List<PiggyBankDTO>>(asset.text);

            if (asset.name == "BoardScale")
                boardScales = JsonConvert.DeserializeObject<List<BoardScaleDTO>>(asset.text);

            if (asset.name == "WaveReward")
                waveRewards = JsonConvert.DeserializeObject<List<WaveRewardDTO>>(asset.text);

            if (asset.name == "Achievement")
                achievements = JsonConvert.DeserializeObject<List<AchievementDTO>>(asset.text);

            if (asset.name == "Tutorial")
                tutorials = JsonConvert.DeserializeObject<List<TutorialDTO>>(asset.text);

            if (asset.name == "TutorialStep")
                tutorialSteps = JsonConvert.DeserializeObject<List<TutorialStepDTO>>(asset.text);

            if (asset.name == "PiggyBankSkin")
                piggyBankSkins = JsonConvert.DeserializeObject<List<PiggyBankSkinDTO>>(asset.text);

            if (asset.name == "LeagueReward")
                leagueRewards = JsonConvert.DeserializeObject<List<LeagueRewardDTO>>(asset.text);

            if (asset.name == "Frame")
                frames = JsonConvert.DeserializeObject<List<FrameDTO>>(asset.text);

            if (asset.name == "Profile")
                profiles = JsonConvert.DeserializeObject<List<ProfileDTO>>(asset.text);

            if (asset.name == "RankingNPC")
                rankingNPCs = JsonConvert.DeserializeObject<List<RankingNPCDTO>>(asset.text);

            if (asset.name == "DummyRanking")
                dummyRankings = JsonConvert.DeserializeObject<List<DummyRankingDTO>>(asset.text);
        }
    }

    public ItemDTO GetItemData(long itemID) {
        if (IsNullOrEmpty(items)) {
            Debug.LogError("GetItemData::items 데이터가 없습니다.");
            return null;
        }

        for (int i = 0; i < items.Count; i++) {
            ItemDTO itemData = items[i];
            if (itemData.itemID == itemID)
                return itemData;
        }

        return null;
    }

    private bool IsNullOrEmpty(IList gameDatas) {
        if (gameDatas == null || gameDatas.Count == 0) 
            return true;
        return false;
    }

    public List<ItemDTO> GetItemDatas(long category) {
        List<ItemDTO> result = new List<ItemDTO>();

        if (IsNullOrEmpty(items)) {
            Debug.LogError("GetItemDatas::items 데이터가 없습니다.");
            return result;
        }

        for (int i = 0; i < items.Count; i++) {
            ItemDTO itemData = items[i];
            if (itemData.category != category)
                continue;

            result.Add(itemData);
        }

        return result;
    }

    public DisruptorDTO GetDisruptorData(long disruptorID) {
        if (IsNullOrEmpty(disruptors)) {
            Debug.LogError("GetDisruptorData::disruptors 데이터가 없습니다.");
            return null;
        }

        for (int i = 0; i < disruptors.Count; i++) {
            if (disruptors[i].disruptorID == disruptorID)
                return disruptors[i];
        }

        return null;
    }

    public StageDTO GetStageData(long stageLevel) {
        if (IsNullOrEmpty(stages)) {
            Debug.LogError("GetStageData::stages 데이터가 없습니다.");
            return null;
        }

        for (int i = 0; i < stages.Count; i++) {
            StageDTO stageData = stages[i];
            if (stageData.level != stageLevel)
                continue;

            return stageData;
        }

        return null;
    }

    public List<BundleDTO> GetBundleDatas(long bundleSetID) {
        List<BundleDTO> result = new List<BundleDTO>();
        if (IsNullOrEmpty(bundles) || IsNullOrEmpty(bundleSets)) {
            Debug.LogError("GetBundleDatas::bundles 또는 bundleSets 데이터가 없습니다.");
            return result;
        }

        List<BundleSetDTO> bundleSetDatas = GetBundleSetDatas(bundleSetID);

        foreach (BundleSetDTO bundleSet in bundleSetDatas) {
            foreach (BundleDTO bundle in bundles) {
                if (bundleSet.bundleDifficulty != bundle.difficulty)
                    continue;

                result.Add(bundle);
            }
        }

        return result;
    }

    public List<BundleSetDTO> GetBundleSetDatas(long bundleSetID) {
        List<BundleSetDTO> result = new List<BundleSetDTO>();
        if (IsNullOrEmpty(bundleSets)) {
            Debug.LogError("GetBundleSetDatas::bundleSets 데이터가 없습니다.");
            return result;
        }

        for (int i = 0; i < bundleSets.Count; i++) {
            BundleSetDTO bundleSet = bundleSets[i];
            if (bundleSet.bundleSetID == bundleSetID)
                result.Add(bundleSet);
        }

        return result;
    }

    public List<ItemDTO> GetItemDatasByItemBundleID(long itemBundleID) {
        List<ItemDTO> result = new List<ItemDTO>();
        if (IsNullOrEmpty(itemBundles)) {
            Debug.LogError("GetItemDatasByItemBundleID::itemBundles 데이터가 없습니다.");
            return result;
        }

        for (int i = 0; i < itemBundles.Count; i++) {
            ItemBundleDTO itemBundle = itemBundles[i];
            if (itemBundles[i].itemBundleID == itemBundleID) {
                ItemDTO itemData = GetItemData(itemBundle.itemID);
                result.Add(itemData);
            }
        }

        return result;
    }

    public List<PatternDTO> GetPatternDatas(long patternID) {
        List<PatternDTO> result = new List<PatternDTO>();
        if (IsNullOrEmpty(patterns)) {
            Debug.LogError("GetPatternDatas::patterns 데이터가 없습니다.");
            return result;
        }

        for (int i = 0; i < patterns.Count; i++) {
            if (patterns[i].patternID == patternID) {
                result.Add(patterns[i]);
            }
        }

        return result;
    }

    public BundleDTO GetBundleData(long bundleID) {
        if (IsNullOrEmpty(bundles)) {
            Debug.LogError("GetBundleData::bundles 데이터가 없습니다.");
            return null;
        }

        foreach (BundleDTO bundle in bundles) {
            if (bundle.bundleID != bundleID)
                continue;

            return bundle;
        }

        return null;
    }

    public BundleDTO GetDefaultBundleData() {
        if (IsNullOrEmpty(bundles)) {
            Debug.LogError("GetBundleData::bundles 데이터가 없습니다.");
            return null;
        }

        foreach (BundleDTO bundle in bundles) {
            if (bundle.difficulty != 1)
                continue;

            return bundle;
        }

        return null;
    }

    public long GetFirstStageLevel() {
        if (IsNullOrEmpty(stages)) {
            Debug.LogError("GetFirstStageLevel::stages 데이터가 없습니다.");
            return 0;
        }
        return stages[0].level;
    }

    public long GetLastStageLevel() {
        if (IsNullOrEmpty(stages)) {
            Debug.LogError("GetLastStageLevel::stages 데이터가 없습니다.");
            return 0;
        }

        return stages[stages.Count - 1].level;
    }

    public List<BoardWaveDTO> GetBoardWaveDatas(long boardID) {
        List<BoardWaveDTO> result = new List<BoardWaveDTO>();

        if (IsNullOrEmpty(boardWaves)) {
            Debug.LogError("GetBoardWaveDatas::boardWaves 데이터가 없습니다.");
            return result;
        }

        for (int i = 0; i < boardWaves.Count; i++) {
            BoardWaveDTO boardWave = boardWaves[i];
            if (boardWave.boardID == boardID)
                result.Add(boardWave);                
        }

        return result;
    }

    public BoardWaveDTO GetBoardWaveData(long boardID, long waveID) {
        if (IsNullOrEmpty(boardWaves)) {
            Debug.LogError("GetBoardWaveData::boardWaves 데이터가 없습니다.");
            return null;
        }

        for (int i = 0; i < boardWaves.Count; i++) {
            BoardWaveDTO boardWaveData = boardWaves[i];
            if (boardWaveData.boardID != boardID ||
                boardWaveData.waveID != waveID)
                continue;

            return boardWaveData;            
        }

        return null;
    }

    public ProductDTO GetProductData(string strID) {
        if (IsNullOrEmpty(products))
            return null;

        for (int i = 0; i < products.Count; i++) {
#if UNITY_ANDROID
            if (products[i].strID != strID)
                continue;
#elif UNITY_IOS
            if (products[i].appleStrID != strID)
                continue;
#endif
            return products[i];
        }

        return null;
    }

    public ProductDTO GetProductDataByPackageID(long packageID) {
        if (IsNullOrEmpty(products))
            return null;

        for (int i = 0; i < products.Count; i++) {
            if (products[i].packageID != packageID)
                continue;

            return products[i];
        }

        return null;
    }

    public List<PackageDTO> GetPackageDatas(long packageID) {
        List<PackageDTO> result = new List<PackageDTO>();
        if (IsNullOrEmpty(packages))
            return result;

        foreach (PackageDTO packageData in packages) {
            if (packageData.packageID == packageID)
                result.Add(packageData);
        }

        return result;
    }

    public PackageDTO GetPackageData(long packageID, PACKAGE_TYPE type) {
        if (IsNullOrEmpty(packages))
            return null;
        
        foreach (PackageDTO packageData in packages) {
            if (packageData.packageID == packageID &&
                packageData.type == (long)type)
                return packageData;
        }

        return null;
    }

    public List<ProductDTO> GetProductDatas(PRODUCT_CATEGORY category, bool incNonConsumable = true) {
        List<ProductDTO> result = new List<ProductDTO>();

        if (IsNullOrEmpty(products))
            return result;

        foreach (ProductDTO productData in products) {
            if (incNonConsumable == false && productData.consumable == 0)
                continue;

            if (productData.category != (long)category)
                continue;

            result.Add(productData);
        }

        return result;
    }

    public SkinDTO GetSkinData(long skinID) {
        if (IsNullOrEmpty(skins))
            return null;

        foreach (var skin in skins) {
            if (skin.skinID != skinID)
                continue;

            return skin;
        }

        return null;
    }

    public ProductDTO GetProductDataBySkinID(long skinID) {
        if (IsNullOrEmpty(packages) || IsNullOrEmpty(products))
            return null;
        
        foreach (PackageDTO packageData in packages) {
            if (packageData.type == (long)PACKAGE_TYPE.SKIN &&
                packageData.value == skinID) {
                return GetProductDataByPackageID(packageData.packageID);
            }
        }

        return null;
    }

    public PackageDTO GetRemoveAdsPackageData() {
        if (IsNullOrEmpty(packages))
            return null;
        
        foreach (PackageDTO packageData in packages) {
            if (packageData.type == (long)PACKAGE_TYPE.REMOVE_ADS)
                return packageData;
        }

        return null;
    }

    public UIOrderDTO GetUIOrderData(UI_NAME uiName) {
        if (IsNullOrEmpty(uiOrders))
            return null;

        foreach (UIOrderDTO uiOrderData in uiOrders) {
            if (uiOrderData.uiName == uiName.ToString())
                return uiOrderData;
        }

        return null;
    }

    public PiggyBankDTO GetPiggyBankData(long piggyBanktype) {
        if (IsNullOrEmpty(piggyBanks))
            return null;

        foreach (PiggyBankDTO piggyBank in piggyBanks) {
            if (piggyBank.type != piggyBanktype)
                continue;

            return piggyBank;
        }

        return null;
    }

    public float GetBoardScale(long grid) {
        if (IsNullOrEmpty(boardScales)) {
            Debug.LogError("GetBoardScale::boardScales is null");
            return 1;
        }

        foreach (BoardScaleDTO boardScaleData in boardScales) {
            if (boardScaleData.grid != grid)
                continue;

            return boardScaleData.scale;
        }

        return 1;
    }

    public WaveRewardDTO GetWaveRewardData(long stageLevel) {
        if (IsNullOrEmpty(waveRewards)) 
            return null;

        foreach (WaveRewardDTO waveRewardData in waveRewards) {
            if (waveRewardData.stageLevel != stageLevel)
                continue;

            return waveRewardData;
        }

        return null;
    }

    public List<AchievementDTO> GetAchievementDatasByGroup(long group) {
        List<AchievementDTO> result = new List<AchievementDTO>();

        if (IsNullOrEmpty(achievements))
            return result;

        foreach (AchievementDTO achievementData in achievements) {
            if (achievementData.group != group)
                continue;

            result.Add(achievementData);
        }

        return result;
    }

    public List<AchievementDTO> GetAchievementDatasByID(long achievementID) {
        List<AchievementDTO> result = new List<AchievementDTO>();

        if (IsNullOrEmpty(achievements))
            return result;

        foreach (AchievementDTO achievementData in achievements) {
            if (achievementData.achievementID != achievementID)
                continue;

            result.Add(achievementData);
        }

        return result;
    }

    public TutorialDTO GetTutorialData(long tutorialID) {
        if (tutorials == null || tutorials.Count == 0)
            return null;

        for (int i = 0; i < tutorials.Count; i++) {
            if (tutorials[i].tutorialID != tutorialID)
                continue;
            return tutorials[i];
        }

        return null;
    }

    public List<TutorialStepDTO> GetTutorialStepDatas(long tutorialID) {
        List<TutorialStepDTO> result = new List<TutorialStepDTO>();

        if (tutorialSteps == null || tutorialSteps.Count == 0)
            return result;

        for (int i = 0; i < tutorialSteps.Count; i++) {
            if (tutorialSteps[i].tutorialID != tutorialID)
                continue;

            result.Add(tutorialSteps[i]);
        }

        return result;
    }

    public ProductDTO GetProductDataByBankSkinID(long bankSkinID) {
        if (IsNullOrEmpty(packages) || IsNullOrEmpty(products))
            return null;
        
        foreach (PackageDTO packageData in packages) {
            if (packageData.type == (long)PACKAGE_TYPE.PIGGYBANK_SKIN &&
                packageData.value == bankSkinID) {
                return GetProductDataByPackageID(packageData.packageID);
            }
        }

        return null;
    }

    public List<PackageDTO> GetLeagueRewardPackageDatas(long leagueID, long rank, float percent) {
        if (IsNullOrEmpty(leagueRewards) || IsNullOrEmpty(packages))
            return new List<PackageDTO>();

        for (int i = 0; i < leagueRewards.Count; i++) {
            LeagueRewardDTO reward = leagueRewards[i];
            if (reward.leagueID != leagueID)
                continue;

            //순위가 정확히 일치하는 보상데이터
            if (reward.rank > 0) {
                if (reward.rank == rank)
                    return GetPackageDatas(reward.packageID);
                else
                    continue;
            } 
            else {
                if (percent <= reward.percent)
                    return GetPackageDatas(reward.packageID);
                else 
                    continue;
            }
        }

        return new List<PackageDTO>();
    }

    public ProfileDTO GetProfileData(long profileID) {
        if (IsNullOrEmpty(profiles))
            return null;
                 
        for (int i = 0; i < profiles.Count; i++) {
            if (profiles[i].profileID == profileID)
                return profiles[i];
        }

        return null;
    }

    public FrameDTO GetFrameData(long frameID) {
        if (IsNullOrEmpty(frames))
            return null;
                 
        for (int i = 0; i < frames.Count; i++) {
            if (frames[i].frameID == frameID)
                return frames[i];
        }

        return null;
    }

    public List<DummyRankingDTO> GetDummyRankInfos(long seasonNo, long leagueID) {
        seasonNo = seasonNo % 10;

        if (leagueID == (long)LEAGUE_LEVEL.TEST_BRONZE)
            leagueID = (long)LEAGUE_LEVEL.BRONZE;
        else if (leagueID == (long)LEAGUE_LEVEL.TEST_SILVER)
            leagueID = (long)LEAGUE_LEVEL.TEST_SILVER;

        List<DummyRankingDTO> result = new List<DummyRankingDTO>();

        if (IsNullOrEmpty(dummyRankings))
            return result;

        for (int i = 0; i < dummyRankings.Count; i++) {
            DummyRankingDTO dummyRankingData = dummyRankings[i];
            if (dummyRankingData.seasonNo == seasonNo && dummyRankingData.league == leagueID)
                result.Add(dummyRankingData);
        }

        return result;
    }

    public RankingNPCDTO GetRankingNPCData(long npcID) {
        if (IsNullOrEmpty(rankingNPCs))
            return null;
                 
        for (int i = 0; i < rankingNPCs.Count; i++) {
            if (rankingNPCs[i].npcID == npcID)
                return rankingNPCs[i];
        }

        return null;
    }
}