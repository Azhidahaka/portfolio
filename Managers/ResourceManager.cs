using System.Collections.Generic;
using UnityEngine;
using GameData;
using LuckyFlow.EnumDefine;
using QuantumTek.EncryptedSave;

public class ResourceManager : MonoBehaviour {

    private const string PREFABS_PATH = "Prefabs/";
    private const string IMAGES_PATH = "Images/";

    private const string GAME_TITLE_PATH = IMAGES_PATH + "GameTitle/";
    private const string EVENT_BANNER_PATH = IMAGES_PATH + "EventBanner/";
    private const string ICO_ITEM_PATH = IMAGES_PATH + "Items/";
    private const string ICO_PRODUCT_PATH = IMAGES_PATH + "Products/";
    private const string ICO_PIGGY_BANK_PATH = IMAGES_PATH + "PiggyBanks/";
    private const string IMG_PROFILE_FRAME_PATH = IMAGES_PATH + "Frame/";
    private const string ICO_EMBLEM_PATH = IMAGES_PATH + "Emblem/";
    private const string ICO_CHALLENGE_PATH = IMAGES_PATH + "Challenge/";
    private const string EFFECT_PATH = PREFABS_PATH + "Effect/";
    private const string ETC_PATH = PREFABS_PATH + "Etc/";

    private const string TUTORIAL_DESC_PATH = PREFABS_PATH + "UI/TutorialDesc/";
    private const string PRODUCT_PATH = PREFABS_PATH + "UI/Product/";

    public static ResourceManager instance;

    private List<Texture> blockTextures = new List<Texture>();
    private Dictionary<long, Texture> dicProfileTexture;
    private Dictionary<long, Texture> dicFrameTexture;

    private void Awake() {
        instance = this;
    }

    private void OnDestroy() {
        instance = null;
    }

    //매치블록 텍스쳐 불러오기
    public void LoadBlockTextures(List<PatternDTO> patternDatas, bool changeWave = false) {
        //웨이브전환 후, 패턴이 기존보다 적어지는 경우는 없어야한다.
        if (changeWave && patternDatas.Count < blockTextures.Count)
            return;

        blockTextures.Clear();        

        long skinID = UserDataModel.instance.SkinID;

        foreach (PatternDTO patternData in patternDatas) {
            string path = IMAGES_PATH + $"Patterns/{skinID:D4}/{patternData.no}";
            Texture texture = Resources.Load<Texture>(path);
            if (texture == null)
                Debug.LogError("텍스쳐 없음:" + path);
            blockTextures.Add(texture);
        }

        Resources.UnloadUnusedAssets();
    }

    public Texture GetBlockTexture(int index) {
        if (index > blockTextures.Count - 1) {
            Debug.LogError($"텍스쳐 없음. index:{index}, blockCount:{blockTextures.Count}");
            return null;
        }
        return blockTextures[index];
    }

    public GameObject GetMatchBlocksBundle(Transform parent) {
        string path = ETC_PATH + "Bundles/Bundle";
        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
            Debug.Log("prefab is null : path = " + path);

        GameObject bundle = Instantiate(prefab, parent);
        bundle.transform.localPosition = Vector3.zero;
        return bundle;
    }

    public GameObject LoadDisruptor(long disruptorType, Transform parent) {
        string prefabName;
        if (disruptorType == (long)DISRUPTOR_TYPE.WALL) 
            prefabName = "blockWall";
        else 
            prefabName = "blockVine";

        string path = $"{ETC_PATH}Disruptors/{prefabName}";

        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
            Debug.Log("prefab is null : path = " + path);

        GameObject bgObject = Instantiate(prefab, parent);
        return bgObject;
    }

    public Texture GetIcoItemTexture(long itemID) {
        string path = $"{ICO_ITEM_PATH}icoItem{itemID.ToString():D5}";
        Texture texture = Resources.Load<Texture>(path);
        if (texture != null)
            return texture;

        string textureDataErrorformat = "아이템 이미지 없음. ID = {0}";
        Debug.LogWarning(string.Format(textureDataErrorformat, itemID));
        return null;
    }

    public GameObject GetBoard(long stageLevel, Transform parent) {
        long skinID = UserDataModel.instance.SkinID;

        StageDTO stageData = GameDataModel.instance.GetStageData(stageLevel);

        string path = ETC_PATH + $"Boards/{skinID:D4}/{(BOARD_NAME)stageData.boardID}";
        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
            Debug.Log("prefab is null : path = " + path);

        GameObject board = Instantiate(prefab, parent);
        board.transform.localPosition = Vector3.zero;
        return board;
    }

