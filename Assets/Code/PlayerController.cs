using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
  public GameObject cratePrefab;

  void Start() {

  }

  void Update() {
    if (Input.GetButtonUp("Fire1")) {
      GameObject instance = Instantiate(cratePrefab);
      instance.transform.position = transform.position + transform.forward * 5 + Vector3.down * 0.5f;
    }
  }
}
