using UnityEngine;
using System;
using System.Collections.Generic;

using DarkRift;

namespace DarkRift.LoginPlugin
{
	public class LoginManager
	{
		//The following constants should be set the same as in the login plugin config file.
		const byte	 tag					= 1;

		const ushort loginSubject			= 0;
		const ushort logoutSubject			= 1;
		const ushort addUserSubject			= 2;
		const ushort loginSuccessSubject	= 3;
		const ushort loginFailedSubject		= 4;
		const ushort logoutSuccessSubject	= 5;
		const ushort addUserSuccessSubject	= 6;
		const ushort addUserFailedSubject	= 7;

		/// <summary>
		/// 	The user ID when logged in.
		/// </summary>
		public static int userID{ private set; get; }

		/// <summary>
		/// 	Are we logged in to a server?
		/// </summary>
		/// <value><c>true</c> if is logged in; otherwise, <c>false</c>.</value>
		public static bool isLoggedIn{ private set; get; }

		/// <summary>
		/// 	The connection to transmit over, if null DarkRiftAPI will be used instead.
		/// </summary>
		public static DarkRiftConnection connection;

		/// <summary>
		/// 	The hash algorithm to use, don't change this once records are set.
		/// </summary>
		public static HashType hashType = HashType.MD5;

		#region Events

		public delegate void SuccessfulLoginEventHandler(int userID);
		public delegate void UnsuccessfulLoginEventHandler();
		public delegate void LogoutEventHandler();
		public delegate void SuccessfulAddUserEventHandler(int userID);
		public delegate void UnsuccessfulAddUserEventHandler();
		
		/// <summary>
		/// 	Occurs when a login is sucessful'
		/// </summary>
		public static event SuccessfulLoginEventHandler onSuccessfulLogin;
		
		/// <summary>
		/// 	Occurs when a login is unsucessful.
		/// </summary>
		public static event UnsuccessfulLoginEventHandler onUnsuccessfulLogin;
		
		/// <summary>
		/// 	Occurs when a player logs out.
		/// </summary>
		public static event LogoutEventHandler onLogout;
		
		/// <summary>
		/// 	Occurs when a player is added remotely.
		/// </summary>
		public static event SuccessfulAddUserEventHandler onAddUser;
		
		/// <summary>
		/// 	Occurs when a new player wasn't able to be added.
		/// </summary>
		public static event UnsuccessfulAddUserEventHandler onAddUserFailed;

		#endregion

		/// <summary>
		/// 	Login with the specified username and password.
		/// </summary>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		public static void Login(string username, string password)
		{
			//Stop people from simply spamming the server
			if (string.IsNullOrEmpty (username) || string.IsNullOrEmpty (password))
				return;

			//Build the data to send
			using( DarkRiftWriter writer = new DarkRiftWriter() )
			{
				writer.Write(username);
				writer.Write(SecurityHelper.GetHash(password, hashType));
				
				//Choose the connection to use
				if( connection == null )
				{
					if( DarkRiftAPI.isConnected )
					{
						//Send via DarkRiftAPI
						DarkRiftAPI.SendMessageToServer(
							tag, 
							loginSubject,
							writer
						);

						BindIfNotBound();
					}
					else
					{
						//Called if you try to login whilst not connected to a server
						Debug.LogError("[LoginPlugin] You can't login if you're not connected to a server! (Do you mean to use DarkRiftAPI?)");
					}
				}
				else
				{
					if( connection.isConnected )
					{
						//Send via DarkRiftConnection
						connection.SendMessageToServer(
							tag, 
							loginSubject,
							writer
						);

						BindIfNotBound();
					}
					else
					{
						//Called if you try to login whilst not connected to a server
						Debug.LogError("[LoginPlugin] You can't login if you're not connected to a server!");
					}
				}
			}
		}

