using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class Client_Login_Request : MonoBehaviour
{
  [Serializable]
  private struct AccountData
  {
    public string user;
    public string password;
    public List<float> times;
    public List<float> scores;

    public AccountData(string u, string p)
    {
      user = u;
      password = p;
      times = new List<float>();
      scores = new List<float>();
    }
  }

  private const short LoginMessage = 132, CreateAccount = 133, LeaderboardData = 134, SpriteData = 135, UpdateChannel = 137;
  Player myPlayer;

  public Sprite myDemoSprite;
  public Player MyPlayer
  {
    get
    {
      return myPlayer;
    }

    set
    {
      myPlayer = value;
    }
  }
  private List<AccountData> accountData;

  private void Start()
  {
    NetworkManager.singleton.client.RegisterHandler(LoginMessage, ClientReceiveLoginAttempt);
    NetworkManager.singleton.client.RegisterHandler(LeaderboardData, ClientReceiveLeaderboardData);
    if (NetworkServer.active)
    {
      NetworkServer.RegisterHandler(LoginMessage, ServerReceiveLoginAttempt);
      NetworkServer.RegisterHandler(CreateAccount, ServerReceiveNewAccount);
      NetworkServer.RegisterHandler(LeaderboardData, ServerReceiveLeaderboardSubmission);
      NetworkServer.RegisterHandler(UpdateChannel, ServerRecieveCarRequest);
      LoadLeaderboardData();
      if (accountData == null)
      {
        accountData = new List<AccountData>();
        SaveLeaderboardData();
      }
    }
  }

  public void SubmitLoginAttempt()
  {
    StringMessage outgoing = new StringMessage();
    outgoing.value = transform.Find("Canvas/Panel/Username").GetComponent<InputField>().text + "," + transform.Find("Canvas/Panel/Password").GetComponent<InputField>().text;
    NetworkManager.singleton.client.Send(LoginMessage, outgoing);
  }

  public void SubmitNewAccount()
  {
    StringMessage outgoing = new StringMessage();
    outgoing.value = transform.Find("Canvas/Panel/Username").GetComponent<InputField>().text + "," + transform.Find("Canvas/Panel/Password").GetComponent<InputField>().text;
    NetworkManager.singleton.client.Send(CreateAccount, outgoing);
  }

  public void ServerRecieveCarRequest(NetworkMessage message)
  {
    StringMessage mes = message.ReadMessage<StringMessage>();
    print("Server Heard Request");
    NetworkServer.SendToAll(UpdateChannel, mes);
    print("Server forwarding request back out");
  }

  void ServerReceiveLoginAttempt(NetworkMessage message)
  {
    LoadLeaderboardData();
    StringMessage inbound = message.ReadMessage<StringMessage>();
    //print("Server: " + inbound.value);
    string[] acctInfo = ParseAccountData(inbound.value);
    string user = acctInfo[0];
    string pass = acctInfo[1];
    //print("Server: User = " + user + " and password = " + pass);
    foreach (AccountData data in accountData)
    {
      if (data.user == user)
      {
        if (pass == data.password)
        {
          StringMessage res = new StringMessage();
          res.value = "SUCCESS," + user;

          StringMessage returnedLeaderboard = new StringMessage();
          AccountData acct = GetDataByUser(data.user);
          if (acct.scores.Count > 0)
          {
            int lim = (acct.times.Count < 5 ? acct.times.Count : 5);
            for (int i = 0; i < lim; i++)
            {
              returnedLeaderboard.value += acct.times[i] + "," + acct.scores[i] + ",";
            }
            returnedLeaderboard.value = returnedLeaderboard.value.Remove(returnedLeaderboard.value.Length - 1);
            NetworkServer.SendToClient(message.conn.connectionId, LeaderboardData, returnedLeaderboard);
          }
          NetworkServer.SendToClient(message.conn.connectionId, LoginMessage, res);
          foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player")){
            if (obj.GetComponent<NetworkIdentity>().connectionToClient != null && obj.GetComponent<NetworkIdentity>().connectionToClient.connectionId == message.conn.connectionId){
              obj.GetComponent<Player>().myName = user;
            }
          }
          //print("Sucess");
          return;
        }
        else
        {
          StringMessage res2 = new StringMessage();
          res2.value = "INVALID PASSWORD";
          NetworkServer.SendToClient(message.conn.connectionId, LoginMessage, res2);
          print("Invalid Password");
          return;
        }
      }
    }
    StringMessage result = new StringMessage();
    result.value = "NONEXISTANT ACCOUNT";
    NetworkServer.SendToClient(message.conn.connectionId, LoginMessage, result);
    print("Invalid Username");
  }

  void ServerReceiveNewAccount(NetworkMessage message)
  {
    StringMessage inbound = message.ReadMessage<StringMessage>();
    string[] acctInfo = ParseAccountData(inbound.value);
    if (accountData == null)
    {
      accountData = new List<AccountData>();
    }
    for (int i = 0; i < accountData.Count; i++)
    {
      if (acctInfo[0] == accountData[i].user)
      {
        print("Account already exists");
        return;
      }
    }
    accountData.Add(new AccountData(acctInfo[0], acctInfo[1]));
    print("Created new entry");
    SaveLeaderboardData();
  }

  void ClientReceiveLoginAttempt(NetworkMessage message)
  {
    //print("Client: " + message.ReadMessage<StringMessage>().value);
    string rec = message.ReadMessage<StringMessage>().value;
    Text textObj = transform.Find("Canvas/Panel/Text").GetComponent<Text>();
    if (rec.IndexOf("SUCCESS") > -1)
    {
      myPlayer.isInLobby = false;
      myPlayer.DestroyLoginInterface();
      myPlayer.myName = rec.Substring(rec.IndexOf(",") + 1);
    }
    else if (rec.IndexOf("NONEXISTANT") > -1)
    {
      textObj.text = "Invalid Username";
    }
    else
    {
      textObj.text = "Invalid Password";
    }
  }

  private void ServerReceiveLeaderboardSubmission(NetworkMessage message)
  {
    string mes = message.ReadMessage<StringMessage>().value;
    AccountData acct = GetDataByUser(mes.Substring(0, mes.IndexOf(",")));
    if (acct.user != "FAILED")
    {
      string[] sep = mes.Split(',');
      acct.times.Add(float.Parse(sep[1]));
      acct.scores.Add(float.Parse(sep[2]));
      acct.times.Sort();
      acct.scores.Sort();
      acct.scores.Reverse();
      SaveLeaderboardData();
      StringMessage returnedLeaderboard = new StringMessage();
      int lim = (acct.times.Count < 5 ? acct.times.Count : 5);
      for (int i = 0; i < lim; i++)
      {
        returnedLeaderboard.value += acct.times[i] + "," + acct.scores[i] + ",";
      }
      returnedLeaderboard.value = returnedLeaderboard.value.Remove(returnedLeaderboard.value.Length - 1);
      NetworkServer.SendToClient(message.conn.connectionId, LeaderboardData, returnedLeaderboard);
    }
  }

  private void ClientReceiveLeaderboardData(NetworkMessage message)
  {
    StringMessage mes = new StringMessage();
    mes = message.ReadMessage<StringMessage>();
    string myLeaderboard = mes.value;
    string[] entry = myLeaderboard.Split(',');
    myPlayer.lead.GetComponent<Leaderboard>().SetHighScores(entry);
  }

  private string[] ParseAccountData(string data)
  {
    string[] ret = new string[2];
    string user = data;
    string pass = user.Substring(user.IndexOf(",") + 1);
    user = user.Substring(0, user.IndexOf(","));
    ret[0] = user;
    ret[1] = pass;
    return ret;
  }

  private AccountData GetDataByUser(string name)
  {
    foreach (AccountData data in accountData)
    {
      if (name == data.user)
      {
        return data;
      }
    }
    return new AccountData("FAILED", "");
  }

  private void SaveLeaderboardData()
  {
    BinaryFormatter bf = new BinaryFormatter();
    FileStream file = File.Create(Application.persistentDataPath + "/leaderboardData.gdi");
    bf.Serialize(file, accountData);
    file.Close();
  }

  private void LoadLeaderboardData()
  {
    if (File.Exists(Application.persistentDataPath + "/leaderboardData.gdi"))
    {
      BinaryFormatter bf = new BinaryFormatter();
      FileStream file = File.Open(Application.persistentDataPath + "/leaderboardData.gdi", FileMode.Open);
      accountData = (List<AccountData>)bf.Deserialize(file);
      file.Close();
    }
    else
    {
      SaveLeaderboardData();
    }
  }
}
