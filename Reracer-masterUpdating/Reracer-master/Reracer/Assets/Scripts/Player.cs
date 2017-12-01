using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
  public float speed, turningRate;
  public GameObject myCamera;
  public GameObject[] myPickups;
  Vector3 startPos;
  Quaternion startRot;
  private const short CommandChannel = 136;

  [SyncVar]
  public bool CatBlocked = false;

  [SyncVar]
  public GameObject Pickup, Obstacle;

  [SyncVar]
  public float myTime;

  float counter = 3f;

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

  [SyncVar]
  public string carSpriteName;

  Image HealthBar;
  int originalSteeringValue = 135, originalSpeedLimit = 60;
  GameObject myShield;
  Rigidbody2D rgd2d;
  public GameObject UI;
  public UI_Controller myUI;
  public GameObject MyLoginUI;
  public GameObject MyCarSelectionUI;
  public GameObject lead;
  public Text CountdownText;
  bool isCountingDown = false;

  [SyncVar]
  public string passAttempt;
  Dictionary<string, string> accountDictionary;

  [SyncVar]
  int serverCount = 0;

  public Sprite[] carSprites;

  public int Health
  {
    get { return myHealth; }
    set { myHealth = value; }
  }
  private void Start()
  {
    if (!isLocalPlayer)
      return;


    NetworkManager.singleton.client.RegisterHandler(CommandChannel, ClientReceiveCommand);
    NetworkManager.singleton.client.RegisterHandler(137, ClientRecieveCarRequest);
    NetworkServer.RegisterHandler(137, ClientRecieveCarRequest);
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
    CountdownText = myCamera.transform.Find("Canvas/CountdownText").GetComponent<Text>();
    lead = myCamera.transform.Find("Leaderboard").gameObject;
    MyCarSelectionUI = Instantiate(MyCarSelectionUI, Vector3.zero, Quaternion.identity);
    MyCarSelectionUI.GetComponent<ClientVehicleSelect>().myPlayer = this;
    if (isServer)
    {
      myCamera.transform.Find("Canvas/Start").GetComponent<CanvasGroup>().alpha = 1;
      myCamera.transform.Find("Canvas/Start").GetComponent<CanvasGroup>().interactable = true;
      myCamera.transform.Find("Canvas/Start").GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
  }
  void Update()
  {

    if (isCountingDown)
    {
      counter -= Time.deltaTime;
      CountdownText.text = Mathf.Ceil(counter).ToString();
      if (CountdownText.text == "0")
      {
        CountdownText.text = "Go!";
      }
    }

    if (isInLobby)
      return;

    if (!dead)
    {
      myTime += Time.deltaTime;
      myScore += Time.deltaTime * 5;
    }

    if (!dead && Input.GetKeyDown(KeyCode.F2))
    {
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
        rgd2d.velocity += (Vector2)transform.up * speed * Time.deltaTime;
      }
    }
    if (Input.GetKey(KeyCode.S))
    {
      rgd2d.velocity -= (Vector2)transform.up * speed * Time.deltaTime;
    }

    if (Input.GetKey(KeyCode.A))
    {
      transform.Rotate(new Vector3(0, 0, turningRate) * Time.deltaTime);
      if (CatBlocked)
      {
        catCounter++;
      }
    }

    if (Input.GetKey(KeyCode.D))
    {
      transform.Rotate(new Vector3(0, 0, -turningRate) * Time.deltaTime);
      if (CatBlocked)
      {
        catCounter++;
      }
    }

    if (Input.GetKeyDown(KeyCode.F))
    {
      CmdUsePickup();
    }

    if (Input.GetKeyDown(KeyCode.O))
    {
      print(GameObject.FindGameObjectsWithTag("Player").Length);
    }

    if (Boosting)
    {
      rgd2d.velocity += (Vector2)transform.up * 3 * speed * Time.deltaTime;
    }

    #region Debug

    if (Input.GetKeyDown(KeyCode.R))
    {
      NetworkServer.SendToAll(CommandChannel, new StringMessage("COUNTDOWN"));
    }

    if (Input.GetKeyDown(KeyCode.G))
    {
      NetworkManager.singleton.client.Send(134, new StringMessage(myName + "," + myTime + "," + Mathf.Ceil(myScore)));
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
    //GameObject.Find("Leaderboard").GetComponent<Leaderboard>().CmdAddEntry(myTime,(int)myScore,myName);
    NetworkManager.singleton.client.Send(134, new StringMessage(myName + "," + myTime + "," + Mathf.Ceil(myScore)));
  }
  #endregion

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
        NetworkManager.singleton.client.Send(134, new StringMessage(myName + "," + myTime + "," + Mathf.Ceil(myScore)));
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
  void RpcRespawn()
  {
    if (isLocalPlayer)
    {
      transform.position = startPos;
      transform.rotation = startRot;
      myPickup = "None!";
      myScore = 0;
      myTime = 0;
      myHealth = 100;
      rgd2d.velocity = Vector3.zero;
    }
  }

  void ClientReceiveCommand(NetworkMessage message)
  {
    string mes = message.ReadMessage<StringMessage>().value;
    if (mes == "RESPAWN")
    {
      if (isLocalPlayer)
      {
        transform.position = startPos;
        transform.rotation = startRot;
        myPickup = "None!";
        myScore = 0;
        myTime = 0;
        myHealth = 100;
        rgd2d.velocity = Vector3.zero;
      }
    }
    else if (mes == "COUNTDOWN")
    {
      isInLobby = true;
      isCountingDown = true;
      if (isLocalPlayer)
      {
        transform.position = startPos;
        transform.rotation = startRot;
        myPickup = "None!";
        myScore = 0;
        myTime = 0;
        myHealth = 100;
        rgd2d.velocity = Vector3.zero;
      }
      StartCoroutine(Countdown());
    }
    else if (isInLobby && mes == "UNLOCK")
    {
      isInLobby = false;
    } else if (isInLobby && mes == "UPDATECARS"){
      
    }
  }

  void Respawn()
  {
    if (isLocalPlayer)
    {
      transform.position = startPos;
      transform.rotation = startRot;
      myPickup = "None!";
      myScore = 0;
      myTime = 0;
      myHealth = 100;
      rgd2d.velocity = Vector3.zero;
    }
  }

  public void DestroyLoginInterface()
  {
    Destroy(MyLoginUI);
    Respawn();
  }

  public void SubmitScoreData()
  {
    StringMessage message = new StringMessage();
    message.value = myTime + "," + myScore;
    NetworkManager.singleton.client.Send(134, message);
  }

  public IEnumerator Countdown()
  {
    counter = 3f;
    yield return new WaitForSeconds(3f);
    isCountingDown = false;
    NetworkServer.SendToAll(CommandChannel, new StringMessage("UNLOCK"));
    yield return new WaitForSeconds(1f);
    CountdownText.text = "";
  }

  public void ClientSetCarType(VehicleType type){
    Sprite getSprite;
    switch (type){
      case (VehicleType.Average):
        speed = 15;
        turningRate = 135;
        getSprite = carSprites[0];
        Health = 100;
        carSpriteName = "Average";
        break;
      case (VehicleType.HighArmor):
        speed = 3;
        turningRate = 105;
        getSprite = carSprites[1];
        Health = 150;
        carSpriteName = "HighArmor";
        break;
      case (VehicleType.HighSpeed):
        speed = 50;
        turningRate = 150;
        getSprite = carSprites[2];
        Health = 50;
        carSpriteName = "HighSpeed";
        break;
      default:
        speed = 15;
        turningRate = 135;
        getSprite = carSprites[0];
        Health = 100;
        carSpriteName = "Average";
        break;
    }
    GetComponent<SpriteRenderer>().sprite = getSprite;
    StringMessage mes = new StringMessage(myName + "," + carSpriteName);
    print("Sending Request: " + mes.value);
    NetworkManager.singleton.client.Send(137, mes);
  }

  public void ClientRecieveCarRequest (NetworkMessage message)
  {
    StringMessage mes = message.ReadMessage<StringMessage>();
    print("Client Receiving: " + mes.value);
    string[] parts = mes.value.Split(',');
    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
    foreach (GameObject p in players)
    {
      if (p.GetComponent<Player>().myName == parts[0])
      {
        print("Client ID'd Player: " + parts[0]);
        p.GetComponent<SpriteRenderer>().sprite = carSprites[2];
        return;
      }
    }
  }
}
