using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitTreeSpawner : MonoBehaviour
{
    public List<GameObject> trees;

    // Start is called before the first frame update
    void Start()
    {
        string floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        if (floorNo == null)
        {
            MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, 1.ToString());
            floorNo = 1.ToString();
        }
        int floor = int.Parse(floorNo);

        for (int i = 0; i < trees.Count; i++)
        {
            trees[i].SetActive(false);
        }

        trees[((floor - 1) / 10) % 10].SetActive(true);

        transform.localEulerAngles = Vector3.up * Random.Range(0, 360f);

        transform.localPosition = Vector3.right * transform.localPosition.x + Vector3.forward * Random.Range(-4.5f, 4.5f);
    }
}
