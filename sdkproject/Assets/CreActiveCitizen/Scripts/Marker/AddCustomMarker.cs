namespace Mapbox.Examples
{
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
        Vector2d _location;

        [SerializeField]
        AbstractMap _map;

        [SerializeField]
        float _spawnScale = 1f;

        //[SerializeField]
        GameObject _markerPrefab;

        List<GameObject> _spawnedObjects;

        private readonly DBManager DB = new DBManager();

        private readonly MarkersMenu markersMenu = new MarkersMenu();

        public Vector2d initialStartLocation;

        private int loggedUserID;

        private GameObject MarkerInstance;

        JObject selectedMarkerData;

        private IEnumerator GetStartLocation(System.Action<bool> callback)
        {
            yield return new WaitForSeconds(1);  // wait until the camera is loaded
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 1.0f));
            initialStartLocation = _map.WorldToGeoPosition(worldPos);
            callback(true);
        }

        void SpawnMarkersOnStart(Vector2d initialLocation)
        {
            StartCoroutine(DB.GetCoordinates(initialLocation.ToString(), (returnedCoordinatesList) => {
                if (returnedCoordinatesList)
                {
                    var DBCoordinatesList = JToken.Parse(DB.coordinatesList);
                    _spawnedObjects = new List<GameObject>();
                    Debug.Log("Coord List:"+DBCoordinatesList);
                    foreach (var item in DBCoordinatesList)
                    {
                        double latitude = item["latitude"].ToObject<double>();
                        double longitude = item["longitude"].ToObject<double>();
                        _location = new Vector2d(latitude, longitude);
                        Debug.Log("location:" + item);
                        GameObject instance = Instantiate(Resources.Load(item["objectName"].ToObject<string>(), typeof(GameObject))) as GameObject;
                        //var instance = Instantiate(_markerPrefab);
                        //ChangeMarkerText(instance, item["userID"].ToObject<int>());
                        instance.transform.localPosition = _map.GeoToWorldPosition(_location, true);

                        /*Vector3 localPos = instance.transform.localPosition;
                        localPos.y = item["positionY"].ToObject<float>();
                        instance.transform.localPosition = localPos;*/
                        instance.transform.localPosition = new Vector3(item["positionX"].ToObject<float>(), item["positionY"].ToObject<float>(), item["positionZ"].ToObject<float>());
                        //instance.transform.localPosition = new Vector3(item["positionX"].ToObject<float>(), item["positionY"].ToObject<float>(), item["positionZ"].ToObject<float>());
                        //instance.transform.TransformPoint (transform.localPosition.x, item["positionY"].ToObject<float>(), transform.localPosition.z);
                        instance.transform.rotation = Quaternion.Euler(item["rotationX"].ToObject<float>(), item["rotationY"].ToObject<float>(), item["rotationZ"].ToObject<float>());
                        instance.transform.localScale = new Vector3(item["scale"].ToObject<float>(), item["scale"].ToObject<float>(), item["scale"].ToObject<float>());
                        //instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
                        _spawnedObjects.Add(instance);
                    }
                }
            }));
        }

        void InitializeMarkersOnStart()
        {
            Debug.Log("InitializeMarkersOnStart was called");
            StartCoroutine(GetStartLocation((returnedInitialLocation) => {
                if (returnedInitialLocation)
                {
                    SpawnMarkersOnStart(initialStartLocation);
                }
            }));

        }

        public JObject GetMarkerInfo()
        {
            JToken objectsList = MarkersMenu.Instance.dropdownObjects;
            List<JToken> markersData = objectsList.Where(t => (string)t["german_name"] == MarkersMenu.Instance.m_option).ToList();
            string selectedMarkerObject = System.String.Join("\n", markersData.Select(v => v.ToString(Formatting.None)));
            JObject selectedMarkerData = JObject.Parse(selectedMarkerObject);
            return selectedMarkerData;
        }

        void ChangeMarkerText(GameObject obj, int userID)
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

        void ChangeVisibility(string ObjectName, float Alpha, bool BlocksRaycasts, bool Interactable)
        {
            CanvasGroup ItemCanvas;
            GameObject Item = GameObject.Find(ObjectName);
            ItemCanvas = Item.GetComponent<CanvasGroup>();
            ItemCanvas.alpha = Alpha;
            ItemCanvas.blocksRaycasts = BlocksRaycasts;
            ItemCanvas.interactable = Interactable;
        }

        float GetObjectVisibility(string ObjectName)
        {
            CanvasGroup ItemCanvas;
            GameObject Item = GameObject.Find(ObjectName);
            ItemCanvas = Item.GetComponent<CanvasGroup>();
            return ItemCanvas.alpha;
        }

        void ChangeUIText(string ObjectName, string message)
        {
            Text messageText = GameObject.Find("Canvas/"+ ObjectName).GetComponent<Text>();
            messageText.text = message;
        }

        void AddMarker(Vector3 touchData)
        {
            Ray ray = Camera.main.ScreenPointToRay(touchData); // Construct a ray from the current touch coordinates
            Plane plane = new Plane(Vector3.up, transform.position);
            // this will return the distance from the camera
            if (plane.Raycast(ray, out float distance))  // if plane hit...
            {
                Vector3 position = ray.GetPoint(distance); // get the point
                _location = _map.WorldToGeoPosition(position);
                selectedMarkerData = GetMarkerInfo();
                MarkerInstance = Instantiate(Resources.Load(selectedMarkerData["name"].ToObject<string>(), typeof(GameObject))) as GameObject;

                MarkerInstance.AddComponent<LeanTouch>();
                var Scale = MarkerInstance.AddComponent<LeanPinchScale>();
                Scale.Sensitivity = 1f;
                Scale.Dampening = -1;
                var Twist = MarkerInstance.AddComponent<LeanTwistRotateAxis>();
                Space World = default;
                Twist.Space = World;

                var MoveY = MarkerInstance.AddComponent<MoveYAxis>();
                MoveY.speed = 0.95f;
                
                //Destroy(instance.GetComponent<RotateObject>());
                //ChangeMarkerText(MarkerInstance, loggedUserID);
                //var instance = Instantiate(_markerPrefab);
                //PlaneInstance.transform.localPosition = _map.GeoToWorldPosition(_location, true);
                //PlaneInstance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);

                MarkerInstance.transform.localPosition = _map.GeoToWorldPosition(_location, true);
                MarkerInstance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
                ChangeVisibility("Cancle", 1f, true, true);
                ChangeVisibility("Apply", 1f, true, true);
                ChangeVisibility("StartMessage", 1.0f, true, true);
                ChangeUIText("StartMessage", "Sie können das Objekt drehen, skalieren und vertikal verschieben");
                //StartCoroutine(DB.PostCoordinates(_location.ToString(), selectedMarkerData["id"].ToObject<string>(), loggedUserID));
                //_spawnedObjects.Add(instance);
                //Debug.Log(selectedMarkerData["name"] + " Change Menu:" + markersMenu.isChanged);
                //selectedMarkerData["name"] = "";
            }
        }

        public void SaveMarkerInfo()
        {
            //Debug.Log("Trans:"+MarkerInstance.transform.localPosition.ToString("F7"));
            Vector3 position = MarkerInstance.transform.position;
            float rotationX = (MarkerInstance.transform.eulerAngles.x < 180f) ? MarkerInstance.transform.eulerAngles.x : MarkerInstance.transform.eulerAngles.x - 360;
            float rotationY = (MarkerInstance.transform.eulerAngles.y < 180f) ? MarkerInstance.transform.eulerAngles.y : MarkerInstance.transform.eulerAngles.y - 360;
            float rotationZ = (MarkerInstance.transform.eulerAngles.z < 180f) ? MarkerInstance.transform.eulerAngles.z : MarkerInstance.transform.eulerAngles.z - 360;
            Vector3 scale = MarkerInstance.transform.localScale;
            //Debug.Log("RotConv:" + RotX+","+RotY+","+RotZ); // Check the docs about converting
            //Debug.Log("Rot:" + MarkerInstance.transform.rotation.eulerAngles.ToString("F7")); // Check the docs about converting
            //Debug.Log("Scale:" + MarkerInstance.transform.localScale.ToString("F7"));
            //Debug.Log("Loc:" + _location);
            StartCoroutine(DB.PostCoordinates(selectedMarkerData["id"].ToObject<int>(), loggedUserID, position.x
                           , position.y, position.z,rotationX,rotationY,rotationZ, scale.x, _location.ToString()));
            ChangeVisibility("Apply", 0f, false, false);
            ChangeVisibility("Cancle", 0f, false, false);
            Destroy(MarkerInstance.GetComponent<LeanTouch>());
            Destroy(MarkerInstance.GetComponent<LeanTwistRotateAxis>());
            Destroy(MarkerInstance.GetComponent<LeanPinchScale>()); 
            Destroy(MarkerInstance.GetComponent<MoveYAxis>());
        }

        public void DeleteMarker()
        {
            Destroy(MarkerInstance);
            ChangeVisibility("Apply", 0f, false, false);
            ChangeVisibility("Cancle", 0f, false, false);
        }

        void Start()
        {
            loggedUserID = System.Int16.Parse(loginManager.Instance.currentUserID);
            InitializeMarkersOnStart();
            //print("Start is called, and userID="+ loginManager.Instance.currentUserID);
        }

        void Update()
        {
            _map.UseWorldScale();  // tell the app, hi we are in AR envionment
            //print("Current userID=" + loginManager.Instance.currentUserID);
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                ChangeVisibility("StartMessage", 0.0f, false, false);
                MarkersMenu.Instance.ShowMenu();
                Vector3 touchData = Input.GetTouch(0).position;
                if (GetObjectVisibility("Apply") <= 0)
                {
                    AddMarker(touchData);
                }
                
                //if (touchData.y > 125)
                //{ // the markers only should be added, if the user hits above the map line

                //}
            }
            
        }
    }
}
