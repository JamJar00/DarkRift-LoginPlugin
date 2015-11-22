using UnityEngine;
using System.Collections;

using UnityEngine.UI;

using DarkRift;
using DarkRift.LoginPlugin;

public class LoginDemo : MonoBehaviour {

	[SerializeField]
	string ip = "127.0.0.1";

	[SerializeField]
	InputField usernameField;

	[SerializeField]
	InputField passwordField;

	[SerializeField]
	GameObject loginPanel;

	[SerializeField]
	GameObject logoutPanel;

	[SerializeField]
	Text userIDLabel;

	void Start () {
		DarkRiftAPI.Connect(ip);

		LoginManager.onSuccessfulLogin 		+= ChangeToLogoutScreen;
		LoginManager.onUnsuccessfulLogin 	+= MakeRed;
		LoginManager.onAddUser 				+= ChangeToLogoutScreen;
		LoginManager.onAddUserFailed 		+= MakeRed;
		LoginManager.onLogout				+= ChangeToLoginScreen;
	}

	public void OnLoginButtonClicked ()
	{
		LoginManager.Login(usernameField.text, passwordField.text);
	}

	public void OnCreateAccountButtonClicked()
	{
		LoginManager.AddUser (usernameField.text, passwordField.text);
	}

	public void LogoutButtonClicked()
	{
		LoginManager.Logout ();
	}

	void MakeRed()
	{
		usernameField.textComponent.color = Color.red;
		passwordField.textComponent.color = Color.red;
	}

	void ChangeToLoginScreen()
	{
		loginPanel.SetActive(true);
		logoutPanel.SetActive(false);

		usernameField.textComponent.color = Color.grey;
		passwordField.textComponent.color = Color.grey;

		usernameField.text = "";
		passwordField.text = "";
	}

	void ChangeToLogoutScreen(int userID)
	{
		loginPanel.SetActive(false);
		logoutPanel.SetActive(true);

		userIDLabel.text = "UserID: " + userID.ToString();
	}
}
