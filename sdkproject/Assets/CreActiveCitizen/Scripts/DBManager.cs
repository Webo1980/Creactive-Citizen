using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Mapbox.Json;
using Mapbox.Json.Serialization;
using System;

public class DBManager : MonoBehaviour
{
    //if (SystemInfo.deviceType == DeviceType.Console)
    /*#if UNITY_EDITOR
        private readonly string sendDataURL = "https://creative-citizen.wineme.wiwi.uni-siegen.de/";
    #else
            private readonly string sendDataURL = "http://localhost/unity_test/";
    #endif*/
    private static readonly string sendDataURL = "https://creative-citizen.wineme.wiwi.uni-siegen.de/";
    public string coordinatesList;
    public static string objectsList;
    public string userID;
    public string forgetMessage;
    public string changeMessage;
    public string registerMessage;
    public string verifyMessage;
    public static bool isDrawing = false;

    // remember to use StartCoroutine when calling this function!
    public static IEnumerator PostCoordinates(int objectID, int userID, float positionX, float positionY, float positionZ
                                       , float rotationX, float rotationY, float rotationZ, float scale, string coordinatesData)
    {
        List<IMultipartFormSection> wwwform = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("objectID", objectID.ToString()),
            new MultipartFormDataSection("userID",  userID.ToString()),
            new MultipartFormDataSection("positionX",  positionX.ToString()),
            new MultipartFormDataSection("positionY",  positionY.ToString()),
            new MultipartFormDataSection("positionZ",  positionZ.ToString()),
            new MultipartFormDataSection("rotationX",  rotationX.ToString()),
            new MultipartFormDataSection("rotationY",  rotationY.ToString()), // BitConverter.GetBytes(
            new MultipartFormDataSection("rotationZ",  rotationZ.ToString()),
            new MultipartFormDataSection("scale",  scale.ToString()),
            new MultipartFormDataSection("coordinatesData", coordinatesData),            
        };
        UnityWebRequest www = UnityWebRequest.Post(sendDataURL + "addCoordinates.php", wwwform);
        yield return www.SendWebRequest();
		if (www.isNetworkError || www.isHttpError)
		{
			Debug.LogError(www.error);
		}
		else {
			Debug.Log(www.downloadHandler.text);
		}
    }

	public IEnumerator GetCoordinates(string coordinatesData, System.Action<bool> callback)
	{
        List<IMultipartFormSection> wwwform = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("coordinatesData", coordinatesData)
        };
        UnityWebRequest www = UnityWebRequest.Post(sendDataURL + "getCoordinates.php", wwwform);
		yield return www.SendWebRequest();
		if (www.isNetworkError || www.isHttpError)
		{
			Debug.LogError(www.error);
		}
		else
		{
			coordinatesList= www.downloadHandler.text;
			callback(true);
		}
	}

    public static IEnumerator GetObjectsLists(System.Action<bool> callback)
    {
        List<IMultipartFormSection> wwwform = new List<IMultipartFormSection>
        {
            //new MultipartFormDataSection("coordinatesData", coordinatesData)
        };
        UnityWebRequest www = UnityWebRequest.Post(sendDataURL + "getObjectsList.php", wwwform);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            objectsList = www.downloadHandler.text;
            callback(true);
        }
    }

    public IEnumerator CheckLoginData(string userName, string password, System.Action<bool> callback)
    {
        List<IMultipartFormSection> wwwform = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("userName", userName),
            new MultipartFormDataSection("password", password)
        };
        Debug.Log("DB password: " + password);
        UnityWebRequest www = UnityWebRequest.Post(sendDataURL + "checkUser.php", wwwform);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
           Debug.LogError(www.error);
        }
        else
        {
           userID = www.downloadHandler.text;
           callback(true);
        }
    }

    public IEnumerator RecoverPassword(string userName, System.Action<bool> callback)
    {
        List<IMultipartFormSection> wwwform = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("userName", userName)
        };
        UnityWebRequest www = UnityWebRequest.Post(sendDataURL + "forgetPassword.php", wwwform);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            forgetMessage = www.downloadHandler.text;
            callback(true);
        }
    }

    public IEnumerator ChangePassword(string code, string newPassword,System.Action<bool> callback)
    {
        List<IMultipartFormSection> wwwform = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("code", code),
            new MultipartFormDataSection("newPassword", newPassword)
        };
        UnityWebRequest www = UnityWebRequest.Post(sendDataURL + "changePassword.php", wwwform);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            changeMessage = www.downloadHandler.text;
            callback(true);
        }
    }

    public IEnumerator Register(string userName,string password, System.Action<bool> callback)
    {
        List<IMultipartFormSection> wwwform = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("userName", userName),
            new MultipartFormDataSection("password", password)
        };
        UnityWebRequest www = UnityWebRequest.Post(sendDataURL + "register.php", wwwform);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            registerMessage = www.downloadHandler.text;
            callback(true);
        }
    }

    public IEnumerator AfterRegistration(string userName, string code, System.Action<bool> callback)
    {
        List<IMultipartFormSection> wwwform = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("userName", userName),
            new MultipartFormDataSection("code", code)
        };
        UnityWebRequest www = UnityWebRequest.Post(sendDataURL + "verifyAccount.php", wwwform);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            verifyMessage = www.downloadHandler.text;
            callback(true);
        }
    }
}