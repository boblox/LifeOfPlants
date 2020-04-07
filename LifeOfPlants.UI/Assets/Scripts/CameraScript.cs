using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private const int angle = 1;
    private const int translate = 2;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.Rotate(-angle, 0, 0);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.Rotate(angle, 0, 0);
            //transform.transform.Rot += new Quaternion(0, 0, angle, 1);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            //transform.Rotate(0, -angle, 0);
            transform.Rotate(new Vector3(0, 1, 0), -angle, Space.World);
            //transform.rotation. = Quaternion.AngleAxis(angle, new Vector3(0, 1, 0));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(new Vector3(0, 1, 0), angle, Space.World);
            //transform.Rotate(0, angle, 0);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            //transform.transform.position += new Vector3(0, 0, translate);
            //transform.Translate(0, 0, translate);
            transform.Translate( new Vector3(0, 0, translate), Space.Self);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate( new Vector3(0, 0, -translate), Space.Self);
            //transform.Translate(0, 0, -translate);
            //transform.transform.position += new Vector3(0, 0, -translate);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate( new Vector3(-translate, 0, 0), Space.Self);

            //transform.Translate(-translate, 0, 0);
            //transform.transform.position += new Vector3(-translate, 0, 0);

        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate( new Vector3(translate, 0, 0), Space.Self);

            //transform.Translate(translate, 0, 0);
            //transform.transform.position += new Vector3(translate, 0, 0);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            //transform.Translate(-translate, 0, 0);
            transform.transform.position += new Vector3(0, -3 * translate, 0);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            //transform.Translate(translate, 0, 0);
            transform.transform.position += new Vector3(0, 3 * translate, 0);
        }
    }
}
