using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnCS : MonoBehaviour {

    public List<Transform> monsterPortal = new List<Transform>();
    private Transform setTr;

    public GameObject mons;
    public GameObject boss;


    public Dictionary<string, GameObject> monsList = new Dictionary<string, GameObject>();

    public void setPortalTr(Transform setTr) { monsterPortal.Add(setTr); }

   public IEnumerator setSpawn(int spawnValue)
    {
        setTr = monsterPortal[Random.Range(0, monsterPortal.Count)]; // 위치 설정
        for (int i = 0; i < spawnValue; i++)
        {
            GameObject tempMons = Instantiate(mons, setTr.transform.position, mons.transform.rotation, transform);
            yield return new WaitForSeconds(1.0f);
        }
    }
}
