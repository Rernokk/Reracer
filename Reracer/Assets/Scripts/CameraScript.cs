using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CameraScript : NetworkBehaviour
{
  Transform myPlayer;
  float myDepth = -20;
  public Transform Player
  {
    get { return myPlayer; }
    set { myPlayer = value; }
  }
  // Use this for initialization
  void Start()
  {
    if (!isLocalPlayer)
    {
      GetComponent<AudioListener>().enabled = false;
    }

  }

  // Update is called once per frame
  void FixedUpdate()
  {
    if (myPlayer != null)
    {
      Vector3 trackPos = Vector2.Lerp(myPlayer.position, transform.position, .75f);
      trackPos.z = myDepth;
      transform.position = trackPos;
    } else {
      Destroy(gameObject);
    }
  }
}
