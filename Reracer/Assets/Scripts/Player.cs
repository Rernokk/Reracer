using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
  public float Speed;
  public GameObject myCamera;
  public GameObject[] myPickups;
  Vector3 startPos;
  Quaternion startRot;

  [SyncVar]
  public bool CatBlocked = false;

  [SyncVar]
  public GameObject Pickup, Obstacle;

  [SyncVar]
  public float myTime;

  [SyncVar]
  public float myScore;

  [SyncVar]
  public string myPickup, myName = "Player 2";

  [SyncVar]
  bool dead = false;

  [SerializeField]
  [SyncVar]
  int myHealth = 100, catCounter = 0;

  [SyncVar]
  bool Boosting = false, Blocking = false;
  
  [SyncVar]
  public bool isInLobby = true;
  Image HealthBar;
  int originalSteeringValue = 135, originalSpeedLimit = 60;
  GameObject myShield;
  Rigidbody2D rgd2d;
  public GameObject UI;
  public UI_Controller myUI;
  public GameObject MyLoginUI;
  
  public GameObject lead;

  [SyncVar]
  public string passAttempt;
  Dictionary<string, string> accountDictionary;

  [SyncVar]
  int serverCount = 0;

  public int Health
  {
    get { return myHealth; }
    set { myHealth = value; }
  }
  private void Start()
  {
    if (!isLocalPlayer)
      return;

    if (isServer)
    {
      myName = "Player 1";
      accountDictionary = new Dictionary<string, string>();
      accountDictionary.Add("Player 1", "Password1");
      accountDictionary.Add("Player 2", "Password2");
    }

    MyLoginUI = Instantiate(MyLoginUI, transform.position, Quaternion.identity);
    rgd2d = GetComponent<Rigidbody2D>();
    myCamera = Instantiate(myCamera, new Vector3(transform.position.x, transform.position.y, -20), Quaternion.identity);
    myCamera.GetComponent<CameraScript>().Player = transform;
    HealthBar = myCamera.transform.Find("Canvas/Health").GetComponent<Image>();
    myTime = 0;
    dead = false;
    startPos = transform.position;
    startRot = transform.rotation;
    MyLoginUI.transform.Find("Canvas").GetComponent<Canvas>().worldCamera = myCamera.GetComponent<Camera>();
    MyLoginUI.transform.Find("Canvas").GetComponent<Canvas>().sortingLayerName = "HUD";
    MyLoginUI.transform.Find("Canvas").GetComponent<Canvas>().planeDistance = 10;
    MyLoginUI.GetComponent<Client_Login_Request>().MyPlayer = this;
  }
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.K))
    {
      CmdSubmitName(myName, passAttempt);
    }

    if (isInLobby)
      return;

    if (!dead && isServer)
    {
      myTime += Time.deltaTime;
      myScore += Time.deltaTime * 5;
    }

    if (!dead && Input.GetKeyDown(KeyCode.F2)){
      CmdSendData();
      dead = true;
    }

    if (!isLocalPlayer)
    {
      return;
    }

    HealthBar.material.SetFloat("_Value", (float)Health / 100);

    if (Input.GetKeyDown(KeyCode.Escape))
    {
      RpcRespawn();
    }

    if (dead)
    {
      return;
    }

    if (Input.GetKey(KeyCode.W))
    {
      if (rgd2d.velocity.magnitude < 60)
      {
        rgd2d.velocity += (Vector2)transform.up * Speed * Time.deltaTime;
      }
    }
    if (Input.GetKey(KeyCode.S))
    {
      rgd2d.velocity -= (Vector2)transform.up * Speed * Time.deltaTime;
    }

    if (Input.GetKey(KeyCode.A))
    {
      transform.Rotate(new Vector3(0, 0, 135) * Time.deltaTime);
      if (CatBlocked)
      {
        catCounter++;
      }
    }

    if (Input.GetKey(KeyCode.D))
    {
      transform.Rotate(new Vector3(0, 0, -135) * Time.deltaTime);
      if (CatBlocked)
      {
        catCounter++;
      }
    }

    if (Input.GetKeyDown(KeyCode.F))
    {
      CmdUsePickup();
    }

    if (Boosting)
    {
      rgd2d.velocity += (Vector2)transform.up * 3 * Speed * Time.deltaTime;
    }

    #region Debug

    if (Input.GetKeyDown(KeyCode.R))
    {
      CmdSendData();
    }
    #endregion
  }

  #region Debugging
  [Command]
  void CmdGeneratePickup()
  {
    if (!isServer)
    {
      return;
    }
    GameObject temp = Instantiate(Pickup, transform.up * 3 + transform.position + new Vector3(0, 0, 100), Quaternion.identity);
    NetworkServer.Spawn(temp);
  }

  [Command]
  void CmdGenerateObstacle()
  {
    if (!isServer)
    {
      return;
    }

    GameObject temp = Instantiate(Obstacle, transform.up * 3 + transform.position + new Vector3(0, 0, 100), Quaternion.identity);
    NetworkServer.Spawn(temp);
  }

  [Command]
  void CmdSendData()
  {
    GameObject.Find("Leaderboard").GetComponent<Leaderboard>().CmdAddEntry(myTime,(int)myScore,myName);
  }
  #endregion

  [Command]
  void CmdSubmitName(string name, string pass){
    if (accountDictionary == null){
      return;
    }
    foreach (string key in accountDictionary.Keys){
      if (key == name){
        if (accountDictionary[key] == pass)
        {
          isInLobby = false;
        }
      }
    }
  }

  [Command]
  void CmdGetLeaderboard()
  {
    if (!isServer)
      return;
    myUI.debugText.text = lead.GetComponent<Leaderboard>().GetLeaderboard();
  }

  [Command]
  void CmdDamage()
  {
    if (!isServer)
    {
      return;
    }
    if (!Blocking)
    {
      Health -= 25;
      if (Health <= 0)
      {
        //Player Dies.
        //Destroy(myCamera.gameObject);
        //Destroy(gameObject);
        dead = true;
      }
    }
    else
    {
      Destroy(myShield);
      Blocking = false;
    }
  }

  [Command]
  void CmdDestroyObject(GameObject collision)
  {
    if (!isServer)
    {
      return;
    }

    Destroy(collision);
  }

  [Command]
  void CmdGeneratePickupDetails()
  {
    if (!isServer)
    {
      return;
    }
    switch (Random.Range(0, myPickups.Length))
    {
      case (0):
        myPickup = "Harpoon";
        break;
      case (1):
        myPickup = "Kitten";
        break;
      case (2):
        myPickup = "Shield";
        break;
    }
  }

  [Command]
  void CmdUsePickup()
  {
    if (!isServer)
    {
      return;
    }

    switch (myPickup)
    {
      case ("Harpoon"):
        NetworkServer.Spawn(Instantiate(myPickups[0], (transform.up * 3 + transform.position) + new Vector3(0, 0, 100), transform.rotation));
        break;
      case ("Kitten"):
        NetworkServer.Spawn(Instantiate(myPickups[1], (-transform.up * 3 + transform.position) + new Vector3(0, 0, 100), transform.rotation));
        break;
      case ("Shield"):
        Blocking = true;
        myShield = Instantiate(myPickups[2], transform, false);
        NetworkServer.Spawn(myShield);
        break;
    }
    myPickup = "None!";
  }

  IEnumerator ActivateRockets()
  {
    Boosting = true;
    yield return new WaitForSeconds(2f);
    Boosting = false;
  }

  IEnumerator ActivateCat()
  {
    CatBlocked = true;
    //CanvasGroup CatImage = myCamera.transform.Find("Canvas/Cat").GetComponent<CanvasGroup>();
    //CatImage.alpha = 1;
    for (int catCounter = 0; catCounter < 240; catCounter++)
    {
      yield return null;
    }
    //CatImage.alpha = 0;
    CatBlocked = false;
  }

  [Command]
  public void CmdLogTimeToServer(float t)
  {
    if (!isServer)
    {
      return;
    }
    print("Logging Time!");
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (!isServer)
    {
      return;
    }

    if (collision.transform.tag == "Obstacle")
    {
      CmdDestroyObject(collision.gameObject);
    }

    CmdDamage();
  }

  private void OnTriggerEnter2D(Collider2D collision)
  {
    if (!isServer)
    {
      return;
    }


    if (collision.transform.tag == "Pickup")
    {
      CmdGeneratePickupDetails();
      CmdDestroyObject(collision.gameObject);
      return;
    }
    else if (collision.transform.tag == "Harpoon")
    {
      CmdDestroyObject(collision.gameObject);
      if (!Boosting && !Blocking)
      {
        StartCoroutine(ActivateRockets());
      }
      CmdDamage();
      return;
    }
    else if (collision.transform.tag == "Kitten")
    {
      CmdDestroyObject(collision.gameObject);
      if (!CatBlocked && !Blocking)
      {
        StartCoroutine(ActivateCat());
      }
      CmdDamage();
      return;
    }
    else if (collision.transform.tag == "FakePickup")
    {
      CmdDestroyObject(collision.gameObject);
      if (!Blocking)
      {
        CmdDamage();
      }
      CmdDamage();
      return;
    }
    else if (collision.transform.tag == "FinishLine")
    {
      dead = true;
      CmdSendData();
    }
  }

  [ClientRpc]
  void RpcRespawn(){
    if (isLocalPlayer){
      transform.position = startPos;
      transform.rotation = startRot;
      myPickup = "None!";
      myScore = 0;
      myTime = 0;
      myHealth = 100;
      rgd2d.velocity = Vector3.zero;
    }
  }
}
