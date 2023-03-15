using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace GameData {

    public class ItemDTO {
        public long itemID;
        public long category;
        public long type;       //1:점수 방어, 2:색 조정, 3:시간/횟수 늘리기
        public long value;
        public long price;
        public long buy;
    }

    public class RewardDTO {
        public long rewardID;
        public long dropItemID;
        public long rewardType;        //확정 보상 : 0, 확률보상 : 1 
        public long probability;
        public long minCount;
        public long maxCount;
    }

    public class StageDTO {
        public long level;
        public float time;
        public long boardID;
        public long matchScore;
        public long rewardGold;
        public long itemBundleID;
    }

    public class ItemBundleDTO {
        public long itemBundleID;
        public long itemID;
    }


    public class ProductDTO {
        public long packageID;
        public string strID;
        public string appleStrID;
        public long category;
        public long cost;
        public long costType;
        public long consumable;
        public long isEvent;
        public long endDate;
    }

    public class PackageDTO {
        public long packageID;
        public long type;
        public long value;
        public long bonus;
    }

    public class DisruptorDTO {
        public long disruptorID;
        public long type;           //EnumDefine.DISRUPTOR_TYPE
        public long probability;
        public long generateMax;
        public long hpMin;
        public long hpMax;

        public int extension;
    }

    public class TutorialDTO {
        public long tutorialID;
        public long condition;  //EnumDefine.TUTORIAL_CONDITION
        public string sceneName1;
        public string sceneName2;
        public long comparingOperator;   //EnumDefine.COMPARING_OPERATOR
        public long value;
        public long requireTutorialID;
        public long use;
        public long skip;
        public long rewardGold;
    }

    public class TutorialStepDTO {
        public long tutorialID;
        public long timeScale;
        public long step;
        public long type;       //EnumDefine.TUTORIAL_STEP_TYPE
        public long target;     //EnumDefine.TUTORIAL_STEP_TARGET
        public long finger;     //EnumDefine.TUTORIAL_STEP_FINGER
        public long startEvent; //EnumDefine.TUTORIAL_STEP_EVENT
        public long endEvent;    //EnumDefine.TUTORIAL_STEP_EVENT
        public float focusScale;
        public float delay;
        public long showNpc;
        public long showMsg;
        public long descPos;
        public float extension;
    }


    public class BundleDTO {
        public long bundleID;
        public long grid;
        public long blockCount;
        public long difficulty;
        public int width;
        public int height;
        public string marking;
        public float transformSize;
    }

    public class BoardWaveDTO {
        public long boardID;
        public long waveID;
        public int grid;
        public float scale;
        public long patternID;
        public long bundleSetID;
        public long wallID;
        public long vineID;
        public long repeat;
        public long goal;
    }

    public class PatternDTO {
        public long patternID;
        public long no;
    }

    public class BundleSetDTO {
        public long bundleSetID;
        public long bundleDifficulty;
        public long probability;
    }

    public class HitDTO {
        public long matchCount;
        public long grade;
        public long textType;
        public long scorePerBlock;
    }

    public class SkinDTO {
        public long skinID;
        public long defaultUnlock;
        public long buy;
    }

    public class UIOrderDTO {
        public string uiName;
        public long order;
        public long variant;
    }

    public class PiggyBankDTO {
        public long type;
        public long capacity;
        public float timeCycle;
        public float timeValue;
        public float turnValue;
        public long showAds;
    }

    public class BoardScaleDTO {
        public long grid;
        public float scale;
    }

    public class WaveRewardDTO {
        public long stageLevel;
        public long clear;
        public long clearAdditional;
        public long clearMax;
    }

    public class AchievementDTO {
        public long group;
        public long achievementID;
        public long requirementID;
        public long repeat;
        public long valueType;
        public long value;
        public long comparingOperator;
        public long incValue;
        public long valueMax;
        public long rewardType;
        public long rewardValue;
    }

    public class PiggyBankSkinDTO {
        public long bankSkinID;
        public long defaultUnlock;
        public long requirePackageID;
        public long buy;
    }

    public class LeagueRewardDTO {
        public int leagueID;
        public long rank;
        public float percent;
        public long packageID;
    }

    public class FrameDTO {
        public long frameID;
        public long defaultUnlock;
        public long requirePackageID;
        public long buy;
    }

    public class ProfileDTO {
        public long profileID;
        public long defaultUnlock;
        public long requirePackageID;
        public long buy;
    }

    public class DummyRankingDTO {
        public long seasonNo;
        public long league;
        public long score;
        public long npcID;
    }

    public class RankingNPCDTO {
        public long npcID;
        public string name;
        public long profileNo;
        public long frameNo;
    }
}
