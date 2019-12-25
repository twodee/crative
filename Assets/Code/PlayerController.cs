using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour {
  public GameObject cratePrefab;
  public GameObject previewCrate;
  public LayerMask buildable;
  public int level;
  public TextMeshProUGUI feedbackText;

  private GameObject[,,] grid;
  private GameObject prefabParent;
  private new Transform camera;
  private bool isComplete;
  private readonly Vector3Int dimensions = new Vector3Int(21, 11, 21);

  void Start() {
    grid = new GameObject[dimensions.x, dimensions.y, dimensions.z];
    prefabParent = GameObject.Find("Prefabs");
    camera = transform.Find("MainCamera");
    GoToLevel(0);
  }

  void GoToLevel(int level) {
    this.level = level;
    feedbackText.text = challenges[level].prompt;
    feedbackText.enabled = true;
    isComplete = false;

    for (int i = 0; i < prefabParent.transform.childCount; i += 1) {
      Destroy(prefabParent.transform.GetChild(i).gameObject);
    }

    for (int z = 0; z < dimensions.z; ++z) {
      for (int y = 0; y < dimensions.y; ++y) {
        for (int x = 0; x < dimensions.x; ++x) {
          grid[x, y, z] = null;
        }
      }
    }
  }

  void Update() {
    if (isComplete && Input.GetButtonUp("Fire1")) {
      GoToLevel((level + 1) % challenges.Length);
      return;
    }

    previewCrate.SetActive(false);
    RaycastHit hit;
    if (Physics.Raycast(camera.position, camera.forward, out hit, 5f, buildable)) {
      GameObject target = hit.transform.gameObject;

      // If they are looking at a crate and are right-clicking, delete it.
      if (target.CompareTag("Crate") && Input.GetButtonUp("Fire2")) {
        Destroy(target);
      }

      // Otherwise they are looking at the ground or a crate. We add a new crate
      // if they are firing or show a preview, maybe.
      else {

        // Determine the looked-at location. When looking at the ground, we
        // just round. When looking at another crate, we find the appropriate
        // neighbor location.
        Vector3Int q;
        if (target.CompareTag("Ground")) {
          q = new Vector3Int(Mathf.RoundToInt(hit.point.x), Mathf.RoundToInt(hit.point.y), Mathf.RoundToInt(hit.point.z));
        } else {
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
        }

        // If the target location is on the grid, but not too close to the
        // player, we add a crate when firing or show the preview if the
        // location is unoccupied.
        if (IsGrid(q) && (new Vector2(q.x, q.z) - new Vector2(camera.transform.position.x, camera.transform.position.z)).magnitude > 1) {
          if (Input.GetButtonUp("Fire1")) {
            GameObject instance = Instantiate(cratePrefab, prefabParent.transform);
            instance.transform.position = q;
            grid[q.x, q.y, q.z] = instance;
            feedbackText.enabled = false;
          } else if (!IsCrate(q)) {
            previewCrate.transform.position = q;
            previewCrate.SetActive(true);
          }
        }
      }
    }

    if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) && Input.GetKeyDown (KeyCode.Slash)) {
      feedbackText.text = challenges[level].prompt;
      feedbackText.enabled = !feedbackText.enabled;
    }

    if (Input.GetButtonUp("Submit")) {
      List<HashSet<Vector3Int>> groups = Group();

      if (challenges[level].IsCorrect(groups)) {
        feedbackText.text = "Congratulations! you won epicly! keep doing that! it's good for you!";
        isComplete = true;
      } else {
        feedbackText.text = "You failed miserably";
      }
      feedbackText.enabled = true;
    }
  }

  private bool IsCrate(Vector3Int p) {
    return grid[p.x, p.y, p.z] != null;
  }

  private bool IsGrid(Vector3Int p) {
    return p.x >= 0 && p.x < dimensions.x &&
      p.y >= 0 && p.y < dimensions.y &&
      p.z >= 0 && p.z < dimensions.z;
  }

  private List<HashSet<Vector3Int>> Group() {
    List<HashSet<Vector3Int>> groups = new List<HashSet<Vector3Int>>();
    bool[,,] isVisited = new bool[dimensions.x, dimensions.y, dimensions.z];

    for (int z = 0; z < dimensions.z; ++z) {
      for (int y = 0; y < dimensions.y; ++y) {
        for (int x = 0; x < dimensions.x; ++x) {
          if (!isVisited[x, y, z] && grid[x, y, z] != null) {
            groups.Add(Flood(new Vector3Int(x, y, z), isVisited));
          }
        }
      }
    }

    return groups;
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

      while (p.x < dimensions.x && IsFloodable(p.x, p.y, p.z)) {
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
          if (!isSeededAfter[d] && p[d] < dimensions[d] && IsFloodable(p.x, p.y, p.z)) {
            queue.Enqueue(p);
            isSeededAfter[d] = true;
          } else if (isSeededAfter[d] && p[d] < dimensions[d] && !IsFloodable(p.x, p.y, p.z)) {
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

  private static readonly Challenge[] challenges = {
    new Challenge ("Make a cube out of eight blocks.", groups => {
      if (groups.Count != 1) {
        return false;
      }

      HashSet<Vector3Int> group = groups[0];
      if (group.Count != 8) {
        return false;
      }

      Vector3Int min = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
      foreach (Vector3Int p in group) {
        if (p.x < min.x) {
          min.x = p.x;
        }
        if (p.y < min.y) {
          min.y = p.y;
        }
        if (p.z < min.z) {
          min.z = p.z;
        }
      }

      return
        group.Contains(new Vector3Int(min.x, min.y, min.z)) &&
        group.Contains(new Vector3Int(min.x + 1, min.y, min.z)) &&
        group.Contains(new Vector3Int(min.x, min.y + 1, min.z)) &&
        group.Contains(new Vector3Int(min.x + 1, min.y + 1, min.z)) &&
        group.Contains(new Vector3Int(min.x, min.y, min.z + 1)) &&
        group.Contains(new Vector3Int(min.x + 1, min.y, min.z + 1)) &&
        group.Contains(new Vector3Int(min.x, min.y + 1, min.z + 1)) &&
        group.Contains(new Vector3Int(min.x + 1, min.y + 1, min.z + 1));
    }),

    new Challenge ("Create a rectangular prism that follows these rules: the height is twice the width, and the width and depth are the same.", groups => {
      if (groups.Count != 1) {
        return false;
      }

      HashSet<Vector3Int> group = groups[0];

      Vector3Int min = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
      Vector3Int max = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

      foreach (Vector3Int p in group) {
        if (p.x < min.x) {
          min.x = p.x;
        } 
        if (p.y < min.y) {
          min.y = p.y;
        }
        if (p.z < min.z) {
          min.z = p.z;
        }
        if (p.x > max.x) {
          max.x = p.x;
        }
        if (p.y > max.y) {
          max.y = p.y;
        }
        if (p.z > max.z) {
          max.z = p.z;
        }
      }

      Vector3Int dims = max - min + new Vector3Int(1, 1, 1);
      if (dims.x != dims.z) {
        return false;
      }

      if (dims.y != dims.x * 2) {
        return false;
      }

      if (group.Count != dims.y * dims.x * dims.z) {
        return false;
      }

      return true;
    }),

    // 3D plus
    new Challenge ("Create a structure out of 7 blocks, with 6 of them touching only 1 other block.", groups => {
      if (groups.Count != 1) {
        return false;
      }

      HashSet<Vector3Int> group = groups[0];

      if (group.Count != 7) {
        return false;
      }

      Vector3Int min = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
      Vector3Int max = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
      foreach (Vector3Int p in group) {
        if (p.x < min.x) {
          min.x = p.x;
        }
        if (p.y < min.y) {
          min.y = p.y;
        }
        if (p.z < min.z) {
          min.z = p.z;
        }
        if (p.x > max.x) {
          max.x = p.x;
        }
        if (p.y > max.y) {
          max.y = p.y;
        }
        if (p.z > max.z) {
          max.z = p.z;
        }
      }

      Vector3Int mid = max + min;
      mid.x /= 2;
      mid.y /= 2;
      mid.z /= 2;

      print(min);
      print(mid);
      print(max);

      return GroupHas(group,
        mid,
        mid + Vector3Int.right,
        mid + Vector3Int.left,
        mid + Vector3Int.up,
        mid + Vector3Int.down,
        mid + new Vector3Int(0, 0, 1),
        mid + new Vector3Int(0, 0, -1));
    })

  };

  private static bool GroupHas(HashSet<Vector3Int> group, params Vector3Int[] locations) {
    return locations.All(p => group.Contains(p));
  }
}

struct Challenge {
  public string prompt;
  public System.Func<List<HashSet<Vector3Int>>, bool> IsCorrect;

  public Challenge(string prompt, System.Func<List<HashSet<Vector3Int>>, bool> IsCorrect) {
    this.prompt = prompt;
    this.IsCorrect = IsCorrect;
  }
}