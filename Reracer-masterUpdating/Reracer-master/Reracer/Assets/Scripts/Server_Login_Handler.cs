using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Server_Login_Handler : MonoBehaviour {
  public bool LoginAttempt(string username, string password){
    print("Logging in with: " + username + ", " + password);
    return true;
  }
}
