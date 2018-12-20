using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSys : MonoBehaviour {

    public GameObject currTarget;
    private Transform tr;
    private BuildingData buildingData;
    private MonsAI MonsStats;
    private PlayerData playerData;
    private bulletMgr bulletMgr;

    public GameObject bulletGO;
    public GameObject shotPos;

    private State currState;
    private State NextState;
    private eBuildingType buildingType;
    public bool isOrigin = true;

    private void Awake()
    {
        tr = GetComponent<Transform>();
        playerData = GameObject.Find("PlayerMgr").GetComponent<PlayerData>();
        bulletMgr = GameObject.Find("BulletManager").GetComponent<bulletMgr>();
        buildingData = GetComponent<BuildingData>();
        buildingType = GetComponent<BuildingData>().ebuildingType;
    }

    private void Start()
    {
        switch (buildingType)
        {
            case eBuildingType.NULL:
                break;
            case eBuildingType.WALL:
                break;
            case eBuildingType.FARM:

                break;
            case eBuildingType.WINDMILL:
                if (!isOrigin)
                    actionAI(State.Tax);
                break;
            case eBuildingType.WELL:
                break;
            case eBuildingType.TOWER:
                if (!isOrigin)
                    actionAI(State.Idle);
                else
                    GetComponent<SphereCollider>().enabled = false;
                break;
            default:
                break;
        }
    }
    
    void actionAI(State act)
    {
        NextState = act;

        StopAllCoroutines();
        switch (buildingType)
        {
            case eBuildingType.NULL:
                break;
            case eBuildingType.WALL:
                break;
            case eBuildingType.FARM:
                break;
            case eBuildingType.WINDMILL:
                break;
            case eBuildingType.WELL:
                break;
            case eBuildingType.TOWER:
                GetComponent<SphereCollider>().enabled = true;
                break;
            default:
                break;
        }

        switch (act)
        {
            case State.Idle:
                StartCoroutine(Search());
                break;
            case State.Move:
                break;
            case State.Attack:
                StartCoroutine(TargetAtk(buildingData._atkDelay));
                break;
            case State.Tax:
                StartCoroutine(TaxSys(buildingData._taxDelay));
                break;
            default:
                break;
        }
    }

    IEnumerator Search()
    {
        switch (buildingType)
        {
            case eBuildingType.NULL:
                break;
            case eBuildingType.WALL:
                break;
            case eBuildingType.FARM:
                break;
            case eBuildingType.WINDMILL:
                break;
            case eBuildingType.WELL:
                break;
            case eBuildingType.TOWER:
                if (currTarget == null)
                {
                    NextState = State.Idle;
                }
                else if (currTarget != null)
                {
                    NextState = State.Attack;
                }

                yield return new WaitForSeconds(0.1f);
                actionAI(NextState);
                break;
            default:
                break;
        }
    }

    IEnumerator TargetAtk(float delay)
    {
        switch (buildingType)
        {
            case eBuildingType.NULL:
                break;
            case eBuildingType.WALL:
                break;
            case eBuildingType.FARM:
                break;
            case eBuildingType.WINDMILL:
                break;
            case eBuildingType.WELL:
                break;
            case eBuildingType.TOWER:
                if (!currTarget)
                {
                    NextState = State.Idle;
                    actionAI(NextState);
                }

                if (currTarget == null)
                {
                    NextState = State.Idle;
                }
                else if (MonsStats._currHP > 0.0f)
                {
                    bulletMgr.addArrow(bulletGO, shotPos.transform, currTarget.transform, buildingData._rangeRadius, buildingData._atkPoint);
                }

                GetComponent<SphereCollider>().enabled = false;
                yield return new WaitForSeconds(delay);

                isTargetLost();
                actionAI(NextState);
                break;
            default:
                break;
        }
    }

    IEnumerator TaxSys(float delay)
    {
        NextState = State.Tax;
        playerData.currGold += (int)buildingData._taxPoint;
        yield return new WaitForSeconds(delay);
        actionAI(NextState);
    }


    void isTargetLost()
    {
        if (currTarget != null && Vector3.Distance(tr.position, currTarget.transform.position) > buildingData._atkRange)
        {
            currTarget = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (currTarget == null && other.gameObject.layer == 9)
        {
            currTarget = other.gameObject;
            MonsStats = currTarget.GetComponent<MonsAI>();
        }
    }
}
