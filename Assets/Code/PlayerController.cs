using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerController : MonoBehaviour {
  public GameObject cratePrefab;
  public GameObject previewCrate;
  public LayerMask buildable;
  public int level;
  public TextMeshProUGUI feedbackText;
  public GameObject controls;

  private RigidbodyFirstPersonController fpController;
  private GameObject[,,] grid;
  private GameObject prefabParent;
  private new Transform camera;
  private bool isComplete;

  private static readonly Challenge[] challenges = {
    Challenge.Cube8,
    Challenge.FloatingSquare,
    Challenge.DoubledPrism,
    Challenge.Checker3,
    Challenge.MirrorElbow,
    Challenge.Outline,
    Challenge.MonotonicTower,
    Challenge.ConnectTowers,
    Challenge.AddOpposite,
    Challenge.Plus3,
    Challenge.CupsideDown,
    Challenge.FlipBoth,
    Challenge.FourQueens,
    Challenge.Loop3,
    Challenge.Factor20,
    Challenge.PrimePrism,
    Challenge.VolumeUnderArea,
    Challenge.VolumeEqualsArea,
    Challenge.VolumeOverArea,
    Challenge.FarApart,
  };

  void Awake() {
    QualitySettings.vSyncCount = 0;
    Application.targetFrameRate = 50;
  }

  void Start() {
    fpController = GetComponent<RigidbodyFirstPersonController>();
    grid = new GameObject[Constants.dimensions.x, Constants.dimensions.y, Constants.dimensions.z];
    prefabParent = GameObject.Find("Prefabs");
    camera = transform.Find("MainCamera");
    GoToLevel(PlayerPrefs.GetInt("level", 0));
  }

  IEnumerator WaitForQuestionMark(bool showFeedback = true) {
    yield return null;
    
    bool isPressed = false;
    while (!isPressed) {
      if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) && Input.GetKeyDown (KeyCode.Slash)) {
        isPressed = true;
      } else {
        yield return null;
      }
    }

    controls.SetActive(false);
    if (showFeedback) {
      feedbackText.enabled = true;
    }
    fpController.enabled = true;
  }

  void GoToLevel(int level) {
    this.level = level;
    PlayerPrefs.SetInt("level", level);
    feedbackText.text = challenges[level].prompt;
    feedbackText.enabled = level != 0;
    if (level == 0) {
      fpController.enabled = false;
      StartCoroutine(WaitForQuestionMark());
      controls.SetActive(true);
    }
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
    
    if (controls.activeInHierarchy) {
      return;
    }

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

    if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) && Input.GetKeyDown (KeyCode.Slash)) {
      feedbackText.text = challenges[level].prompt;
      feedbackText.enabled = true;
    }

    if (Input.GetButtonUp("Submit")) {
      List<HashSet<Vector3Int>> groups = Group();
      
      if (challenges[level].IsCorrect(groups, challenges[level].initialBlocks)) {
        if (level == 19) {
          feedbackText.text = "You're done with crative, but you're not done being creative! Click to start a new game.";
        } else {
          feedbackText.text = "You got it! Click to continue.";
        }
        isComplete = true;
      } else {
        feedbackText.text = "Not quite. Keep trying!";
      }
      feedbackText.enabled = true;
    }

    if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) && Input.GetKeyDown (KeyCode.Slash)) {
      print("foog");
      controls.SetActive(true);
      fpController.enabled = false;
      StartCoroutine(WaitForQuestionMark(false));
    }

    if (Input.GetKeyDown(KeyCode.R)) {
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

