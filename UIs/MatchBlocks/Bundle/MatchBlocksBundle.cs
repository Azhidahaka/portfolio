using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MatchBlocksBundle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    private Vector3 startLocalPosition;
    private Vector3 startLocalScale;
    private Vector2 startRectransformSize;

    public RectTransform rectTransform;
    public float delay = 0.1f;

    public Animator animator;

    public GridLayoutGroup gridLayoutGroup;

    public float distanceFromFinger = 10.0f;
    public AnimCallbackLinker animCallbackLinker;

    private List<MatchBlocksBlockUnit> blocks = new List<MatchBlocksBlockUnit>();
    
    private MatchBlocksBlockUnit[] candidates;

    private List<string> triggers = new List<string>();
    private int delayIndex;

    private GameData.BundleDTO bundleData;
    private int width;
    private int height;
    private List<int> blockTextureIndexes = new List<int>();
    private bool begin = false;

    public void SetData(GameData.BundleDTO bundleData, bool rotate = false) {
        this.bundleData = bundleData;

        if (rotate == false) {
            width = bundleData.width;
            height = bundleData.height;
            SetBlockInfos();
        }

        blocks.Clear();

        if (candidates == null)
            candidates = GetComponentsInChildren<MatchBlocksBlockUnit>(true);

        gridLayoutGroup.constraintCount = width;

        for (int i = 0; i < candidates.Length; i++) {
            if (i > bundleData.marking.Length - 1) {
                Common.ToggleActive(candidates[i].gameObject, false);
                continue;
            }

            Common.ToggleActive(candidates[i].gameObject, true);

            int x = i % gridLayoutGroup.constraintCount;
            int y = i / gridLayoutGroup.constraintCount;

            candidates[i].SetData(blockTextureIndexes[i]);
            candidates[i].SetCoordinates(x, y);
            
            blocks.Add(candidates[i]);
        }
        
        //float size = (float)Constant.MATCH_BLOCK_BUNDLE_SIZE / bundleData.transformSize * 2;
        float size = (float)bundleData.transformSize * 2;
        //rectTransform.sizeDelta = new Vector2(size, size);
        rectTransform.localScale = new Vector2(size, size); //new Vector2(0.1f, 0.1f);

        //transform.localScale = new Vector3(bundleData.transformSize, bundleData.transformSize, 1);
        transform.localScale = new Vector3(0.5f, 0.5f, 1);

        animCallbackLinker.SetCallback(ShowAction);
    }

    public void SetData(BundleInfoDTO bundleInfo) {
        bundleData = bundleInfo.bundleData;
        width = bundleInfo.width;
        height = bundleInfo.height;
        blockTextureIndexes = bundleInfo.blockTextureIndexes;
        SetData(bundleData, true);
    }

    private void SetBlockInfos() {
        blockTextureIndexes.Clear();
        for (int i = 0; i < bundleData.marking.Length; i++) {
            int blockTextureIndex = Constant.INCORRECT;
            if (bundleData.marking[i] != '0') 
                blockTextureIndex = MatchBlocksReferee.instance.GetRandomBlockTextureIndex();
            blockTextureIndexes.Add(blockTextureIndex);
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (MatchBlocksReferee.instance.IsPlaceableState() == false)
            return;

        begin = true;
        MatchBlocksSound.instance.PlayEffect(MatchBlocksSound.ACTION.PICK_UP_BUNDLE);

        startLocalPosition = transform.localPosition;
        startLocalScale = transform.localScale;
        transform.localScale = MatchBlocksReferee.instance.GetBoardScale();
        startRectransformSize = rectTransform.localScale;
        rectTransform.localScale = new Vector2(1, 1);

        EventManager.Notify(EventEnum.MatchBlocksDragStart, this);
    }

    public void OnDrag(PointerEventData eventData) {
        if (begin == false)
            return;

        Vector2 additionalPosition = new Vector3(0, distanceFromFinger * ((float)Screen.height / 1280));
        Vector3 pos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position + additionalPosition, UICamera.instance.cam, out pos);

        transform.position = pos;
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (begin == false)
            return;
        begin = false;

        transform.localPosition = startLocalPosition;
        transform.localScale = startLocalScale;
        rectTransform.localScale = startRectransformSize;
        
        EventManager.Notify(EventEnum.MatchBlocksDragEnd);
    }

    public void ResetAction(GameData.BundleDTO bundleData, int index) {
        this.bundleData = bundleData;
        delayIndex = index;
        
        float interval = 0.47f / 2;
        Invoke("HideAction", interval * index);
    }

    private void HideAction() {
        AnimationUtil.SetTrigger(animator, "BundleChange");
    }

    public void ShowAction() {
        SetData(bundleData);

        if (delayIndex == 2)
            Invoke("NotifyResetEnd", 0.35f);
    }

    private void NotifyResetEnd() {
        EventManager.Notify(EventEnum.MatchBlocksBundleResetEnd);
    }

    public GameData.BundleDTO GetBundleData() {
        return bundleData;
    }

    public List<MatchBlocksBlockUnit> GetBlocks() {
        return blocks;
    }

    public void OnBundleClick() {
        if (MatchBlocksReferee.instance.BundleDragging ||
            MatchBlocksReferee.instance.IsPlaceableState() == false)
            return;

        MatchBlocksSound.instance.PlayEffect(MatchBlocksSound.ACTION.ROTATE);

        RotateClockwise();
        EventManager.Notify(EventEnum.MatchBlocksRotateBundle);
    }

    private void RotateClockwise() {
        int prevWidth = width;
        int prevHeight = height;

        int temp = width;
        width = height;
        height = temp;

        int[] after = new int[height * width];
        for (int i = 0; i < blockTextureIndexes.Count; i++) {
            int beforeX = i % prevWidth;
            int beforeY = i / prevWidth;
            int afterX = prevHeight - 1 - beforeY;
            int afterY = beforeX;
            after[afterY * width + afterX] = blockTextureIndexes[i];
        }

        blockTextureIndexes.Clear();
        blockTextureIndexes.AddRange(after);
        SetData(bundleData, true);
    }

    public void ResetBlockPatterns(float delay, float interval) {
        List<MatchBlocksBlockUnit> actionBlocks = new List<MatchBlocksBlockUnit>();
        for (int i = 0; i < blocks.Count; i++) {
            if (blocks[i].IsBlank())
                continue;
            actionBlocks.Add(blocks[i]);
        }

        for (int i = 0; i < blocks.Count; i++) {
            MatchBlocksBlockUnit block = blocks[i];
            if (block.IsBlank())
                continue;

            float x = block.GetCoordinates().x;
            block.ResetPattern(delay + interval * x);
            blockTextureIndexes[i] = block.GetBlockTextureIndex();
        }
    }

    public class SortByCoordinates : IComparer<MatchBlocksBlockUnit> {
        public int Compare(MatchBlocksBlockUnit left, MatchBlocksBlockUnit right) {
            Vector2 leftCoordinates = left.GetCoordinates();
            Vector2 rightCoordinates = right.GetCoordinates();
            if (leftCoordinates.x < rightCoordinates.x)
                return -1;

            if (leftCoordinates.x > rightCoordinates.x)
                return 1;

            if (leftCoordinates.y < rightCoordinates.y)
                return -1;

            if (leftCoordinates.y > rightCoordinates.y)
                return -1;

            return 0;
        }
    }

    public BundleInfoDTO GetBundleInfo() {
        BundleInfoDTO bundleInfo = new BundleInfoDTO();
        bundleInfo.bundleData = bundleData;
        bundleInfo.blockTextureIndexes = blockTextureIndexes;
        bundleInfo.width = width;
        bundleInfo.height = height;

        return bundleInfo;
    }

    public long GetBundleWidth() {
        float maxX = 0;
        for (int i = 0; i < blocks.Count; i++) {
            if (blocks[i].IsBlank())
                continue;
            Vector2 coordinates = blocks[i].GetCoordinates();
            if (coordinates.x > maxX)
                maxX = coordinates.x;
        }

        return (long)maxX + 1;
    }
}
