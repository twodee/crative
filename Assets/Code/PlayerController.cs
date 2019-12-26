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

  private static readonly Challenge[] challenges = {
    Challenge.Cube8,
    Challenge.Checker3,
    Challenge.FloatingSquare,
    Challenge.MirrorElbow,
    Challenge.DoubledPrism,
    Challenge.Outline,
    Challenge.Plus3,
  };

  void Start() {
    grid = new GameObject[Constants.dimensions.x, Constants.dimensions.y, Constants.dimensions.z];
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

    for (int z = 0; z < Constants.dimensions.z; ++z) {
      for (int y = 0; y < Constants.dimensions.y; ++y) {
        for (int x = 0; x < Constants.dimensions.x; ++x) {
          grid[x, y, z] = null;
        }
      }
    }

    foreach (Vector3Int p in challenges[level].initialBlocks) {
      GameObject instance = Instantiate(cratePrefab, prefabParent.transform);
      instance.transform.position = p;
      grid[p.x, p.y, p.z] = instance;
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
        if (IsGrid(q) && (new Vector3(q.x, q.y, q.z) - camera.transform.position).magnitude > 0.87) {
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
      feedbackText.enabled = true;
    }

    if (Input.GetButtonUp("Submit")) {
      List<HashSet<Vector3Int>> groups = Group();

      if (challenges[level].IsCorrect(groups, challenges[level].initialBlocks)) {
        feedbackText.text = "You did it! Life's full of good things. Click to continue.";
        isComplete = true;
      } else {
        feedbackText.text = "Try again, we all make mistakes sometimes.";
      }
      feedbackText.enabled = true;
    }

    if (Input.GetKeyDown(KeyCode.Alpha0)) {
      GoToLevel(level);
    }
  }

  private bool IsCrate(Vector3Int p) {
    return grid[p.x, p.y, p.z] != null;
  }

  private bool IsGrid(Vector3Int p) {
    return p.x >= 0 && p.x < Constants.dimensions.x &&
      p.y >= 0 && p.y < Constants.dimensions.y &&
      p.z >= 0 && p.z < Constants.dimensions.z;
  }

  private List<HashSet<Vector3Int>> Group() {
    List<HashSet<Vector3Int>> groups = new List<HashSet<Vector3Int>>();
    bool[,,] isVisited = new bool[Constants.dimensions.x, Constants.dimensions.y, Constants.dimensions.z];

    for (int z = 0; z < Constants.dimensions.z; ++z) {
      for (int y = 0; y < Constants.dimensions.y; ++y) {
        for (int x = 0; x < Constants.dimensions.x; ++x) {
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

      while (p.x < Constants.dimensions.x && IsFloodable(p.x, p.y, p.z)) {
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
          if (!isSeededAfter[d] && p[d] < Constants.dimensions[d] && IsFloodable(p.x, p.y, p.z)) {
            queue.Enqueue(p);
            isSeededAfter[d] = true;
          } else if (isSeededAfter[d] && p[d] < Constants.dimensions[d] && !IsFloodable(p.x, p.y, p.z)) {
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

