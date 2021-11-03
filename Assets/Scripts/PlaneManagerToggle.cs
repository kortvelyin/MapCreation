using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Mirror;

public class PlaneManagerToggle : NetworkBehaviour
{
    public ARPlaneManager planeManager;
    private ARTrackedImageManager aRTrackedImageManager;
    public GameObject worldMap;
    private getplanes gpscript;

    // Start is called before the first frame update
    void Start()
    {
        gpscript = GetComponent<getplanes>();
        //if (isLocalPlayer)
            planeManager = GameObject.Find("AR Session Origin").GetComponent<ARPlaneManager>();
        if (planeManager != null)
        {
            Debug.Log("Plane manager found to switch in player");
        }
        aRTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        worldMap = GameObject.Find("WorldMap");
        aRTrackedImageManager.trackedImagesChanged += OnImageChanged;
        planeManager.enabled = false;
    }


    public void OnImageChanged(ARTrackedImagesChangedEventArgs args)
    {
        if (isLocalPlayer)
        {
            if (worldMap != null)
            {
                foreach (var TrackedImage in args.added)
                {
                    worldMap.transform.position = TrackedImage.transform.position;
                    worldMap.transform.rotation = TrackedImage.transform.rotation;

                    Debug.Log("Picture is seen and name: " + TrackedImage.name);
                    planeManager.enabled = true;
                   // gpscript.CmdAskForPlanesFromServerOnStart(gpscript.planesDict.Count);
                }


                /*foreach (var TrackedImage in args.updated)
                 {
                     if (TrackedImage == Image)
                     {
                         worldMap.transform.position = TrackedImage.transform.position;
                         worldMap.transform.rotation = TrackedImage.transform.rotation;

                         Debug.Log("Picture is seen and name: " + TrackedImage.name);
                     }
                 }*/
            }
        }

    }

    void OnDisable()
    {

        if (!isLocalPlayer)
            return;
       

        aRTrackedImageManager.trackedImagesChanged += OnImageChanged;

       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchOffPlaneManager()
    {

        planeManager.enabled = true;
    }


}
