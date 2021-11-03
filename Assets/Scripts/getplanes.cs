using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using Mirror;
using Newtonsoft.Json;



public class getplanes : NetworkBehaviour
{
    public struct PlaneData// egy változó a plane-nek a pontjaira, és a plane id-jára
    {
        public Vector3 position;
        public string Jvertice;
        public Quaternion rotation;
        public int id;
        public int boundarylength;
        public uint playerNetID;


        public PlaneData(string Jvertice, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
        {
            this.position = position;
            this.rotation = rotation;
            this.Jvertice = Jvertice;
            this.id = id;
            this.boundarylength = boundarylength;
            this.playerNetID = playerNetID;

        }
    }


    ARAnchorManager m_AnchorManager;
    List<ARAnchor> m_Anchors = new List<ARAnchor>();
    static Dictionary<string, PlaneData> verticesDict = new Dictionary<string, PlaneData>(); //amiben a szerver tartja az adatokat
    static Dictionary<string, GameObject> planesDict = new Dictionary<string, GameObject>(); //amiben a kliensek tartják az adatokat
    private uint playerNetID;
    private GameObject StepChild;  //to get a position  and rotation calculation
    public GameObject meshF;
    public ARPlaneManager planeManager;
    public Material mat;
   // ARPlane planeNew;
    public Text debug;
    Unity.Collections.NativeArray<Vector2> vectors;
    //PlayerObject playerobjscript;
   // Vector3[] pontok = new Vector3[11];
    //private GameObject origo;
    private ARTrackedImageManager aRTrackedImageManager;
    public GameObject worldMap; //ami alá be vannak rakva a plane-k
    public bool samePos = false; //a plane-k fedik-e egymást
    public float PosDiff = 20f;  //a magasság távolság szűrésre, jelenleg a plafon magassága van kb beállítva
    public int RotDiff = 360;  //rotáció eltérés, ha ki akarjátok próbálni
    public static bool readImage = false; //beolvasta-e már az origót jelentő képet
    public static float DiameterCheck = 0; //számontartja hogy mekkora a legnagyobb pont-középpont távolság
    public bool OneWay; //ezt az editorban bepipálva csak a host(ha van külön csak szerver akkor az első játékos) osztja meg azt amit lát
    public GameObject[] players; //ez alapján fönti el hogy ki az első

    void Start()
    {

        players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("NumberOfPlayersIs: " + players.Length);
        if(players.Length<2)
        // GameObject newCanvas = Instantiate(canvas); 
        playerNetID = GetComponent<NetworkIdentity>().netId;
        m_AnchorManager = GetComponent<ARAnchorManager>();

        worldMap = GameObject.Find("WorldMap");
        while (worldMap == null)
            Debug.Log("coudnt find worldmap");
        if (worldMap != null)
            Debug.Log("Found worldmap");
        planeManager = GameObject.Find("AR Session Origin").GetComponent<ARPlaneManager>();
        if (planeManager != null)
        {
            Debug.Log("Plane manager found");
        }
        aRTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        if (aRTrackedImageManager != null)
            Debug.Log("found image tracker");
        StepChild = GameObject.Find("StepChild");
        planeManager.planesChanged += OnPlanesChanged;
        Debug.Log("Subscribed to event");
        aRTrackedImageManager.trackedImagesChanged += OnImageChanged;
        planeManager.enabled = false;


#if UNITY_EDITOR //megcsinálja azokat amit képbeolvasáskor csinál a telefon
        readImage = true;
        if (!isServer)
            CmdAskForPlanesFromServerOnStart(this.gameObject, planesDict.Count);
        Debug.Log("asked for planes on start");
        planeManager.enabled = true;

#endif
    }


