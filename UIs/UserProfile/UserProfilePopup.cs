using LuckyFlow.EnumDefine;
using LuckyFlow.Event;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserData;

public class UserProfilePopup : UIBase {
    public Profile profile;
    public UserStatus userStatus;

    public ScrollRect scrollRect;
    public GridLayoutGroup layoutGroup;

    public GameObject prefabRecordSlot;

    private List<UserProfilePopupSlot> listSlot = new List<UserProfilePopupSlot>();
    private List<UserProfilePopupSlot> reusableListSlot = new List<UserProfilePopupSlot>();

    private void Awake() {
        Common.ToggleActive(prefabRecordSlot, false);
    }

    private void OnEnable() {
        EventManager.Register(EventEnum.NicknameChanged, OnNicknameChanged);
    }

    private void OnNicknameChanged(object[] args) {
        userStatus.SetData();
    }

    private void OnDisable() {
        EventManager.Remove(EventEnum.NicknameChanged, OnNicknameChanged);
    }

    public void SetData() {
        profile.SetData(UserDataModel.instance.publicUserData);
        userStatus.SetData();

        SetScrollView();
    }

    private void SetScrollView() {
        if (listSlot == null) {
            listSlot = new List<UserProfilePopupSlot>();
            reusableListSlot = new List<UserProfilePopupSlot>();
        }
        else {
            foreach (UserProfilePopupSlot scrollViewItem in listSlot) {
                if (reusableListSlot.Contains(scrollViewItem) == false)
                    reusableListSlot.Add(scrollViewItem);
                Common.ToggleActive(scrollViewItem.gameObject, false);
            }
            listSlot.Clear();
        }

        for (int i = 0; i < listSlot.Count; i++) {
            Destroy(listSlot[i].gameObject);
        }
        listSlot.Clear();

        GameObject scrollViewGO;

        List<PublicUserDataDTO.LeagueRecordDTO> records = UserDataModel.instance.publicUserData.leagueRecordList;
        for (int i = 0; i < records.Count; i++) {
            PublicUserDataDTO.LeagueRecordDTO record = records[i];
            UserProfilePopupSlot scrollViewItem;

            if (i >= reusableListSlot.Count) {
                scrollViewGO =  Instantiate(prefabRecordSlot, layoutGroup.transform);
                scrollViewItem = scrollViewGO.GetComponent<UserProfilePopupSlot>();
                reusableListSlot.Add(scrollViewItem);
            }
            else {
                scrollViewItem = reusableListSlot[i];
                scrollViewGO = scrollViewItem.gameObject;
            }

            Common.ToggleActive(scrollViewGO, true);
            scrollViewItem = scrollViewGO.GetComponent<UserProfilePopupSlot>();
            scrollViewItem.SetData(record);
            listSlot.Add(scrollViewItem);
        }
    }

    public void OnBtnChangeNicknameClick() {
        NicknameChangePopup nicknameChangePopup = UIManager.instance.GetUI<NicknameChangePopup>(UI_NAME.NicknameChangePopup);
        nicknameChangePopup.SetData();
        nicknameChangePopup.Show();
    }

    public void OnBtnChangeProfileClick() {
        ProfileChangePopup popup = UIManager.instance.GetUI<ProfileChangePopup>(UI_NAME.ProfileChangePopup);
        popup.SetData(ProfileChangePopup.TAB.PROFILE);
        popup.Show();
    }


}
