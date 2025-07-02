using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject[] EnterRoom;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("DoorSystem1"))
        {
            EnterRoom[0].SetActive(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("DoorSystem1"))
        {
            EnterRoom[0].SetActive(false);
        }
    }
}
