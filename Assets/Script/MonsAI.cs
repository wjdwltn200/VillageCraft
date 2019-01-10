using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum State
{
   Idle,
   Move,
   Attack,
   Search,
   Tax,
   Chase
}

public class MonsAI : MonoBehaviour {

    public GameObject target;
    public EffPoolMgr effPoolMgr;
    public ParticleSystem currParticle;

    public GameObject currTarget;
    private damageSys TargetDamageSys;

    public SphereCollider currHitCol;

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
    public float _moveSpeed;
    public float _atkPoint = 0.0f;
    public float _atkArea = 0.0f;
    public float _atkDelay = 0.0f;
    public float _atkRange = 0.0f;
    public float _rangeRadius = 0.0f;

    public ParticleSystem DieEff;
    private WaitForSeconds wait;
    private WaitForSeconds atkDelayWait;
    private SphereCollider targetCheck;

    public int tempInt = 0;

    public float ViewAngle;
    public float ViewDistance;
    public LayerMask TargetMask;
    public LayerMask EnemyMask;
    public LayerMask ObstacleMask;

    private float setAngle = 0.0f;
    private float AngleTime = 0.0f;
    private int AngleVlaue = 0;

    private void Awake()
    {
        tr = GetComponent<Transform>();
        Rg = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        wait = new WaitForSeconds(0.1f);
        atkDelayWait = new WaitForSeconds(_atkDelay);

        //targetCheck = GetComponentInChildren<SphereCollider>();
        //targetCheck.radius = _atkArea * 0.1f;

        // 능력치 초기화
        _currHP = _maxHP;
    }

    void Start () {
        transform.GetChild(1).gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;
        transform.GetChild(1).gameObject.transform.GetChild(1).GetComponent<Image>().enabled = false;
        transform.GetChild(1).gameObject.transform.GetChild(2).GetComponent<Image>().enabled = false;

        Instantiate(DieEff, transform);
        lookAt(target.transform.position);
        ViewDistance = GetComponent<SphereCollider>().radius;

        currState = State.Move;
        StartCoroutine(Act_Move());
    }

    private void Update()
    {
        DrawView();             //Scene뷰에 시야범위 그리기
        AngleTime += Time.deltaTime;
        if (AngleTime >= 5.0f)
        {
            lookAt(target.transform.position);
            AngleTime = 0.0f;
        }

        if (_currHP <= 0.0f)
            Die();
    }