    public void OnImageChanged(ARTrackedImagesChangedEventArgs args)
    {

        if (worldMap != null)
        {
            foreach (var TrackedImage in args.added)
            {

                worldMap = GameObject.Find("WorldMap");
                if (worldMap != null)
                    Debug.Log("Found worldmap");
                worldMap.transform.position = TrackedImage.transform.position;
                worldMap.transform.rotation = TrackedImage.transform.rotation;
                Debug.Log("Picture is seen and name: " + TrackedImage.name);
                readImage = true;
                CmdAskForPlanesFromServerOnStart(this.gameObject, planesDict.Count);
                if (OneWay) //ha csak a host oszt meg, ez biztosítja hogy csak ő kapcsolja be a planemanagert
                {
                    if (players.Length < 2)
                        planeManager.enabled = true;
                }
                else
                    planeManager.enabled = true;
            }


            foreach (var TrackedImage in args.updated)
            {
                worldMap.transform.position = TrackedImage.transform.position;
                worldMap.transform.rotation = TrackedImage.transform.rotation;
            }
        }

    }


    [Command]
    public void CmdAskForPlanesFromServerOnStart(GameObject target, int PlanesCount)
    {

        if (isServer)
        {
            Debug.Log("In CmdAskforplanews as Server");
            Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
            Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);

            if (PlanesCount < 1)
                foreach (var entry in verticesDict)
                {
                    NetworkIdentity opponentIdentity = target.GetComponent<NetworkIdentity>();
                    // RpcAddPlaneToClient(entry.Value.Jvertice, entry.Value.position, entry.Value.rotation, entry.Value.id, entry.Value.boundarylength, entry.Value.playerNetID);
                    TargetCreatePlanesFromServer(opponentIdentity.connectionToClient, entry.Value.Jvertice, entry.Value.position, entry.Value.rotation, entry.Value.id, entry.Value.boundarylength, entry.Value.playerNetID);
                }
        }
    }

    [TargetRpc]
    public void TargetCreatePlanesFromServer(NetworkConnection target, string json, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
        Debug.Log("sorry");
        var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
        Vector3[] verticess = vertices.ToArray();
        AddPlane(verticess, position, rotation, id, boundarylength, playerNetID);

    }

   

    void OnDisable()
    {

        if (!isLocalPlayer)
            return;
        if (planeManager == null)
            Debug.Log("too early2");
        else
        {
            planeManager.planesChanged -= OnPlanesChanged;
            Debug.Log("Unsubscribed to event");
        }

        aRTrackedImageManager.trackedImagesChanged -= OnImageChanged;

        foreach (var plane in planesDict)
        {
            // CmdRemoveMapInfo(plane.GetInstanceID(), playerNetID);
           // RemovePlane(plane.Value.gameObject., playerNetID);
        }
    }




    void OnPlanesChanged(ARPlanesChangedEventArgs eventArgs)
    {

        // Debug.Log("Planes Changed");
        if (isLocalPlayer)
        {

            // Debug.Log("We are local players");
            foreach (ARPlane plane in eventArgs.removed)
            {
                plane.boundaryChanged -= UpdatePlane;
                Debug.Log("PlanesRemoved, unsubscribed form event(probably)");
                CmdRemoveMapInfo(plane.GetInstanceID(), playerNetID);
                // CmdRemovePlaneFromServer(plane.GetInstanceID(), playerNetID);

            }



            foreach (ARPlane plane in eventArgs.added)
            {
                Debug.Log("In AddPlaneEvent as client");
                Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
                Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);
                plane.boundaryChanged += UpdatePlane;
                Debug.Log("PlanesAdded and subscibed to event (probably)");

                //if (StepChild == null)
                {
                    StepChild = GameObject.Find("WorldMap").gameObject.transform.GetChild(0).gameObject;
                    Debug.Log("Found Stepchild in add, name: " + StepChild.name);
                }
                vectors = plane.boundary;

                Vector3[] vertices = new Vector3[plane.boundary.Length];
                int i;
                for (i = 0; i < plane.boundary.Length; i++)
                {
                    vertices[i] = new Vector3(vectors[i].x, 0, vectors[i].y);
                }
                var settings = new Newtonsoft.Json.JsonSerializerSettings();
                // This tells your serializer that multiple references are okay.
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;


                string json = JsonConvert.SerializeObject(vertices, settings);
               

                StepChild.transform.position = plane.transform.position;
                StepChild.transform.rotation = plane.transform.rotation;
                Debug.Log("rotation: " + StepChild.transform.rotation);
                Debug.Log("localrotation: " + StepChild.transform.localRotation);
                //StepChild.transform.parent = worldMap.transform;
                // CmdAddMapInfo(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
                // CmdAddPlaneToServer(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
                CmdAddMapInfo(json, StepChild.transform.localPosition, StepChild.transform.localRotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
                //CmdAskForPlanesFromServerOnStart(planesDict.Count);
                // StepChild.transform.parent = null;
                //// CmdAddMapInfo(json, plane.transform.position, plane.transform.rotation, plane.GetInstanceID(), plane.boundary.Length, playerNetID);
                //CmdCreatePlaneFromData(json, plane.transform.position, Quaternion.Euler(newPlaneRot), plane.GetInstanceID(), plane.boundary.Length, playerNetID);
            }


        }

    }

