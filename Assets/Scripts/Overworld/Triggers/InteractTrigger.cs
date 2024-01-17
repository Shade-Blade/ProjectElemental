using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractTrigger : MonoBehaviour
{
    public Vector3 relativeEvaluationPoint; //relative to transform position

    public float dotProductBonus;

    public bool active = false;

    //Unfortunately due to technical limitations this doesn't show up in inspector
    //So a workaround is to just make special interact triggers for different use cases I guess
    //  (although they will usually just bypass the interactable field)
    public IInteractable interactable;

    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position + relativeEvaluationPoint, 0.1f);

        Gizmos.DrawLine(transform.position + relativeEvaluationPoint + new Vector3(-0.2f, 0.7f, 0f), transform.position + relativeEvaluationPoint + new Vector3(0, 0f, 0f));
        Gizmos.DrawLine(transform.position + relativeEvaluationPoint + new Vector3(0, 0f, 0f), transform.position + relativeEvaluationPoint + new Vector3(0.2f, 0.7f, 0f));
        Gizmos.DrawLine(transform.position + relativeEvaluationPoint + new Vector3(0.2f, 0.7f, 0f), transform.position + relativeEvaluationPoint + new Vector3(-0.2f, 0.7f, 0f));
    }

    public void OnTriggerEnter(Collider other)
    {
        ProcessTrigger(other);
    }

    public void OnTriggerStay(Collider other)
    {
        ProcessTrigger(other);
    }

    public void OnTriggerExit(Collider other)
    {
        WorldPlayer wp = other.transform.GetComponent<WorldPlayer>();
        //Debug.Log(other.transform.GetComponent<WorldPlayer>());
        if (wp != null)
        {
            //?
            active = false;
        }
    }

    public void ProcessTrigger(Collider other)
    {
        WorldPlayer wp = other.transform.GetComponent<WorldPlayer>();
        //Debug.Log(other.transform.GetComponent<WorldPlayer>());
        if (wp != null)
        {
            //you can only interact with stuff in a neutral state
            if (wp.InNeutralState())
            {
                active = true;
                MainManager.Instance.AddInteractTrigger(this);
            }
        }
    }

    public void FixedUpdate()
    {
        if (!active)
        {
            //kick ourselves out of the list
            MainManager.Instance.RemoveInteractTrigger(this);
        }
        active = false;
    }

    public Vector3 GetEvalPoint()
    {
        return transform.position + relativeEvaluationPoint;
    }

    public float DotProductBonus()
    {
        //makes it easier to get it from behind
        return dotProductBonus;
    }

    //npcs turn towards you and don't move if their interact trigger is active
    public bool GetActive()
    {
        return active;
    }

    public virtual void Interact()
    {
        if (interactable != null)
        {
            interactable.Interact();
        } else
        {
            Debug.Log("Interact");
            StartCoroutine(TestCutscene());
        }
    }

    public IEnumerator TestCutscene()
    {
        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(MainManager.Instance.testTextFile, 0));

        string pdParse = MainManager.Instance.playerData.ToString();
        Debug.Log(pdParse);
        PlayerData pd = PlayerData.Parse(pdParse.Split("\n"), 0, out int _);
        Debug.Log(pd);
        Debug.Log(pdParse.Equals(pd.ToString()));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;

        //FormattedString.ParseArg(menuResult, "arg1");

        //string var = FormattedString.ParseArg(menuResult, "arg");

        string[] vars = new string[] { menuResult };

        //Debug.Log("B");

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(MainManager.Instance.testTextFile, 6, null, vars));
    }
}

public interface IInteractable
{
    public void Interact();
}