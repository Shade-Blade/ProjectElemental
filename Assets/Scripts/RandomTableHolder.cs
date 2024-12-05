using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RandomTableHolder : MonoBehaviour
{
    [System.Serializable]
    public class StringListWrapper
    {
        public List<string> list;
    }

    public class DefaultRandomizer : IRandomSource
    {
        public float Get()
        {
            return Random.Range(0, 1.0f);
        }
    }
    public List<RandomTable<string>> tableList;
    [SerializeField]
    public List<StringListWrapper> stringBoxList;

    // Start is called before the first frame update
    void Start()
    {
        //Random.InitState((int)System.DateTime.Now.Ticks);
        MakeTable();
    }

    void MakeTable()
    {
        tableList = new List<RandomTable<string>>();
        for (int i = 0; i < stringBoxList.Count; i++)
        {
            tableList.Add(new RandomTable<string>(stringBoxList[i].list));
        }
    }

    public string Generate()
    {
        MakeTable();
        string output = "";
        for (int i = 0; i < tableList.Count; i++)
        {
            output += tableList[i].Output();
            if (i != tableList.Count-1)
            {
                output += ", ";
            }
        }
        return output;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RandomTableHolder))]
[CanEditMultipleObjects] 
public class RandomTableHolderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate"))
        {
            RandomTableHolder r = (RandomTableHolder)target;
            if (r != null)
            {
                Debug.Log(r.Generate());
            }
        }
        if (GUILayout.Button("Destroy Table"))
        {
            RandomTableHolder r = (RandomTableHolder)target;
            if (r != null)
            {
                r.tableList = null;
            }
        }
    }
}
#endif