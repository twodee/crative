using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
  public GameObject cratePrefab;
  public GameObject previewCrate;
  public LayerMask buildable;
  private GameObject[,,] grid;
  private GameObject prefabParent;
  private new Transform camera;

  void Start() {
    grid = new GameObject[21, 11, 21];
    prefabParent = GameObject.Find("Prefabs");
    camera = transform.Find("MainCamera");
  }

  void Update() {
    bool isCratable = false;

    RaycastHit hit;
    if (Physics.Raycast(transform.position, camera.forward, out hit, 5f, buildable)) {
      GameObject target = hit.transform.gameObject;

      Vector3Int q = Vector3Int.zero;

      if (target.CompareTag("Ground")) {
        Vector3 p = hit.point;
        q = new Vector3Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z));
        isCratable = IsCratable(q);
      } else {
        Vector3 diff = hit.point - hit.transform.position;
        Debug.LogFormat("{0} {1} {2}", diff.x, diff.y, diff.z);
        //print(hit.point - hit.transform.position);
      }

      if (isCratable) {
        previewCrate.transform.position = q;

        if (Input.GetButtonUp("Fire1")) {
          GameObject instance = Instantiate(cratePrefab, prefabParent.transform);
          instance.transform.position = q;
          grid[q.x, q.y, q.z] = instance;
        }
      }
    }

    previewCrate.SetActive(isCratable);

    //  {
    //  Vector3 p = transform.position + transform.forward * 5 + Vector3.down * 0.5f;
    //  Vector3Int q = new Vector3Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z));
    //  bool isCratable = IsCratable(q);
    //  previewCrate.SetActive(isCratable);
    //  if (isCratable) {
    //    previewCrate.transform.position = q;
    //  }

    // 
    //}

    //if (Input.GetButtonUp("Fire2")) {
    //  RaycastHit hit;
    //  if (Physics.Raycast(transform.position, transform.forward, out hit, 5, 1 << 8)) {
    //    Vector3 p = hit.transform.position;
    //    Vector3Int q = new Vector3Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z));
    //    grid[q.x, q.y, q.z] = null;
    //    print(q);
    //    Destroy(hit.transform.gameObject);
    //  }
    //}
  }

  private bool IsCratable(Vector3Int p) {
    return p.x >= 0 && p.x <= 20 &&
      p.y >= 0 && p.y <= 10 &&
      p.z >= 0 && p.z <= 20 &&
      grid[p.x, p.y, p.z] == null;
  }
}

