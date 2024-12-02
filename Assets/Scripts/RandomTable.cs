using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//random table that returns stuff
[System.Serializable]
public class RandomTable<T>
{
    List<IRandomTableEntry<T>> table;

    public IRandomTableEntry<T> this[int index]
    {
        get => table[index];
        set => table[index] = value;
    }

    public RandomTable(List<IRandomTableEntry<T>> p_table)
    {
        table = p_table;
    }
    public RandomTable(IRandomTableEntry<T>[] p_table)
    {
        table = new List<IRandomTableEntry<T>>(p_table);
    }
    public RandomTable(params T[] p_table)
    {
        List<T> testList = new List<T>(p_table);
        table = testList.ConvertAll<IRandomTableEntry<T>>((e) => (new RandomTableEntry<T>(e, 1)));
    }
    public RandomTable(List<T> list)
    {
        table = list.ConvertAll<IRandomTableEntry<T>>((e) => (new RandomTableEntry<T>(e, 1)));
    }
    public RandomTable(T[] t_table, float[] w_table)
    {
        if (t_table.Length != w_table.Length)
        {
            throw new ArgumentException("Table lengths are unequal");
        }

        table = new List<IRandomTableEntry<T>>();

        for (int i = 0; i < t_table.Length; i++)
        {
            table.Add(new RandomTableEntry<T>(t_table[i], w_table[i]));
        }
    }
    public RandomTable(List<T> t_list, List<float> w_list)
    {
        if (t_list.Count != w_list.Count)
        {
            throw new ArgumentException("Table lengths are unequal");
        }

        table = new List<IRandomTableEntry<T>>();

        for (int i = 0; i < t_list.Count; i++)
        {
            table.Add(new RandomTableEntry<T>(t_list[i], w_list[i]));
        }
    }
    public T Output(IRandomSource random)
    {
        float totalWeight = 0;
        for (int i = 0; i < table.Count; i++)
        {
            totalWeight += table[i].GetWeight();
        }
        float check = random.Get() * totalWeight;
        //Debug.Log(totalWeight + " "+random.Get());
        for (int i = 0; i < table.Count; i++)
        {
            check -= table[i].GetWeight();
            if (check < 0)
            {
                return table[i].Output(random);
            }
        }
        return default;
    }

    public T Output()
    {
        return Output(new GlobalRandomGenerator());
    }

    public static T ChooseRandom(T[] array)
    {
        return array[RandomGenerator.GetIntRange(0, array.Length)];
    }
    public static T ChooseRandom(List<T> list)
    {
        return list[RandomGenerator.GetIntRange(0, list.Count)];
    }

    //Forbids duplicates, if list has less elements than count, you will just get the entire list back (though it will be in a shuffled order)
    public static List<T> ChooseRandom(List<T> list, int count)
    {
        List<T> output = new List<T>();

        List<T> copy = new List<T>();
        for (int i = 0; i < list.Count; i++)
        {
            copy.Add(list[i]);
        }

        for (int i = 0; i < count; i++)
        {
            if (copy.Count == 0)
            {
                return output;
            }

            T newitem = ChooseRandom(copy);
            output.Add(newitem);
            copy.Remove(newitem);
        }

        return output;
    }
}

//return a thing
public interface IRandomTableEntry<T>
{
    float GetWeight();
    T Output(IRandomSource random);
}

//make random sourcing more abstract so that I can have more deterministic generation
public interface IRandomSource
{
    float Get(); //value between 0-1
}

public class RandomTableEntry<T> : IRandomTableEntry<T>
{
    public float weight;
    public T t;

    public RandomTableEntry(T p_t, float p_weight = 1) {
        t = p_t;
        weight = p_weight;
    }

    public float GetWeight()
    {
        return weight;
    }
    public T Output(IRandomSource random)
    {
        return t;
    }
}