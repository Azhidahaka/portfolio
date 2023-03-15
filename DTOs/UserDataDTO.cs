using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuckyFlow.EnumDefine;
using QuantumTek.EncryptedSave;

namespace UserData {
    public class UserProfileDTO {
        public string userName;
        public string nickname;

        public long gold;
        public long diamond;
        public long lastAdsReset;
        public long dailyAdsViewCount;

        public List<long> nonconsumableIDs;
        public List<long> skinIDs = new List<long>();  //상품으로 포함되지 않은 스킨들만 추가
        public List<long> bankSkinIDs = new List<long>();
        public List<long> profileIDs = new List<long>();
        public List<long> frameIDs = new List<long>();

        public long piggyBankType;
        public float collectedGold;
        public long collectedChangedTime;

        public string scoreRowIndate;
                 

        public long flagNo;
        public long loginUtcZero;

        public long challengeTicket;
        public long challengeTicktBuyCount;
        
        public long shopRewardAdsCooldownEnd;

        public long agreeTerm;
        public long agreePrivacy;
        public long agreeNightPush;

        public long userNumber;
        public long freeNicknameChangeCount;

        public long profileFrameNo;
        public long challengeSendRemainCount;

        public long neverShowGoldWarning = 0;

        public UserProfileDTO() {
            gold = 5000;
            diamond = 0;
            nonconsumableIDs = new List<long>();
            skinIDs = new List<long>();
            piggyBankType = (long)PIGGY_BANK_TYPE.FREE;
            collectedGold = 0;
            flagNo = 0;
            challengeTicket = Constant.CHALLENGE_TICKET_MAX;
            challengeTicktBuyCount = 0;
            userName = "";
            freeNicknameChangeCount = 1;
            challengeSendRemainCount = Constant.CHALLENGE_DAILY_SEND_MAX;
        }

        public void SetGoldChangedTime() {
            collectedChangedTime = Common.GetUnixTimeNow();
        }

        public void SetLoginUtcZero() {
            loginUtcZero = Common.GetUTCDateZero(Common.GetUTCNow());
        }
    }

    public class RefereeNoteDTO {
        public long stageLevel;

        public long maxHitCount;
        public long accumulateHitCount;
        public bool noSpace = false;
        public int boardWaveIndex;
        public int nextBoardWaveIndex;
        public long turnCount;

        public long totalScore;
        public long waveScore;

        public long nextWaveRemainTurn;

        public List<SpaceDTO> spaceInfos = new List<SpaceDTO>();

        public List<BundleInfoDTO> bundleInfos = new List<BundleInfoDTO>();

        public bool waveRewardReceived = false;

        public long waveCount;
        public long itemUsedCount;

        public float remainTime;

        public long availableGold;
        public long availableGoldMax;
        public long showNoSpaceAdsCount = 0;

        public long rewardGold;
    }

    public class GameOptionDTO {
        public double effectVolumeRatio;
        public double bgmVolumeRatio;
        public bool vibrate;
        public long language;

        public GameOptionDTO() {
            effectVolumeRatio = 1.0f;
            bgmVolumeRatio = 1.0f;
            vibrate = true;
        }

        public void SetDefaultLanguage() {
            if (Application.systemLanguage == SystemLanguage.Korean)
                language = (long)LANGUAGE.kor;
            else
                language = (long)LANGUAGE.eng;

            ES_Save.Save(language, Constant.LANGUAGE_PATH);
        }
    }

    public class StatisticsDTO {
        public Dictionary<long, List<SingleRankDTO>> dicSingleRanks = new Dictionary<long, List<SingleRankDTO>>();
        public SingleRankDTO lastSingleRank = new SingleRankDTO();

        public List<long> clearedAchievementIDs = new List<long>();
        public List<long> receivedAchievementIDs = new List<long>();

        //Key == 업적ID
        public Dictionary<long, long> dicAchievementLevel = new Dictionary<long, long>();   

