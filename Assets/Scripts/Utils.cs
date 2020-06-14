using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static float ManhattanDistance(Vector2 A, Vector2 B)
    {
        return Mathf.Abs(A.x - B.x) + Mathf.Abs(A.y - B.y);
    }

    public static int RandomFromWeights(List<float> weights)
    {
        // Normalize
        float total = 0;
        for (int i = 0; i < weights.Count; ++i)
        {
            total += weights[i];
        }

        List<float> normalizedWeights = new List<float>(weights.Count);
        for (int i = 0; i < weights.Count; ++i)
        {
            normalizedWeights.Add(weights[i] / total);
        }

        float k = Random.Range(0f, 1f);
        if (k <= normalizedWeights[0])
        {
            return 0;
        }
        else
        {
            return FindSmallestIndex(normalizedWeights, k);
        }
    }

    private static int FindSmallestIndex(List<float> normalizedWeights, float k)
    {
        for (int i = 1; i < normalizedWeights.Count; ++i)
        {
            float previous = normalizedWeights[i - 1];
            float current = normalizedWeights[i];
            if (previous < k && k <= current)
            {
                return i;
            }
        }

        return normalizedWeights.Count - 1;
    }
}
