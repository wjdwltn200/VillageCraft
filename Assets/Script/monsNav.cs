using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class monsNav : MonoBehaviour {

    public GameObject target;

    //Animator anim;
    NavMeshAgent nav;

    private void Awake()
    {
        //anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
    }
	
	// Update is called once per frame
	void Update () {
        nav.SetDestination(target.transform.position);	
	}
}
