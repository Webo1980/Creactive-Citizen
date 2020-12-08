namespace Mapbox.Examples
{
    using UnityEngine;
    using UnityEngine;
    using Mapbox.Utils;
    using Mapbox.Unity.Map;
    using Mapbox.Unity.MeshGeneration.Factories;
    using Mapbox.Unity.Utilities;
    using System.Collections.Generic;
    using System.Collections;
    using Mapbox.Json.Linq;
    using System.Linq;
    using TMPro;
    using Mapbox.Json;
    using Lean;
    using Lean.Touch;
    using UnityEngine.UI;

    //using Mapbox.Json;

    public class AddCustomMarker : MonoBehaviour
    {
        [Geocode]
        Vector2d _location;  // to save the current location

        [SerializeField]
        AbstractMap _map;    // the used map

        [SerializeField]
        float _spawnScale = 1f;  // at what size should the marker spawned on the screen

        
        List<GameObject> _spawnedObjects;   // list of markers to be loaded on the application start

        public DBManager DB = new DBManager();  // instance of the DB class


        private readonly MarkersMenu markersMenu = new MarkersMenu();  // Instance of the menu class

        public Vector2d initialStartLocation;   // a variable to store the current user location, once the app is started

        public int loggedUserID;  // global variable saves the current logged user ID

        public Plane MarkerPlane;

        private GameObject MarkerInstance;  // to save the spawned marker data

        private GameObject CloseMarkerInstance;  // to save the marker close icon 

        JObject selectedMarkerData;  // save the currrent marker from the menu data

        public AudioSource audioSource;  //audio source

        private IEnumerator GetStartLocation(System.Action<bool> callback)  // get the current user location once the GPS scene is loaded
        {
            yield return new WaitForSeconds(1);  // wait until the camera is loaded
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 1.0f)); // get the current location of screen center related to the real world
            initialStartLocation = _map.WorldToGeoPosition(worldPos); // assign the current GEO position to variable
            callback(true);
        }

        void SpawnMarkersOnStart(Vector2d initialLocation) // use the start location to search the database
        {
            // Search the database using the start location
            StartCoroutine(DB.GetCoordinates(initialLocation.ToString(), (returnedCoordinatesList) => {
                if (returnedCoordinatesList)
                {
                    var DBCoordinatesList = JToken.Parse(DB.coordinatesList); // save the JSON data in a list
                    _spawnedObjects = new List<GameObject>();
                    foreach (var item in DBCoordinatesList) // loop through the list
                    {
                        if (loggedUserID == item["userID"].ToObject<int>()) // show the close icon only for th users' markers
                        {
                           CloseMarkerInstance = GameObject.Find("CloseMarker");  // search the closeMarker in the scene
                           CloseMarkerInstance.GetComponent<MeshRenderer>().enabled = true;  // show the close icon
                        }
                        double latitude = item["latitude"].ToObject<double>();  // assign the latitude to a variable 
                        double longitude = item["longitude"].ToObject<double>();  // assign the longitude to a variable
                        _location = new Vector2d(latitude, longitude);
                        // search the resources folder for the selected marker to spawn it.
                        GameObject instance = Instantiate(Resources.Load(item["objectName"].ToObject<string>(), typeof(GameObject))) as GameObject;
                        //ChangeMarkerText(instance, item["userID"].ToObject<int>()); 
                        // add the marker to the AR world
                        instance.transform.localPosition = _map.GeoToWorldPosition(_location, true);
                        // get the last known position for the marker and add it
                        instance.transform.localPosition = new Vector3(item["positionX"].ToObject<float>(), item["positionY"].ToObject<float>(), item["positionZ"].ToObject<float>());
                        // get the last known rotation for the marker and add it
                        instance.transform.rotation = Quaternion.Euler(item["rotationX"].ToObject<float>(), item["rotationY"].ToObject<float>(), item["rotationZ"].ToObject<float>());
                        // get the last known scale for the marker and add it
                        instance.transform.localScale = new Vector3(item["scale"].ToObject<float>(), item["scale"].ToObject<float>(), item["scale"].ToObject<float>());
                        // add the markers objects to the final list
                        _spawnedObjects.Add(instance);
                    }
                }
            }));
        }

        void InitializeMarkersOnStart()  // start intitializing the markers list when ready on start
        {
            StartCoroutine(GetStartLocation((returnedInitialLocation) => {
                if (returnedInitialLocation)
                {
                    SpawnMarkersOnStart(initialStartLocation);
                }
            }));

        }

        public JObject GetMarkerInfo()  // get the selected marker data
        {
            JToken objectsList = MarkersMenu.Instance.dropdownObjects;
            // search the marker object (in english) using the german name 
            List<JToken> markersData = objectsList.Where(t => (string)t["german_name"] == MarkersMenu.Instance.m_option).ToList();
            string selectedMarkerObject = System.String.Join("\n", markersData.Select(v => v.ToString(Formatting.None)));
            JObject selectedMarkerData = JObject.Parse(selectedMarkerObject);
            return selectedMarkerData;
        }

        void ChangeMarkerText(GameObject obj, int userID) // add a text to the markers. It is commented now in funtion SpawnMarkersOnStart, as the text UI should be enhanced
        {
            TextMesh textObject = (TextMesh)obj.GetComponentInChildren(typeof(TextMesh));
            if (loggedUserID == userID)
            {
                textObject.text = "Von Dir";
            }
            else
            {
                textObject.text = "Benutzer ID:" + userID;
            }
        }

        // this function is used to show/ hide (text, objects, icons), that has componant Canvas Group
        public static void ChangeVisibility(string ObjectName, float Alpha, bool BlocksRaycasts, bool Interactable)
        {
            CanvasGroup ItemCanvas;
            GameObject Item = GameObject.Find(ObjectName);
            ItemCanvas = Item.GetComponent<CanvasGroup>();
            ItemCanvas.alpha = Alpha;
            ItemCanvas.blocksRaycasts = BlocksRaycasts;
            ItemCanvas.interactable = Interactable;
        }

        // to know whether the object is shown/ hidden
        float GetObjectVisibility(string ObjectName)
        {
            CanvasGroup ItemCanvas;
            GameObject Item = GameObject.Find(ObjectName);
            ItemCanvas = Item.GetComponent<CanvasGroup>();
            return ItemCanvas.alpha;
        }

        // used to change the text message
        void ChangeUIText(string ObjectName, string message)
        {
            Text messageText = GameObject.Find("Canvas/"+ ObjectName).GetComponent<Text>();
            messageText.text = message;
        }

        // this function is using an asset called Lean Touch to move the obejct
        void MoveMarker(GameObject obj)
        {
            obj.AddComponent<LeanTouch>();
            var Scale = obj.AddComponent<LeanPinchScale>();
            Scale.Sensitivity = 1f;
            Scale.Dampening = -1;
            var Twist = obj.AddComponent<LeanTwistRotateAxis>();
            Space World = default;
            Twist.Space = World;
            var Move = obj.AddComponent<LeanDragTranslate>();
        }
        
        bool AddMarker(Vector3 touchData) // all the magic happens here
        {
            Ray ray = Camera.main.ScreenPointToRay(touchData); // Construct a ray from the current touch coordinates
            MarkerPlane = new Plane(Vector3.up, transform.position);
            bool isMarkerSet = false;
            // this will return the distance from the camera
            if (MarkerPlane.Raycast(ray, out float distance))  // if plane hit...
            {
                Vector3 position = ray.GetPoint(distance); // get the point
                _location = _map.WorldToGeoPosition(position);
                selectedMarkerData = GetMarkerInfo();
                MarkerInstance = Instantiate(Resources.Load(selectedMarkerData["name"].ToObject<string>(), typeof(GameObject))) as GameObject;

                MoveMarker(MarkerInstance);
                
                MarkerInstance.transform.localPosition = _map.GeoToWorldPosition(_location, true);
                
                //ChangeMarkerText(MarkerInstance, loggedUserID);

                MarkerInstance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
                PlayClip(audioSource, "SetMarker");
                ChangeVisibility("Cancle", 1f, true, true);
                ChangeVisibility("Apply", 1f, true, true);
                //StartCoroutine(DB.PostCoordinates(_location.ToString(), selectedMarkerData["id"].ToObject<string>(), loggedUserID));
                //_spawnedObjects.Add(instance);
                //Debug.Log(selectedMarkerData["name"] + " Change Menu:" + markersMenu.isChanged);
                //selectedMarkerData["name"] = "";
                isMarkerSet = true;
            }
            return isMarkerSet;
        }


        public void SaveMarkerInfo()
        {
            Debug.Log("Trans:"+MarkerInstance.transform.localPosition.ToString("F7"));
            Vector3 position = MarkerInstance.transform.position;
            float rotationX = (MarkerInstance.transform.eulerAngles.x < 180f) ? MarkerInstance.transform.eulerAngles.x : MarkerInstance.transform.eulerAngles.x - 360;
            float rotationY = (MarkerInstance.transform.eulerAngles.y < 180f) ? MarkerInstance.transform.eulerAngles.y : MarkerInstance.transform.eulerAngles.y - 360;
            float rotationZ = (MarkerInstance.transform.eulerAngles.z < 180f) ? MarkerInstance.transform.eulerAngles.z : MarkerInstance.transform.eulerAngles.z - 360;
            Vector3 scale = MarkerInstance.transform.localScale;
            //Debug.Log("RotConv:" + RotX+","+RotY+","+RotZ); // Check the docs about converting
            //Debug.Log("Rot:" + MarkerInstance.transform.rotation.eulerAngles.ToString("F7")); // Check the docs about converting
            //Debug.Log("Scale:" + MarkerInstance.transform.localScale.ToString("F7"));
            //Debug.Log("Loc:" + _location);
            IEnumerator PostData = DBManager.PostCoordinates(selectedMarkerData["id"].ToObject<int>(), loggedUserID, position.x
                           , position.y, position.z, rotationX, rotationY, rotationZ, scale.x, _location.ToString());
            StartCoroutine(PostData);
            //StartCoroutine(DBManager.PostCoordinates(selectedMarkerData["id"].ToObject<int>(), loggedUserID, position.x
              //             , position.y, position.z,rotationX,rotationY,rotationZ, scale.x, _location.ToString()));
            ChangeVisibility("Apply", 0f, false, false);
            ChangeVisibility("Cancle", 0f, false, false);
            ChangeVisibility("MessageToUser", 0f, false, false); 
            selectedMarkerData = GetMarkerInfo();
            CloseMarkerInstance = GameObject.Find("CloseMarker"+ selectedMarkerData["name"].ToObject<string>());
            CloseMarkerInstance.GetComponent<MeshRenderer>().enabled = true;
            Destroy(MarkerInstance.GetComponent<LeanTouch>());
            Destroy(MarkerInstance.GetComponent<LeanTwistRotateAxis>());
            Destroy(MarkerInstance.GetComponent<LeanPinchScale>());
            Destroy(MarkerInstance.GetComponent<LeanDragTranslate>());
            PlayClip(audioSource, "SaveMarker");
        }

        public void CancelAddingMarker()
        {
            Destroy(MarkerInstance);
            ChangeVisibility("Apply", 0f, false, false);
            ChangeVisibility("Cancle", 0f, false, false);
            ChangeVisibility("MessageToUser", 0f, false, false);
            PlayClip(audioSource,"DeleteMarker");
        }

        public static void PlayClip(AudioSource audioSource,string audioClipName)
        {
            AudioClip PlayedClip = (AudioClip)Resources.Load("Sounds/" + audioClipName);
            if (PlayedClip != null)
            {
                //Debug.Log("Sounds/" + audioClipName);
                //Debug.Log("Sourcs=" + audioSource);
                audioSource.clip = PlayedClip;
                audioSource.Play();
            }
        }

        void Start()
        {
            loggedUserID = System.Int16.Parse(loginManager.Instance.currentUserID);
            InitializeMarkersOnStart();
        }

        void Update()
        {
            _map.UseWorldScale();  // tell the app, hi we are in AR envionment
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && DrawOnScreen.isDrawing == false)
            {
                Debug.Log("Start adding Markers");
                Vector3 touchData = Input.GetTouch(0).position;
                if (GetObjectVisibility("Apply") <= 0)
                {
                    if(AddMarker(touchData)==true)
                    { 
                        ChangeVisibility("MessageToUser", 1f, true, true);
                        ChangeUIText("MessageToUser", "Sie können das Objekt drehen, skalieren und vertikal verschieben");
                    }
                }
            }

            if(DrawOnScreen.isDrawing == true)
            {
                DrawOnScreen.StartDrawing();
            }
        }
    }
}
