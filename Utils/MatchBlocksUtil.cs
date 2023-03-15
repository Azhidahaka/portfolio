using LuckyFlow.EnumDefine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MatchBlocksUtil {

    public static SpaceDTO.DisruptorDTO GetDisruptorInfo(GameData.DisruptorDTO vineData, GameData.DisruptorDTO wallData) {

        //방해물이 등장하지 않는 스테이지면 null 리턴
        if (vineData == null && wallData == null)
            return null;

        GameData.DisruptorDTO disruptorData;

        if (vineData != null && wallData == null)
            disruptorData = vineData;
        else if (vineData == null && wallData != null)
            disruptorData = wallData;
        //둘다 등장하는 난이도면 50:50
        else {
            int typeRandomValue = Random.Range(0, 2);
            if (typeRandomValue == 0)
                disruptorData = wallData;
            else
                disruptorData = vineData;
        }

        int randomValue = Random.Range(1, 10001);
        if (randomValue > disruptorData.probability)
            return null;

        SpaceDTO.DisruptorDTO disruptorInfo = new SpaceDTO.DisruptorDTO();
        disruptorInfo.type = disruptorData.type;
        disruptorInfo.hp = Random.Range((int)disruptorData.hpMin, 
                                        (int)disruptorData.hpMax + 1);
        //덩굴인경우 줄기부터 시작
        disruptorInfo.stem = disruptorInfo.type == (long)DISRUPTOR_TYPE.VINE;

        return disruptorInfo;
    }

    public static MatchBlocksBoardSpace GetBoardSpaceByCoordinates(MatchBlocksBoardSpace[] boardSpaces, int x, int y) {
        for (int i = 0; i < boardSpaces.Length; i++) {
            Vector2 coordinates = boardSpaces[i].GetCoordinates();
            if (coordinates.x == x && coordinates.y == y)
                return boardSpaces[i];
        }

        return null;
    }

    public static MatchBlocksBoardSpace GetNearestBoardBlock(MatchBlocksBoardSpace[] boardSpaces, Vector3 targetPosition) {
        MatchBlocksBoardSpace shortestBoardBlock = null;
        float shortest = float.MaxValue;

        for (int i = 0; i < boardSpaces.Length; i++) {
            if (boardSpaces[i].gameObject.activeSelf == false)
                continue;

            Vector2 offset = boardSpaces[i].transform.position - targetPosition;
            if (shortest > offset.sqrMagnitude) {
                shortestBoardBlock = boardSpaces[i];
                shortest = offset.sqrMagnitude;
            }
        }

        return shortestBoardBlock;
    }

    public static List<MatchBlocksBoardSpace> GetRangedSpaces(MatchBlocksBoardSpace[] boardSpaces, Vector2 coordinates, GameData.ItemDTO itemData) {
        List<MatchBlocksBoardSpace> result = new List<MatchBlocksBoardSpace>();

        long range = itemData.value;
        if (itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.CLEAR_SELECTED_PATTERNS) {
            MatchBlocksBoardSpace selectedSpace = GetBoardSpaceByCoordinates(boardSpaces, (int)coordinates.x, (int)coordinates.y);
            SpaceDTO selectedSpaceInfo = selectedSpace.GetSpaceInfo();

            UserData.RefereeNoteDTO refereeNote = MatchBlocksReferee.instance.GetRefereeNote();
            List<SpaceDTO> spaceInfos = refereeNote.spaceInfos;
            for (int i = 0; i < spaceInfos.Count; i++) {
                if (selectedSpaceInfo.blockTextureIndex == spaceInfos[i].blockTextureIndex) {
                    MatchBlocksBoardSpace rangedSpace = GetBoardSpaceByCoordinates(boardSpaces, 
                                                                                   (int)spaceInfos[i].coordinates.x,
                                                                                   (int)spaceInfos[i].coordinates.y);
                    result.Add(rangedSpace);
                }
            }

            return result;
        }
        else if (range == 0) {
            MatchBlocksBoardSpace rangedSpace = GetBoardSpaceByCoordinates(boardSpaces, (int)coordinates.x, (int)coordinates.y);
            if (rangedSpace != null)
                result.Add(rangedSpace);
            return result;
        }

        int XMin = (int)( coordinates.x - range );
        int XMax = (int)( coordinates.x + range );

        int YMin = (int)( coordinates.y - range );
        int YMax = (int)( coordinates.y + range );

        for (int x = XMin; x <= XMax; x++) {
            for (int y = YMin; y <= YMax; y++) {
                if (itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.REMOVE_DIAMOND_RANGED_BLOCK &&
                    Math.Abs(coordinates.x - x) + Math.Abs(coordinates.y - y) > range)
                    continue;

                MatchBlocksBoardSpace rangedSpace = GetBoardSpaceByCoordinates(boardSpaces, x, y);

                if (rangedSpace == null || 
                    rangedSpace.IsEmpty())
                        continue;

                result.Add(rangedSpace);
            }
        }

        return result;
    }

    public static bool IsRemoveItem(GameData.ItemDTO itemData) {
        if (itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.REMOVE_DIAMOND_RANGED_BLOCK ||
            itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.REMOVE_RECTANGLE_RANGED_BLOCK ||
            itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.CLEAR_SELECTED_PATTERNS ||
            itemData.type == (long)MATCH_BLOCKS_ITEM_TYPE.HAMMER)
            return true;
        return false;
    }

    public static void ConfirmGold(Callback startGameCallback) {
        if (TutorialManager.instance.IsTutorialInProgress() == false &&
            UserDataModel.instance.userProfile.neverShowGoldWarning == 0 &&
            UserDataModel.instance.ContinueGame == false &&
            UserDataModel.instance.userProfile.gold < Constant.MATCH_BLOCKS_AVAILABLE_GOLD) {
            string format = TermModel.instance.GetTerm("format_have_gold_warning");
            string msg = string.Format(format, Common.GetCommaFormat(Constant.MATCH_BLOCKS_AVAILABLE_GOLD));
            Callback neverShow = () => {
                WebStage.instance.ReqNeverShowGoldWarning(startGameCallback);
            };

            string txtNever = TermModel.instance.GetTerm("msg_never_show_again");

            MessageUtil.ShowWarning(CommonPopup.BUTTON_TYPE.YES_MODIFY_NO, msg, startGameCallback, null, neverShow, "", "", txtNever, false, null);
        }
        else
            startGameCallback();
    }
}
