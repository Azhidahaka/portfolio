using LuckyFlow.Event;
using QuantumTek.EncryptedSave;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class NoticeManager : MonoBehaviour {
    public static NoticeManager instance;

    private Queue<UserData.NoticeDTO> queue = new Queue<UserData.NoticeDTO>();
    private Dictionary<string, Texture> dicImage = new Dictionary<string, Texture>();

    private void Awake() {
        instance = this;
    }

    private void Start() {
        StartCoroutine(JobGetTexture());
    }

    private IEnumerator JobGetTexture() {
        while (true) {
            yield return new WaitWhile(() => queue.Count == 0);

            UserData.NoticeDTO noticeInfo = queue.Dequeue();
            if (string.IsNullOrEmpty(noticeInfo.imageKey))
                continue;

            string fullUrl = Constant.NOTICE_IMAGE_URL_PREFIX + noticeInfo.imageKey;
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(fullUrl);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) 
                Debug.LogError(www.error);
            else {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

                dicImage.Add(noticeInfo.uuid, texture);
                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(GetTexturePath(noticeInfo.uuid), bytes);
                EventManager.Notify(EventEnum.GetNoticeImageComplete, noticeInfo.uuid);
            }
        }
    }

    private string GetTexturePath(string uuid) {
        string path = $"{DataPathUtil.GetPersistentDataPath()}{uuid}.png";
        return path;
    }

    private Texture LoadTexture(UserData.NoticeDTO noticeInfo) {
        string path = GetTexturePath(noticeInfo.uuid);

        if (File.Exists(path) == false)
            return null;

        Texture2D texture = new Texture2D(512,128);

        byte[] bytes = File.ReadAllBytes(path);
        texture.LoadImage(bytes);
        texture.Apply();
        return texture;
    }

    public void RequestImage(UserData.NoticeDTO noticeInfo) {
        if (string.IsNullOrEmpty(noticeInfo.imageKey))
            return;

        queue.Enqueue(noticeInfo);
    }

    public Texture GetImage(UserData.NoticeDTO noticeInfo) {
        string key = noticeInfo.uuid;
        if (dicImage.ContainsKey(key))
            return dicImage[key];

        Texture texture = LoadTexture(noticeInfo);
        if (texture != null)
            dicImage.Add(key, texture);

        return texture;
    }

    public void DeleteImages(List<string> uuids) {
        for (int i = 0; i < uuids.Count; i++) {
            string path = GetTexturePath(uuids[i]);
            if (File.Exists(path) == false)
                continue;

            File.Delete(path);
        }
    }
}
