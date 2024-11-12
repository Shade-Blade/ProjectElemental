using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    public Vector3 frontSidePos;
    public Vector3 backSidePos;
    public float rotationDir;
    public float sideRotationDir;
    public Collider doorCollider;
    public Transform doorModelTransform;
    public float duration;

    public bool insideActive;

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(1,1,0,1);
        Gizmos.DrawSphere(frontSidePos, 0.2f);
        Gizmos.DrawSphere(frontSidePos + transform.forward * -0.5f, 0.1f);

        Gizmos.color = new Color(0.5f, 0.5f, 0, 1);
        Gizmos.DrawSphere(backSidePos, 0.2f);
        Gizmos.DrawSphere(backSidePos + transform.forward * 0.5f, 0.1f);
    }

    public void SetInside() //if you start inside a building then the door should be in its inside state at the start
    {
        insideActive = true;
        doorModelTransform.transform.localEulerAngles = Vector3.right * sideRotationDir * 90;
    }

    public IEnumerator FrontSideOpen()
    {
        Debug.Log("front side start");
        bool playerAtPos = false;
        List<bool> followerAtPos = new List<bool>();
        bool doorAtPos = false;
        
        IEnumerator PlayerMove(WorldPlayer wp)
        {
            yield return wp.ScriptedMoveTo(frontSidePos);
            playerAtPos = true;
        }
        IEnumerator FollowerMove(WorldFollower wf, int index)
        {
            yield return wf.ScriptedMoveTo(frontSidePos + transform.forward * -0.5f);
            followerAtPos[index] = true;
        }
        IEnumerator PlayerMoveInside(WorldPlayer wp)
        {
            yield return wp.ScriptedMoveTo(backSidePos);
            playerAtPos = true;
        }
        IEnumerator FollowerMoveInside(WorldFollower wf, int index)
        {
            yield return wf.ScriptedMoveTo(backSidePos);
            followerAtPos[index] = true;
        }
        void FollowerReset()
        {
            for (int i = 0; i < followerAtPos.Count; i++)
            {
                followerAtPos[i] = false;
            }
        }
        bool FollowerCheck()
        {
            for (int i = 0; i < followerAtPos.Count; i++)
            {
                if (!followerAtPos[i])
                {
                    return false;
                }
            }
            return true;
        }
        IEnumerator DoorMove()
        {
            doorModelTransform.transform.localEulerAngles = Vector3.zero;
            float lifetime = 0;
            while (lifetime < duration)
            {
                lifetime += Time.deltaTime;
                doorModelTransform.transform.localEulerAngles = Vector3.up * rotationDir * (lifetime / duration) * 90;
                yield return null;
            }
            doorModelTransform.transform.localEulerAngles = Vector3.up * rotationDir * 90;
            doorAtPos = true;
        }
        IEnumerator DoorMoveB()
        {
            doorModelTransform.transform.localEulerAngles = Vector3.zero;
            float lifetime = 0;
            while (lifetime < duration)
            {
                lifetime += Time.deltaTime;
                doorModelTransform.transform.localEulerAngles = Vector3.up * rotationDir * (1 - (lifetime / duration)) * 90;
                yield return null;
            }
            doorModelTransform.transform.localEulerAngles = Vector3.zero;
            doorAtPos = true;
        }
        IEnumerator DoorMoveC()
        {
            doorModelTransform.transform.localEulerAngles = Vector3.zero;
            float lifetime = 0;
            while (lifetime < duration)
            {
                lifetime += Time.deltaTime;
                doorModelTransform.transform.localEulerAngles = Vector3.right * sideRotationDir * (lifetime / duration) * 90;
                yield return null;
            }
            doorModelTransform.transform.localEulerAngles = Vector3.right * sideRotationDir * 90;
            doorAtPos = true;
        }

        StartCoroutine(PlayerMove(WorldPlayer.Instance));
        for (int i = 0; i < WorldPlayer.Instance.followers.Count; i++)
        {
            followerAtPos.Add(false);
            StartCoroutine(FollowerMove(WorldPlayer.Instance.followers[i], i));
        }
        StartCoroutine(DoorMove());

        yield return new WaitUntil(() => (playerAtPos && FollowerCheck() && doorAtPos));
        doorCollider.enabled = false;

        playerAtPos = false;
        FollowerReset();
        doorAtPos = false;

        StartCoroutine(PlayerMoveInside(WorldPlayer.Instance));
        for (int i = 0; i < WorldPlayer.Instance.followers.Count; i++)
        {
            StartCoroutine(FollowerMoveInside(WorldPlayer.Instance.followers[i], i));
        }

        yield return new WaitUntil(() => (playerAtPos && FollowerCheck()));

        yield return StartCoroutine(DoorMoveB());
        yield return StartCoroutine(DoorMoveC());

        doorCollider.enabled = true;
        insideActive = true;
        Debug.Log("front side end");
    }
    public IEnumerator BackSideOpen()
    {
        bool playerAtPos = false;
        List<bool> followerAtPos = new List<bool>();
        bool doorAtPos = false;

        IEnumerator PlayerMove(WorldPlayer wp)
        {
            yield return wp.ScriptedMoveTo(backSidePos);
            playerAtPos = true;
        }
        IEnumerator FollowerMove(WorldFollower wf, int index)
        {
            yield return wf.ScriptedMoveTo(backSidePos + transform.forward * 0.5f);
            followerAtPos[index] = true;
        }
        IEnumerator PlayerMoveInside(WorldPlayer wp)
        {
            yield return wp.ScriptedMoveTo(frontSidePos);
            playerAtPos = true;
        }
        IEnumerator FollowerMoveInside(WorldFollower wf, int index)
        {
            yield return wf.ScriptedMoveTo(frontSidePos);
            followerAtPos[index] = true;
        }
        void FollowerReset()
        {
            for (int i = 0; i < followerAtPos.Count; i++)
            {
                followerAtPos[i] = false;
            }
        }
        bool FollowerCheck()
        {
            for (int i = 0; i < followerAtPos.Count; i++)
            {
                if (!followerAtPos[i])
                {
                    return false;
                }
            }
            return true;
        }
        IEnumerator DoorMove()
        {
            doorModelTransform.transform.localEulerAngles = Vector3.zero;
            float lifetime = 0;
            while (lifetime < duration)
            {
                lifetime += Time.deltaTime;
                doorModelTransform.transform.localEulerAngles = Vector3.up * rotationDir * (lifetime / duration) * 90;
                yield return null;
            }
            doorModelTransform.transform.localEulerAngles = Vector3.up * rotationDir * 90;
            doorAtPos = true;
        }
        IEnumerator DoorMoveB()
        {
            doorModelTransform.transform.localEulerAngles = Vector3.zero;
            float lifetime = 0;
            while (lifetime < duration)
            {
                lifetime += Time.deltaTime;
                doorModelTransform.transform.localEulerAngles = Vector3.up * rotationDir * (1 - (lifetime / duration)) * 90;
                yield return null;
            }
            doorModelTransform.transform.localEulerAngles = Vector3.zero;
            doorAtPos = true;
        }
        IEnumerator DoorMoveC()
        {
            doorModelTransform.transform.localEulerAngles = Vector3.zero;
            float lifetime = 0;
            while (lifetime < duration)
            {
                lifetime += Time.deltaTime;
                doorModelTransform.transform.localEulerAngles = Vector3.right * sideRotationDir * (1 - (lifetime / duration)) * 90;
                yield return null;
            }
            doorModelTransform.transform.localEulerAngles = Vector3.zero;
            doorAtPos = true;
        }

        doorAtPos = false;
        yield return StartCoroutine(DoorMoveC());

        StartCoroutine(PlayerMove(WorldPlayer.Instance));
        for (int i = 0; i < WorldPlayer.Instance.followers.Count; i++)
        {
            followerAtPos.Add(false);
            StartCoroutine(FollowerMove(WorldPlayer.Instance.followers[i], i));
        }
        StartCoroutine(DoorMove());

        yield return new WaitUntil(() => (playerAtPos && FollowerCheck() && doorAtPos));
        doorCollider.enabled = false;

        playerAtPos = false;
        FollowerReset();
        doorAtPos = false;

        StartCoroutine(PlayerMoveInside(WorldPlayer.Instance));
        for (int i = 0; i < WorldPlayer.Instance.followers.Count; i++)
        {
            StartCoroutine(FollowerMoveInside(WorldPlayer.Instance.followers[i], i));
        }

        yield return new WaitUntil(() => (playerAtPos && FollowerCheck()));
        yield return StartCoroutine(DoorMoveB());

        doorCollider.enabled = true;
        insideActive = true;
    }
}
