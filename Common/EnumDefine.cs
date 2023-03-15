using System;

namespace LuckyFlow.EnumDefine {
    //효과음 목록
    [Serializable]
    public enum SOUND_CLIP_EFFECT { //파일명 추가
        NONE = 0,
        Click = 1,
        Tick = 2,
	    Wrong = 3,
	    BlockClear = 4,
	    WaveReward = 5,
	    GameOver =6,
	    Timer = 7,
	    Button = 8,
	    BundlePickup = 9,
	    UseBundleChange = 10,
	    UseColorChange = 11,
	    UseBombItem = 12,
	    UseHammerItem = 13,
	    BuyGameItem = 14,
	    BuyShopItem =15,
	    BundleRotate = 16,
	    PiggyBankCollect = 17,
	    GlassShow = 18,
	    VirusShow = 19,
	    WallShow = 20,
	    GlassRemove =21,
	    VirusRemove = 22,
	    WallRemove =23,
	    GainScore = 24,
	    VirusGrow = 25,
	    WaveClear = 26,
	    CollectCoins = 27,
	    CollectJewels = 28,
	    MissionClearAlarm = 29,
	    Put = 30,
        LeagueResult = 31,
    }

    //BGM 목록
    [Serializable]
    public enum SOUND_CLIP_BGM {   //파일명 추가
        NONE = 0,
	    Bgm01 = 1,
	    Bgm02 = 2,
	    Bgm03 = 3,
	    Bgm04 = 4,
        Bgm05 = 5,
    }

    //언어
    public enum LANGUAGE {
        none = 0,
        kor = 1,
        eng = 2,
    }

    public enum USER_DATA_KEY {
        TABLE_VERSION_INFO = 0,
        USER_PROFILE = 1,
        STATISTICS = 2,
        GAME_OPTIONS = 3,
        LAST_SELECTED_INFOS = 4,

        SERVER_SYNC_END = 5,

        REFEREE_NOTE = 6,
        MAIL_INFOS = 7,

        NOTICE_INFOS = 8,

        PUBLIC_USER_DATAS = 9,

        LEAGUE_SCORE = 10,
    }

    public enum ITEM_CATEGORY {
        MATCH_BLOCKS = 1,
    }

    public enum MATCH_BLOCKS_ITEM_TYPE {
        NONE = 0,
        RESET_BUNDLES = 101,
        CHANGE_BUNDLE_PATTERNS = 201,
        REMOVE_DIAMOND_RANGED_BLOCK = 301,
        REMOVE_RECTANGLE_RANGED_BLOCK = 302,
        HAMMER = 401,
        INCREASE_TIME = 501,
        CLEAR_SELECTED_PATTERNS = 601,
    }

    public enum SCENE_LOADING_PROGRESS {
        NONE = 0,
        PREPROCESSING = 5,
        CLEAR_BEFORE_SCENE = 10,
        LOAD_NEXT_SCENE_ASYNC = 70,
        INIT_NEXT_SCENE = 99,
        DONE = 100,
    }

    public enum COMPARING_OPERATOR {
        EQUAL = 0,
        GREATER_OR_EQUAL = 1,
        LESS_OR_EQUAL = 2,
        GREATER = 3,
        LESS = 4,
        NOT_EQUAL = 5,
    }

    public enum DISRUPTOR_TYPE {
        NONE = 0,
        ICE = 1,
        WALL = 2,
        VINE = 3,
    }

    public enum CANVAS_ORDER {
        BASE_10 = 10,
        MENU_20 = 20,
        DECO_30 = 30,
        POPUP_40 = 40,
        POPUP_DECO_45 = 45,
        TUTORIAL_50 = 50,
        LOADING_60 = 60,
        WARNING_70 = 70,
    }

    public enum PRODUCT_CATEGORY {
        GOODS = 1,
        SKINS = 2,
        PIGGYBANK_SKINS = 3,
    }

    public enum PRODUCT_COST_TYPE {
        CASH = 1,
        DIAMOND = 2,
        GOLD = 3,
    }

