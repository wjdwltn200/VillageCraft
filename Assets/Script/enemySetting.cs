using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class enemySetting : MonoBehaviour {
    private MonsterSpawnCS monsterSpawnCS;

    private void Awake()
    {
        monsterSpawnCS = GameObject.Find("MonsterSpawn").GetComponent<MonsterSpawnCS>();
    }

    public void setMons()
    {
        StartCoroutine(monsterSpawnCS.setSpawn(10));
    }
}
