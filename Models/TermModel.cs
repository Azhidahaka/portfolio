using Newtonsoft.Json;
using LuckyFlow.EnumDefine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using QuantumTek.EncryptedSave;

public class TermModel : MonoBehaviour {
    public static TermModel instance;

    private Dictionary<string, TermDTO> dicTerm = new Dictionary<string, TermDTO>();

    private void Awake() {
        instance = this;
        LoadTermData();
    }

    private void OnDestroy() {
        instance = null;
    }

    private void LoadTermData() {
        TextAsset textAsset = Resources.Load<TextAsset>("Datas/Term");
        List<TermDTO> termList = JsonConvert.DeserializeObject<List<TermDTO>>(textAsset.text);
        for (int i = 0; i < termList.Count; i++) {
            
            dicTerm.Add(termList[i].code, termList[i]);
        }
    }

    public string GetTerm(string code, string[] paramValues = null) {
        if (dicTerm.ContainsKey(code) == false) {
            Debug.LogError("텀 없음::" + code);
            return code;
        }

        LANGUAGE language = (LANGUAGE)UserDataModel.instance.gameOptions.language;
        string result = string.Empty;
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
            result = dicTerm[code].kor;
        else
            result = dicTerm[code].eng;

        result = result.Replace("\\n", "\n");

        if (paramValues == null)
            return result;

        for (int i = 0; i < paramValues.Length; i++) {
            result = string.Format(result, paramValues);
        }

        return result;
    }

    public string GetStageLevelText(long stageLevel) {
        string format = "difficulty_{0}";
        string result = GetTerm(string.Format(format, stageLevel));
        return result;
    }

    public string GetSkinName(long skinID) {
        string result = GetTerm($"skin_name_{skinID}");
        return result;
    }

    public string GetPiggyBankSkinName(long bankSkinID) {
        string result = GetTerm($"piggybank_skin_name_{bankSkinID}");
        return result;
    }

    public string GetProductName(long packageID) {
        string result = GetTerm($"product_name_{packageID}");
        return result;
    }

    public string GetAchievementDesc(List<GameData.AchievementDTO> achievementDatas) {
        string format = GetTerm($"format_achievement_{achievementDatas[0].group}");
        ACHIEVEMENT_GROUP group = (ACHIEVEMENT_GROUP)achievementDatas[0].group;
        string msg = "";
        switch (group) {
            case ACHIEVEMENT_GROUP.WAVE_CLEAR_COUNT:
            case ACHIEVEMENT_GROUP.BLOCK_CLEAR_COUNT:
            case ACHIEVEMENT_GROUP.CHALLENGE_MODE_CLEAR_COUNT:
            case ACHIEVEMENT_GROUP.SINGLE_MODE_CLEAR_COUNT:
            case ACHIEVEMENT_GROUP.GET_PIGGY_BANK_REWARD_COUNT:
            case ACHIEVEMENT_GROUP.EARNED_SCORE_IN_ONE_ROUND_WITHOUT_ITEM_USE:
            case ACHIEVEMENT_GROUP.VISIT_SHOP:
            case ACHIEVEMENT_GROUP.CHALLENGE_MODE_RANKING:
            case ACHIEVEMENT_GROUP.USE_ITEM:
            case ACHIEVEMENT_GROUP.REMOVE_ICE:
            case ACHIEVEMENT_GROUP.REMOVE_VINE:
            case ACHIEVEMENT_GROUP.REMOVE_WALL:
            case ACHIEVEMENT_GROUP.CHANGE_SKIN:
            case ACHIEVEMENT_GROUP.CLEAR_DAILY_ACHIEVEMENT:
            case ACHIEVEMENT_GROUP.DAILY_ADS_VIEW:
                msg = string.Format(format, Common.GetCommaFormat(achievementDatas[0].value));
                break;

            case ACHIEVEMENT_GROUP.ACC_GET_GOLD: 
            case ACHIEVEMENT_GROUP.ACC_GET_SCORE: 
            case ACHIEVEMENT_GROUP.EARNED_SCORE_IN_ONE_ROUND:
            case ACHIEVEMENT_GROUP.BEST_HIT:
            case ACHIEVEMENT_GROUP.USE_DIAMOND: {
                long value = AchievementUtil.GetComparingValue(achievementDatas[0]);
                msg = string.Format(format, Common.GetCommaFormat(value));
                break;
            }

            case ACHIEVEMENT_GROUP.LOGIN_COUNT:
            case ACHIEVEMENT_GROUP.CONTINUOUS_LOGIN:
            default:
                msg = format;
                break;
        }

        return msg;
    }

    public string GetTutorialDesc(long tutorialID, long step) {
        string format = "msg_tutorial_{0}_{1}";
        return GetTerm(string.Format(format, tutorialID, step.ToString("D2")));
    }

    public string GetFrameName(long frameID) {
        string result = GetTerm($"frame_name_{frameID}");
        return result;
    }

    public string GetProfileName(long profileID) {
        string result = GetTerm($"profile_name_{profileID}");
        return result;
    }
}
