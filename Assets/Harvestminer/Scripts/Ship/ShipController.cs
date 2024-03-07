using UnityEngine;

public class ShipController : MonoBehaviour, IInteractable
{
    public Transform cam_pos;

    public float shipSpeed = 5f;
    public float rotationSpeed = 5f;

    public Vector3 offset;

    private Camera cam;
    private Rigidbody ship_rg;

    void OnEnable()
    {
        ship_rg = GameManager.instance.ship.GetComponent<Rigidbody>();
    }

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
        Transform ship = GameManager.instance.ship;
        ship.position += ship.forward * shipSpeed * Time.deltaTime;

        if (shipSpeed < 0)
            shipSpeed = 0;

        if (!GameManager.instance.isControlShip)
            return;


        if (Input.GetAxis("Vertical") != 0)
            shipSpeed += Input.GetAxis("Vertical") * Time.deltaTime;

        if (Input.GetAxis("Horizontal") != 0)
            ship.Rotate(Vector3.up, Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime);
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
