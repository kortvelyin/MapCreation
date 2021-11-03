using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rigidbodyhandler : MonoBehaviour
{
    private bool toggle = false;

    public void ToggleStateRb()
    {
        toggle = !toggle;

        Rigidbody m_Rigidbody;
        m_Rigidbody = GetComponent<Rigidbody>();
        if (!toggle)
        {
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
        else
            m_Rigidbody.constraints = RigidbodyConstraints.None;
    }
    // Start is called before the first frame update
   
}
