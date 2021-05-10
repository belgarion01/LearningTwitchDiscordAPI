using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatterBadge
{
    public string ID { get; private set; }
    public string Version { get; private set; }

    public ChatterBadge(string id, string version)
    {
        ID = id;
        Version = version;
    }
}
