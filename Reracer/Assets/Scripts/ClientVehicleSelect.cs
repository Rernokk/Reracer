using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VehicleType { HighSpeed, Average, HighArmor };
public class ClientVehicleSelect : MonoBehaviour {
  Player player;
  CanvasGroup subGroup;
  public Player myPlayer {
    get {
      return player;
    }

    set {
      player = value;
    }
  }

  private void Start()
  {
    subGroup = transform.Find("Canvas").GetComponent<CanvasGroup>();
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.U)){
      DisplaySelection();
    }
  }

  public void DisplaySelection(){
    subGroup.alpha = 1;
    subGroup.interactable = true;
    subGroup.blocksRaycasts = true;
  }

  public void SetVehicleType(string type){
    switch(type){
      case ("HighSpeed"):
        player.SetCarType(VehicleType.HighSpeed);
        break;

      case ("Average"):
        player.SetCarType(VehicleType.Average);
        break;

      case ("HighArmor"):
        player.SetCarType(VehicleType.HighArmor);
        break;

      default:
        player.SetCarType(VehicleType.Average);
        break;
    }
    subGroup.blocksRaycasts = false;
    subGroup.alpha = 0;
    subGroup.interactable = false;
  }
}
