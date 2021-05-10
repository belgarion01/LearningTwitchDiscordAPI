using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ParseHelper
{
    public static int IndexOfWithSkip(this string source, char val, int nbOfSkip = 0)
    {
        int index = source.IndexOf(val);

        for (int i = 0; i < nbOfSkip; i++)
        {
            if (index == -1) return -1;

            index = source.IndexOf(val, index + 1);
        }

        return index;
    }
}
