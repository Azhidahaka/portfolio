using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryUtil {
    public enum SET_TYPE {
        INCREASE,
        SET,
    }

    public static void SetValue(SET_TYPE setType, Dictionary<long, long> dic, long key, long value) {
        if (dic.ContainsKey(key)) {
            if (setType == SET_TYPE.INCREASE) {
                dic[key] += value;
                if (dic[key] > Constant.ACHIVEMENT_VALUE_MAX)
                    dic[key] = Constant.ACHIVEMENT_VALUE_MAX;
            }
            else
                dic[key] = value;
        }
        else 
            dic.Add(key, value);
    }

    public static long GetValue(Dictionary<long, long> dic, long key) {
        if (dic.ContainsKey(key)) {
            return dic[key];
        }
        else 
            return 0;
    }
}