    public enum PACKAGE_TYPE {
        DIAMOND = 1,
        GOLD = 2,
        SKIN = 3,
        REMOVE_ADS = 4,
        PIGGYBANK = 5,
        ADD_NON_CONSUMABLE = 6,
        PIGGYBANK_SKIN = 7,
        CHALLENGE_TICKET = 8,
        FRAME = 9,
        PROFILE = 10,
    }

    public enum ADS_TYPE {
        UNITY = 1,
        ADMOB = 2,
    }

    public enum ADS_PLACEMENT {
        REWARDED_VIDEO,
        INTERSTITIAL,
        BANNER,
    }

    public enum RESTORATION_RESULT {
        NONE = 0,
        SUCCESS = 1,
        FAIL = 2,
    }

    public enum UI_VARIANT {
        NOT_USE = 0,
        USE = 1,
    }

    public enum EFFECT_NAME {
        vfxBlockClear,
        vfxBombClear,
        vfxMagicClear,
        vfxBlockWarningHide,
        vfxBlockWarningShow,
        vfxClearScore,
        vfxPiggyMoney,
        ScoreEffect,
        GainScoreEffect,
        vfxBlockWarningGlass,
        vfxGetGold,
        vfxGetDiamond,
        vfxGetGoods,
        vfxGetSkin,
        vfxGetItem,
    }

    public enum STAGE_LEVEL {
        Easy = 1,
        Normal = 2,
        Hard = 3,
        ExtremeHard = 4,
        LeagueBronze = 5,
        LeagueSilver = 6,
    }

    public enum BOARD_NAME {
        BoardEasy = 1,
        BoardNormal = 2,
        BoardHard = 3,
        BoardExtremeHard = 4,
        BoardLeague = 5
    }


    public enum PIGGY_BANK_TYPE {
        FREE = 1,
        DIAMOND = 2,
        CASH = 3,
    }

    public enum PIG_STATE {
        None,
        Unable,
        Idle,
        Full,
        Click,
    }

    public enum STATISTICS_TYPE {
        INSTANT_START,

        //업적 달성시 리셋
        WAVE_CLEAR_COUNT = 1001,                       
        BLOCK_CLEAR_COUNT = 1002,                     
        USE_DIAMOND = 1003,                           
        GET_PIGGY_BANK_REWARD_COUNT = 1004,  
        REMOVE_ICE_COUNT = 1005,
        REMOVE_WALL_COUNT = 1006,
        REMOVE_VINE_COUNT = 1007,

        INSTANT_END,
        DAILY_START,

        //날짜 경과시 리셋
        DAILY_LOGIN_COUNT = 2001,                           
        DAILY_CONTINUOUS_LOGIN_COUNT = 2002,                 
        DAILY_CHALLENGE_MODE_CLEAR_COUNT = 2003,           
        DAILY_SINGLE_MODE_CLEAR_COUNT = 2004,               
        DAILY_VIEW_ADS_COUNT = 2005,
        DAILY_VISIT_SHOP = 2006,     
        DAILY_USE_ITEM = 2007,
        DAILY_ACHIEVEMENT_CLEAR_COUNT = 2008,
        DAILY_SHOP_REWARD_ADS_VIEW_COUNT = 2009,
        
        DAILY_END,
        ROUND_START,

        //한판마다 리셋
        ROUND_EARNED_SCORE = 3001,              
        ROUND_ITEM_USECOUNT = 3002,    
        ROUND_CREATE_GLASS = 3003,
        ROUND_CREATE_VINE = 3004,
        ROUND_CREATE_WALL = 3005,

        ROUND_END,
        ACC_START,

        //정수 최대치까지 누적
        ACC_LOGIN_COUNT = 4001,
        ACC_GET_GOLD = 4002,                           
        ACC_GET_SCORE = 4003,                          
        ACC_USE_GOLD = 4004,                           
        ACC_USE_DIAMOND = 4005,                         
        ACC_USE_ITEM = 4006,
        ACC_WAVE_CLEAR_COUNT = 4007,
        ACC_BLOCK_CLEAR_COUNT = 4008,
        ACC_GET_PIGGY_BANK_REWARD_COUNT = 4009,
        ACC_VIEW_ADS_COUNT = 4010,
        ACC_VISIT_SHOP = 4011,                           
        ACC_SKIN_CHANGE_COUNT = 4012,
        ACC_PLAYED_COUNT = 4013,

