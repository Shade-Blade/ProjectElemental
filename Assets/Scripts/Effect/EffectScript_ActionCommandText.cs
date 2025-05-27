using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectScript_ActionCommandText : MonoBehaviour
{
    public float maxLifetime;
    private float lifetime = 0;

    public float maxScale;
    public float maxScaleTime;
    public float shrinkStartTime;

    public float bezierDuration;
    Vector3 startPos;
    public Vector3 bezierOffset = Vector3.up * 1.0f;
    public Vector3 bezierOffsetB = Vector3.up * 1.5f + Vector3.right * 1.5f;
    public Vector3 bezierOffsetC = Vector3.up * 1.5f + Vector3.left * 1.5f;

    public TMPro.TMP_Text text;

    public int dir = 0; //-1 = left, 0 = center, 1 = right

    public GameObject proxy;

    public BattleHelper.ActionCommandText act;

    private Vector3 backSpriteScale = new Vector3(0.175f, 0.175f, 0.175f);

    public void Setup()
    {
        startPos = transform.position;
        text = GetComponent<TMPro.TMP_Text>();
    }

    //against player: goes left
    //against enemy: goes right
    public void SetDir(bool isPlayer)
    {
        dir = isPlayer ? 1 : -1;
    }

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        text = GetComponent<TMPro.TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        float heaviness = 0.75f;
        float completion = (lifetime / bezierDuration);
        completion *= 0.8f;
        float falsecompletion = completion * (1f + heaviness) + completion * completion * -heaviness;

        float scompletion = (lifetime / maxScaleTime);
        if (scompletion > 1)
        {
            scompletion = 1;
        }
        float scaleheaviness = 3f;
        float scalecompletion = scompletion * (1f + scaleheaviness) + scompletion * scompletion * -scaleheaviness;

        float endscale = (1 - (lifetime - shrinkStartTime) / (maxLifetime - shrinkStartTime));
        if (lifetime < shrinkStartTime)
        {
            endscale = 1;
        }

        //float startdegree = 0;

        switch (dir)
        {
            case 1:
                proxy.transform.position = MainManager.BezierCurve(falsecompletion, startPos, startPos + bezierOffsetB, startPos);
                //startdegree = 45;
                break;
            case -1:
                proxy.transform.position = MainManager.BezierCurve(falsecompletion, startPos, startPos + bezierOffsetC, startPos);
                //startdegree = 315;
                break;
            default:
                proxy.transform.position = MainManager.BezierCurve(falsecompletion, startPos, startPos + bezierOffset, startPos);
                break;
        }

        if (lifetime < maxScaleTime)
        {
            proxy.transform.localScale = Vector3.one * maxScale * (scalecompletion);
        }
        else
        {
            proxy.transform.localScale = new Vector3(1, 1, 1) * maxScale * endscale;
        }

        lifetime += Time.deltaTime;
        if (lifetime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }
}
