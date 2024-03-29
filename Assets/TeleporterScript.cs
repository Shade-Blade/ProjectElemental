using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterScript : InteractTrigger
{
    public Vector3[] flyPath;
    public bool backwardsFly;
    public float duration;
    public GameObject teleporterDot;

    public override void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position + relativeEvaluationPoint, 0.1f);

        Gizmos.DrawLine(transform.position + relativeEvaluationPoint + new Vector3(-0.2f, 0.7f, 0f), transform.position + relativeEvaluationPoint + new Vector3(0, 0f, 0f));
        Gizmos.DrawLine(transform.position + relativeEvaluationPoint + new Vector3(0, 0f, 0f), transform.position + relativeEvaluationPoint + new Vector3(0.2f, 0.7f, 0f));
        Gizmos.DrawLine(transform.position + relativeEvaluationPoint + new Vector3(0.2f, 0.7f, 0f), transform.position + relativeEvaluationPoint + new Vector3(-0.2f, 0.7f, 0f));

        for (int i = 0; i < flyPath.Length; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(flyPath[i], 0.1f);
            if (i == 0)
            {
            }
            else
            {
                Gizmos.DrawLine(flyPath[i - 1], flyPath[i]);
            }
        }

        for (int i = 0; i < 30; i++)
        {
            float c1 = i / 30f;
            float c2 = (i + 1) / 30f;
            if (i % 2 == 0)
            {
                Gizmos.color = Color.white;
            }
            else
            {
                Gizmos.color = Color.black;
            }
            Gizmos.DrawLine(MainManager.BezierCurve(c1, flyPath), MainManager.BezierCurve(c2, flyPath));
        }
    }

    public override void Interact()
    {
        Debug.Log("Interact");
        StartCoroutine(TeleportCutscene());
    }

    public IEnumerator TeleportCutscene()
    {
        WorldPlayer wp = WorldPlayer.Instance;

        wp.gameObject.SetActive(false);
        for (int i = 0; i < wp.followers.Count; i++)
        {
            wp.followers[i].gameObject.SetActive(false);
        }

        float initialTime = Time.time;
        Vector3 initialPos = transform.position;
        float completion = (Time.time - initialTime) / duration;
        //make a bezier curve

        GameObject dot = Instantiate(teleporterDot, transform);

        while (completion < 1)
        {
            if (backwardsFly)
            {
                wp.transform.position = MainManager.BezierCurve(1 - completion, flyPath);
            }
            else
            {
                wp.transform.position = MainManager.BezierCurve(completion, flyPath);
            }

            dot.transform.position = wp.transform.position;
            completion = (Time.time - initialTime) / duration;
            yield return null;
        }

        Destroy(dot);

        //force position = endpoint (prevent lag glitches)
        wp.transform.position = flyPath[flyPath.Length - 1];

        wp.gameObject.SetActive(true);
        for (int i = 0; i < wp.followers.Count; i++)
        {
            wp.followers[i].gameObject.SetActive(true);
        }
        wp.FollowerWarpSetState();

    }
}
