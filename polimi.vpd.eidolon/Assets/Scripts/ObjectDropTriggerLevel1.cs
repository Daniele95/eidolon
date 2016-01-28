﻿using UnityEngine;
using System.Collections;

public class ObjectDropTriggerLevel1 : MonoBehaviour {

    private ActionHelperLevel1 actionHelper;

	// Use this for initialization
	void Start () {
        actionHelper = ActionHelperLevel1.GetManager() as ActionHelperLevel1;
	}
	
	void OnTriggerEnter (Collider other)
    {
        if (CheckEnteredObject(other.gameObject.name))
        {
            if (!actionHelper.PlacedObjects.Contains(other.gameObject))
            {
                actionHelper.PlacedObjects.Add(other.gameObject);
                Debug.Log("placing: " + other.gameObject.name);
                Debug.Log("number of objects placed: " + actionHelper.PlacedObjects.Count.ToString());
                actionHelper.RunPlacingCutscene();
            }
        }

        if (other.gameObject.name.Equals("Christopher"))
        {
            if (actionHelper.PlacedObjects.Count >= 3)
            {
                actionHelper.LoadSecondScene();
            }
        }
    }

    private bool CheckEnteredObject(string name)
    {
        if (name.Equals("DVD") || name.Equals("Soda") || name.Equals("Tickets") ||
            name.Equals("PopCorn"))
        {
            return true;
        }
        return false;
    }
}
