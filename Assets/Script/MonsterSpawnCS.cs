using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveDate
{
    public int _waveValue;
    public float _setTimer;
    public bool _isBoss;
}

public class MonsterSpawnCS : MonoBehaviour {

    public List<WaveDate> waveDates = new List<WaveDate>();
    public List<Transform> monsterPortal = new List<Transform>();
    private Transform setTr;

    public GameObject mons;
    public GameObject boss;

    public Text _waveTimer;
    public Text _waveNumber;

    public Dictionary<string, GameObject> monsList = new Dictionary<string, GameObject>();

    public void setPortalTr(Transform setTr) { monsterPortal.Add(setTr); }
    private void Start()
    {
        StartCoroutine(setSpawn());

    }

    public IEnumerator setSpawn()
    {
        while (monsterPortal.Count != 4)
        {
            yield return null;
        }

        for (int i = 0; i < waveDates.Count; i++)
        {
            _waveNumber.text = (i + 1).ToString() + "번째 웨이브";
            float tempTime = waveDates[i]._setTimer;
            while (tempTime >= 0.0f)
            {
                tempTime -= Time.deltaTime;
                _waveTimer.text = "적 출현까지.. " + tempTime.ToString("N1");
                yield return null;
            }
            tempTime = 0.0f;
            _waveTimer.text = "적 등장!";

            setTr = monsterPortal[Random.Range(0, monsterPortal.Count)]; // 위치 설정
            for (int j = 0; j < waveDates[i]._waveValue; j++)
            {
                if (waveDates[i]._isBoss)
                {
                    Instantiate(boss, setTr.transform.position, mons.transform.rotation, transform);
                    waveDates[i]._isBoss = false;
                }

                Instantiate(mons, setTr.transform.position, mons.transform.rotation, transform);
                yield return new WaitForSeconds(1.0f);
            }
        }
    }
}
