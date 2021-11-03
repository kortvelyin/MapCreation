using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
//using UnityEngine.UI;

public class PlayerObject : NetworkBehaviour
{
   // public Button moveBtn;
    public GameObject PlayerUnitPrefab;
    //public GameObject myPlayerUnit;
    ObjectSpawner spawner;
   
    public GameObject spawned;
    private NetworkConnection conn;
    public GameObject[] players;

    // Start is called before the first frame update
    void Start()
    {

        if (isLocalPlayer == false)
            return;


        CmdSpawnMyUnit();
       
        spawner = FindObjectOfType<ObjectSpawner>();

       
    }
   

    [ClientRpc]
    public void RpcButton ()
    {
        playerButton();
    }

   public void playerButton()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        foreach(GameObject player in players)
        {
            player.transform.Find("PlayerButtons").gameObject.SetActive(false);
        }
        transform.Find("PlayerButtons").gameObject.SetActive(true);

    }
   
    [Command]
    public void CmdSpawnMyUnit()
    {
       GameObject obj= Instantiate(PlayerUnitPrefab);
        //NetworkServer.Spawn(obj);
        //myPlayerUnit = obj;
        NetworkServer.Spawn(obj, conn);
        RpcButton();
       
    }
    [System.NonSerialized]
    public bool once = false;

    // Update is called once per frame
    void Update()
    {
        
        if (isLocalPlayer == false)
        {
            return;
        }


       if (once)
        {
            NetworkServer.Spawn(spawned);
            once = false;
        }

        /* if (Input.GetKeyDown(KeyCode.Space))
         {
             PlayerUnitPrefab.transform.Translate(0, 1, 0);
         }

         moveBtn = GetComponent<Button>();
         //moveBtn.GetComponent<NetworkIdentity>().AssignClientAuthority(PlayerUnitPrefab.GetComponent<NetworkIdentity>().connectionToClient);
         moveBtn.onClick.AddListener(RpcMoveObject);*/

    }


    /*[ClientRpc]
    void RpcMoveObject()
    {
        PlayerUnitPrefab.transform.Translate(0, 1, 0);
    }*/
   /* [Command]
    void CmdMove()
    {
        this.transform.Translate(1, 1, 2);
    }

    public void Move()
    {
        CmdMove();
    }*/


}
