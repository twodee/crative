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
    bool isGrid = false;

    RaycastHit hit;
    if (Physics.Raycast(transform.position, camera.forward, out hit, 5f, buildable)) {
      GameObject target = null;

      Vector3Int q = Vector3Int.zero;

      if (hit.transform.CompareTag("Ground")) {
        Vector3 p = hit.point;
        q = new Vector3Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z));
        isGrid = IsGrid(q);
      } else {
        target = hit.transform.gameObject;

        Vector3 diff = hit.point - hit.transform.position;
        Vector3 mag = new Vector3(Mathf.Abs(diff.x), Mathf.Abs(diff.y), Mathf.Abs(diff.z));
        Vector3 p = hit.transform.position;
        if (mag.x > mag.y && mag.x > mag.z) {
          q = new Vector3Int(Mathf.RoundToInt(p.x + Mathf.Sign(diff.x)), Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z));
        } else if (mag.y > mag.x && mag.y > mag.z) {
          q = new Vector3Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y + Mathf.Sign(diff.y)), Mathf.RoundToInt(p.z));
        } else {
          q = new Vector3Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z + Mathf.Sign(diff.z)));
        }
        isGrid = IsGrid(q);
      }

      if (isGrid) {
        if (target != null && Input.GetButtonUp("Fire2")) {
          q = new Vector3Int(Mathf.RoundToInt(target.transform.position.x), Mathf.RoundToInt(target.transform.position.y), Mathf.RoundToInt(target.transform.position.z));
          grid[q.x, q.y, q.z] = null;
          Destroy(target);
        } else {
          previewCrate.transform.position = q;
          if (Input.GetButtonUp("Fire1")) {
            GameObject instance = Instantiate(cratePrefab, prefabParent.transform);
            instance.transform.position = q;
            grid[q.x, q.y, q.z] = instance;
          }
        }
      }
    }

    previewCrate.SetActive(isGrid);
  }

  private bool IsCrate(Vector3Int p) {
    return grid[p.x, p.y, p.z] != null;
  }

  private bool IsGrid(Vector3Int p) {
    return p.x >= 0 && p.x <= 20 &&
      p.y >= 0 && p.y <= 10 &&
      p.z >= 0 && p.z <= 20;
  }
}

