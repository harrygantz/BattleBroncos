using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HitboxManager : MonoBehaviour
{
    private GameObject oldColliders;
    public GameObject HitboxFolder;

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Collider hit something!");
    }

    void setHitBox(string hboxInfo)
    {
        if (hboxInfo == "Clear")
        {
            oldColliders.SetActive(false);
            return;
        }

        string[] hboxArray = hboxInfo.Split(':');
        string hboxName = hboxArray[1];
        string hboxParentName = hboxArray[0];

        if (oldColliders != null)
            oldColliders.SetActive(false);
       
        GameObject hboxParent = HitboxFolder.transform.Find(hboxParentName).gameObject;
        GameObject colliderObject = hboxParent.transform.Find(hboxName).gameObject;
        colliderObject.SetActive(true);
        oldColliders = colliderObject;
    }
}