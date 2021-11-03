using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerUnit : NetworkBehaviour
{
   public Button moveBtn;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer == false)
          return;

      /* if (Input.GetKeyDown(KeyCode.Space))
        {
            this.transform.Translate(0, 1, 0);
        }*/

        moveBtn = GetComponent<Button>();
        moveBtn.GetComponent<NetworkIdentity>().AssignClientAuthority(this.GetComponent<NetworkIdentity>().connectionToClient);
        moveBtn.onClick.AddListener(RpcMoveObject);
       
    }

    [ClientRpc]
    public void RpcMoveObject()
    {
        this.transform.Translate(0, 1, 0);
    }

    [Command]
    public void CmdMove()
    {
        this.transform.Translate(0, 1, 0);
    }

    public void Move()
    {
        CmdMove();
    }


}
