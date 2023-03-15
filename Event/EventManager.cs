using UnityEngine;
using System.Collections.Generic;
using System;

public delegate void ObjectCallback(params object[] data);

namespace LuckyFlow.Event {
    public class EventManager {
        private static EventManager instance = new EventManager();
        private Dictionary<EventEnum, ObjectCallback> multiObjEventMap = new Dictionary<EventEnum, ObjectCallback>();

        public static void Notify(EventEnum eventType, params object[] data) {
            ShowLog("Event Notify::" + eventType, data);
            instance.NotifyObservers(eventType, data);
        }

        public static void Register(EventEnum eventType, ObjectCallback observer, bool onceFlag = false) {
            instance.RegisterObserver(eventType, observer, onceFlag);
        }

        public static void Remove(EventEnum eventType, ObjectCallback observer) {
            instance.RemoveObserver(eventType, observer);
        }

        public static void RemoveObserver(EventEnum eventType) {
            instance.RemoveEvent(eventType);
        }

        /// <summary>
        /// 이 옵저버가 소멸되므로 이 옵저버가 등록한 모든 이벤트 델리게이트 삭제
        /// </summary>
        /// <param name="eventType">등록 키값.</param>
        /// <param name="data">전달할 값.</param>
        public static void RemoveObserver(object obj) {
            instance.RemoveObserverObject(obj);
        }

        /// <summary>
        /// 등록된 옵저버들에게 알림.
        /// </summary>
        /// <param name="eventType">등록 키값.</param>
        /// <param name="data">전달할 값.</param>
        public void NotifyObservers(EventEnum eventType, params object[] data) {
            if (multiObjEventMap.ContainsKey(eventType))
                multiObjEventMap[eventType](data);
        }

        /// <summary>
        /// 옵저버 등록.
        /// </summary>
        /// <param name="eventType">등록 키값.</param>
        /// <param name="observer">등록 시킬 옵저버(콜백).</param>
        /// <param name="onceFlag">한번만 호출될 콜백 등록. 기본 true.</param>
        public void RegisterObserver(EventEnum eventType, ObjectCallback observer, bool onceFlag = false) {
            if (!multiObjEventMap.ContainsKey(eventType))
                multiObjEventMap.Add(eventType, observer);
            else
                multiObjEventMap[eventType] += observer;
        }

        public void RemoveObserverObject(object obj) {
            List<EventEnum> keys = new List<EventEnum>(multiObjEventMap.Keys);

            for (int i = 0; i < keys.Count; i++) {
                EventEnum key = keys[i];
                ObjectCallback callBack = multiObjEventMap[key];
                Delegate[] dels = callBack.GetInvocationList();
                foreach (Delegate del in dels) {
                    if (del.Target == obj) {
                        multiObjEventMap[key] -= callBack;
                        if (null == multiObjEventMap[key])
                            multiObjEventMap.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// 옵저버 제거.
        /// </summary>
        /// <param name="eventType">등록한 키값.</param>
        /// <param name="observer">등록한 옵저버(콜백).</param>
        public void RemoveObserver(EventEnum eventType, ObjectCallback observer) {
            if (multiObjEventMap.ContainsKey(eventType)) {
                multiObjEventMap[eventType] -= observer;
                if (null == multiObjEventMap[eventType]) {
                    multiObjEventMap.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// 이벤트 제거.
        /// </summary>
        /// <param name="eventType">등록한 키값.</param>
        /// <param name="observer">등록한 옵저버(콜백).</param>
        public void RemoveEvent(EventEnum eventType) {
            if (multiObjEventMap.ContainsKey(eventType)) {
                multiObjEventMap.Remove(eventType);
            }
        }

        private static void ShowLog(string message, object[] data = null) {
            if (Constant.SHOW_EVENT_LOG == false)
                return;

            if (data != null) {
                for (int i = 0; i < data.Length; i++) {
                    message += ", " + data[i].ToString();
                }
            }

            Debug.Log(message);
        }
    }
}

