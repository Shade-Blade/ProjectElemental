using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GlobalRandomGenerator : IRandomSource //weird way to use global random
{
    public float Get()
    {
        return Random.Range(0, 1.0f);
    }
}

public class RandomGenerator : IRandomSource
{
    private Random.State state;

    public RandomGenerator()
    {
        state = Random.state;
    }
    public RandomGenerator(bool RandomSeed)
    {
        Random.State preState = Random.state;
        state = Random.state;
        Random.state = preState;
    }
    public RandomGenerator(int seed)
    {
        Random.State preState = Random.state;
        Random.InitState((int)Hash((uint)seed));

        state = Random.state;
        Random.state = preState;
    }

    public float Get()
    {
        Random.State preState = Random.state;
        Random.state = state;
        float o = Random.Range(0, 1f);

        state = Random.state;
        Random.state = preState; //hide the effect of our randomization
        return o;
    }
    public float GetRange(float a, float b)
    {
        return a + (b - a) * Get(); //this formula still works no matter if a > b or b > a
    }
    public int GetIntRange(int a, int b) //inclusive, exclusive
    {
        return (int)(a + 0f + (b - a) * Get()); //this formula still works no matter if a > b or b > a
    }
    //box muller transform
    public float GetNormalDistribution(float mean, float stdev) //clamp this if you want to prevent very rare, high value events
    {
        float a = Get();
        float b = Get();

        float o1 = Mathf.Sqrt(-2 * Mathf.Log(a)) * Mathf.Cos(2 * Mathf.PI * b);

        o1 *= stdev;
        o1 += mean;

        return o1;
    }
    //exponential distribution with a minimum possible value of min
    //looks like decay * e ^ (-decay * x)
    public float GetExponentialDistribution(float min, float decay)
    {
        return min + (- Mathf.Log(1 - Get()) / decay);
    }
    public float GetExponentialDistribution(float decay)
    {
        return GetExponentialDistribution(0, decay);
    }

    //mean = lambda
    //poi(A) + poi(B) = poi(A + B)
    public int GetPoissonDistribution(float lambda)
    {
        if (lambda < 0)
        {
            throw new ArgumentException();
        }
        float check = Mathf.Exp(-lambda);
        float b = 1;
        int i = 0;
        do
        {
            i++;
            b *= Get();
        } while (b > check);
        return i - 1;
    }

    public int GetBinomialDistribution(float probability, int tries)
    {
        int output = 0;
        float num = 0.0f;
        for (int i = 0; i < tries; i++)
        {
            num = Get();
            if (num < probability)
            {
                output++;
            }
        }

        return output;
    }

    //1 to max (inclusive)
    public int rollDie(int max)
    {
        float num = Get() * max;
        int output = (int)Mathf.Ceil(num);
        if (output == 0) //this really should never happen
        {
            output = 1;
        }
        return output;
    }
    public int rollDice(int max, int count)
    {
        int output = 0;
        for (int i = 0; i < count; i++)
        {
            output += rollDie(max);
        }

        return output;
    }

    //from 1 to max
    //probability goes down linearly
    //First probability is (2 / max + 1)
    public int WedgeDistribution(int max)
    {
        //Debug.Log("Roll for " + max);

        if (max <= 0)
        {
            return max;
        }

        int triangleNumber = (max * max + max) / 2;
        float num = Get();
        int index = 1;

        //Debug.Log(num + " " + triangleNumber);

        while (true)
        {
            //Debug.Log("Subtract " + (max + 1f - index) / triangleNumber);
            num -= (max + 1f - index) / triangleNumber;
            if (num < 0)
            {
                //Debug.Log("Got a " + index);
                return index;
            }
            if (index > max)
            {
                throw new ArgumentException("Went over maximum of triangle distribution");
            }
            index++;
        }
    }
    //1 to max (with opposite shape as triangle distribution)
    public int ReverseWedgeDistribution(int max)
    {
        return max + 1 - WedgeDistribution(max);
    }

    //spread = max - mid + 1
    public int TriangleDistribution(int center, int spread, bool plusHalf = false) //calls Get() twice!
    {
        if (plusHalf)
        {
            float a = Get();
            if (a > 0.5f)
            {
                return WedgeDistribution(spread) + center;
            } else
            {
                return center - 1 + ReverseWedgeDistribution(spread);
            }
        } else
        {
            float a = Get();
            //highest = 2 / spread + 1
            //top + wedge * 2 = 1
            //top = 1 - wedge * 2
            //probabilities = (1 - top)/2 +? top
            //0.5 +- top/2

            //geometrical argument says that this is spread ^ 2
            int denominator = spread * spread;
            int numerator = (spread * spread + spread) / 2;

            //Debug.Log(a + " >? " + (numerator / (denominator + 0f)) + " " + numerator + "/" + denominator);

            if (a > numerator / (denominator + 0f))
            {
                //int b = WedgeDistribution(spread - 1);
                return WedgeDistribution(spread - 1) + center;
            }
            else
            {
                //int c = WedgeDistribution(spread);
                return -WedgeDistribution(spread) + center + 1;
            }
        }
    }
    public int TriangleDistributionB(int min, int max)
    {
        return TriangleDistribution((min + max) / 2, 1 + (max - min)/2, max-min % 2 == 0);
    }

    public static uint Hash(uint a)
    {
        //xor can be thought of as "invert these bits"
        //do some arbitrary stuff
        uint x = a;
        x += 12582521;
        x ^= ~LoopShift(x, 7);
        x ^= LoopShift(x, 17);
        x ^= ~LoopShift(x, 13);
        x ^= LoopShift(x, 23);
        return x;
    }

    private static uint LoopShift(uint a, int shift)
    {
        shift %= 32; //int has 32 bits, so loop shifting by 32 does nothing
        if (shift == 0)
        {
            return a;
        }

        uint i = a;
        uint b = i >> 32-shift;
        i <<= shift;
        i += b;

        return i;
    }
}
