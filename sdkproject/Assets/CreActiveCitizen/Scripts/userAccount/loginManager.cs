using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class loginManager : MonoBehaviour
{
    [SerializeField]
    private InputField userNameField;  // the input field name from design
    [SerializeField]
    private InputField passwordField;
    [SerializeField]
    private InputField repeatPasswordField;
    [SerializeField]
    private InputField codeField;

    private DBManager DB = new DBManager();
    public string currentUserID;
    public static loginManager Instance;
    string userName;
    string password;
    //Button.GetComponent<Button>().onClick.AddListener(() => { LoadScene(0); LoadScene();}); 
    void Awake() // to store data between scenes 
    {
        //Let the gameobject persist over the scenes
        DontDestroyOnLoad(gameObject);
        //Check if the Instance instance is null
        if (Instance == null)
        {
            //This instance becomes the single instance available
            Instance = this;
        }
        //Otherwise check if the Instance instance is not this one
        else if (Instance != this)
        {
            //In case there is a different instance destroy this one.
            Destroy(gameObject);
        }
    }

    /*void Start()
    {
       //userNameField = GameObject.Find("userNameField").GetComponent<InputField>().text;
        //[SerializeField]
        //passwordField = GameObject.Find("passwordField").GetComponent<InputField>().text;
    }*/
    //Save data to login manager  
    /*public void SavePlayer()
    {
        loginManager.Instance.userNameField = userNameField;
        loginManager.Instance.passwordField = passwordField;
        loginManager.Instance.repeatPasswordField = repeatPasswordField;
        loginManager.Instance.codeField = codeField;
    }

    //At start, load data from login manager.
    void Start()
    {
        //string v =GameObject.Find("userNameField").GetComponent<InputField>().text;
        DontDestroyOnLoad(userNameField);
        DontDestroyOnLoad(passwordField);
        //Debug.Log("Start userName=" + v);
        //userNameField.text = v;
        userNameField = loginManager.Instance.userNameField;
        passwordField = loginManager.Instance.passwordField;
        repeatPasswordField = loginManager.Instance.repeatPasswordField;
        codeField = loginManager.Instance.codeField;
    }*/

    
    public void CheckLoginInfo() {
        //string userName = GameObject.Find("userNameField").GetComponent<InputField>().text.Trim().ToLower();
        //string password = GameObject.Find("passwordField").GetComponent<InputField>().text;
        userName = Instance.userNameField.text.Trim().ToLower();
        password = Instance.passwordField.text;
        Text wrongLoginInfo = GameObject.Find("Canvas/ErrorPanel").GetComponent<Text>();
        Debug.Log("userName=" + userName);
        Debug.Log("password=" + password);
        if (userName == "" || password == "")
        {
            wrongLoginInfo.text= "Sowohl Benutzername als auch Passwort sollten ausgefüllt werden";
        }
        else
        {
            Debug.Log("userNameAfter=" + userName);
            Debug.Log("passwordAfter=" + password);
            Debug.Log("Active? "+gameObject.activeInHierarchy);
            //Debug.Break();
            StartCoroutine(Instance.DB.CheckLoginData(Instance.userName, Instance.password, (returnedMessage) => {
                if (returnedMessage) {
                    //Debug.Log("login message: "+DB.userID);
                    if (DB.userID == "0") {
                        wrongLoginInfo.text = "Benutzername oder Kennwort ist ungültig ";
                    }
                    else
                    {
                        loginManager.Instance.currentUserID=DB.userID;
                        //Debug.Log("LoginManager ID: " + loginManager.Instance.currentUserID);
                        LoadScene(1);  // GPS Scene
                    }
                }
           }));
        }
    }

    public void ForgetPassword(){
        string userName = userNameField.text.Trim().ToLower();
        Text wrongLoginInfo = GameObject.Find("Canvas/ErrorPanel").GetComponent<Text>();
        Debug.Log("I am in forget password");
        StartCoroutine(DB.RecoverPassword(userName, (returnedMessage) => {
            if (returnedMessage)
            {
                if (DB.forgetMessage == "User is not registered")
                {
                    wrongLoginInfo.text = "E-Mail ist ungültig oder ist nicht existiert";
                }
                else
                {
                    if (DB.forgetMessage == "Message sent")
                    {
                        LoadScene(3);  // verification code scene
                    }
                    else
                    {
                        Debug.Log(DB.forgetMessage);
                        wrongLoginInfo.text = "Etwas schief ist gelaufen";
                    }
                }
            }
        }));
    }

    public void ChangePassword()
    {
        string code = codeField.text.Trim();
        string password = passwordField.text;
        //string newPassword = passwordField.text;
        string repeatPassword = repeatPasswordField.text;
        Text wrongLoginInfo = GameObject.Find("Canvas/ErrorText").GetComponent<Text>();
        StartCoroutine(DB.ChangePassword(code, password, (returnedMessage) => {
            if (returnedMessage)
            {
                if (DB.forgetMessage == "code is wrong")
                {
                    wrongLoginInfo.text = "Der Code ist ungültig";
                }
                else
                {
                    if (DB.forgetMessage == "Message sent")
                    {
                        LoadScene(3);  // verification code scene
                    }
                    else
                    {
                        Debug.Log(DB.forgetMessage);
                        wrongLoginInfo.text = "Etwas schief ist gelaufen";
                    }
                }
            }
        }));
    }


    public void Register()
    {
        string userName = userNameField.text.Trim().ToLower();
        string password = passwordField.text;
        string repeatPassword = repeatPasswordField.text;
        Debug.Log("I am in Register");
        Text wrongLoginInfo = GameObject.Find("Canvas/ErrorPanel").GetComponent<Text>();
        if (userName == "" || password == "")
        {
            wrongLoginInfo.text = "Sowohl Benutzername als auch Passwort sollten ausgefüllt werden";
        }
        else if (password != repeatPasswordField.text)
        {
            wrongLoginInfo.text = "Passwort und Passwort-Wiederholen müssen gleich sein";
        }
        else
        {
            StartCoroutine(DB.Register(userName, password, (returnedMessage) => {
                if (returnedMessage)
                {
                    if (DB.registerMessage == "User is already registered")
                    {
                        wrongLoginInfo.text = "Dieser E-Mail ist bereits angemeldet";
                    }
                    else
                    {
                        if (DB.registerMessage == "Message sent")
                        {
                            LoadScene(5);  // after registeration scene
                        }
                        else
                        {
                            Debug.Log(DB.registerMessage);
                            wrongLoginInfo.text = "Etwas schief ist gelaufen";
                        }
                    }
                }
            }));
        }
    }

    public void VerifyEMail()
    {
        string userName = userNameField.text.Trim().ToLower();
        string code = codeField.text.Trim();
        Text wrongLoginInfo = GameObject.Find("Canvas/ErrorPanel").GetComponent<Text>();
        if (userName == "" || code == "")
        {
            wrongLoginInfo.text = "Sowohl Benutzername als auch Code sollten ausgefüllt werden";
        }
        else
        {
            StartCoroutine(DB.AfterRegistration(userName, code, (returnedMessage) => {
                if (returnedMessage)
                {
                    Debug.Log(DB.verifyMessage);
                    if (DB.verifyMessage == "0")
                    {
                        wrongLoginInfo.text = "Der Code oder der E-Mail ist ungültig";
                    }
                    else
                    {
                        loginManager.Instance.currentUserID = DB.verifyMessage;
                        LoadScene(4);  // GPS Scene
                    }
                }
            }));
        }
    }

    public void SignOut() {
        Application.Quit();
        Debug.Log("The application has quite");
        //Destroy(Instance);
        //Debug.Log("loginManager has destroied");
        //LoadScene(0); //login scene 
    }

    public void LoadScene(int sceneNumber) {
        SceneManager.LoadSceneAsync(sceneNumber);
    }
}
