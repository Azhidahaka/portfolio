using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace LuckyFlow.Event {
    public enum EventEnum {
        EffectVolumeChanged,
        BGMVolumeChanged,
        EffectMute,
        BGMMute,

        FinishedBGM,

        LoadTitle,

        SceneLoadingProgressChanged,

        MatchBlocksDragStart,
        MatchBlocksDragEnd,

        MatchBlocksShowRemoveBlockPopup,
        MatchBlocksRemoveCancel,
        MatchBlocksRefereeDecision,
        MatchBlocksResetBundles,
        MatchBlocksUseItem,
        MatchBlocksBundleResetEnd,

        MatchBlocksGameOver,
        MatchBlocksSubmitBlockInfos,
        MatchBlocksRemoveBlockSelected,
        MatchBlocksRemoveBlockSelectedDrag,
        MatchBlocksRemoveBlockConfirm,
        MatchBlocksAddBundleComplete,
        MatchBlocksShowEndNotice,
        MatchBlocksEffectEnd,

        MatchBlocksEnd,

        MatchBlocksStart,

        MatchBlocksChangeBoardWave,
        MatchBlocksChangeBoardWaveCountDown,
        MatchBlocksChangeBundlePatterns,
        MatchBlocksChangeBundlePatternsEnd,

        MatchBlocksBtnPiggyBankClick,

        MatchBlocksGetWaveRewardComplete,
        MatchBlocksRotateBundle,

        AuthStateChanged,
        LoginSelectBtnBackClick,

        SkinChanged,
        BankSkinChanged,

        ShopMenuTabSelected,
        ShopMenuSkinSelected,
        ShopMenuProductSelected,

        GoodsUpdate,
        UserDataUserInfoUpdate,
        UserDataFlagUpdate,
        UserDataMailInfoUpdate,

        BuyProductComplete,
        
        WithdrawPiggyGoldComplete,

        SelectNationPopupFlagSelected,

        UpdateAchievementCleared,
        RefreshDate,

        TutorialCheck,
        TutorialEnd,

        TutorialDelayEnd,
        BundleDeployed,
        RewardTooltipShow,
        LoadMatchBlocksComplete,

        SimpleRankingUpdate,
        GetNoticeImageComplete,
        NoticeRead,
        PiggySkinListPopupGoShop,

        UpdateVolume,

        ProfileSkinChanged,

        FriendListPopupShowBtnBreak,
        FriendListPopupBtnFriendBreakClick,

        GetPublicUserData,

        ChallengeSelectGold,

        ChallengeFriendListPopupSelectFriend, //Æ©Åä¸®¾ó
        ChallengeMessageSend,

        GameLevelPopupShow,     //Æ©Åä¸®¾ó
        ChallengeFriendListShow,//Æ©Åä¸®¾ó
        ChallengeBeforePopupShow,//Æ©Åä¸®¾ó
        ChallengeBeforePopupBtnSendClick,

        NicknameChanged,

        FriendsStateUpdate,
        CheckBanner,
        NotifiedNewFriend,
        NotifiedNewMessage,

        MailboxChallengeTabSelected,

        ShowChallengeResult,
        HideChallengeResult,

        AchievementGetRewardAllComplete,
        AchievementGetRewardComplete,
        RankingButtonClick,
    }
}