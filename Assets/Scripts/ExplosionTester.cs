using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionTester : MonoBehaviour //WorldObject
{
    public GameObject exploder;
    float lifetime;

    public float timePer;
    public int count;
    public bool random;
    public int power;

    public void Update() //WorldUpdate()
    {
        lifetime += Time.deltaTime;
        if (lifetime > timePer)
        {
            lifetime -= timePer;

            GameObject g;
            for (int i = 0; i < count; i++)
            {
                g = Instantiate(exploder, MainManager.Instance.transform);

                //EffectScript_Fountain es_f = g.GetComponent<EffectScript_Fountain>();
                //es_f.Setup(1, power);

                if (random)
                {
                    g.transform.position = transform.position + Vector3.up * Random.Range(0, 1f) + Vector3.right * Random.Range(-1f, 1f) + Vector3.forward * Random.Range(-1f, 1f);
                    g.transform.rotation = Random.rotation;
                } else
                {
                    g.transform.position = transform.position;
                }
            }
        }
    }
}
