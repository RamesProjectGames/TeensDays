using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiQuestAnim : MonoBehaviour
{
    public GameObject objectUi;
    public Transform player;
    public float maxRotasion = 30f;
    public float minRotasion = 0;
    public float pointOfY;

    private float currentAngle = 0f;

    [SerializeField] private Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        LeanTween.moveLocalY(objectUi, pointOfY, 1f).setLoopPingPong();
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion rotation = mainCamera.transform.rotation;
        transform.LookAt(transform.position + rotation * Vector3.forward, rotation * Vector3.up);

        //// Hitung arah ke player
        //Vector3 targetDirection = player.position - transform.position;

        //// Hitung rotasi yang diinginkan, tetapi hanya pada sumbu X
        //Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        //// Ambil sudut rotasi saat ini dan target hanya pada sumbu X
        //float currentAngle = transform.eulerAngles.x; // Rotasi pada sumbu X
        //float targetAngle = targetRotation.eulerAngles.x; // Rotasi target pada sumbu X

        //// Menghitung selisih sudut
        //float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);

        //// Membatasi rotasi agar tidak melebihi maxRotationAngle
        //if (Mathf.Abs(angleDifference) > maxRotasion)
        //{
        //    targetAngle = currentAngle + Mathf.Sign(angleDifference) * maxRotasion;
        //}

        //// Interpolasi rotasi objek ke target secara bertahap pada sumbu X
        //float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, maxRotasion * Time.deltaTime);

        //// Set hanya rotasi pada sumbu X, tanpa mengubah sumbu Y dan Z
        //transform.rotation = Quaternion.Euler(newAngle, transform.eulerAngles.y, transform.eulerAngles.z);
    }

    private void _Reset()
    {
        mainCamera = Camera.main;
    }
}