        ACC_END,
        ETC_START,
        //기타
        ETC_CHALLENGE_MODE_BEST_RANK = 5001,
        ETC_BEST_HIT = 5002,

        ETC_END,
    }

    public enum ACHIEVEMENT_GROUP {
        START = 0,
        LOGIN_COUNT = 1,
        CONTINUOUS_LOGIN = 2,
        WAVE_CLEAR_COUNT = 3,
        BLOCK_CLEAR_COUNT = 4,
        CHALLENGE_MODE_CLEAR_COUNT = 5,
        SINGLE_MODE_CLEAR_COUNT = 6,
        ACC_GET_GOLD = 7,
        ACC_GET_SCORE = 8,
        EARNED_SCORE_IN_ONE_ROUND = 9,
        USE_DIAMOND = 10,
        GET_PIGGY_BANK_REWARD_COUNT = 11,
        EARNED_SCORE_IN_ONE_ROUND_WITHOUT_ITEM_USE = 12,
        VISIT_SHOP = 13,
        CHALLENGE_MODE_RANKING = 14,
        BEST_HIT = 15,
        USE_ITEM = 16,
        REMOVE_ICE = 17,
        REMOVE_WALL = 18,
        REMOVE_VINE = 19,
        CHANGE_SKIN = 20,
        CLEAR_DAILY_ACHIEVEMENT = 21,
        DAILY_ADS_VIEW = 22,
        END,
    }

    public enum ACHIEVEMENT_REPEAT {
        NONE = 0,
        IMMEDIATELY = 101,
        IMMEDIATELY_INCREASE = 102,
        DAILY = 201,
    }

    public enum ACHIEVEMENT_REWARD_TYPE {
        GOLD = 1,
        DIAMOND = 2,
    }

    public enum TUTORIAL_CONDITION {
        NONE = 0,
        ACC_BLOCK_CLEAR_COUNT = 1,
        ACC_HIT_COUNT = 2,
        NEXT_WAVE_REMAIN_TURN = 3,
        WAVE_SCORE = 4,
        LOGIN_COUNT = 5,
        NO_SPACE = 6,
        CREATE_GLASS_COUNT = 7,
        CREATE_VINE_COUNT = 8,
        CREATE_WALL_COUNT = 9,
        SHOW_RESULT_PERSONAL = 10,
        ACC_PLAYED_COUNT = 11,
        RANKING_ACTIVE = 12,

        SHOW_LEAGUE_POPUP = 13,
        LEAGUE_EXIST = 14,

        FRIEND_COUNT = 15,
        GOLD_AMOUNT = 16,
        SHOW_MAILBOX = 17,
        REFEREE_SCORE = 18,
        CHALLENGE_NO_RECEIVER = 19,
        CHALLENGE_SEND_REMAIN_COUNT = 20,
    }

    public enum TUTORIAL_STEP_TYPE {
        FOCUS = 1,
        DESC = 2,
        DESC_PAGE = 3,
        TRANSPARENT_BG = 4,
        SHOW_OBJECT = 5,
        HIDE_OBJECT = 6,
        FOCUS_WITH_DESC = 7,
        RECT_FOCUS = 8,
    }

    public enum TUTORIAL_STEP_TARGET {
        NONE = 0,
        BUNDLE_GENERATOR = 1,
        MATCH_BLOCKS_FINGER = 2,
        MATCH_BLOCKS_BOARD = 3,
        MATCH_BLOCKS_TXT_WAVE_SCORE = 4,
        MATCH_BLOCKS_BTN_WAVE_REWARD = 5,
        HOME_TXT_DIFFICULTY = 6,
        HOME_BTN_NEW_START = 7,
        LOCATION_MATCH_BLOCKS_BTN_BOMB_ITEM = 8,
        RESULT_PERSONAL_RANK = 9,
        LOCATION_RESULT_PERSONAL_BTN_CHALLENGE = 10,
        LOCATION_HOME_BTN_PIGGYBANK = 11,
        MATCH_BLOCKS_FIRST_BUNDLE = 12,

        HOME_BTN_SOLO_TRAINING = 13,
        GAME_LEVEL_BTN_EASY = 14,
        MATCH_BLOCKS_POS_GOLD = 15,

