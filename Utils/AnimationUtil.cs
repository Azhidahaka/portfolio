using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationUtil {
    public static float GetAnimationLength(Animator animator, string name) {
        float length = 0;
        RuntimeAnimatorController animController = animator.runtimeAnimatorController;

        for (int i = 0; i < animController.animationClips.Length; i++)
            if (animController.animationClips[i].name == name)
                length = animController.animationClips[i].length;

        return length;
    }

    public static void SetTrigger(Animator animator, string name) {
        ResetTriggers(animator);
        animator.SetTrigger(name);
    }

    public static void ResetTriggers(Animator animator) {
        AnimatorControllerParameter[] parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++) {
            animator.ResetTrigger(parameters[i].name);
        }
    }
}
