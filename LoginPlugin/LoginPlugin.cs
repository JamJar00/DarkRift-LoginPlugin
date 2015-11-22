using System;
using System.Collections.Generic;

using DarkRift;
using DarkRift.Storage;
using DarkRift.ConfigTools;

namespace LoginPlugin
{
	public class LoginPlugin : Plugin
	{
		public override string name { 			get { 	return "LoginPlugin"; 			}}
		public override string version { 		get {	return "1.1"; 					}}
		public override Command[] commands {
			get {
				return new Command[]{
					//new Command("AddUser", "Adds a user to the login system. AddUser username password", AddUserToDatabase)
				};
			}
		}
		public override string author {			get {	return "Jamie Read";			}}
		public override string supportEmail {	get {	return "Jamie.Read@outlook.com";}}

		public delegate void SuccessfulLoginEventHandler(int userID, ConnectionService con);
		public delegate void UnsuccessfulLoginEventHandler(ConnectionService con);
		public delegate void LogoutEventHandler(int userID, ConnectionService con);
		public delegate void SuccessfulAddUserEventHandler(int userID, string username, ConnectionService con);
		public delegate void UnsuccessfulAddUserEventHandler(string username, ConnectionService con);

		/// <summary>
		/// 	Occurs when a login is sucessful
		/// </summary>
		public event SuccessfulLoginEventHandler onSuccessfulLogin;

		/// <summary>
		/// 	Occurs when a login is unsucessful.
		/// </summary>
		public event UnsuccessfulLoginEventHandler onUnsucessfulLogin;

		/// <summary>
		/// 	Occurs when a player logs out.
		/// </summary>
		public event LogoutEventHandler onLogout;

		/// <summary>
		/// 	Occurs when a player is added remotely.
		/// </summary>
		public event SuccessfulAddUserEventHandler onAddUser;

		/// <summary>
		/// 	Occurs when a new player wasn't able to be added.
		/// </summary>
		public event UnsuccessfulAddUserEventHandler onAddUserFailed;

		ConfigReader settings;

		byte tag;
		ushort loginSubject;
		ushort logoutSubject;
		ushort addUserSubject;

		ushort loginSuccessSubject;
		ushort loginFailedSubject;
		ushort logoutSuccessSubject;
		ushort addUserSuccessSubject;
		ushort addUserFailedSubject;

        bool debug;

