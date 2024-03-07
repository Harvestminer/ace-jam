using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudManager : AManager<CloudManager>
{
    public Transform clouds;
    [Range(0, 50)]
    public float cloudHeight;
    [Range(0.01f, 1f)]
    public float followSpeed;

    private Transform cam;

    private void OnEnable()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        clouds.position = Vector3.MoveTowards(new Vector3(cam.position.x, cloudHeight, cam.position.z), cam.position, followSpeed * Time.deltaTime);
        clouds.up = Vector3.up;
    }
}
