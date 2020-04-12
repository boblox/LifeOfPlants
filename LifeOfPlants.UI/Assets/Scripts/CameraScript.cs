using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private float _rotationX;
    private const float minimumVertical = -60f;
    private const float maximumVertical = 60f;
    private const float sensitivityVertical = 3;
    private const float sensitivityHorizontal = 3;
    private const float speed = 40;
    private CharacterController _charController;

    // Start is called before the first frame update
    void Start()
    {
        _charController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        _rotationX -= Input.GetAxis("Mouse Y") * sensitivityVertical;
        _rotationX = Mathf.Clamp(_rotationX, minimumVertical, maximumVertical);

        //var deltaRotationX = Input.GetAxis("Mouse Y") * sensitivityVertical;
        //var rotationX = transform.localEulerAngles.x - deltaRotationX;
        //rotationX = Mathf.Clamp(rotationX, minimumVertical, maximumVertical);
        float deltaRotationY = Input.GetAxis("Mouse X") * sensitivityHorizontal;
        float rotationY = transform.localEulerAngles.y + deltaRotationY;
        transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0);

        float deltaX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float deltaZ = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        Vector3 movement = new Vector3(deltaX, 0, deltaZ);
        movement = transform.TransformDirection(movement); 
        _charController.Move(movement);
    }
}
