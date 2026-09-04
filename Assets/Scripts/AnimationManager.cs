using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public Animator animator;
    void Start()
    {
        if(animator == null)
            animator = GetComponent<Animator>();
    }
    public void TriggerAnimation(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }
    public void BoolAnimation(string boolName)
    {
        animator.SetBool(boolName, !animator.GetBool(boolName));
    }
}