        LEAGUE_FIRST_SLOT = 16,
        LEAGUE_FIRST_SLOT_POS_BTN_REWARD = 17,
        LEAGUE_FIRST_SLOT_POS_BTN_START = 18,

        RESULT_POPUP_PERSONAL_BTN_CHALLENGE = 19,
        CHALLENGE_FRIEND_FIRST_SLOT_BTN_SELECT = 20,
        CHALLENGE_FRIEND_BTN_CONFIRM = 21,

        CHALLENGE_BEFORE_POS_BTN_1000 = 22,
        CHALLENGE_BEFORE_BTN_SEND = 23,

        MAILBOX_TAB_CHALLENGE = 24,

        CHALLENGE_FRIEND_FIRST_SLOT = 25,

        MAILBOX_FIRST_SLOT = 26,
        MAILBOX_FIRST_SLOT_BTN_REWARD = 27,
        CHALLENGE_RESULT_BTN_CLOSE = 28,
    }

    public enum TUTORIAL_STEP_FINGER {
        HIDE = 0,
        SHOW = 1,
    }

    public enum TUTORIAL_STEP_EVENT {
        NONE = 0,
        
        TutorialDelayEnd = 1,
        BundleDeployed = 2,
        RewardTooltipShow = 3,
        SetDifficultyEasy = 4,
        LoadMatchBlocksComplete = 5,
        MatchBlocksAddBundleComplete = 6,
        MatchBlocksRotateBundle = 7,

        GameLevelPopupShow = 8,

        ChallengeFriendListShow = 9,
        ChallengeBeforePopupShow = 10,

        ChallengeFriendListPopupSelectFriend = 11,
        ChallengeBeforePopupBtnSendClick = 12,

        MailboxChallengeTabSelected = 13,

        ShowChallengeResult = 14,
        HideChallengeResult = 15,
    }

    public enum TUTORIAL_ID {
        FIRST_MATCHBLOCKS = 1001,
        SEND_CHALLENGE_MESSAGE = 6001,
        SHOW_CHALLENGE_RESULT = 6002,
    }

    public enum TUTORIAL_DESC_POS {
        NONE = 0,
        TOP = 1,
        BOTTOM = 2,
    }

    public enum TUTORIAL_SHOW_MSG {
        NONE = 0,
        NEW = 1,
        KEEP = 2,
    }

    public enum FEDERATION_TYPE {
        GUEST = 0,
        GOOGLE = 1,
        APPLE = 2,
        FACEBOOK = 3,
        GAMECENTER = 4,
    }

    public enum POLICY_AGREE_STATE {
        NOT_SET = 0,
        AGREE = 1,
        DISAGREE = 2,
    }

    public enum LIST_ADD_TYPE {
        INSERT,
        SET,
        APPEND,
    }

    public enum USER_DATA_TABLE_NAME {
        START,
        userDatas,
        mails,
        publicUserDatas,
        leagueScores,

        END,
    }

    public enum MAIL_REWARD_TYPE {
        GOLD = 1,
        DIAMOND = 2,
        SKIN = 3,
        CHALLENGE_TICKET = 4,
        PIGGYBANK_SKIN = 5,
        PACKAGE = 6,
    }

    public enum PIGGYBANK_SKIN_BUY_TYPE {
        NOT_FOR_SALE = 0,
        BUY = 1,
        PACKAGE = 2,
    }

    public enum LEAGUE_LEVEL {
        NONE = 0,

        BRONZE = 10,
        SILVER = 20,

        TEST_BRONZE = 110,
        TEST_SILVER = 120,

        END,
    }

    public enum CHALLENGE_MSG_INDEX {
        DIFFICULTY = 0,
        BETTING_GOLD = 1,
        SENDER_IN_DATE = 2,
        SENDER_SCORE = 3,
        RECEIVER_IN_DATE = 4,
        RECEIVER_SCORE = 5,
        STATE = 6,
        CREATE_DATE = 7,
    }

    public enum CHALLENGE_STATE {
        RECEIVE = 1,       //도전장 받음
        REJECT = 2,         //거절됨
        CHECK_RESULT = 3,         //결과확인
    }
}


