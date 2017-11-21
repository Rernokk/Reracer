using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Client_Login_Request : NetworkBehaviour
{
  Player myPlayer;
  public Player MyPlayer
  {
    get
    {
      return myPlayer;
    }

    set {
      myPlayer = value;
    }
  }

  [ClientRpc]
  public void RpcSubmitLoginInfo()
  {
    if (myPlayer == null){
      return;
    }
    print("Submitting");
    print(myPlayer.myName);
    myPlayer.transform.position += Vector3.up * 5f;
  }
}
