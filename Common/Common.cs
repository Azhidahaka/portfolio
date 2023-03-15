using LuckyFlow.EnumDefine;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public delegate void Callback();
public class Common {
    public static string GetTimerFormat(double time) {
        double days = time / 3600 / 24;
        double hours = time / 3600 % 24;
        double minute = time % 3600 / 60;
        double second = time % 3600 % 60;
        if ((int)days == 0)
            return string.Format("{0:00}:{1:00}:{2:00}", (int)hours, (int)minute, (int)second);
        return string.Format("{0:00}D  {1:00}:{2:00}:{3:00}", (int)days, (int)hours, (int)minute, (int)second);
    }

    public static string GetCommaFormat(double value) {
        return string.Format("{0:#,0.#}", value);
    }

    public static string GetCostFormat(PRODUCT_COST_TYPE costType, long cost, Color notEngoughColor) {
        string result;

        result = GetCommaFormat(cost);

        if ((costType == PRODUCT_COST_TYPE.DIAMOND &&
            UserDataModel.instance.userProfile.diamond < cost) ||
            (costType == PRODUCT_COST_TYPE.GOLD &&
            UserDataModel.instance.userProfile.gold < cost))
            result = GetColoredText(notEngoughColor, result);
        
        return result;
    }

    public static string GetCostFormat(long compareValue, long cost, Color notEngoughColor) {
        string result;

        result = GetCommaFormat(cost);

        if (compareValue < cost)
            result = GetColoredText(notEngoughColor, result);
        
        return result;
    }

    public static string GetCostFormat(PRODUCT_COST_TYPE costType, long cost) {
        return GetCostFormat(costType, cost, Color.red);
    }

    public static string GetValueResourceFormat(float value) {

        string result = "";
        float resultValue = 0;

        if (value < 100000) {
            resultValue = value;
            result = GetCommaFormat(resultValue);
        }
        else if (value < 1000000) {
            resultValue = value / 1000;
            result = string.Format("{0:F1}K", Math.Truncate(resultValue * 10) / 10);
        }
        else if (value < 1000000000) {
            resultValue = value / ( 1000 * 1000 );
            result = string.Format("{0:F1}M", Math.Truncate(resultValue * 10) / 10);
        }
        else {
            resultValue = value / ( 1000 * 1000 * 1000 );
            result = string.Format("{0:F1}B", Math.Truncate(resultValue * 10) / 10);
        }

        return result;
    }

    public static string GetColoredText(Color color, string text) {
        string colorStr = string.Format("{0:X2}", (long)( color.r * 255 )) +
                          string.Format("{0:X2}", (long)( color.g * 255 )) +
                          string.Format("{0:X2}", (long)( color.b * 255 ));

        //UNITY_UI
        string result = $"<color=#{colorStr}>{text}</color>";

        //NGUI
        //string result = "[" + colorStr + "]" + text + "[-]";

        return result;
    }

    public static void ToggleActive(GameObject go, bool active) {
        if (go == null)
            return;

        if (active) {
            if (go.activeSelf == false)
                go.SetActive(true);
        }
        else {
            if (go.activeSelf)
                go.SetActive(false);
        }
    }

    public static void ToggleEnable(Behaviour behaviour, bool enable) {
        if (enable) {
            if (behaviour.enabled == false)
                behaviour.enabled = true;
        }
        else {
            if (behaviour.enabled)
                behaviour.enabled = false;
        }
    }

    public static string GetCountFormat(long value) {
        string format = "{0}";
        string text = string.Format(format, GetCommaFormat(value));
        return text;
    }

    public static string GetLevelShortFormat(long level) {
        string format = TermModel.instance.GetTerm("lv_short");
        return string.Format(format, level);
    }

    //표시형태 : current + additional / require
    public static string GetCountFormatWithRequire(long current, long require, long additional = 0) {
        Color color = Color.green;
        if (current + additional < require)
            color = Color.red;

        string format = "{0}/{1}";
        string resultText = "";
        if (additional == 0) {
            resultText = string.Format(format,
                                       GetColoredText(color, current.ToString()),
                                       require
                                      );
        }
        else {
            resultText = string.Format(format,
                                       GetColoredText(color, current.ToString() + "+" + additional.ToString()),
                                       require
                                      );
        }

        return resultText;
    }

