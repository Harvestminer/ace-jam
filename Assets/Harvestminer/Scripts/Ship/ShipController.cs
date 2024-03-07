using UnityEngine;

public class ShipController : MonoBehaviour, IInteractable
{
    public Transform cam_pos;

    public float shipSpeed = 5f;
    public float rotationSpeed = 5f;

    public Vector3 offset;

    private Camera cam;

    public void Interact()
    {
        if (cam == null)
            cam = Camera.main;

        if (!GameManager.instance.isControlShip)
            changeControl();
    }

    void LateUpdate()
    {
        if (Input.GetButtonDown("Cancel") && GameManager.instance.isControlShip)
            changeControl();
    }

    void Update()
    {
        if (!GameManager.instance.isControlShip)
            return;

        if (Input.GetAxis("Vertical") != 0)
        {
            Transform ship = GameManager.instance.ship;

            ship.position += ship.forward * Input.GetAxis("Vertical") * shipSpeed * Time.deltaTime;
        }

        if (Input.GetAxis("Horizontal") != 0)
        {
            Transform ship = GameManager.instance.ship;

            ship.Rotate(Vector3.up, Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime);
        }
    }

    private void changeControl()
    {
        GameManager.instance.isControlShip = !GameManager.instance.isControlShip;

        Transform player = GameManager.instance.player;
        Transform _parent = GameManager.instance.isControlShip ? GameManager.instance.ship : player;

        cam.GetComponent<ThirdPersonOrbitCamBasic>().player = _parent;
        cam.GetComponent<ThirdPersonOrbitCamBasic>().pivotOffset = 
            GameManager.instance.isControlShip ? offset : new Vector3(0, 1.7f, 0);
        cam.transform.parent = _parent;

        player.GetComponent<BasicBehaviour>().enabled = !GameManager.instance.isControlShip;
        player.GetComponent<MoveBehaviour>().enabled = !GameManager.instance.isControlShip;
        player.GetComponent<AimBehaviourBasic>().enabled = !GameManager.instance.isControlShip;
        player.GetComponent<FlyBehaviour>().enabled = !GameManager.instance.isControlShip;

    }
}
