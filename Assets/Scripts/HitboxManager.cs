using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class HitboxManager : MonoBehaviour
{
    private Transform oldCollidersTransform;
    public GameObject HitboxFolder;

    void setHitBox(string hboxInfo)
    {
        if (hboxInfo == "Clear")
        {
            setCollidersActive(oldCollidersTransform, false);
            return;
        }

        string[] hboxArray = hboxInfo.Split(':');
        string hboxName = hboxArray[1];
        string hboxParentName = hboxArray[0];

        if (oldCollidersTransform != null)
            setCollidersActive(oldCollidersTransform, false);
       
        GameObject hboxParent = HitboxFolder.transform.Find(hboxParentName).gameObject;
        Transform collidersTransform = hboxParent.transform.Find(hboxName);

        setCollidersActive(collidersTransform, true);
        oldCollidersTransform = collidersTransform;
    }

    void setCollidersActive(Transform collidersTransform, bool active)
    {
        foreach (Transform child in collidersTransform)
        {
            child.gameObject.SetActive(active);
        }
    }
}