    void UpdatePlane(ARPlaneBoundaryChangedEventArgs eventArgs)
    {

        if (isLocalPlayer)
        {

            // Debug.Log("Update was called by boundaryChanged");
            //Debug.Log("PlanesUpdated");
            vectors = eventArgs.plane.boundary;

            Vector3[] vertices = new Vector3[eventArgs.plane.boundary.Length];
            int i;
            for (i = 0; i < eventArgs.plane.boundary.Length; i++)
            {
                vertices[i] = new Vector3(vectors[i].x, 0, vectors[i].y);
            }
           // if (StepChild == null)
            {
                StepChild = GameObject.Find("WorldMap").gameObject.transform.GetChild(0).gameObject;
            }
            Debug.Log("Found Stepchild in update, name: " + StepChild.name);
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            // This tells your serializer that multiple references are okay.
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;


            string json = JsonConvert.SerializeObject(vertices, settings);
           
            /* Debug.Log("In updatePlaneEvent as client");
             Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
             Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);*/

            StepChild.transform.position = eventArgs.plane.transform.position;
            StepChild.transform.rotation = eventArgs.plane.transform.rotation;
            Debug.Log("rotation: " + StepChild.transform.rotation);
            Debug.Log("localrotation: " + StepChild.transform.localRotation);
            // StepChild.transform.parent = worldMap.transform;
            // CmdUpdateMapInfo(json, eventArgs.plane.transform.position, eventArgs.plane.transform.rotation, eventArgs.plane.GetInstanceID(), eventArgs.plane.boundary.Length, playerNetID);
            CmdUpdateMapInfo(json, StepChild.transform.localPosition, StepChild.transform.localRotation, eventArgs.plane.GetInstanceID(), eventArgs.plane.boundary.Length, playerNetID);
            // CmdAskForPlanesFromServerOnStart();
            // StepChild.transform.parent = null;

        }
    }


