using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.SocialPlatforms.Impl;


public class Auth : MonoBehaviour
{
    public string ApiUrl = "https://sid-restapi.onrender.com/api";

    TMP_InputField usernameInput;
    TMP_InputField passwordInput;

    //private string Token;
    private string Username;
    private int Score;

    public GameObject panelGame;
    public GameObject panelRegister;
    public GameObject panelPuntajes;
    public TMP_Text welcomeText;
    public TMP_Text scoreText;

     public GameObject panelLead;
    public TMP_Text[] names;
    public TMP_Text[] scores;
    public TMP_Text confirmacionregistro;


    void Start()
    {
        panelGame.SetActive(false);
        panelLead.SetActive(false);

        //Token = PlayerPrefs.GetString("token");

       /* if (string.IsNullOrEmpty(Token))
        {
            Debug.LogWarning("No hay Token almacenado");
        }
        else
        {
            Username = PlayerPrefs.GetString("username");
            Score = PlayerPrefs.GetInt("score");
            StartCoroutine(GetProfile(Username));
            //StartCoroutine(ChangeScores(Username));
            //StartCoroutine(OrderScores(Username));

        }*/

        usernameInput = GameObject.Find("usernameInput").GetComponent<TMP_InputField>();
        passwordInput = GameObject.Find("passwordInput").GetComponent<TMP_InputField>();
    }
    public void Register()
    {
        AuthData authData = new AuthData();
        authData.password = passwordInput.text;
        authData.username = usernameInput.text;

        string json = JsonUtility.ToJson(authData);
        StartCoroutine(SendRegister(json));

    }
    public void Login()
    {
        AuthData authData = new AuthData();
        authData.password = passwordInput.text;
        authData.username = usernameInput.text;

        string json = JsonUtility.ToJson(authData);
        StartCoroutine(SendLogin(json));

    }
    public void Logout()
    {
        // Clear stored token and username
        PlayerPrefs.DeleteKey("token");
        PlayerPrefs.DeleteKey("username");

        // Hide the game panel
        panelGame.SetActive(false);

        // Show the register panel
        panelRegister.SetActive(true);
        panelPuntajes.SetActive(false);
    }
    /* public void ChangeScore()
    {
        AuthData authData = new AuthData();
        authData.data = new UserData();
        authData.username = Username;
        authData.data.score = Score;
        Score += 300;
        authData.data.score = Score;
        
        string json = JsonUtility.ToJson(authData);
        StartCoroutine(ChangeScores(json));
    }*/
    public void ChangeScore()
    {
        // Check if the Username is not null or empty
        if (!string.IsNullOrEmpty(Username))
        {
            // Get the current score from PlayerPrefs
            int currentScore = PlayerPrefs.GetInt("score");

            // Add 300 to the current score
            int newScore = currentScore + 300;

            // Update the PlayerPrefs with the new score
            PlayerPrefs.SetInt("score", newScore);

            // Update the score display
            scoreText.text = "Score:  " + newScore;

            // Send the score change to the server
            AuthData authData = new AuthData();
            authData.data = new UserData();
            authData.username = Username;
            authData.data.score = newScore; // Send the new score to the server

            string json = JsonUtility.ToJson(authData);
            StartCoroutine(ChangeScores(json));
        }
        else
        {
            Debug.LogWarning("Username is null or empty.");
        }
    }

    public void ShowScores()
    {
        AuthData authData = new AuthData();
        authData.password = passwordInput.text;
        authData.username = usernameInput.text;



        string json = JsonUtility.ToJson(authData);
        StartCoroutine(OrderScores());
    }
    IEnumerator GetProfile(string username)
    {
        string token = PlayerPrefs.GetString("token");
        UnityWebRequest request = UnityWebRequest.Get(ApiUrl + "/usuarios/" + username);
        request.SetRequestHeader("x-token", token);
        yield return request.SendWebRequest();
        if (request.isNetworkError)
        {
            Debug.Log("NETWORK ERROR: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                PlayerPrefs.SetInt("score", data.usuario.data.score);
                Debug.Log("User " + data.usuario.username + " initialized game");
                Debug.Log("Score: " + data.usuario.data.score);
                panelGame.SetActive(true);
                welcomeText.text = "Hola " + data.usuario.username;
                scoreText.text = "Score:  " + data.usuario.data.score;
                //SceneManager.LoadScene("Game");

            }
            else
            {
                Debug.Log(request.error);
            }
        }

    }
    IEnumerator SendRegister(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(ApiUrl + "/usuarios", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("NETWORK ERROR: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                confirmacionregistro.text = "Registro exitoso!";
                Debug.Log("User " + data.usuario._id + " registered");
            }
            else
            {
                Debug.Log(request.error);
            }

        }

    }

    IEnumerator SendLogin(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(ApiUrl + "/auth/login", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("NETWORK ERROR: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                Debug.Log("User " + data.usuario.username + " logged in");
                Username = data.usuario.username;
                PlayerPrefs.SetString("token", data.token);
                PlayerPrefs.SetString("username", data.usuario.username);
                PlayerPrefs.SetInt("score", data.usuario.data.score);
                panelGame.SetActive(true);
                welcomeText.text = "Wecolme " + data.usuario.username;
                scoreText.text = "Score:  " + data.usuario.data.score;
                Debug.Log(data.token);

            }
            else
            {
                Debug.Log(request.error);
            }

        }

    }
    IEnumerator OrderScores()
    {
        string token = PlayerPrefs.GetString("token");
        UnityWebRequest request = UnityWebRequest.Get(ApiUrl + "/usuarios");
        request.SetRequestHeader("x-token", token);
        //request.method = "PATCH";
        yield return request.SendWebRequest();
        if (request.isNetworkError)
        {
            Debug.Log("NETWORK ERROR: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                //PlayerPrefs.SetString("token", data.token);
                //PlayerPrefs.SetString("username", data.usuario.username);
                //data.usuarios = data.usuarios.Where(u => u.userData.score > 5).ToArray();
                panelLead.SetActive(true);
                data.usuarios = data.usuarios.OrderByDescending(u => u.data.score).Take(5).ToArray();
                foreach (User u in data.usuarios)
                {
                    Debug.Log($"{u.username}\", SCORE: {u.data.score},");
                }
                for (int i = 0; i < data.usuarios.Length && i < names.Length && i < scores.Length; i++)
                {
                    names[i].text = data.usuarios[i].username;
                    scores[i].text = data.usuarios[i].data.score.ToString();
                }
            }
        }
    }
    IEnumerator ChangeScores(string json)
    {
        string token = PlayerPrefs.GetString("token");
        UnityWebRequest request = UnityWebRequest.Put(ApiUrl + "/usuarios", json);
        request.SetRequestHeader("x-token", token);
        request.method = "PATCH";
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        if (request.isNetworkError)
        {
            Debug.Log("NETWORK ERROR: " + request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);

                scoreText.text = "Score:  " + data.usuario.data.score;
                //SceneManager.LoadScene("Game");

            }
            else
            {
                Debug.Log(request.error);
            }
        }

    }
}
[System.Serializable]
public class AuthData
{
    public string username;
    public string password;
    public User usuario;
    public UserData data;
    public string token;
    public User[] usuarios;
}
[System.Serializable]
public class UserList
{
    //public User[] usuarios;
}

[System.Serializable]
public class User
{
    public string _id;
    public string username;
    public UserData data;
}
[System.Serializable]
public class UserData
{
    public int score;
}
