using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitEnemySpawnerScript : DefaultEnemySpawnerScript
{
    public int floor;

    //Kill the enemy to break the obstacle
    public GameObject obstacle;

    public override void Start()
    {
        string floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        if (floorNo == null)
        {
            MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, 1.ToString());
        }
        floor = int.Parse(floorNo);

        if (!blockAutoSpawn)
        {
            Spawn();
        }
    }

    public override void Spawn()
    {
        //Decide the encounter early
        float weirdness = 0;
        if (floor < 20)
        {
            weirdness = 0;
        } else
        {
            weirdness = (floor - 20) / 40f;
        }
        encounter = EncounterData.GeneratePitEncounter(floor, weirdness);

        base.Spawn();

        if (wee is WorldEnemy_FlyingChaser)
        {
            wee.transform.position = transform.position + Vector3.up * 0.75f;
        }
    }

    public override void WorldUpdate()
    {
        if (wee == null)
        {
            Destroy(obstacle);
            Destroy(gameObject);
        }
    }
}