		public LoginPlugin ()
		{
			if( !IsInstalled() )
			{
				InstallSubdirectory (
					new Dictionary<string, byte[]> ()
					{
						{"settings.cnf", System.Text.ASCIIEncoding.ASCII.GetBytes("Tag:\t\t\t1\nLoginSubject:\t0\nLogoutSubject:\t1\nAddUserSubject:\t2\nLoginSuccessSubject:\t3\nLoginFailedSubject:\t4\nLogoutSuccessSubject:\t5\nAddUserSuccessSubject:\t6\nAddUserFailedSubject:\t7\nAllowAddUser:\tTrue")}
					}
				);

				DarkRiftServer.database.ExecuteNonQuery (
					"CREATE TABLE IF NOT EXISTS users(" +
					"id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
					"username VARCHAR(50) NOT NULL, " +
					"password VARCHAR(50) NOT NULL ) "
				);
			}

			//Load settings
			settings = new ConfigReader (GetSubdirectory() + "/settings.cnf");

			if (!byte.TryParse(settings ["Tag"], out tag))
			{
				Interface.LogFatal ("[LoginPlugin] Tag property could not be resolved from settings.cnf");
                DarkRiftServer.Close(true);
			}
			if (!ushort.TryParse (settings ["LoginSubject"], out loginSubject))
            {
				Interface.LogFatal ("[LoginPlugin] LoginSubject property could not be resolved from settings.cnf");
                DarkRiftServer.Close(true);
            }
			if (!ushort.TryParse (settings ["LogoutSubject"], out logoutSubject))
            {
				Interface.LogFatal ("[LoginPlugin] LogoutSubject property could not be resolved from settings.cnf");
                DarkRiftServer.Close(true);
            }
			if (!ushort.TryParse (settings ["AddUserSubject"], out addUserSubject))
            {
				Interface.LogFatal ("[LoginPlugin] AddUserSubject property could not be resolved from settings.cnf");
                DarkRiftServer.Close(true);
            }
			if (!ushort.TryParse (settings ["LoginSuccessSubject"], out loginSuccessSubject))
            {
				Interface.LogFatal ("[LoginPlugin] LoginSuccessSubject property could not be resolved from settings.cnf");
                DarkRiftServer.Close(true);
            }
			if (!ushort.TryParse (settings ["LoginFailedSubject"], out loginFailedSubject))
            {
				Interface.LogFatal ("[LoginPlugin] LoginFailedSubject property could not be resolved from settings.cnf");
                DarkRiftServer.Close(true);
            }
			if (!ushort.TryParse (settings ["LogoutSuccessSubject"], out logoutSuccessSubject))
            {
				Interface.LogFatal ("[LoginPlugin] LogoutSuccessSubject property could not be resolved from settings.cnf");
                DarkRiftServer.Close(true);
            }
			if (!ushort.TryParse (settings ["AddUserSuccessSubject"], out addUserSuccessSubject))
            {
				Interface.LogFatal ("[LoginPlugin] AddUserSuccessSubject property could not be resolved from settings.cnf");
                DarkRiftServer.Close(true);
            }
			if (!ushort.TryParse (settings ["AddUserFailedSubject"], out addUserFailedSubject))
            {
				Interface.LogFatal ("[LoginPlugin] AddUserFailedSubject property could not be resolved from settings.cnf");
                DarkRiftServer.Close(true);
            }
			if (settings ["AllowAddUser"] == "")
            {
				Interface.LogFatal ("[LoginPlugin] AllowAddUser property not found in settings.cnf");
                DarkRiftServer.Close(true);
            }

            debug = settings.IsTrue("Debug");

			ConnectionService.onServerMessage += OnServerMessage;
		}

		//TODO stop users logging in if they're already logged in, it'll circumvent the logout event D:

