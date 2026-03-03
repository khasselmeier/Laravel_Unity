using TMPro;
using UnityEngine;

public class AuthUI : MonoBehaviour
{
    [SerializeField] private LaravelAuthClient api;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("Output")]
    [SerializeField] private TMP_Text statusText;

    public void OnRegisterClicked()
    {
        statusText.text = "Registering...";

        StartCoroutine(api.Register(
            nameInput.text,
            emailInput.text,
            passwordInput.text,
            onOk: res =>
            {
                api.SaveToken(res.token);
                statusText.text = $"Registered: {res.user.name}\nToken saved.";
            },
            onErr: err =>
            {
                statusText.text = "Register failed:\n" + err;
            }
        ));
    }

    public void OnLoginClicked()
    {
        statusText.text = "Logging in...";

        StartCoroutine(api.Login(
            emailInput.text,
            passwordInput.text,
            onOk: res =>
            {
                api.SaveToken(res.token);
                statusText.text = $"Logged in: {res.user.email}\nToken saved.";
            },
            onErr: err =>
            {
                statusText.text = "Login failed:\n" + err;
            }
        ));
    }

    public void OnMeClicked()
    {
        statusText.text = "Calling /me...";

        StartCoroutine(api.Me(
            onOk: user =>
            {
                statusText.text = $"Me:\n{user.id} {user.name} {user.email}";
            },
            onErr: err =>
            {
                statusText.text = "Me failed:\n" + err;
            }
        ));
    }

    public void OnLogoutLocalClicked()
    {
        api.ClearToken();
        statusText.text = "Local token cleared.";
    }
}