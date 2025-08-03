using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class SlotPointController : MonoBehaviour
{
    public int id;

    private void Awake()
    {
        string objName = gameObject.name;

        Match match = Regex.Match(objName, @"\d+");

        if (match.Success)
        {
            id = int.Parse(match.Value);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no numeric ID in its name.");
        }
    }
}