		void OnServerMessage(ConnectionService con, NetworkMessage data)
		{
			if (data.tag == tag)
			{
				//Login
				if (data.subject == loginSubject)
				{
					try
					{
						using (DarkRiftReader reader = (DarkRiftReader)data.data)
						{
							bool isLoggedIn = false;
							try
							{
								isLoggedIn = (bool)con.GetData(name, "IsLoggedIn");
							}
							catch (KeyNotFoundException)
							{
							
							}

							if (!isLoggedIn)
							{
								try
								{
									//Query then database
									DatabaseRow[] rows = DarkRiftServer.database.ExecuteQuery(
										"SELECT id FROM users WHERE username = @username AND password = @password LIMIT 1 ",
										new QueryParameter("username", reader.ReadString()),
										new QueryParameter("password", reader.ReadString())
									);

									//If 1 is returned then the details are correct
									if (rows.Length == 1)
									{
										int id = Convert.ToInt32(rows[0]["id"]);

										con.SetData(name, "IsLoggedIn", true);
										con.SetData(name, "UserID", id);

										if (onSuccessfulLogin != null)
											onSuccessfulLogin(id, con);

										if (debug)
											Interface.Log("[LoginPlugin] Successfull login (" + id + ").");

										con.SendReply(tag, loginSuccessSubject, id);
									}
									else
									{
										if (onUnsucessfulLogin != null)
											onUnsucessfulLogin(con);

										if (debug)
											Interface.Log("[LoginPlugin] Unsuccessfull login.");

										con.SendReply(tag, loginFailedSubject, 0);
									}
								}
								catch (DatabaseException e)
								{
									Interface.LogError("[LoginPlugin] SQL error during login:" + e.ToString());

									if (onUnsucessfulLogin != null)
										onUnsucessfulLogin(con);

									if (debug)
										Interface.Log("[LoginPlugin] Unsuccessfull login.");

									con.SendReply(tag, loginFailedSubject, 0);
								}
							}
							else
							{
								Interface.LogError("[LoginPlugin] Client tried to login while still logged in!");

								if (onUnsucessfulLogin != null)
									onUnsucessfulLogin(con);

								if (debug)
									Interface.Log("[LoginPlugin] Unsuccessfull login.");

								con.SendReply(tag, loginFailedSubject, 0);
							}
						}
					}
					catch(InvalidCastException)
					{
						Interface.LogError("[LoginPlugin] Invalid data recieved in a Login request.");

						if (onUnsucessfulLogin != null)
							onUnsucessfulLogin(con);

						if (debug)
							Interface.Log("[LoginPlugin] Unsuccessfull login.");

						con.SendReply(tag, loginFailedSubject, 0);
					}
				}

				//Logout
				if (data.subject == logoutSubject)
				{
					con.SetData(name, "IsLoggedIn", false);

					int id = (int)con.GetData(name, "UserID");
					con.SetData(name, "UserID", -1);

					if( onLogout != null )
						onLogout(id, con);

                    if( debug )
                        Interface.Log("[LoginPlugin] Successful logout.");

					con.SendReply (tag, logoutSuccessSubject, 0);
				}

				//Add User
				if (data.subject == addUserSubject && settings.IsTrue ("AllowAddUser"))
				{
					try
					{
						using (DarkRiftReader reader = (DarkRiftReader)data.data)
						{
							string username = reader.ReadString();
							string password = reader.ReadString();

							try
							{
	                            object o = DarkRiftServer.database.ExecuteScalar (
	                                "SELECT EXISTS(SELECT 1 FROM users WHERE username = @username AND password = @password) ",
	                                new QueryParameter("username", username),
	                                new QueryParameter("password", password)
	                            );

	    						if (!Convert.ToBoolean(o))
	    						{
	    							int id = AddUserToDatabase (username, password);

	    							con.SetData (name, "IsLoggedIn", true);
	    							con.SetData (name, "UserID", id);

	    							if (onAddUser != null)
	    								onAddUser (id, username, con);

	                                if( debug )
	                                    Interface.Log("[LoginPlugin] User added.");

	    							con.SendReply (tag, addUserSuccessSubject, id);
	    						}
	    						else
	    						{
	    							if (onAddUserFailed != null)
	    								onAddUserFailed (username, con);

	                                if( debug )
	                                    Interface.Log("[LoginPlugin] Add user failed.");

	    							con.SendReply (tag, addUserFailedSubject, 0);
	    						}
	                        }
	                        catch(DatabaseException e)
	                        {
	                            Interface.LogError("[LoginPlugin] SQL error during AddUser:" + e.ToString());
	                            
	                            if( onAddUserFailed != null )
	                                onAddUserFailed(username, con);

	                            if( debug )
	                                Interface.Log("[LoginPlugin] Add user failed.");

	                            con.SendReply (tag, loginFailedSubject, 0);
	                        }
						}
					}
					catch(InvalidCastException)
					{
						Interface.LogError("[LoginPlugin] Invalid data recieved in an AddUser request.");

                        if( onAddUserFailed != null )
                            onAddUserFailed("", con);

                        if( debug )
                            Interface.Log("[LoginPlugin] Add user failed.");

                        con.SendReply (tag, loginFailedSubject, 0);
					}
				}
			}
		}

		int AddUserToDatabase(string username, string password)
		{
			DarkRiftServer.database.ExecuteNonQuery (
				"INSERT INTO users(username, password) VALUES(@username, @password)",
                new QueryParameter("username", username),
                new QueryParameter("password", password)
			);

			return Convert.ToInt32(DarkRiftServer.database.ExecuteScalar (
				"SELECT last_insert_id() "
			));
		}
	}
}

