using LuckyFlow.Event;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountdownWarning : UIBase {
    public TextMeshProUGUI lblCount;
    public Animator animatorText;

    private UserData.RefereeNoteDTO refereeNote;

    public void SetData(UserData.RefereeNoteDTO refereeNote) {
        this.refereeNote = refereeNote;
    }

    private void OnEnable() {
        if (refereeNote == null)
            return;

        StopAllCoroutines();
        StartCoroutine(JobCount());
    }

    private IEnumerator JobCount() {
        while(refereeNote.remainTime > 0) {
            //float gap = refereeNote.remainTime - ;
            lblCount.text = Mathf.Round(refereeNote.remainTime).ToString();
            animatorText.SetTrigger("Count");
            yield return new WaitForSeconds(1.0f);
        }

        EventManager.Notify(EventEnum.MatchBlocksGameOver);
    }

    public override void Show() {
        base.Show();
    }
}
