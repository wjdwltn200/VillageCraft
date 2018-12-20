using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationState : MonoBehaviour {
    Animator anim;

    public HeroAI heroData;

    private moveCtrl moveCtrlCS;
    public float moveZ;
    public float moveY;

    public GameObject weapon;
    private CapsuleCollider weaponColl;

    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>();
        moveCtrlCS = GetComponent<moveCtrl>();
        weaponColl = weapon.GetComponent<CapsuleCollider>();
        weaponColl.enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
        animState();
    }

    void animState()
    {
        if (moveCtrlCS.isAtk)
        {
            weaponColl.enabled = true;
            anim.SetBool("isRun", false);
            anim.SetBool("isAtk", true);
            moveCtrlCS.isAtk = false;
        }

        if (moveCtrlCS.v > 0.1f) anim.SetBool("isRun", true);
        else if (moveCtrlCS.v == 0.0f) anim.SetBool("isRun", false);
    }

    public void stopAtk()
    {
        weaponColl.enabled = false;
        anim.SetBool("isAtk", false);
        moveCtrlCS.isAtk = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //GameObject tempGO = other.gameObject;
        //if (tempGO.layer == 9)
        //{
        //    tempGO.GetComponent<damageSys>().setHpPoint(heroData._atkPoint);
        //}
    }
}
