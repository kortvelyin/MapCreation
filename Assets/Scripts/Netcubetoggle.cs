using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Netcubetoggle : MonoBehaviour
{

    public void ToggleStateHCube()
    {
        GetComponent<Netcubetoggle>().enabled = !GetComponent<Netcubetoggle>().enabled;
    }
    
}