    public GameObject GetSkinThumbnail(long skinID, Transform parent) {
        string path = $"{ETC_PATH}SkinThumb/SkinThumb{skinID:D4}";
        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
            Debug.Log("prefab is null : path = " + path);

        GameObject thumbnail = Instantiate(prefab, parent);
        thumbnail.transform.localPosition = Vector3.zero;
        return thumbnail;
    }

    public Texture GetIcoProductTexture(long packageID) {
        string path = $"{ICO_PRODUCT_PATH}icoProduct{packageID.ToString():D4}";
        Texture texture = Resources.Load<Texture>(path);
        if (texture != null)
            return texture;

        string textureDataErrorformat = "Product 이미지 없음. Package ID = {0}";
        Debug.LogWarning(string.Format(textureDataErrorformat, packageID));
        return null;
    }

    public GameObject LoadEffect(EFFECT_NAME effectName, Transform parent) {
        string path = $"{EFFECT_PATH}{effectName.ToString()}";
        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
            Debug.Log("prefab is null : path = " + path);

        GameObject obj = Instantiate(prefab, parent);
        obj.transform.localPosition = Vector3.zero;
        return obj;
    }

    public void LoadProfileTextures() {
        dicProfileTexture = new Dictionary<long, Texture>();
        string facePath = $"{IMAGES_PATH}Face";
        Texture[] faceTextureArray = Resources.LoadAll<Texture>(facePath);
        for (int i = 0; i < faceTextureArray.Length; i++) {
            Texture texture = faceTextureArray[i];
            long no;
            long.TryParse(texture.name.Replace("icoFace", ""), out no);
            if (dicProfileTexture.ContainsKey(no) == false)
                dicProfileTexture.Add(no, texture);
        }
        /*
        string flagPath = $"{IMAGES_PATH}Flags";
        Texture[] flagTextureArray = Resources.LoadAll<Texture>(flagPath);
        for (int i = 0; i < flagTextureArray.Length; i++) {
            Texture texture = flagTextureArray[i];
            long no;
            long.TryParse(texture.name.Replace("icoFlag", ""), out no);
            if (dicProfileTexture.ContainsKey(no) == false)
                dicProfileTexture.Add(no, texture);
        }*/
    }

    public Dictionary<long, Texture> GetProfileTextures() {
        if (dicProfileTexture == null)
            LoadProfileTextures();

        return dicProfileTexture;
    }

    public Texture GetProfileTexture(long no) {
        if (dicProfileTexture == null)
            LoadProfileTextures();

        if (dicProfileTexture.ContainsKey(no))
            return dicProfileTexture[no];

        return null;
    }

    public void LoadProfileFrameTextures() {
        dicFrameTexture = new Dictionary<long, Texture>();
        string path = $"{IMAGES_PATH}Frame";
        Texture[] frameTextureArray = Resources.LoadAll<Texture>(path);
        for (int i = 0; i < frameTextureArray.Length; i++) {
            Texture texture = frameTextureArray[i];
            long no;
            long.TryParse(texture.name.Replace("frame", ""), out no);
            if (dicFrameTexture.ContainsKey(no) == false)
                dicFrameTexture.Add(no, texture);
        }
    }

    public Dictionary<long, Texture> GetDicProfileFrame(long no) {
        if (dicFrameTexture == null)
            LoadProfileFrameTextures();

        return dicFrameTexture;
    }

    public Texture GetAchievementRewardIco(long rewardType) {
        string textureName;
        if (rewardType == (long)ACHIEVEMENT_REWARD_TYPE.GOLD)
            textureName = "icoGold";
        else
            textureName = "icoDiamond";
        string path = $"{ICO_ITEM_PATH}{textureName}";
        Texture texture = Resources.Load<Texture>(path);
        if (texture != null)
            return texture;

        string textureDataErrorformat = "이미지 없음. ID = {0}";
        Debug.LogWarning(string.Format(textureDataErrorformat, textureName));
        return null;
    }

    public GameObject LoadTutorialDesc(long tutorialID, long step, Transform parent) {
        string prefabName = $"Tutorial{tutorialID.ToString("D4")}{step.ToString("D2")}";

        string path = TUTORIAL_DESC_PATH + prefabName.ToString();
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
            Debug.LogError("prefab is null : path = " + path);

        GameObject obj = Instantiate(prefab, parent);
        obj.transform.localPosition = Vector3.zero;
        return obj;
    }

    public GameObject LoadProductPrefab(long packageID, Transform parent) {
        string prefabName = $"prefabProduct{packageID.ToString("D4")}";

        string path = PRODUCT_PATH + prefabName.ToString();
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
            Debug.LogError("prefab is null : path = " + path);

        GameObject obj = Instantiate(prefab, parent);
        obj.transform.localPosition = Vector3.zero;
        return obj;
    }