    public static void ChangeLayersRecursively(Transform transform, string name) {
        transform.gameObject.layer = LayerMask.NameToLayer(name);
        foreach (Transform child in transform) {
            ChangeLayersRecursively(child, name);
        }
    }

    public static long GetUnixTimeNow() {
        return (long)GetUnixTimeNowDouble();
    }

    public static long GetUTCNow() {
        TimeSpan timeSpan = ( DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0) );
        return (long)timeSpan.TotalSeconds;
    }

    public static double GetUnixTimeNowDouble() {
        double elapsedTime = Time.realtimeSinceStartup - BackendLogin.instance.startTime;
        return BackendLogin.instance.serverTime + elapsedTime;
    }

    public static string GetStringDateNow() {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        DateTime date = origin.AddSeconds(GetUnixTimeNowDouble());
        return date.ToString("yyyyMMddHHmmssfff");
    }

    public static string ConvertTimestampToString(double time) {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        DateTime date = origin.AddSeconds(time);
        return date.ToString("yyyyMMddHHmmssfff");
    }

    public static string ConvertTimeStampToInDateFormat(double time) {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        DateTime date = origin.AddSeconds(time);
        return date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    public static double ConvertStringToTimestamp(string time) {
        DateTime parsedDate = DateTime.Parse(time);
        TimeSpan timeSpan = ( parsedDate - new DateTime(1970, 1, 1, 0, 0, 0) );
        return timeSpan.TotalSeconds;
    }

    public static double ConvertStringToTimestampUTC(string time) {
        DateTime parsedDate = DateTime.Parse(time);
        DateTime parsedUTCDate = TimeZoneInfo.ConvertTimeToUtc(parsedDate);
        TimeSpan timeSpan = ( parsedUTCDate - new DateTime(1970, 1, 1, 0, 0, 0) );
        return timeSpan.TotalSeconds;
    }

    public static string GetCharacterHPFormat(long currentHP, long maxHP) {
        string hpFormat = "{0} / {1}";
        string result = string.Format(hpFormat, currentHP, maxHP);
        return result;
    }

    //AABBCC <- 이런 형태로 넘어와야한다
    public static Color ConvertTextToColor(string text) {
        List<int> colorHexCodes = new List<int>();
        for (int i = 0; i < 6; i = i + 2) {
            string splitedText = text[i].ToString() + text[i + 1].ToString();
            int colorHexCode = Convert.ToInt32(splitedText, 16);
            colorHexCodes.Add(colorHexCode);
        }

        Color color = new Color();
        color.r = colorHexCodes[0] / 255.0f;
        color.g = colorHexCodes[1] / 255.0f;
        color.b = colorHexCodes[2] / 255.0f;
        color.a = 1;

        return color;
    }

    public static string GetCurrentMaxFormat(long current, long max) {
        string expFormat = "{0} / {1}";
        string result = string.Format(expFormat, current, max);
        return result;
    }

    public static string GetRecoverHPFormat(long afterHP, long maxHP, long minItemRecoverValue) {
        Color afterHPcolor = Color.white;
        if (afterHP >= maxHP + minItemRecoverValue)
            afterHPcolor = Color.red;
        else if (afterHP > maxHP)
            afterHPcolor = Color.yellow;

        string format = "{0} / {1}";
        string resultText = string.Format(format,
                                          GetColoredText(afterHPcolor, afterHP.ToString()),
                                          maxHP);
        return resultText;
    }

    public static long GetUTCTodayZero() {
        DateTime today = DateTime.UtcNow.Date;
        long epochTicks = new DateTime(1970, 1, 1).Ticks;
        long utcZero = ( ( today.Ticks - epochTicks ) / TimeSpan.TicksPerSecond );        //UTC기준 오늘 0시
        return utcZero;
    }

    public static long GetUTCDateZero(long time) {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        DateTime date = origin.AddSeconds(time);

        long epochTicks = new DateTime(1970, 1, 1).Ticks;
        long utcZero = ( ( date.Ticks - epochTicks ) / TimeSpan.TicksPerSecond ) - (long)date.TimeOfDay.TotalSeconds;        //UTC기준 0시

        return utcZero;
    }

    public static long GetUTCMondayZero() {
        DateTime dateToday = DateTime.Today;
        DateTime dateMonday = dateToday.AddDays(Convert.ToInt32(DayOfWeek.Monday) - Convert.ToInt32(dateToday.DayOfWeek));

        long epochTicks = new DateTime(1970, 1, 1).Ticks;
        long utcZero = ( ( dateMonday.Ticks - epochTicks ) / TimeSpan.TicksPerSecond );        //UTC기준 월요일 0시
        return utcZero;
    }

    public static string GetAddCountFormat(long count) {
        string format = "+{0}";
        if (count < 0) {
            format = "-{0}";
            count = Math.Abs(count);
        }

        string result = string.Format(format, count);
        return result;
    }

    public static string GetTimeStringFormat(long time, bool includeSec = false, bool isUnitOmit = false) {
        int days = (int)( time / 3600 / 24 );
        int hours = (int)( time / 3600 % 24 );
        int minute = (int)( time % 3600 / 60 );
        int second = (int)( time % 3600 % 60 );
        string dayTxt = ( isUnitOmit ) ? "D" : TermModel.instance.GetTerm("day");
        string hourTxt = ( isUnitOmit ) ? "h" : TermModel.instance.GetTerm("hour");
        string minTxt = ( isUnitOmit ) ? "m" : TermModel.instance.GetTerm("minute");
        string secTxt = ( isUnitOmit ) ? "s" : TermModel.instance.GetTerm("second");
        StringBuilder sb = new StringBuilder();
        if (days > 0)
            sb.Append(days + dayTxt + " ");
        if (hours > 0)
            sb.Append(hours + hourTxt + " ");
        if (minute > 0)
            sb.Append(minute + minTxt + " ");
        if (includeSec && second > 0)
            sb.Append(second + secTxt);

        if (days == 0 && hours == 0 && minute == 0 && !includeSec)
            sb.Append(second.ToString() + secTxt);
        return sb.ToString();
    }

    public static string GetShortTimerFormat(float time, bool isUnitOmit = false) {
        int days = (int)( time / 3600 / 24 );
        int hours = (int)( time / 3600 % 24 );
        int minute = (int)( time % 3600 / 60 );
        int second = (int)Math.Ceiling(time % 3600 % 60);

        string dayTxt = ( isUnitOmit ) ? "D" : TermModel.instance.GetTerm("day");
        string hourTxt = ( isUnitOmit ) ? "h" : TermModel.instance.GetTerm("hour");
        string minTxt = ( isUnitOmit ) ? "m" : TermModel.instance.GetTerm("minute");
        string secTxt = ( isUnitOmit ) ? "s" : TermModel.instance.GetTerm("second");

        if (days > 0)
            return string.Format("{0:0}{1}", days, dayTxt);

        else if (hours > 0)
            return string.Format("{0:0}{1}", hours, hourTxt);

        else if (minute > 0)
            return string.Format("{0:0}{1}", minute, minTxt);

        else if (second > 0)
            return string.Format("{0:0}{1}", second, secTxt);

        return "";
    }

    public static string GetMoneyFormat(float value) {
        return string.Format("$ {0:f2}", value);

    }

    public static T ToEnum<T>(string value) {
        if (Enum.IsDefined(typeof(T), value) == false) {
            return default(T);
        }

        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static string GetTargetOverFormat(long current, long target, long max) {
        Color currentColor = Color.white;
        if (current > target)
            currentColor = Color.red;

        string format = "{0} / {1}";
        string resultText = string.Format(format,
                                          GetColoredText(currentColor, current.ToString()),
                                          max);
        return resultText;
    }

    public static string GetDateStringFormat(double time) {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        DateTime date = origin.AddSeconds(time);
        return date.ToString("yyyy-MM-dd HH:mm");
    }

    public static string DecToHex(long value) {
        // 대문자 X일 경우 결과 hex값이 대문자로 나온다.
        string hex = value.ToString("X");
        /*
        if (hex.Length % 2 != 0) {
            hex = "0" + hex;
        }*/
        return hex;
    }

    public static string ConvertTimestampToLeagueDate(double time) {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        DateTime date = origin.AddSeconds(time);
        DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);
        return localDate.ToString("yyyy.MM.dd");
    }

    public static string ConvertTimestampToDateTime(double time) {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        DateTime date = origin.AddSeconds(time);
        DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);
        return localDate.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public static string ConvertTimestampToHM(double time) {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        DateTime date = origin.AddSeconds(time);
        DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);
        return localDate.ToString("HH:mm");
    }
}
