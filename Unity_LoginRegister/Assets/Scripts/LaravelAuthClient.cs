using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class LaravelAuthClient : MonoBehaviour
{
    [Header("API")]
    [SerializeField] private string baseUrl = "https://laravel-loginregister.test";

    private const string TokenKey = "laravel_token";

    [Serializable] private class RegisterRequest { public string name; public string email; public string password; }
    [Serializable] private class LoginRequest { public string email; public string password; }

    [Serializable] public class UserDto { public int id; public string name; public string email; }
    [Serializable] public class AuthResponse { public string token; public UserDto user; }

    public string Token => PlayerPrefs.GetString(TokenKey, "");

    public void SaveToken(string token)
    {
        PlayerPrefs.SetString(TokenKey, token);
        PlayerPrefs.Save();
    }

    public void ClearToken()
    {
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.Save();
    }

    public IEnumerator Register(string name, string email, string password, Action<AuthResponse> onOk, Action<string> onErr)
    {
        var body = JsonUtility.ToJson(new RegisterRequest { name = name, email = email, password = password });
        yield return SendJson<AuthResponse>("POST", "/api/register", body, auth: false, onOk, onErr);
    }

    public IEnumerator Login(string email, string password, Action<AuthResponse> onOk, Action<string> onErr)
    {
        var body = JsonUtility.ToJson(new LoginRequest { email = email, password = password });
        yield return SendJson<AuthResponse>("POST", "/api/login", body, auth: false, onOk, onErr);
    }

    public IEnumerator Me(Action<UserDto> onOk, Action<string> onErr)
    {
        yield return SendJson<UserDto>("GET", "/api/me", null, auth: true, onOk, onErr);
    }

    private IEnumerator SendJson<T>(string method, string path, string jsonBody, bool auth, Action<T> onOk, Action<string> onErr)
    {
        var url = baseUrl.TrimEnd('/') + path;

        using var req = new UnityWebRequest(url, method);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");

        if (!string.IsNullOrEmpty(jsonBody))
        {
            var raw = Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler = new UploadHandlerRaw(raw);
            req.SetRequestHeader("Content-Type", "application/json");
        }

        if (auth)
        {
            var token = Token;
            if (string.IsNullOrEmpty(token))
            {
                onErr?.Invoke("No token saved. Login first.");
                yield break;
            }
            req.SetRequestHeader("Authorization", "Bearer " + token);
        }

        yield return req.SendWebRequest();

        //error info
        if (req.result != UnityWebRequest.Result.Success)
        {
            onErr?.Invoke($"{req.responseCode} {req.error}\n{req.downloadHandler.text}");
            yield break;
        }

        try
        {
            var text = req.downloadHandler.text;

            //if the API returns an empty body, just default it
            if (string.IsNullOrWhiteSpace(text))
            {
                onOk?.Invoke(default);
                yield break;
            }

            var data = JsonUtility.FromJson<T>(text);
            onOk?.Invoke(data);
        }
        catch (Exception e)
        {
            onErr?.Invoke("JSON parse error: " + e.Message);
        }
    }
}