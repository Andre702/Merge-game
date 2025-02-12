using UnityEngine;
using System;
using System.Collections.Generic;

public class Calc : MonoBehaviour
{
    public List<double> results = new();
    void Start()
    {
        int i_max = 70;
        int i_min = 30;
        

        for (double i = i_min; i <= i_max; i++)
        {
            double result = i / Math.Sqrt(2);
            results.Add(result - Math.Floor(result));
        }

        double best = 1.0;
        int best_index = 0;

        foreach (var number in results)
        {
            if (number < best)
            {
                best = number;
                best_index = results.IndexOf(number);
            }
        }

        Debug.Log($"Best division is of number: {best_index + i_min}, and has a fraction of: {best}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