        //Key == 업적의 valueType
        public Dictionary<long, long> dicInstantRecord = new Dictionary<long, long>();  //업적보상 수령 후 리셋
        public Dictionary<long, long> dicDailyRecord = new Dictionary<long, long>();    //날짜 경과 후 리셋
        public Dictionary<long, long> dicRoundRecord = new Dictionary<long, long>();  //퍼즐 종료후 리셋
        public Dictionary<long, long> dicAccRecord = new Dictionary<long, long>();  //누적(정수 최대치까지)
        public Dictionary<long, long> dicEtcRecord = new Dictionary<long, long>();  //리셋하지 않음

        public List<long> tutorialCompleteIDs = new List<long>();
        public List<long> tutorialStartIDs = new List<long>();

        public string lastNoticeIndate;
        public long lastRegisteredScore;
        public long getStartDiamonds;

        public long tutorialChallengeScore;
        public long tutorialChallengeDifficulty;
    }

    public class TableVersionInfoDTO {
        public string inDate;
        public double updatedAt;
    }

    public class LastSelectedInfoDTO {
        public long stageLevel;
        public long skinID;
        public long bankSkinID;
    }

    public class SingleRankDTO {
        public long rank;
        public long score;
        public long time;
    }

    public class ChallengeRankDTO {
        public long rank;
        //public string nickname;
        public long score;
        public string gamerInDate;
        public ExtensionDTO extension = new ExtensionDTO();

        public class ExtensionDTO {
            public string nickname;
            public long flagNo; //사용하지 않음
            public long profileNo;
            public long frameNo;
        }
    }

    public class NoticeDTO {
        public string title;
        public string content;
        public string postingDate;        
        public string author;
        public string uuid;
        public string inDate;      
        public bool isPublic = true;
        public string imageKey;
        public bool read = false;

        public bool rightTitle = false;
        public string version = "";
    }

    public class MailInfoDTO {
        public double no;
        public long category;
        public string title;
        public string content;
        public long createTIme;
        public long expireTime;
        public List<MailRewardDTO> rewards;
        public bool read = false;
        public bool received = false;
        public string inDate;
        public long partNo;

        public MailInfoDTO() {

        }

        public MailInfoDTO(MailInfoDTO origin, bool includeRewards = false) {
            no = origin.no;
            category = origin.category;
            title = origin.title;
            content = origin.content;
            createTIme = origin.createTIme;
            expireTime = origin.expireTime;
            if (includeRewards)
                rewards = origin.rewards;
            else
                rewards = new List<MailRewardDTO>();
            read = origin.read;
            received = origin.received;
            inDate = origin.inDate;
        }
    }

    public class MailRewardDTO {
        public long type;
        public long count;
    }

    public class PublicUserDataDTO {
        public string lastLogin;

        public long no;

        public long bestLeagueID;
        public long bestLeagueScore;
        
        public long currentLeagueID;            //친구정보 보기에서만 사용
        public long currentLeagueScore;         //친구정보 보기에서만 사용

        public string nickname;

        public long profileNo;
        public long profileFrameNo;

        public string leagueRecords;

        public long active;

        public class LeagueRecordDTO {
            public long seasonNo;
            public long leagueID;
            public long score;
            public long rank;
        }

        public List<LeagueRecordDTO> leagueRecordList = new List<LeagueRecordDTO>();
    }

    public class SimpleUserDTO {
        public string nickname;
        public string inDate;
        public string lastLogin;
    }

    public class LeagueScoreDTO {
        public string scoreInDate;
        public long leagueID;
        public long leagueScore;
        public string extension;
        public long lastRank;
        public long seasonNo;
        public string uuid;
    }

    public class LeagueDataDTO {
        public double rankStartDateAndTime;
        public double rankEndDateAndTime;
        public string uuid;
        public string title;
        public long seasonNo;
    }

    //현재 내가 속한 리그의 순위와 백분률을 가져온다.
    public class MyRankInfo {
        public int rank;
        public float percent;
    }

    //도전장
    public class ChallengeMsgDTO {
        public long difficulty;
        public long bettingGold;
        public string senderInDate;
        public long senderScore;
        public string receiverInDate;
        public long receiverScore;
        public long state;

        public long createDate;
    }

    public class FriendMessageDTO {
        public string msgInDate;
        public string content;
        public bool isRead;
        public bool isReceiverDelete;
        public bool isSenderDelete;
        public ChallengeMsgDTO challengeMsgInfo;
    }
}
