using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
  public GameObject cratePrefab;
  public GameObject previewCrate;
  public LayerMask buildable;
  public float rayPush;

  private GameObject[,,] grid;
  private GameObject prefabParent;
  private new Transform camera;
  private const int WIDTH = 21;
  private const int HEIGHT = 11;
  private const int DEPTH = 21;

  void Start() {
    grid = new GameObject[WIDTH, HEIGHT, DEPTH];
    prefabParent = GameObject.Find("Prefabs");
    camera = transform.Find("MainCamera");
  }

  void Update() {
    bool isGrid = false;

    //Debug.DrawRay(camera.position + camera.forward * rayPush, camera.forward * 5, Color.cyan);

    RaycastHit hit;
    if (Physics.Raycast(camera.position + camera.forward * rayPush, camera.forward, out hit, 5f, buildable)) {
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

    if (Input.GetButtonUp("Fire1")) {
      Group();
    }
  }

  private bool IsCrate(Vector3Int p) {
    return grid[p.x, p.y, p.z] != null;
  }

  private bool IsGrid(Vector3Int p) {
    return p.x >= 0 && p.x <= 20 &&
      p.y >= 0 && p.y <= 10 &&
      p.z >= 0 && p.z <= 20;
  }

  private void Group() {
    List<HashSet<Vector3Int>> groups = new List<HashSet<Vector3Int>>();
    bool[,,] isVisited = new bool[WIDTH, HEIGHT, DEPTH];

    for (int z = 0; z < DEPTH; ++z) {
      for (int y = 0; y < HEIGHT; ++y) {
        for (int x = 0; x < WIDTH; ++x) {
          if (!isVisited[x, y, z] && grid[x, y, z] != null) {
            groups.Add(Flood(new Vector3Int(x, y, z), isVisited));
          }
        }
      }
    }

    Debug.LogFormat("number of groups: {0}", groups.Count);
  }

  private HashSet<Vector3Int> Flood(Vector3Int seed, bool[,,] isVisited) {
    HashSet<Vector3Int> group = new HashSet<Vector3Int>();

    Queue<Vector3Int> queue = new Queue<Vector3Int>();
    System.Func<int, int, int, bool> IsFloodable = (x, y, z) => !isVisited[x, y, z] && grid[x, y, z] != null;

    if (IsFloodable(seed.x, seed.y, seed.z)) {
      queue.Enqueue(seed);
    }

    while (queue.Count > 0) {
      Vector3Int p = queue.Dequeue();

      // Travel to left extreme so we work in just one direction.
      while (p.x > 0 && IsFloodable(p.x - 1, p.y, p.z)) {
        --p.x;
      }

      bool[] isSeededBefore = { false, false, false };
      bool[] isSeededAfter = { false, false, false };

      while (p.x < WIDTH && IsFloodable(p.x, p.y, p.z)) {
        isVisited[p.x, p.y, p.z] = true;
        group.Add(p);

        for (int d = 1; d <= 2; ++d) {
          // Check the before scanline.
          --p[d];
          if (!isSeededBefore[d] && p[d] >= 0 && IsFloodable(p.x, p.y, p.z)) {
            queue.Enqueue(p);
            isSeededBefore[d] = true;
          } else if (isSeededBefore[d] && p[d] >= 0 && !IsFloodable(p.x, p.y, p.z)) {
            isSeededBefore[d] = false;
          }

          // Check the after scanline.
          p[d] += 2;
          if (!isSeededAfter[d] && p[d] >= 0 && IsFloodable(p.x, p.y, p.z)) {
            queue.Enqueue(p);
            isSeededAfter[d] = true;
          } else if (isSeededAfter[d] && p[d] >= 0 && !IsFloodable(p.x, p.y, p.z)) {
            isSeededAfter[d] = false;
          }

          // Restore component.
          --p[d];
        }

        ++p.x;
      }
    }

    return group;
  }
}