    public Vector3 DirFromAngle(float angleInDegrees)
    {
        //탱크의 좌우 회전값 갱신
        angleInDegrees += transform.eulerAngles.y;
        //경계 벡터값 반환
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public void DrawView()
    {
        Vector3 leftBoundary = DirFromAngle(-ViewAngle / 2);
        Vector3 rightBoundary = DirFromAngle(ViewAngle / 2);
        Debug.DrawLine(tr.position, tr.position + leftBoundary * ViewDistance, Color.blue);
        Debug.DrawLine(tr.position, tr.position + rightBoundary * ViewDistance, Color.blue);

    }

    public bool FindVisibleTargets()
    {
        Collider[] targets = Physics.OverlapSphere(tr.position, ViewDistance, EnemyMask);

        for (int i = 0; i < targets.Length; i++)
        {
            Transform target = targets[i].transform;
            Vector3 dirToTarget = (target.position - tr.position).normalized;

            if (Vector3.Dot(tr.forward, dirToTarget) > Mathf.Cos((ViewAngle / 2) * Mathf.Deg2Rad))
            {
                //Ray ray = new Ray();
                //ray.origin = new Vector3(tr.position.x, tr.position.y + 1.5f, tr.position.z);
                //ray.direction = (target.position - tr.position).normalized;
                //Debug.DrawRay(tr.position, ray.direction * ViewDistance, Color.red);

                RaycastHit hit;
                Vector3 pos = new Vector3(tr.position.x, tr.position.y + 0.5f, tr.position.z);
                if (Physics.Raycast(pos, dirToTarget, out hit, ViewDistance - 1.0f, EnemyMask))
                {
                    Debug.DrawLine(tr.position, target.position, Color.red);
                    return true;
                }
            }
        }
        return false;
    }

    public void Die()
    {
        StopAllCoroutines();
        effPoolMgr.addEff(currParticle, transform.position);
        Destroy(gameObject);
    }

    void actionAI (State eMove)
    {
        currState = eMove;

        Rg.constraints = RigidbodyConstraints.None;
        Rg.constraints = RigidbodyConstraints.FreezeRotation;

        anim.SetBool("isRun", false);
        anim.SetBool("isAtk", false);

        switch (currState)
        {
            case State.Search:
                StartCoroutine(Act_Search());
                break;
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



    IEnumerator Act_Move() // 이동
    {
        anim.SetBool("isRun", true);

        if ((FindVisibleTargets()))
        {
            currState = State.Search; // 시야에 걸리면 다른 회전(정지)
        }
        else transform.Translate(Vector3.forward * (_moveSpeed * Time.deltaTime), Space.Self); // 무조건 전진

        if (!currTarget) targetSet();
        else currState = State.Chase;

        yield return null;
        actionAI(currState);
    }


    public void targetSet()
    {
        Collider[] targets = Physics.OverlapSphere(tr.position, ViewDistance, TargetMask);
        if (targets.Length != 0) currTarget = targets[0].transform.gameObject;
        if (currTarget) currState = State.Chase;
    }

    IEnumerator Act_Search() // 검색
    {
        AngleVlaue = Random.Range(0, 2);
        while (FindVisibleTargets())
        {
            if (AngleVlaue == 0)
            {
                transform.rotation = new Quaternion(tr.rotation.x, tr.rotation.y + 0.1f, tr.rotation.x, tr.rotation.w);
            }
            else if (AngleVlaue == 1)
            {
                transform.rotation = new Quaternion(tr.rotation.x, tr.rotation.y - 0.1f, tr.rotation.x, tr.rotation.w);
            }
            yield return null;
        }
        AngleVlaue = 0;

        currState = State.Move;
        if(!currTarget) targetSet();

        yield return null;
        actionAI(currState);
    }

    IEnumerator Act_Chase() // 추적
    {
        if (currTarget) lookAt(currTarget.transform.position);

        currState = State.Move;

        if (currTarget &&
            Vector3.Distance(tr.position, currTarget.transform.position) > (ViewDistance * 2.0f))
        {
            currTarget = null;
        }else if (currTarget &&
            Vector3.Distance(tr.position, currTarget.transform.position) <= _atkRange)
        {
            currState = State.Attack;
        }


        yield return null;
        actionAI(currState);
    }



    IEnumerator Act_Attack() // 공격
    {
        anim.SetBool("isAtk", true);
        if (currTarget)
        {
            lookAt(currTarget.transform.position);
            currState = State.Chase;
        }

        yield return new WaitForSeconds(2.0f);
        actionAI(currState);
    }


    //IEnumerator Act_Move()
    //{
    //    anim.SetBool("isRun", true); // 이동 애니메이션

    //    if (currTarget == null) // 적 미 발견시
    //    {
    //        NextState = State.Move; // 계속 이동
    //        yield return wait; // 대기
    //    }
    //    else if (currTarget != null) // 적 발견시
    //    {
    //        NextState = State.Chase; // 추적으로 변경
    //        yield return wait; // 대기
    //    }

    //    actionAI(NextState);  //  행동 전환
    //}

    //IEnumerator Act_Chase()
    //{
    //    anim.SetBool("isRun", true); // 이동 애니메이션

    //    if (!currTarget) // 적 미 발견시(사망 등의 예외 경우)
    //    {
    //        NextState = State.Move; // 본래 목표를 향해 이동
    //        yield return wait; // 대기
    //    }
    //    else if (currTarget) // 적이 존재 할 경우
    //    {
    //        if (isTargetAreaDistance()) // 목표물을 놓친 경우
    //        {
    //            NextState = State.Move; // 본래 목표를 향해 이동
    //            currTarget = null; // 기존 목표는 비워준다
    //            yield return wait; // 대기
    //        }
    //        else
    //        {
    //            if (isTargetAtkRangeDistance()) // 공격 사정거리 내 진입
    //            {
    //                NextState = State.Attack; // 공격 상태 입력
    //                yield return wait; // 대기
    //            }
    //            else // 공격 사정거리에는 없지만, 추격 에리어에는 존재
    //            {
    //                NextState = State.Chase; // 계속 추격
    //                yield return wait; // 대기
    //            }
    //        }
    //    }

    //    actionAI(NextState); // 상태 전환
    //}


    //IEnumerator Act_Attack()
    //{
    //    yield return wait; // 애니메이션 대기

    //    lookAt(); // 목표 대상을 바라본다
    //    anim.SetBool("isAtk", true); // 공격 애니메이션 실행

    //    if (!currTarget) // 목표를 잃어버린 경우
    //    {
    //        NextState = State.Move;
    //    }
    //    else if (!isTargetAtkRangeDistance()) // 공격 사정거리 내 없을 경우
    //    {
    //        NextState = State.Chase; // 목표물을 다시 추격
    //    }
    //    else if (TargetDamageSys.getHpPoint() > 0.0f) // 목표물의 HP가 남아 있는 경우 다시 공격
    //    {
    //        NextState = State.Attack;
    //        Rg.constraints = RigidbodyConstraints.FreezeAll; // 움직임 굳히기

    //        yield return atkDelayWait; // 공격 딜레이 만큼 대기
    //    }

    //    actionAI(NextState);
    //}

    public float trDis(Vector3 pos)
    {
        return Vector3.Distance(tr.position, pos);
    }

    public void attackSys()
    {
        if (isTargetAtkRangeDistance())
        {
            currTarget.GetComponent<damageSys>().setHpPoint(_atkPoint);
        }
    }

    void lookAt(Vector3 targerVec)  // 직선상으로 바라보기
    {
        tr.LookAt(targerVec);
        tr.rotation = new Quaternion(0, tr.rotation.y, 0, tr.rotation.w);
    }

    bool isTargetAreaDistance()
    {
        // true = 놓침
        if (!currTarget) return true;
        return (Vector3.Distance(currTarget.transform.position, tr.position) > _atkArea);
    }

    bool isTargetAtkRangeDistance()
    {
        // true = 공격범위 내
        if (!currTarget) return false;
        return (Vector3.Distance(currTarget.transform.position, tr.position) <= _atkRange);
    }

}
