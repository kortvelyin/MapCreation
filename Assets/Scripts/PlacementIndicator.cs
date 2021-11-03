using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Networking;

public class PlacementIndicator : MonoBehaviour
{
    private ARRaycastManager rayManager;
    private GameObject visual;
    [HideInInspector]
    public Pose yourPose;
    //private ARAnchorManager anchorManager;

    // Start is called before the first frame update
    void Start()
    {
        ///get the components
        rayManager = FindObjectOfType<ARRaycastManager>();
        //anchorManager = GetComponent<ARAnchorManager>();
        visual = transform.GetChild(0).gameObject;

        //hide the placement visua
        visual.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //shoot a raycast from the center of the screen
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        rayManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 3), hits, TrackableType.Planes);

        //if we hit a ARplane, update the position and rotation
        if (hits.Count > 0)
        {
            transform.position = hits[0].pose.position;
            transform.rotation = hits[0].pose.rotation;
            yourPose = hits[0].pose;
            if (!visual.activeInHierarchy)
                visual.SetActive(true);
            
        }
    }
}
