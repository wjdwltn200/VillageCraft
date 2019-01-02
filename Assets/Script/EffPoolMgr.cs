using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffPoolMgr : MonoBehaviour {

    public List<ParticleSystem> aliveListEff;

    private void Awake()
    {
        aliveListEff = new List<ParticleSystem>();
    }

    public void addEff(ParticleSystem eff, Vector3 pos)
    {
        ParticleSystem tempEff;
        if (aliveListEff.Count > 0)
        {
            for (int i = 0; i < aliveListEff.Count; i++)
            {
                if (aliveListEff[i].isPlaying) continue;
                tempEff = aliveListEff[i];
                tempEff.transform.position = pos;
                tempEff.Play();
                return;
            }
        }

        tempEff = Instantiate(eff, pos, transform.rotation, transform);
        tempEff.transform.position = pos;
        tempEff.Play();
        aliveListEff.Add(tempEff);
        return;
    }
}
