using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_SectionStatus_MenuNode : MonoBehaviour
{
    public Pause_SectionStatus.MenuNodeType menuNodeType;

    public Pause_SectionStatus_MenuNode up;
    public Pause_SectionStatus_MenuNode down;
    public Pause_SectionStatus_MenuNode left;
    public Pause_SectionStatus_MenuNode right;

    public Pause_SectionStatus_MenuNode upright;
    public Pause_SectionStatus_MenuNode downright;
    public Pause_SectionStatus_MenuNode upleft;
    public Pause_SectionStatus_MenuNode downleft;

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, 10f);
        Gizmos.color = Color.gray;

        List<Pause_SectionStatus_MenuNode> list = new List<Pause_SectionStatus_MenuNode>
        {
            up,
            upright,
            right,
            downright,
            down,
            downleft,
            left,
            upleft
        };

        List<Color> colorList = new List<Color>
        {
            new Color(1f,0.5f,0.5f,1),
            new Color(1f,0.75f,0.5f,1),
            new Color(1f,1f,0.5f,1),
            new Color(0.5f,1f,0.5f,1),
            new Color(0.5f,1f,1f,1),
            new Color(0.5f,0.5f,1f,1),
            new Color(0.75f,0.5f,1f,1),
            new Color(1f,0.5f,1f,1),
        };

        int index = 0;
        foreach (Pause_SectionStatus_MenuNode mn in list)
        {
            Gizmos.color = colorList[index];
            index++;
            if (mn != null)
            {
                Gizmos.DrawSphere(mn.transform.position, 5f);
                Gizmos.DrawLine(transform.position, mn.transform.position);
            }
        }
    }

    public void RecalculateDirections(List<Pause_SectionStatus_MenuNode> list)
    {
        //the same thing 4 times
        //may be able to reuse work but ehh
        up = RecalculateDirection(list, Vector3.up);
        down = RecalculateDirection(list, Vector3.down);
        left = RecalculateDirection(list, Vector3.left);
        right = RecalculateDirection(list, Vector3.right);

        upright = RecalculateDirection(list, (Vector3.up + Vector3.right).normalized);
        downright = RecalculateDirection(list, (Vector3.down + Vector3.right).normalized);
        upleft = RecalculateDirection(list, (Vector3.up + Vector3.left).normalized);
        downleft = RecalculateDirection(list, (Vector3.down + Vector3.left).normalized);
    }

    public Pause_SectionStatus_MenuNode RecalculateDirection(List<Pause_SectionStatus_MenuNode> list, Vector3 direction)
    {
        //Very bad time complexity stuff here (?)
        //Each node does an O(n log n) operation 8 times (sorting)
        //So this is O(n^2 log n)
        //  (*though C# might be using some weird implementation of sorting for small lists)

        List<Pause_SectionStatus_MenuNode> listB = new List<Pause_SectionStatus_MenuNode>(list);

        listB = listB.FindAll((e) =>
        {
            Vector3 delta = e.transform.position - transform.position;
            if (delta == Vector3.zero)
            {
                return false;
            }

            float dot = Vector3.Dot(delta.normalized, direction);

            return dot > 0.75f;
        });

        //use a metric to compare
        float Metric(Pause_SectionStatus_MenuNode node)
        {
            Vector3 delta = node.transform.position - transform.position;
            float dot = Vector3.Dot(delta.normalized, direction);

            //Lower dot = lower distance
            return delta.magnitude * (1.6f - dot) * (1.6f - dot);
        }

        listB.Sort((a, b) =>
        {
            return MainManager.FloatCompare(Metric(a),Metric(b));
        });

        return listB.Count > 0 ? listB[0] : null;
    }
}
