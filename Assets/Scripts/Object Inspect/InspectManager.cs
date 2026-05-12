using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectManager : MonoBehaviour
{
    public static InspectManager Instance;
    public Camera lookAtCamera;
    [System.Serializable]
    public class InspectData
    {
        public string guid;
        public GameObject sourcePrefab;
        public Transform spawnedObject;
    }
    public List<InspectData> inspectObjects = new List<InspectData>();

    void Awake()
    {
        if(Instance == null) Instance = this;
    }
    public InspectData OnItemSelected(GameObject itemPrefab, bool lookAway = false)
    {
        // CHECK DUPLICATE
        foreach (var item in inspectObjects)
        {
            if (item.sourcePrefab == itemPrefab)
            {
                Debug.Log("Already inspecting: " + itemPrefab.name);
                return item;
            }
        }

        // SPAWN
        Vector3 spawnPos =
    new Vector3(1001.02002f,999.599976f,999.210022f);

        Transform spawned = Instantiate(
            itemPrefab,
            spawnPos,
            Quaternion.identity
        ).transform;

        // FACE CAMERA
        Camera cam = lookAtCamera;

        // Make object face same direction as camera
        spawned.forward = cam.transform.forward;

        if(!lookAway)
        {
            // OPTIONAL:
            // If object backwards, use this instead:
            //
            spawned.forward = -cam.transform.forward;
        }

        // GENERATE GUID
        string guid = Guid.NewGuid().ToString();

        InspectData data = new InspectData
        {
            guid = guid,
            sourcePrefab = itemPrefab,
            spawnedObject = spawned
        };

        inspectObjects.Add(data);

        Debug.Log("Spawned Inspect Object GUID: " + guid);

        return data;
    }
    public void RemoveInspectObject(string guid)
    {
        for (int i = inspectObjects.Count - 1; i >= 0; i--)
        {
            if (inspectObjects[i].guid == guid)
            {
                Destroy(inspectObjects[i].spawnedObject.gameObject);
                inspectObjects.RemoveAt(i);

                Debug.Log("Removed GUID: " + guid);
                return;
            }
        }
    }

    public InspectData GetInspectObject(string guid)
    {
        return inspectObjects.Find(x => x.guid == guid);
    }

     public void ShowObject(string guid)
    {
        var data = GetInspectObject(guid);

        if (data == null || data.spawnedObject == null)
            return;

        data.spawnedObject.gameObject.SetActive(true);

        Debug.Log("Show Object: " + guid);
    }

    public void HideObject(string guid)
    {
        var data = GetInspectObject(guid);

        if (data == null || data.spawnedObject == null)
            return;

        data.spawnedObject.gameObject.SetActive(false);

        Debug.Log("Hide Object: " + guid);
    }

    public void HideAll()
    {
        foreach (var item in inspectObjects)
        {
            if (item.spawnedObject != null)
            {
                item.spawnedObject.gameObject.SetActive(false);
            }
        }
    }

    public void ShowAll()
    {
        foreach (var item in inspectObjects)
        {
            if (item.spawnedObject != null)
            {
                item.spawnedObject.gameObject.SetActive(true);
            }
        }
    }
}