		/// <summary>
		/// 	Logs out the user from the server.
		/// </summary>
		public static void Logout()
		{
			//Build the data to send
			using(DarkRiftWriter writer = new DarkRiftWriter())
			{
				writer.Write(0);

				//Choose the connection to use
				if( connection == null )
				{
					if( DarkRiftAPI.isConnected )
					{
						//Send via DarkRiftAPI
						DarkRiftAPI.SendMessageToServer(
							tag, 
							logoutSubject,
							writer
						);

						BindIfNotBound();
					}
					else
					{
						//Called if you try to login whilst not connected to a server
						Debug.LogError("[LoginPlugin] You can't logout if you're not connected to a server! (Do you mean to use DarkRiftAPI?)");
					}
				}
				else
				{
					if( connection.isConnected )
					{
						//Send via DarkRiftConnection
						connection.SendMessageToServer(
							tag, 
							logoutSubject,
							writer
						);

						BindIfNotBound();
					}
					else
					{
						//Called if you try to login whilst not connected to a server
						Debug.LogError("[LoginPlugin] You can't logout if you're not connected to a server!");
					}
				}
			}
		}

		/// <summary>
		/// 	Asks the server to add a new user to the database.
		/// </summary>
		/// <param name="username">The username to add.</param>
		/// <param name="password">The password to add.</param>
		public static void AddUser(string username, string password)
		{
			//Stop people from simply spamming the server
			if (string.IsNullOrEmpty (username) || string.IsNullOrEmpty (password))
				return;
			
			//Build the data to send
			using(DarkRiftWriter writer = new DarkRiftWriter())
			{
				writer.Write(username);
				writer.Write(SecurityHelper.GetHash(password, hashType));

				//Choose the connection to use
				if( connection == null )
				{
					if( DarkRiftAPI.isConnected )
					{
						//Send via DarkRiftAPI
						DarkRiftAPI.SendMessageToServer(
							tag, 
							addUserSubject,
							writer
						);

						BindIfNotBound();
					}
					else
					{
						//Called if you try to login whilst not connected to a server
						Debug.LogError("[LoginPlugin] You can't add a user if you're not connected to a server! (Do you mean to use DarkRiftAPI?)");
					}
				}
				else
				{
					if( connection.isConnected )
					{
						//Send via DarkRiftConnection
						connection.SendMessageToServer(
							tag, 
							addUserSubject,
							writer
						);

						BindIfNotBound();
					}
					else
					{
						//Called if you try to login whilst not connected to a server
						Debug.LogError("[LoginPlugin] You can't add a user if you're not connected to a server!");
					}
				}
			}
		}

		static void OnDataHandler(byte dataTag, ushort subject, object data)
		{
			Debug.Log("Data");
			if( dataTag == tag )
			{
				if( subject == loginSuccessSubject )
				{
					try
					{
						userID = (int)data;
						isLoggedIn = true;

						if( onSuccessfulLogin != null )
							onSuccessfulLogin(userID);
					}
					catch(InvalidCastException)
					{
						Debug.LogError("Invalid data recieved with a LoginSuccessSubject, should be the user's id");
					}
				}
				else if( subject == loginFailedSubject )
				{
					isLoggedIn = false;

					if( onUnsuccessfulLogin != null )
						onUnsuccessfulLogin();
				}
				else if( subject == logoutSuccessSubject )
				{
					isLoggedIn = false;

					if( onLogout != null )
						onLogout();
				}
				else if( subject == addUserSuccessSubject )
				{
					try
					{
						userID = (int)data;
						isLoggedIn = true;

						if( onAddUser != null )
							onAddUser(userID);
					}
					catch(InvalidCastException)
					{
						Debug.LogError("Invalid data recieved with a LoginSuccessSubject, should be the user's id");
					}
				}
				else if( subject == addUserFailedSubject )
				{
					if( onAddUserFailed != null )
						onAddUserFailed();
				}
			}
		}

		/// <summary>
		/// 	Binds to onData if not bound to onData already.
		/// </summary>
		static void BindIfNotBound()
		{
			if( DarkRiftAPI.isConnected )
			{
				DarkRiftAPI.onData -= OnDataHandler;
				DarkRiftAPI.onData += OnDataHandler;
			}
			else
			{
				connection.onData -= OnDataHandler;
				connection.onData += OnDataHandler;
			}
		}
	}
}
