using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationState : MonoBehaviour {
    Animator anim;

    private moveCtrl moveCtrlCS;
    public float moveZ;
    public float moveY;

    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>();
        moveCtrlCS = GetComponent<moveCtrl>();
    }
	
	// Update is called once per frame
	void Update () {
        animState();

    }

    void animState()
    {
        if (moveCtrlCS.isAtk)
        {
            anim.SetBool("isRun", false);
            anim.SetBool("isAtk", true);
            moveCtrlCS.isAtk = false;
        }

        if (moveCtrlCS.v > 0.1f) anim.SetBool("isRun", true);
        else if (moveCtrlCS.v == 0.0f) anim.SetBool("isRun", false);
    }

    public void stopAtk()
    {
        anim.SetBool("isAtk", false);
    }
}
