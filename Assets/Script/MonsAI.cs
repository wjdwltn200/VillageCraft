using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum State
{
   Move,
   Attack
}


public class MonsAI : MonoBehaviour {

    public GameObject target;

    //Animator anim;
    private NavMeshAgent nav;
    public GameObject currTarget;
    private BuildingData currData;

    private Transform tr;
    private Rigidbody Rg;
    private State currState;
    private State NextState;

    private Animator anim;

    public float NavSpeedMax;
    public float NavSpeedAcc;


    private void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
        tr = GetComponent<Transform>();
        Rg = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        // Nav 초기화
        nav.speed = NavSpeedMax;
        nav.acceleration = NavSpeedAcc;
    }

    void Start () {
        currState = State.Move;
        StartCoroutine(Act_Move());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (currTarget == null && collision.gameObject.tag == "BUILDING")
        {
            currTarget = collision.gameObject;
            currData = currTarget.GetComponent<BuildingData>();
            actionAI(State.Attack);
        }
    }

    void actionAI (State eMove)
    {
        currState = eMove;

        Rg.constraints = RigidbodyConstraints.None;
        Rg.constraints = RigidbodyConstraints.FreezeRotation;

        anim.SetBool("isRun", false);
        anim.SetBool("isAtk", false);
        Debug.Log("0. rot : " + tr.rotation.ToString());


        StopAllCoroutines();
        switch (currState)
        {
            case State.Move:
                StartCoroutine(Act_Move());
                break;
            case State.Attack:
                StartCoroutine(Act_Attack());
                break;
            default:
                break;
        }
    }

    IEnumerator Act_Move()
    {
        GetComponent<NavMeshAgent>().enabled = true;
        nav.SetDestination(target.transform.position);

        anim.SetBool("isRun", true);

        if (currTarget == null)
        {
            NextState = State.Move;
        }else if (currTarget != null)
        {
            NextState = State.Attack;
        }
        yield return new WaitForSeconds(5.0f);
        actionAI(NextState);
    }

    IEnumerator Act_Attack()
    {
        if (!currTarget) yield return new WaitForSeconds(0.1f);
        yield return new WaitForSeconds(0.1f);

        if (currData.currHP <= 0.0f)
        {
            NextState = State.Move;
        }
        else if (currData.currHP > 0.0f)
        {
            NextState = State.Attack;
            Rg.constraints = RigidbodyConstraints.FreezeAll;

            Quaternion rot = Quaternion.LookRotation(currTarget.transform.position - tr.position);
            rot.x = rot.z = 0.0f;
            tr.rotation = Quaternion.Slerp(tr.rotation, rot, Time.deltaTime * 10.0f);
            anim.SetBool("isAtk", true);
            Debug.Log("1. rot : " + tr.rotation.ToString());

            currData.currHP -= 1.0f;
            if (currData.currHP <= 0.0f)
                currData.Des();
        }

        GetComponent<NavMeshAgent>().enabled = false;
        yield return new WaitForSeconds(1.0f);

        actionAI(NextState);
    }
}
