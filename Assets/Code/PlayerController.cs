using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
  public GameObject cratePrefab;
  public GameObject previewCrate;
  private GameObject[,,] grid;
  private GameObject prefabParent;

  void Start() {
    grid = new GameObject[21, 11, 21];
    prefabParent = GameObject.Find("Prefabs");
  }

  void Update() {
    {
      Vector3 p = transform.position + transform.forward * 5 + Vector3.down * 0.5f;
      Vector3Int q = new Vector3Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z));
      bool isCratable = q.x >= 0 && q.x <= 20 &&
          q.y >= 0 && q.y <= 10 &&
          q.z >= 0 && q.z <= 20 &&
          grid[q.x, q.y, q.z] == null;
      previewCrate.SetActive(isCratable);
      if (isCratable) {
        previewCrate.transform.position = q;
      }

      if (isCratable && Input.GetButtonUp("Fire1")) {
        GameObject instance = Instantiate(cratePrefab, prefabParent.transform);
        instance.transform.position = q;
        grid[q.x, q.y, q.z] = instance;
      }
    }

    if (Input.GetButtonUp("Fire2")) {
      RaycastHit hit;
      if (Physics.Raycast(transform.position, transform.forward, out hit, 5, 1 << 8)) {
        Vector3 p = hit.transform.position;
        Vector3Int q = new Vector3Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), Mathf.RoundToInt(p.z));
        grid[q.x, q.y, q.z] = null;
        print(q);
        Destroy(hit.transform.gameObject);
      }
    }
  }
}