    public Texture GetIcoPiggyBankTexture(long piggyBankType) {
        string path = $"{ICO_PIGGY_BANK_PATH}icoPig{piggyBankType.ToString("D2")}";
        Texture texture = Resources.Load<Texture>(path);
        if (texture != null)
            return texture;

        string textureDataErrorformat = "돼지저금통 이미지 없음. ID = {0}";
        Debug.LogWarning(string.Format(textureDataErrorformat, piggyBankType));
        return null;
    }

    public Texture GetMailRewardIco(long rewardType) {
        string textureName;
        if (rewardType == (long)MAIL_REWARD_TYPE.GOLD)
            textureName = "icoGold";
        else if (rewardType == (long)MAIL_REWARD_TYPE.DIAMOND)
            textureName = "icoDiamond";
        else 
            textureName = "icoTicket";

        string path = $"{ICO_ITEM_PATH}{textureName}";
        Texture texture = Resources.Load<Texture>(path);
        if (texture != null)
            return texture;

        string textureDataErrorformat = "이미지 없음. ID = {0}";
        Debug.LogWarning(string.Format(textureDataErrorformat, textureName));
        return null;
    }

    public GameObject GetPiggyBank(long bankSkinID, Transform parent) {
        string path = $"{ETC_PATH}PiggyBank/Piggy{bankSkinID:D4}";
        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
            Debug.Log("prefab is null : path = " + path);

        GameObject piggyBank = Instantiate(prefab, parent);
        //piggyBank.transform.localPosition = Vector3.zero;
        return piggyBank;
    }

    public Texture GetEventBannerImage(long imageNo) {
        string path = $"{EVENT_BANNER_PATH}eventBanner{imageNo:D2}";
        Texture texture = Resources.Load<Texture>(path);
        if (texture != null)
            return texture;

        return null;
    }

    public Texture GetGameTitleTexture() {
        long imageNo = 1;

        LANGUAGE language = (LANGUAGE)UserDataModel.instance.gameOptions.language;
        if (language == LANGUAGE.none) {
            if (ES_Save.Exists(Constant.LANGUAGE_PATH)) {
                UserDataModel.instance.gameOptions.language = ES_Save.Load<long>(Constant.LANGUAGE_PATH);
                language = (LANGUAGE)UserDataModel.instance.gameOptions.language;
            }
            else {
                UserDataModel.instance.gameOptions.SetDefaultLanguage();
                language = (LANGUAGE)UserDataModel.instance.gameOptions.language;
            }
        }

        if (language == LANGUAGE.kor)
            imageNo = 2;

        string path = $"{GAME_TITLE_PATH}gameTitle{imageNo:D2}";
        Texture texture = Resources.Load<Texture>(path);
        if (texture != null)
            return texture;

        return null;
    }

    public Texture GetProfileFrame(long no) {
        if (dicFrameTexture == null)
            LoadProfileFrameTextures();

        if (dicFrameTexture.ContainsKey(no))
            return dicFrameTexture[no];

        return null;
    }

    public Texture GetLeagueEmblem(long leagueID) {
        string emblemName;

        switch ((LEAGUE_LEVEL)leagueID) {
            case LEAGUE_LEVEL.NONE:
            case LEAGUE_LEVEL.TEST_BRONZE:
            case LEAGUE_LEVEL.BRONZE:
                emblemName = "emblemBronze";
                break;
            case LEAGUE_LEVEL.TEST_SILVER:
            case LEAGUE_LEVEL.SILVER:
                emblemName = "emblemSilver";
                break;

            default:
                emblemName = "emblemGold";
                break;
        }
        
        string path = $"{ICO_EMBLEM_PATH}{emblemName}";
        Texture texture = Resources.Load<Texture>(path);
        if (texture != null)
            return texture;

        return null;
    }

    public Texture GetPackageIco(long rewardType) {
        string textureName;
        if (rewardType == (long)PACKAGE_TYPE.GOLD)
            textureName = "icoGold";
        else if (rewardType == (long)PACKAGE_TYPE.DIAMOND)
            textureName = "icoDiamond";
        else 
            textureName = "icoTicket";

        string path = $"{ICO_ITEM_PATH}{textureName}";
        Texture texture = Resources.Load<Texture>(path);
        if (texture != null)
            return texture;

        string textureDataErrorformat = "이미지 없음. ID = {0}";
        Debug.LogWarning(string.Format(textureDataErrorformat, textureName));
        return null;
    }

    public Texture GetChallengeIco() {
        string path = $"{ICO_CHALLENGE_PATH}challengeMail";
        Texture texture = Resources.Load<Texture>(path);
        if (texture != null)
            return texture;

        Debug.LogWarning("이미지 없음. challengeMail");
        return null;
    }
}