using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;
using LuckyFlow.EnumDefine;

public class AnalyticsUtil {
    private static void LogEvent(string eventName) {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        try {
            FirebaseAnalytics.LogEvent(eventName);
        }
        catch (System.Exception e) {
            Debug.LogError($"AnalyticsUtil::LogEvent::{e.Message}");
        }
#endif
    }

    public static void LogTutorialBegin(long tutorialID) {
        string eventName = $"tutorial_{tutorialID}_begin";
        LogEvent(eventName);
    }

    public static void LogTutorialComplete(long tutorialID) {
        string eventName = $"tutorial_{tutorialID}_complete";
        LogEvent(eventName);
    }

    public static void LogGetAchievementReward(long achievementID) {
        string eventName = $"get_achievement_{achievementID}_reward";
        LogEvent(eventName);
    }

    public static void LogBuyProduct(long productID) {
        string eventName = $"buy_product_{productID}";
        LogEvent(eventName);
    }

    public static void LogAdImpressionPiggy() {
        string eventName = "ad_impression_piggy";
        LogEvent(eventName);
    }

    public static void LogAdImpressionDailyReward() {
        string eventName = "ad_impression_daily_reward";
        LogEvent(eventName);
    }

    public static void LogShareSingleResult() {
        string eventName = "share_single_result";
        LogEvent(eventName);
    }

    public static void LogShareChallengeResult() {
        string eventName = "share_challenge_result";
        LogEvent(eventName);
    }

    public static void LogUseItem(long itemType) {
        string eventName = "";
        switch ((MATCH_BLOCKS_ITEM_TYPE)itemType) {
            case MATCH_BLOCKS_ITEM_TYPE.RESET_BUNDLES:
                eventName = "use_item_reset_bundle";
                break;

            case MATCH_BLOCKS_ITEM_TYPE.CHANGE_BUNDLE_PATTERNS:
                eventName = "use_item_reset_pattern";
                break;

            case MATCH_BLOCKS_ITEM_TYPE.REMOVE_DIAMOND_RANGED_BLOCK:
                eventName = "use_item_bomb";
                break;

            case MATCH_BLOCKS_ITEM_TYPE.REMOVE_RECTANGLE_RANGED_BLOCK:
                eventName = "use_item_rectangle_bomb";
                break;

            case MATCH_BLOCKS_ITEM_TYPE.HAMMER:
                eventName = "use_item_hammer";
                break;

            case MATCH_BLOCKS_ITEM_TYPE.INCREASE_TIME:
                eventName = "use_item_inc_time";
                break;

            case MATCH_BLOCKS_ITEM_TYPE.CLEAR_SELECTED_PATTERNS:
                eventName = "use_item_magic_wand";
                break;


            default:
                return;
        }

        LogEvent(eventName);
    }

    public static void LogPlayStage(long stageLevel) {
        string difficulty = "";
        switch ((STAGE_LEVEL)stageLevel) {
            case STAGE_LEVEL.Easy:
                difficulty = "easy";
                break;

            case STAGE_LEVEL.Normal:
                difficulty = "normal";
                break;

            case STAGE_LEVEL.Hard:
                difficulty = "hard";
                break;

            case STAGE_LEVEL.LeagueBronze:
                difficulty = "bronze";
                break;

            case STAGE_LEVEL.LeagueSilver:
                difficulty = "silver";
                break;

            default:
                return;
        }

        string eventName = $"play_stage_{difficulty}";
        LogEvent(eventName);
    }

    public static void LogBuyChallengeTicket() {
        string eventName = $"buy_challenge_ticket";
        LogEvent(eventName);
    }

    public static void LogApplySkin(long skinID) {
        string eventName = $"apply_skin_{skinID}";
        LogEvent(eventName);
    }
}
