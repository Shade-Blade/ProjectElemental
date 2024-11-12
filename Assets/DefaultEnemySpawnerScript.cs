using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This will become the standard way to put enemies into a map as it will summon the enemies with the right scripts and sprite IDs (consistent with battle stuff)
public class DefaultEnemySpawnerScript : WorldObject
{
    public WorldEntityData wed; //note: ignores spriteID by substituting in the right one
    public EncounterData encounter;
    public BattleStartArguments bsa = new BattleStartArguments();
    public string areaFlag;
    public int meid;

    //set by Setup
    public WorldEnemyEntity wee;

    public bool blockAutoSpawn;

    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (wed == null || wed.inactive)
        {
            Gizmos.color = Color.black;
        }
        Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.1f);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(transform.position + new Vector3(0f, 0f, 0f), transform.position + new Vector3(0, 0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.25f, 0f), transform.position + new Vector3(0.5f, 0.25f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0f, 0f, 0f), transform.position + new Vector3(-0.5f, -0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0f, 0f, 0f), transform.position + new Vector3(0.5f, -0.5f, 0f));
    }
    public void OnDrawGizmosSelected()
    {
        float thickness = 0.01f;

        Vector3 startPosition = transform.position;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.color = new Color(0.2f, 0.6f, 0.2f, 0.3f); //this is gray, could be anything
        if (startPosition != Vector3.zero)
        {
            Gizmos.matrix = Matrix4x4.TRS(startPosition, transform.rotation, new Vector3(1, thickness, 1));
            Gizmos.DrawSphere(Vector3.zero, wed.wanderRadius * 1.25f);
            Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); //this is gray, could be anything
            Gizmos.matrix = Matrix4x4.TRS(startPosition, transform.rotation, new Vector3(1, thickness, 1));
        }
        else
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, thickness, 1));
            Gizmos.DrawSphere(Vector3.zero, wed.wanderRadius * 1.25f);
            Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); //this is gray, could be anything
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, thickness, 1));
        }

        Gizmos.DrawSphere(Vector3.zero, wed.wanderRadius);

        Gizmos.matrix = oldMatrix;
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        if (!blockAutoSpawn)
        {
            Spawn();
            Destroy(gameObject);
        }
    }

    public virtual void Spawn()
    {
        if (encounter.encounterList.Count == 0)
        {
            Debug.LogError("Default Enemy Spawner has no encounter associated with it");
        }

        //Spawn the enemy using the encounter data
        BattleHelper.EntityID eid = encounter.encounterList[0].GetEntityID();
        Debug.Log(encounter.encounterList[0]);

        wed.spriteID = EnemyBuilder.EntityIDToSpriteID(eid).ToString();
        WorldEnemyEntity wee = SpawnWorldEntity(eid);
        wee.wed = wed;
        wee.encounter = encounter;
        wee.touchEncounter = true;
        wee.bsa = bsa;
        wee.areaFlag = areaFlag;
        wee.meid = meid;
        wee.transform.position = transform.position;
        wee.Resetup();
        this.wee = wee;
    }

    public WorldEnemyEntity SpawnWorldEntity(BattleHelper.EntityID eid)
    {
        GameObject outputObject = null;
        //WorldEnemyEntity wee = null;
        /*
        switch (eid)
        {

        }
        */

        BattleEntityData bed = BattleEntityData.GetBattleEntityData(eid);

        if ((bed.entityProperties & (ulong)BattleHelper.EntityProperties.Airborne) != 0)
        {
            outputObject = Instantiate(Resources.Load<GameObject>("Overworld/DefaultEnemies/Enemy_FlyingChaser"), mapScript.transform);
        } else
        {
            outputObject = Instantiate(Resources.Load<GameObject>("Overworld/DefaultEnemies/Enemy_Chaser"), mapScript.transform);
        }

        return outputObject.GetComponent<WorldEnemyEntity>();
    }
}
