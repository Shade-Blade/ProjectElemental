using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool
{
    //how this works
    //value gets incremented by (probability)
    //if the result is > 1, subtract 1 and return true

    //since (value) is something between (0 - 1) the probability assuming random value is (probability)

    //though in order to avoid float errors I use ints instead

    public int value;
    public int threshold;

    public Pool(int p_value = 6, int p_threshold = 12)
    {
        value = p_value;
        threshold = p_threshold;
    }

    //Add P to value
    //If value reaches 1, subtract 1 and return true
    public bool Increment(int probability)
    {
        value += probability;
        bool check = false;
        while (value >= threshold)
        {
            check = true;
            value -= threshold;
        }

        return check;
    }

    public void Set(int set)
    {
        value = set;
    }
}

public class DoublePool
{
    public Pool poolA;
    public Pool poolB;

    //gluing two pools together in a special way

    public DoublePool(int startA = 6, int thresholdA = 12, int startB = 6, int thresholdB = 12)
    {
        poolA = new Pool(startA, thresholdA);
        poolB = new Pool(startB, thresholdB);
    }

    //Add P to value
    //If value reaches 1, subtract 1 and return true
    //If first one passes 1, only increment 2 if it does not go past 1
    public bool Increment(int p1, int p2)
    {
        poolA.value += p1;
        bool check = false;
        while (poolA.value >= poolA.threshold)
        {
            check = true;
            poolA.value -= poolA.threshold;
        }

        poolB.value += p2;
        if (check)
        {
            if (poolB.value >= poolB.threshold)
            {
                //undo increment
                poolB.value -= p2;
            }
        } else
        {
            while (poolB.value >= poolB.threshold)
            {
                check = true;
                poolB.value -= poolB.threshold;
            }
        }

        //Debug.Log("(" + poolA.value + ", " + poolB.value + ")");

        return check;
    }

    public void Set(int setA, int setB)
    {
        poolA.Set(setA);
        poolB.Set(setB);
    }
}

//this system seems similar to some equal representation algorithms
public class RepresentativePool
{
    int totalCount;
    int[] individualCounts;
    float[] targetProbabilities;
    
    public RepresentativePool(float[] targets)
    {
        totalCount = 0;
        individualCounts = new int[targets.Length];
        targetProbabilities = new float[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            targetProbabilities[i] = targets[i];
        }
    }

    public void Reset()
    {
        totalCount = 0;
        for (int i = 0; i < individualCounts.Length; i++)
        {
            individualCounts[i] = 0;
        }
    }

    //index
    public int Get()
    {
        totalCount++;

        float[] offsets = new float[individualCounts.Length];

        float lowestOffset = 0;
        int lowestIndex = 0;
        for (int i = 0; i < offsets.Length; i++)
        {
            offsets[i] = ((float)individualCounts[i] / totalCount) - targetProbabilities[i];
            if (i == 0)
            {
                lowestOffset = offsets[i];
            }
            if (offsets[i] < lowestOffset)
            {
                lowestOffset = offsets[i];
                lowestIndex = i;
            }
        }

        individualCounts[lowestIndex]++;
        return lowestIndex;
    }
}