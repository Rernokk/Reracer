using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
  Rigidbody2D rgd2d;
  public float Speed;
  public GameObject myCamera;
  public GameObject[] myPickups;

  [SyncVar]
  public GameObject Pickup, Obstacle;

  [SerializeField]
  [SyncVar]
  int myHealth = 100;

  [SyncVar]
  bool dead = false;

  [SyncVar]
  public string myPickup;

  public int Health
  {
    get { return myHealth; }
    set { myHealth = value; }
  }
  void Start()
  {
    if (!isLocalPlayer)
      return;

    rgd2d = GetComponent<Rigidbody2D>();
    myCamera = Instantiate(myCamera, new Vector3(transform.position.x, transform.position.y, -20), Quaternion.identity);
    myCamera.GetComponent<CameraScript>().Player = transform;
  }
  void Update()
  {
    if (!isLocalPlayer)
    {
      return;
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
    }

    if (Input.GetKey(KeyCode.D))
    {
      transform.Rotate(new Vector3(0, 0, -135) * Time.deltaTime);
    }

    if (Input.GetKeyDown(KeyCode.F))
    {
      CmdUsePickup();
    }

    if (Input.GetKeyDown(KeyCode.R))
    {
      CmdGeneratePickup();
    }

    if (Input.GetKeyDown(KeyCode.G))
    {
      CmdGenerateObstacle();
    }
  }

  #region Debugging
  [Command]
  void CmdGeneratePickup()
  {
    if (!isServer)
    {
      return;
    }
    GameObject temp = Instantiate(Pickup, transform.up * 3 + transform.position, Quaternion.identity);
    NetworkServer.Spawn(temp);
  }

  [Command]
  void CmdGenerateObstacle()
  {
    if (!isServer)
    {
      return;
    }

    GameObject temp = Instantiate(Obstacle, transform.up * 3 + transform.position, Quaternion.identity);
    NetworkServer.Spawn(temp);
  }
  #endregion

  [Command]
  void CmdDamage()
  {
    if (!isServer)
    {
      return;
    }
    Health -= 100;
    if (Health <= 0)
    {
      //Player Dies.
      Destroy(myCamera.gameObject);
      Destroy(gameObject);
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
    if (!isServer){
      return;
    }
    myPickup = "Harpoon";
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
        NetworkServer.Spawn(Instantiate(myPickups[0], (transform.up * 3 + transform.position), Quaternion.identity));
        break;
    }
    myPickup = "None!";
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
    if (!isServer){
      return;
    }

    if (collision.transform.tag == "Pickup")
    {
      CmdGeneratePickupDetails();
      CmdDestroyObject(collision.gameObject);
      return;
    }
  }
}
