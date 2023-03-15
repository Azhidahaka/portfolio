using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpaceDTO {
    [System.Serializable]

    public class DisruptorDTO {
        public long type;
        public long hp;                 //벽일때만 의미있음
        public bool stem = false;       //덩굴 줄기
    }

    public bool empty = false;
    public Vector2 coordinates;
    public int blockTextureIndex = Constant.INCORRECT;
    public DisruptorDTO disruptorInfo = new DisruptorDTO();
}

public class BundleInfoDTO {
    public GameData.BundleDTO bundleData;
    public int width;
    public int height;
    public List<int> blockTextureIndexes = new List<int>();
}


