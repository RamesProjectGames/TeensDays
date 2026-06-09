using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOGManager : MonoBehaviour
{
    [Header("Cameras")]
    public CinemachineFreeLook freeLookCam;
    public CinemachineVirtualCamera cinematicCam;

    [SerializeField] private GameObject fogParticle_SMP;
    [SerializeField] private GameObject SMPBuilding;
    [SerializeField] private GameObject constructionSMP;

    [SerializeField] private GameObject fogParticle_SMA;
    [SerializeField] private GameObject SMABuilding;
    [SerializeField] private GameObject constructionSMA;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            fogParticle_SMP.SetActive(true);

           StartCoroutine(ShowObjectAfterDelay_SMP());
        }
    }

    private IEnumerator ShowObjectAfterDelay_SMP()
    {
        // Pindah ke cinematic camera
        cinematicCam.Priority = 20;
        freeLookCam.Priority = 10;

        yield return new WaitForSeconds(5f);

        SMPBuilding.SetActive(true);
        constructionSMP.SetActive(false);

        yield return new WaitForSeconds(10f);

        cinematicCam.Priority = 10;
        freeLookCam.Priority = 20;
    }

    private IEnumerator ShowObjectAfterDelay_SMA()
    {
        yield return new WaitForSeconds(5f);

        SMABuilding.SetActive(true);
        constructionSMA.SetActive(true);
    }

    // Start is called before the first frame update
}
