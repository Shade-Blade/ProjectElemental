using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSightDotScript : MonoBehaviour
{
    public float lifetime;
    private float time;
    public float startScale;
    public Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = Vector3.one * startScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.one * startScale * (1 - (time / lifetime));
        time += Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        if (time > lifetime)
        {
            Destroy(gameObject);
        }
    }
}
