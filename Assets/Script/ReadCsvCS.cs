using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadCsvCS : MonoBehaviour {

    private void Awake()
    {
        startDateLoad();
    }

    void startDateLoad()
    {
        List<Dictionary<string, object>> date = CSVReader.Read("StageDateCSV");

        for (var i = 0; i < date.Count; i++)
        {
            this.GetComponent<MonsterSpawnCS>().waveDates.Add(new WaveDate());
            this.GetComponent<MonsterSpawnCS>().waveDates[i]._setTimer = (int)date[i]["setTimer"];
            this.GetComponent<MonsterSpawnCS>().waveDates[i]._isBoss = (string)date[i]["isBoss"] == "TRUE" ? true : false;
            this.GetComponent<MonsterSpawnCS>().waveDates[i]._waveValue = (int)date[i]["waveValue"];
        }
    }
}
