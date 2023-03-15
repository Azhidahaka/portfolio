using LuckyFlow.EnumDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constant {

    //-->로그
    public static bool SHOW_EVENT_LOG = false;  //이벤트 로그
    public static bool APP_LOG = false; //App.cs 로그
    public static bool SHOW_TWEEN_NUMBER_LOG = false;
    //<--로그

    //-->사운드
    public const long MULTIPLE_EFFECT_PLAY_MAX = 5;
    //<--사운드

    public const int INCORRECT = -1;

    public const int MATCH_BLOCK_BUNDLE_COUNT = 3;
    public const int MATCH_BLOCK_MATCH_MIN_COUNT = 5;
    public const int MATCH_BLOCK_HIT_MAX = 50;

    public const int MATCH_BLOCK_BUNDLE_SIZE = 200;

    public const string USER_DATA_TABLE_NAME = "userDatas";
    public const string SCORE_TABLE_NAME = "scores";
    public const string FRIEND_TABLE_NAME = "Friend";
    public const string LEAGUE_SCORE_TABLE_NAME = "leagueScores";

    public static Dictionary<LANGUAGE, string> APPLE_APP_STORE_DIC = new Dictionary<LANGUAGE, string>() {
        {LANGUAGE.kor, $"https://apps.apple.com/kr/app/apple-store/id{"1562531884"}/"},
        {LANGUAGE.eng, $"https://apps.apple.com/app/apple-store/id{"1562531884"}/"},
    };

    public const string SKIN_ID_PATH = "_SKIN_ID";
    public const string LANGUAGE_PATH = "_LANGUAGE";
    public const long DAILY_ADS_VIEW_MAX = 25;
    public const long GOODS_MAX = 999999999;

    public const long STAGE_LEVEL_MAX = 3;
    public const long STAGE_LEVEL_MIN = 1;

    public const long WALL_HP_MAX = 4;
    public const long NEXT_WAVE_WAIT = 3;
    public const long COUNTDOWN_SECONDS = 10;
    public const float MIN_DEBIT_GOLD_PERCENT = 10.0f;   //퍼센트
    //public const float MIN_DEBIT_GOLD_PERCENT = 0.0001f;   //퍼센트
    //public const float MIN_DEBIT_GOLD_PERCENT = 0f;   //퍼센트
    public const long WAVE_REWARD_NORMAL = 20;
    public const long WAVE_REWARD_NORMAL_ADDITIONAL = 10;

    public const long WAVE_REWARD_CLEAR = 100;
    public const long WAVE_REWARD_CLEAR_ADDITIONAL = 50;

    public const long SCORE_PER_REMOVED_BLOCK = 10;

    public const long SHOW_RANKING_COUNT = 5;
    public const long NO_SPACE_GET_GOLD = 1000;

    public const string CHALLENGE_RANKING_UUID = "eba3a7a0-1a11-11ec-a199-efc96572d1a5";
    
    public const int SHOW_CHALLENGE_RANKING_COUNT = 10;
    public const long SHOW_DETAIL_CHALLENGE_RANKING_COUNT = 10;
    public const long ACHIVEMENT_VALUE_MAX = 100000000;

    public const string TEST_1_SCORE_ROW_INDATE = "2021-09-02T07:50:22.832Z";
    public const string TEST_2_SCORE_ROW_INDATE = "2021-09-02T07:47:26.839Z";
    public const long MAX_GAIN_GOODS_PARTICLES = 20;

    public static Dictionary<LANGUAGE, string> INFO_URL_DIC = new Dictionary<LANGUAGE, string>() {
        {LANGUAGE.kor, "https://luckyflow-ko.blogspot.com/p/matchblocks-info.html"},
        {LANGUAGE.eng, "https://luckyflow-en.blogspot.com/p/matchblocks-info.html"},
    };

    public static Dictionary<LANGUAGE, string> TERM_OF_SERVICE_URL_DIC = new Dictionary<LANGUAGE, string>() {
        {LANGUAGE.kor, "https://luckyflow-ko.blogspot.com/p/term-of-service.html"},
        {LANGUAGE.eng, "https://luckyflow-en.blogspot.com/p/term-of-service.html"},
    };

    public static Dictionary<LANGUAGE, string> PRIVACY_POLICY_URL_DIC = new Dictionary<LANGUAGE, string>() {
        {LANGUAGE.kor, "https://matchblocks-ko.blogspot.com/p/privacy-policy.html"},
        {LANGUAGE.eng, "https://matchblocks-eng.blogspot.com/p/privacy-policy.html"},
    };

    public static List<long> CHALLENGE_TICKET_PRICE_LIST = new List<long>() {
        1,
        2,
        3,
        4,
        5
    };

    public const long CHALLENGE_TICKET_MAX = 3;
    public const long SHOP_REWARD_ADS_COOLDOWN = 60;
    public const long SHOP_REWARD_ADS_VIEW_MAX = 3;
    public const long SHOP_REWARD_ADS_DIAMOND = 1;

    public const long SHOP_REWARD_ADS_START_COOLDOWN = 300;

    public const string PATH_AGREE_TERM = "AGREE_TERM";
    public const string PATH_AGREE_PRIVACY = "AGREE_PRIVACY";
    public const string PATH_AGREE_NIGHT_PUSH = "AGREE_NIGHT_PUSH";
    public const string PATH_FORCE_REFRESH_PUSH = "FORCE_REFRESH_PUSH";

    public const long START_DIAMOND = 20;
    //public const float RESET_PATTERN_DURATION = 1.0f;
    public const float RESET_PATTERN_DURATION = 0.775f;
    public const float RESET_PATTERN_DELAY_MAX = RESET_PATTERN_DURATION / 3;

    public const int RANKING_LIST_GAP = 20;

    public const string NOTICE_IMAGE_URL_PREFIX = "http://upload-console.thebackend.io";

    public const long CHANGE_NICKNAME_COST_DIAMOND = 50;
    public const int FRIEND_RECOMMAND_COUNT = 3;
    public const int FRIEND_COUNT_LIMIT = 20;

    public static bool SKIP_TUTORIAL = false;

    public const long LEAGUE_USE_GOLD_LIMIT = 5000;
    public const long CHALLENGE_DAILY_SEND_MAX = 3;
    public const long CHALLENGE_GOLD_DEFAULT = 1000;

    public const long PUBLIC_USER_DATA_SAVE_MAX = 50;
    public const string PUBLIC_USER_DATAS_PATH = "MANAGER_PUBLIC_USER_DATAS";

    public const long MATCH_BLOCKS_AVAILABLE_GOLD = 5000;
    public const long BUY_PROFILE_COST_DIAMOND = 10;

    public static bool SHOW_TEST_LEAGUE = false;
    public const long DUMMY_RANK_COUNT = 15;
}