    [Command] //Serverre küldi a Plane adatokat
    public void CmdAddMapInfo(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {

        if (isServer)
        {
            var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
            Vector3[] verticess = vertices.ToArray();
           
            foreach (var entry in verticesDict)
            {
                
                if (entry.Value.playerNetID != playerNetID && Mathf.Abs((entry.Value.position - position).magnitude) <= DiameterCheck && Mathf.Abs(entry.Value.rotation.eulerAngles.z - rotation.eulerAngles.z) < RotDiff && Mathf.Abs(entry.Value.position.y - position.y) < PosDiff)
                {
                    var entryVertices = JsonConvert.DeserializeObject<List<Vector3>>(entry.Value.Jvertice);
                    Vector3[] entryVerticess = entryVertices.ToArray();
                    ////////
                    ///a zöld részben van az a megoldás hogy előbb átkonvertálom az adatokat és aztán megkapja a függvény,
                    ///jeleneleg csak átadom a függvénynek és ott megfelelően számít az adatokkal
                    ////////
                  /*  Vector2[] entryPolygon = new Vector2[entryVerticess.Length];
                     Vector2[] newPolygon = new Vector2[verticess.Length];
                     for (int i=0; i<entryVerticess.Length; i++)
                     {
                         entryPolygon[i] = new Vector2(entryVerticess[i].x + entry.Value.position.x, entryVerticess[i].z + entry.Value.position.z);
                     }
                     for (int i = 0; i < verticess.Length; i++)
                     {
                         newPolygon[i] = new Vector2(verticess[i].x + position.x, verticess[i].z + position.z);
                     }

                     Vector2 worldPoint = new Vector2(position.x, position.z);
                     if(entryPolygon.Length>2 && newPolygon.Length>2)
                    if (IsPointInPolygon4(entryPolygon, worldPoint, newPolygon))
                     {*/
                    if(entryVerticess.Length>2 && verticess.Length>2)
                    if (IsPointInPolygon(entryVerticess, entry.Value.position, position, verticess))
                    {
                        
                        RpcRemovePlaneFromClient(id, playerNetID);
                            if (isServerOnly)
                                RemovePlane(id, playerNetID);
                        Debug.Log("Yes, Points are also in mesh");
                        samePos = true;
                            break;
                        }
                    else
                    {
                        Debug.Log("No");
                      
                        samePos = false;
                    }
                    


                }
            }

            foreach (var point in vertices)  //számol egy max távolságot a középponttól (tudom hogy nem a legvélravezetőbb megoldás, de nem tudtam jobbat
            {
                if (Mathf.Abs((point - position).magnitude) > DiameterCheck)
                    DiameterCheck = (point - position).magnitude;
            }
            Debug.Log("Diameter Check: " + DiameterCheck);

            if (!samePos)
            {
                Debug.Log("I was kept and added");
                string verticesid = id.ToString() + playerNetID.ToString();
                if (verticesDict.ContainsKey(verticesid))
                {
                    Debug.Log("New Map Info existed already");
                }

                else
                {
                  
                    PlaneData pData;

                    pData.position = position;
                    pData.rotation = rotation;
                    pData.Jvertice = json;
                    pData.id = id;
                    pData.boundarylength = boundarylength;
                    pData.playerNetID = playerNetID;

                    verticesDict.Add(verticesid, pData);

                    RpcAddPlaneToClient(json, position, rotation, id, boundarylength, playerNetID);
                    if (isServerOnly)
                        AddPlane(verticess, position, rotation, id, boundarylength, playerNetID);
                }
            }
            else
            {
                samePos = false;
                Debug.Log("we dont add this");
            }

        }


    }

    [Command]
    public void CmdRemoveMapInfo(int id, uint playerNetID)
    {
        if (isServer)
        {
            /*Debug.Log("In RemoveCmd as Server");
            Debug.Log("asking plane informations from server, number of planes in Planesdict: " + planesDict.Count);
            Debug.Log("asking plane informations from server, number of planes inverticesdict: " + verticesDict.Count);*/
            string verticesid = id.ToString() + playerNetID.ToString();
            if (verticesDict.ContainsKey(verticesid))
            {
                verticesDict.Remove(verticesid);
                if (isServerOnly)
                    RemovePlane(id, playerNetID);
                RpcRemovePlaneFromClient(id, playerNetID);
            }
            else
            {
                Debug.Log("Tried to Remove a Map Info that didn't exist");
            }
        }
    }

    [Command]
    public void CmdUpdateMapInfo(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
        if (isServer)
        {
            var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
            Vector3[] verticess = vertices.ToArray();
            foreach (var entry in verticesDict)
            {
                if (entry.Value.playerNetID != playerNetID && Mathf.Abs((entry.Value.position - position).magnitude) <= DiameterCheck && Mathf.Abs(entry.Value.rotation.eulerAngles.z- rotation.eulerAngles.z) < RotDiff && Mathf.Abs(entry.Value.position.y - position.y) < PosDiff)
                {
                    var entryVertices = JsonConvert.DeserializeObject<List<Vector3>>(entry.Value.Jvertice);
                    Vector3[] entryVerticess = entryVertices.ToArray();
                    /* Vector2[] entryPolygon = new Vector2[entryVerticess.Length];
                     Vector2[] newPolygon = new Vector2[verticess.Length];
                     for (int i=0; i<entryVerticess.Length; i++)
                     {
                         entryPolygon[i] = new Vector2(entryVerticess[i].x + entry.Value.position.x, entryVerticess[i].z + entry.Value.position.z);
                     }
                     for (int i = 0; i < verticess.Length; i++)
                     {
                         newPolygon[i] = new Vector2(verticess[i].x + position.x, verticess[i].z + position.z);
                     }

                     Vector2 worldPoint = new Vector2(position.x, position.z);
                     if(entryPolygon.Length>2 && newPolygon.Length>2)
                    if (IsPointInPolygon4(entryPolygon, worldPoint, newPolygon))
                     {*/
                    if (entryVerticess.Length > 2 && verticess.Length > 2)
                        if (IsPointInPolygon(entryVerticess, entry.Value.position, position, verticess))
                    {
                       
                        RpcRemovePlaneFromClient(id, playerNetID);
                            if (isServerOnly)
                                RemovePlane(id, playerNetID);
                        Debug.Log("Yes, Points are also in mesh");
                        samePos = true;
                            break;
                    }
                    else
                    {
                        Debug.Log("No");
                        samePos = false;
                    }

                    
                  
                }
            }
            if (!samePos)
            {
                Debug.Log("i was kept and updated");
                string verticesid = id.ToString() + playerNetID.ToString();
                if (verticesDict.ContainsKey(verticesid))
                {
                  
                    PlaneData pData;
                    // Debug.Log("Updating plane in cmd");
                    pData.position = position;
                    pData.rotation = rotation;
                    pData.Jvertice = json;
                    pData.id = id;
                    pData.boundarylength = boundarylength;
                    pData.playerNetID = playerNetID;

                    verticesDict[verticesid] = pData;

                    if(isServerOnly)
                    UpdatePlane(verticess, position, rotation, id, boundarylength, playerNetID);
                    RpcUpdatePlaneOnClient(json, position, rotation, id, boundarylength, playerNetID);
                   
                }
                else
                {
                    Debug.Log("Tried to Update Map Info that didn't exist");
                     verticesid = id.ToString() + playerNetID.ToString();

                     PlaneData pData;

                     pData.position = position;
                     pData.rotation = rotation;
                     pData.Jvertice = json;
                     pData.id = id;
                     pData.boundarylength = boundarylength;
                     pData.playerNetID = playerNetID;

                     verticesDict.Add(verticesid, pData);

                    if (isServerOnly)
                        AddPlane(verticess, position, rotation, id, boundarylength, playerNetID);
                    RpcAddPlaneToClient(json, position, rotation, id, boundarylength, playerNetID);
                }

            }
            else
            {
                samePos = false;
                Debug.Log("we dont update this");
            }
        }

    }

    [ClientRpc]//Plane adatból állítja elõ a mesh-t
    void RpcRemovePlaneFromClient(int id, uint playerNetID)
    {
        // if (isLocalPlayer)
        // {
        RemovePlane(id, playerNetID);
        // }
    }

    [ClientRpc]//Plane adatból állítja elõ a mesh-t
    void RpcUpdatePlaneOnClient(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
        // if (isLocalPlayer)
        // {
        var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
        Vector3[] verticess = vertices.ToArray();
        UpdatePlane(verticess, position, rotation, id, boundarylength, playerNetID);
        // }
    }

    [ClientRpc]//Plane adatból állítja elõ a mesh-t
    void RpcAddPlaneToClient(string json, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
        // if (isLocalPlayer)
        // {
        var vertices = JsonConvert.DeserializeObject<List<Vector3>>(json);
        Vector3[] verticess = vertices.ToArray();
        AddPlane(verticess, position, rotation, id, boundarylength, playerNetID);
        // }
    }

    public void RemovePlane(int id, uint playerNetID)
    {

        string idtoDict = id.ToString() + playerNetID.ToString();

        if (planesDict.ContainsKey(idtoDict))
        {
            planesDict[idtoDict].transform.parent = null;
            Destroy(planesDict[idtoDict].gameObject);
            if (planesDict[idtoDict] != null)
                planesDict.Remove(idtoDict);
            else
                Debug.Log("Nem volt mit eltavolitani");
        }

    }

    public void UpdatePlane(Vector3[] vertices, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
        if (readImage)//az új játékos beolvasta-e már az origót, mert addig nem hozza létre magának a többiek új planejeit
        {
            string idtoDict = id.ToString() + playerNetID.ToString();
            Mesh mesh = new Mesh();
            if (planesDict.ContainsKey(idtoDict))
            {
                if (boundarylength > 2)
                {
                    /// Debug.Log("updating plane on client");
                    int[] tria = new int[3 * (boundarylength - 2)];
                    for (int c = 0; c < boundarylength - 2; c++)
                    {
                        tria[3 * c] = 0;
                        tria[3 * c + 1] = c + 1;
                        tria[3 * c + 2] = c + 2;
                    }
                    mesh.vertices = vertices;
                    mesh.triangles = tria;
                    mesh.RecalculateNormals();
                    planesDict[idtoDict].GetComponent<MeshFilter>().mesh = mesh;
                    planesDict[idtoDict].GetComponent<MeshRenderer>().material = mat;
                    Destroy(planesDict[idtoDict].GetComponent<MeshCollider>());
                    planesDict[idtoDict].AddComponent<MeshCollider>();
                    planesDict[idtoDict].transform.localPosition = position;
                    planesDict[idtoDict].transform.localRotation = rotation;
                    

                    /*if (isServer)
                    {
                        NetworkServer.UnSpawn(planesDict[idtoDict].gameObject);
                       // NetworkServer.Spawn(planesDict[idtoDict]);
                    }*/
                }
            }
            else
                Debug.Log("Tried to update a plane that didn't exist");
            //}
        }
    }

    public void AddPlane(Vector3[] vertices, Vector3 position, Quaternion rotation, int id, int boundarylength, uint playerNetID)
    {
        if (readImage)//az új játékos beolvasta-e már az origót, mert addig nem hozza létre magának a többiek új planejeit
        {
            Debug.Log("im in add plane");
           
            ARAnchor anchor = null;
            string idtoDict = id.ToString() + playerNetID.ToString();
            worldMap = GameObject.Find("WorldMap");
            while (worldMap == null)
                Debug.Log("coudnt find worldmap");
            if (worldMap != null)
                Debug.Log("Found worldmap");

            Mesh mesh = new Mesh();
            if (planesDict.ContainsKey(idtoDict))
            {
                Debug.Log("It tried to add a plane that already existed");
                return;
            }

            Debug.Log("Adding Plane on client");
            GameObject newMeshF = Instantiate(meshF);

            if (worldMap != null)
            {
                newMeshF.transform.parent = worldMap.transform;
                //Debug.Log("Parenting it, child of worldMap: " + worldMap.transform.childCount.ToString() + " planesDict count: " + planesDict.Count);
            }
            else
            {

                while (worldMap != null)
                {
                    worldMap = GameObject.Find("WorldMap");
                    newMeshF.transform.parent = worldMap.transform;
                    Debug.Log("trying to locate worldmap");
                }

            }
            newMeshF.transform.parent = worldMap.transform;
           // Debug.Log("Parenting it, child of worldMap: " + worldMap.transform.childCount.ToString() + " planesDict count: " + planesDict.Count);
            if (boundarylength > 2)
            {
                int[] tria = new int[3 * (boundarylength - 2)];
                for (int c = 0; c < boundarylength - 2; c++)
                {
                    tria[3 * c] = 0;
                    tria[3 * c + 1] = c + 1;
                    tria[3 * c + 2] = c + 2;
                }
                mesh.vertices = vertices;
                mesh.triangles = tria;
                mesh.RecalculateNormals();
                newMeshF.GetComponent<MeshFilter>().mesh = mesh;
                newMeshF.GetComponent<MeshRenderer>().material = mat;
                newMeshF.AddComponent<MeshCollider>();
                newMeshF.AddComponent<Rigidbody>().isKinematic = true;
                newMeshF.transform.localPosition = position;
                newMeshF.transform.localRotation = rotation;
                anchor = newMeshF.GetComponent<ARAnchor>();
               
                /*if(isServer)
                NetworkServer.Spawn(newMeshF);*/
                planesDict.Add(idtoDict, newMeshF);
                if (anchor == null)
                {
                    anchor = newMeshF.AddComponent<ARAnchor>();
                    /*if (anchor != null)
                    {
                        Debug.Log("Anchor added");
                    }*/
                }
                m_Anchors.Add(anchor);
                // }
            }
        }
    }
    void Update()
    {
        if (OneWay)
        {
            if (players.Length < 2)
            {
                if (readImage == true && planeManager.enabled == false)
                {
                    Debug.Log("planemanager was disabled");
                    planeManager.enabled = true;
                }

            }
        }
        else
        {
            if (readImage == true && planeManager.enabled == false)
            {
                Debug.Log("planemanager was disabled");
                planeManager.enabled = true;
            }
        }

    }

    

    public static bool IsPointInPolygon4(Vector2[] entryPolygon, Vector2 centerPoint, Vector2[] newPolygon)
    {
        float minX = entryPolygon[0].x;
        float maxX = entryPolygon[0].x;
        float minY = entryPolygon[0].y;
        float maxY = entryPolygon[0].y;
        bool result = false;
        int j = entryPolygon.Length - 1;
        for (int i = 0; i < entryPolygon.Length; i++)
        {
            Vector2 q = entryPolygon[i];
            minX = Mathf.Min(q.x, minX);
            maxX = Mathf.Max(q.x, maxX);
            minY = Mathf.Min(q.y, minY);
            maxY = Mathf.Max(q.y, maxY);
            if (entryPolygon[i].y < centerPoint.y && entryPolygon[j].y >= centerPoint.y || entryPolygon[j].y < centerPoint.y && entryPolygon[i].y >= centerPoint.y)
            {
                if (entryPolygon[i].x + (centerPoint.y - entryPolygon[i].y) / (entryPolygon[j].y - entryPolygon[i].y) * (entryPolygon[j].x - entryPolygon[i].x) < centerPoint.x)
                {
                    result = !result;
                }
            }
            j = i;
        }
        if (!result)
            return result;
        else
        {
            Debug.Log("center was inside");
            for (int i = 0; i < newPolygon.Length; i++)
            {
                if (newPolygon[i].x < minX || newPolygon[i].x > maxX || newPolygon[i].y < minY || newPolygon[i].y > maxY)
                {
                    Debug.Log("Point wasnt inside");
                    return false;
                }
            }
            Debug.Log("Point was inside");
            return true;
        }
    }

    public static bool IsPointInPolygon(Vector3[] entryPolygon, Vector3 entryCenter, Vector3 centerPoint, Vector3[] newPolygon)
    {
        float minX = entryPolygon[0].x + entryCenter.x;
        float maxX = entryPolygon[0].x + entryCenter.x;
        float minY = entryPolygon[0].z + entryCenter.z;
        float maxY = entryPolygon[0].z + entryCenter.z;
        bool result = false;
        int j = entryPolygon.Length - 1;
        for (int i = 0; i < entryPolygon.Length; i++)
        {
            Vector3 q = entryPolygon[i] + entryCenter;
            minX = Mathf.Min(q.x, minX);
            maxX = Mathf.Max(q.x, maxX);
            minY = Mathf.Min(q.z, minY);
            maxY = Mathf.Max(q.z, maxY);
            ///////
            /////megnézi hogy a koordináta a két határoló pont magassága közé esik-e
            ///aztán egy egyenletre megvizsgálja hogy  a pont a planeben vagy azon kívül van-e
            //////
            if (entryCenter.z + entryPolygon[i].z < centerPoint.z && entryCenter.z + entryPolygon[j].z >= centerPoint.z || entryCenter.z + entryPolygon[j].z < centerPoint.z && entryCenter.z + entryPolygon[i].z >= centerPoint.z)
            {
                if (entryCenter.x + entryPolygon[i].x + (centerPoint.z - entryCenter.z + entryPolygon[i].z) / ((entryCenter.z + entryPolygon[j].z) - (entryCenter.z + entryPolygon[i].z)) * ((entryCenter.x + entryPolygon[j].x) - (entryCenter.x + entryPolygon[i].x)) < centerPoint.x)
                {
                    result = !result;
                }
            }
            j = i;
        }
        if (!result)
            return result;
        else
        {
            Debug.Log("center was inside");
            for (int i = 0; i < newPolygon.Length; i++)
            {
                if (centerPoint.x + newPolygon[i].x < minX || centerPoint.x + newPolygon[i].x > maxX || centerPoint.z + newPolygon[i].z < minY || centerPoint.z + newPolygon[i].z > maxY)
                {
                    Debug.Log("Point wasnt inside");
                    return false;
                }
            }
            Debug.Log("Point was inside");
            return true;
        }
    }
}