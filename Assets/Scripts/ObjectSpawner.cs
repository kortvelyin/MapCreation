
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using Mirror;


//[RequireComponent(typeof(ARAnchorManager))]
public class ObjectSpawner : NetworkBehaviour
{
    //private ARAnchorManager anchorManager;
    // private Camera arcamera;
    //private List<ARAnchor> anchors = new List<ARAnchor>();
    // private int positionCount = 0;
    //private ARRaycastManager rayManager;
    ARAnchorManager m_AnchorManager;
    
    int noOfGO=0;
    private int worked = 0;
    [SerializeField]
    private UnityEngine.UI.Button arWhite;

    [SerializeField]
    private UnityEngine.UI.Button arPink;

    [SerializeField]
    private UnityEngine.UI.Button arPurple;

    private string anchorToResolve;
    public GameObject objectToSpawn0;
    public GameObject objectToSpawn1;
    public GameObject objectToSpawn2;
    public GameObject objectToSpawn;
    private Transform placementIndicator;
    private int fingerID = -1;
    private Pose thisPose;
    private int thing=0;
    
    public Text anchorThingie;
    public Text debug1;
    public Text debug2;
    public Text debug3;
    public ARPlaneManager planeManager;
    PlayerObject player;
    private NetworkConnection conn;
    public GameObject prefab
    {
        get => objectToSpawn;
        set => objectToSpawn = value;
    }

   



    // Start is called before the first frame update
    public void ToggleState()
    {
        GetComponent<ObjectSpawner>().enabled = !GetComponent<ObjectSpawner>().enabled;
    
    }
    void Awake()
    {
        m_AnchorManager = GetComponent<ARAnchorManager>();
       // placementIndicator = FindObjectOfType<PlacementIndicator>();
        player = FindObjectOfType<PlayerObject>();

       

#if !UNITY_EDITOR
     fingerID = 0; 
#endif
    }

    private void Start()
    {
       
        if (isLocalPlayer)
        {
            placementIndicator = GameObject.Find("Placement Indicator").transform;
            
        }
        if (placementIndicator != null)
        {
            Debug.Log("placementIndicator manager found");
        }
    }
    public void toobject0()
    {
        objectToSpawn = objectToSpawn0;
        noOfGO = 0;
        Debug.Log("noOfGO is 0");
    }
    public void toobject1()
    {
        objectToSpawn = objectToSpawn1;
        noOfGO = 1;
        Debug.Log("noOfGO is 1");
    }
    public void toobject2()
    {
        objectToSpawn = objectToSpawn2;
        noOfGO = 2;
        Debug.Log("noOfGO is 2");
    }

    
    // Update is called once per frame
    void Update()
    {
        //debug1.text = "I'm here";

        /* */

        if (EventSystem.current.IsPointerOverGameObject(fingerID))    // is the touch on the GUI
        {
         
            return;
        }
        else if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
           
                //ARAnchor anchor = null;
                //var anchor = m_AnchorManager.AddAnchor(new Pose(placementIndicator.transform.position, placementIndicator.transform.rotation));
               // GameObject obj = Instantiate(objectToSpawn, placementIndicator.position, placementIndicator.rotation);
            CmdSpawnObjects(noOfGO, placementIndicator.position, placementIndicator.rotation);
            //player.spawned = obj;
            //player.once = true;

            //NetworkServer.Spawn(obj);
           /** anchor = obj.GetComponent<ARAnchor>();
                if (anchor == null)
                {
                    anchor = obj.AddComponent<ARAnchor>();
                if (anchor != null)
                {
                    worked++;
                    anchorThingie.text =worked.ToString();
                }
                }
            m_Anchors.Add(anchor);/*/

            /* _cloudAnchor =  ARAnchorManagerExtensions.HostCloudAnchor(m_AnchorManager, anchor);
                 debug2.text = "2:ok";
            //  _cloudAnchor = ARAnchorManagerExtensions.ResolveCloudAnchorId(m_AnchorManager, _cloudAnchor.cloudAnchorId);
             //debug2.text = "2:ok++";


             if (_cloudAnchor)
             {
                 if (ARAnchorManagerExtensions.EstimateFeatureMapQualityForHosting(m_AnchorManager, _cloudAnchor.pose) == FeatureMapQuality.Good)
                     debug3.text = "good";
                 if (ARAnchorManagerExtensions.EstimateFeatureMapQualityForHosting(m_AnchorManager, _cloudAnchor.pose) == FeatureMapQuality.Sufficient)
                     debug3.text = "Sufficient";
                 if (ARAnchorManagerExtensions.EstimateFeatureMapQualityForHosting(m_AnchorManager, _cloudAnchor.pose) == FeatureMapQuality.Insufficient)
                     debug3.text = "Insufficient";
                 // Check the Cloud Anchor state.
                 CloudAnchorState cloudAnchorState = _cloudAnchor.cloudAnchorState;
                 if (cloudAnchorState == CloudAnchorState.Success)
                 {
                     debug1.text = "1:ok";
                     //obj.transform.SetParent(_cloudAnchor.transform, false);
                     anchorToResolve = _cloudAnchor.cloudAnchorId;
                     _cloudAnchor = null;
                 }

                 else if (cloudAnchorState == CloudAnchorState.TaskInProgress)
                 {
                     // Wait, not ready yet.
                     debug1.text = "eh";
                     thing++;
                     if (thing == 50) 
                     debug1.text = "eeeeeeh";
                 }
                 else
                 {
                     debug1.text = "that's rough buddy";
                     // An error has occurred.
                 }
             }*/

            //// ARCloudAnchorManager.Instance.QueueAnchor(anchor);

        }
        // _cloudAnchor = ARAnchorManagerExtensions.ResolveCloudAnchorId(m_AnchorManager, anchorToResolve);
    }
    List<ARAnchor> m_Anchors = new List<ARAnchor>();

  [Command]
  public void CmdSpawnObjects( int no,Vector3 pos, Quaternion rot)
    {
        ARAnchor anchor = null;
        GameObject obj = Instantiate(objectToSpawn0, pos, rot);
        anchor = obj.GetComponent<ARAnchor>();
        if (anchor == null)
        {
            anchor = obj.AddComponent<ARAnchor>();
            if (anchor != null)
            {
                // worked++;
                //anchorThingie.text = worked.ToString();
            }
        }
        m_Anchors.Add(anchor);
        // RpcSpawnObjects(no,pos,rot);
        NetworkServer.Spawn(obj, conn);

    }

    [ClientRpc]
    public void RpcSpawnObjects(int no, Vector3 pos, Quaternion rot)
    {
        GameObject obj;
        ARAnchor anchor = null;
        switch(no)
        {
            case 0:
                obj = Instantiate(objectToSpawn0, pos, rot);
                Debug.Log("in 0");
                break;
            case 1:
                obj = Instantiate(objectToSpawn1, pos, rot);
                Debug.Log("in 1");
                break;
            case 2:
                obj = Instantiate(objectToSpawn2, pos, rot);
                Debug.Log("in 2");
                break;
            default:
                obj = Instantiate(objectToSpawn0, pos, rot);
                break;
        }
        

        anchor = obj.GetComponent<ARAnchor>();
        if (anchor == null)
        {
            anchor = obj.AddComponent<ARAnchor>();
            if (anchor != null)
            {
               // worked++;
                //anchorThingie.text = worked.ToString();
            }
        }
        m_Anchors.Add(anchor);
        //NetworkServer.Spawn(obj);
    }
}
