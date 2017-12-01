using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class KittenScript : NetworkBehaviour
{
  Rigidbody2D rgd2d;
  [SerializeField]
  float speed;
  // Use this for initialization
  void Start()
  {
    if (!isServer)
    {
      return;
    }
    rgd2d = GetComponent<Rigidbody2D>();
  }

  // Update is called once per frame
  void Update()
  {
    if (!isServer)
    {
      return;
    }
    rgd2d.velocity = -transform.up * speed * Time.deltaTime;
  }

  private void OnTriggerEnter2D(Collider2D collision)
  {
    if (!isServer)
    {
      return;
    }
    print("Hit on Server");
    Destroy(gameObject);
  }
}
