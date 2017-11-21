using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UI_Controller : NetworkBehaviour
{
  CanvasGroup cat, instructions;
  Text item;
  public Text debugText;
  Player script;
  Leaderboard lead;

  [SyncVar]
  public string myText;
  // Use this for initialization
  void Start()
  {
    cat = transform.Find("Canvas/Cat").GetComponent<CanvasGroup>();
    item = transform.Find("Canvas/Panel/Item").GetComponent<Text>();
    debugText = transform.Find("Canvas/Debug/Text").GetComponent<Text>();
    instructions = transform.Find("Canvas/InstructionsPanel").GetComponent<CanvasGroup>();
    script = GetComponent<CameraScript>().Player.GetComponent<Player>();
    lead = script.lead.GetComponent<Leaderboard>();
    script.myUI = this;
    myText = "";
  }

  // Update is called once per frame
  void Update()
  {
    cat.alpha = (script.CatBlocked ? 1 : 0);
    string myText = script.myPickup;
    item.text = myText;

    if (isServer)
    {
      debugText.text = lead.myLeaderText;
    }

    if (Input.GetKeyDown(KeyCode.F1)){
      instructions.alpha = Mathf.Abs(instructions.alpha - 1);
    }

  }
}
