using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum State
{
   Idle,
   Move,
   Attack,
   Tax,
   Chase
}

public class MonsAI : MonoBehaviour {

    public GameObject target;

    //Animator anim;
    private NavMeshAgent nav;
    public GameObject currTarget;
    private damageSys TargetDamageSys;

    private Transform tr;
    private Rigidbody Rg;
    private State currState;
    private State NextState;

    private Animator anim;

    public float NavSpeedMax;
    public float NavSpeedAcc;

    public string _name = "-";
    public float _maxHP;
    public float _currHP;
    public float _atkPoint = 0.0f;
    public float _atkArea = 0.0f;
    public float _atkDelay = 0.0f;
    public float _atkRange = 0.0f;
    public float _rangeRadius = 0.0f;

    public ParticleSystem DieEff;
    private WaitForSeconds wait;
    private WaitForSeconds atkDelayWait;

    public int tempInt = 0;

    private void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
        tr = GetComponent<Transform>();
        Rg = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        wait = new WaitForSeconds(0.1f);
        atkDelayWait = new WaitForSeconds(_atkDelay);

        GetComponent<SphereCollider>().radius = _atkArea;

        // Nav 초기화
        nav.speed = NavSpeedMax;
        nav.acceleration = NavSpeedAcc;

        // 능력치 초기화
        _currHP = _maxHP;
    }

    void Start () {
        Instantiate(DieEff, transform);

        currState = State.Move;
        StartCoroutine(Act_Move());
    }

    private void Update()
    {
        if (_currHP <= 0.0f)
            Die();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currTarget == null &&
        (other.tag == "BUILDING") ||
        (other.tag == "PLAYER"))
        {
            TargetDamageSys = other.GetComponent<damageSys>();
            currTarget = other.gameObject;
        }
    }

    public void Die()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }

    void actionAI (State eMove)
    {
        currState = eMove;

        Rg.constraints = RigidbodyConstraints.None;
        Rg.constraints = RigidbodyConstraints.FreezeRotation;
        nav.enabled = true;

        anim.SetBool("isRun", false);
        anim.SetBool("isAtk", false);

        StopAllCoroutines();
        switch (currState)
        {
            case State.Move:
                StartCoroutine(Act_Move());
                break;
            case State.Attack:
                StartCoroutine(Act_Attack());
                break;
            case State.Chase:
                StartCoroutine(Act_Chase());
                break;
            default:
                break;
        }
    }

    IEnumerator Act_Move()
    {
        nav.SetDestination(target.transform.position); // 본래 목표를 향해 이동
        anim.SetBool("isRun", true); // 이동 애니메이션
        
        if (currTarget == null) // 적 미 발견시
        {
            NextState = State.Move; // 계속 이동
            yield return new WaitForSeconds(3.0f); // 대기
        }
        else if (currTarget != null) // 적 발견시
        {
            NextState = State.Chase; // 추적으로 변경
            yield return wait; // 대기
        }

        actionAI(NextState);  //  행동 전환
    }

    IEnumerator Act_Chase()
    {
        nav.SetDestination(currTarget.transform.position); // 추격
        anim.SetBool("isRun", true); // 이동 애니메이션

        if (!currTarget) // 적 미 발견시(사망 등의 예외 경우)
        {
            NextState = State.Move; // 본래 목표를 향해 이동
            yield return wait; // 대기
        }
        else if (currTarget) // 적이 존재 할 경우
        {
            if (isTargetAreaDistance()) // 목표물을 놓친 경우
            {
                NextState = State.Move; // 본래 목표를 향해 이동
                currTarget = null; // 기존 목표는 비워준다
                yield return wait; // 대기
            }
            else
            {
                if (isTargetAtkRangeDistance()) // 공격 사정거리 내 진입
                {
                    NextState = State.Attack; // 공격 상태 입력
                    yield return wait; // 대기
                }
                else // 공격 사정거리에는 없지만, 추격 에리어에는 존재
                {
                    NextState = State.Chase; // 계속 추격
                    yield return wait; // 대기
                }
            }
        }

        actionAI(NextState); // 상태 전환
    }


    IEnumerator Act_Attack()
    {
        yield return wait; // 애니메이션 대기

        nav.enabled = false;
        lookAt(); // 목표 대상을 바라본다
        anim.SetBool("isAtk", true); // 공격 애니메이션 실행
        TargetDamageSys.setHpPoint(_atkPoint); // 목표물 hp 차감
        if (TargetDamageSys.getHpPoint() <= 0.0f) currTarget = null; // 목표물이 죽은 경우

        if (!currTarget) // 목표를 잃어버린 경우
        {
            NextState = State.Move;
            yield return wait;
        }
        else if (!isTargetAtkRangeDistance()) // 공격 사정거리 내 없을 경우
        {
            NextState = State.Chase; // 목표물을 다시 추격
            yield return wait;
        }
        else if (TargetDamageSys.getHpPoint() > 0.0f) // 목표물의 HP가 남아 있는 경우 다시 공격
        {
            NextState = State.Attack;
            Rg.constraints = RigidbodyConstraints.FreezeAll; // 움직임 굳히기

            yield return atkDelayWait; // 공격 딜레이 만큼 대기
        }

        actionAI(NextState);
    }

    public void attackSys()
    {
        if (isTargetAtkRangeDistance())
        {
            tempInt++;
            Debug.Log(tempInt + " : 공격 횟수");
            currTarget.GetComponent<damageSys>().setHpPoint(_atkPoint);
        }
    }

    void lookAt()  // 직선상으로 바라보기
    {
        tr.LookAt(currTarget.transform.position);
        tr.rotation = new Quaternion(0, tr.rotation.y, 0, tr.rotation.w);
        //Quaternion rot = Quaternion.LookRotation(currTarget.transform.position - tr.position);
        //rot.x = rot.z = 0.0f;
        //tr.rotation = Quaternion.Slerp(tr.rotation, rot, Time.deltaTime * 10.0f);
    }

    bool isTargetAreaDistance()
    {
        // true = 놓침
        return (Vector3.Distance(currTarget.transform.position, tr.position) > _atkArea);
    }

    bool isTargetAtkRangeDistance()
    {
        // true = 공격범위 내
        return (Vector3.Distance(currTarget.transform.position, tr.position) <= _atkRange);
    }
}
