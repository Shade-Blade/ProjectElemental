using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestScript : WorldObject, IInteractable
{
    public WorldCollectibleScript collectible;

    public InteractTrigger it;

    public bool open;

    public GameObject chestTop;

    public float openDuration = 0.2f;

    public void Start()
    {
        it.interactable = this;
        collectible.gameObject.SetActive(false);
    }

    public override void WorldUpdate()
    {
        if (!open && collectible == null)
        {
            InstaOpen();
        }
    }

    public void InstaOpen()
    {
        open = true;
        chestTop.transform.localEulerAngles = Vector3.right * 50;
        if (it.gameObject != null)
        {
            Destroy(it.gameObject);
        }
    }

    public void Interact()
    {
        StartCoroutine(MainManager.Instance.ExecuteCutscene(OpenCutscene()));
    }

    public IEnumerator OpenCutscene()
    {
        if (open)
        {
            InstaOpen();
            yield return null;
        }

        open = true;
        Destroy(it.gameObject);

        //open chest
        collectible.gameObject.SetActive(true);
        float time = 0;

        Vector3 pointA = transform.position + 0.3f * Vector3.up;
        Vector3 pointC = WorldPlayer.Instance.transform.position + Vector3.up * 0.8f;
        Vector3 pointB = (pointA + pointC) / 2 + Vector3.up * (pointC.y - pointA.y) * 0.4f;

        while (time < openDuration)
        {
            chestTop.transform.localEulerAngles = Vector3.right * 50 * (time / openDuration);
            time += Time.deltaTime;

            yield return null;
        }

        chestTop.transform.localEulerAngles = Vector3.right * 50;

        while (time < openDuration * 2)
        {
            collectible.transform.position = MainManager.BezierCurve(((time - openDuration) / openDuration), pointA, pointB, pointC);
            time += Time.deltaTime;

            yield return null;
        }
        collectible.transform.position = pointC;

        //pick up the item and destroy it        
        yield return collectible.PickupCoroutine();
    }
}
