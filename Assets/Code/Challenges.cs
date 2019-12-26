using System.Linq;
using System.Collections.Generic;
using UnityEngine;

struct Challenge {
  public string prompt;
  public System.Func<List<HashSet<Vector3Int>>, Vector3Int[], bool> IsCorrect;
  public Vector3Int[] initialBlocks;

  public Challenge(string prompt, System.Func<List<HashSet<Vector3Int>>, Vector3Int[], bool> IsCorrect, params Vector3Int[] initialBlocks) {
    this.prompt = prompt;
    this.IsCorrect = IsCorrect;
    this.initialBlocks = initialBlocks;
  }

  private static bool Has(HashSet<Vector3Int> group, params Vector3Int[] ps) {
    return ps.All(p => group.Contains(p));
  }

  private static int CountNeighbors(HashSet<Vector3Int> group, Vector3Int p) {
    return new Vector3Int[] {
      p + new Vector3Int(1, 0, 0),
      p + new Vector3Int(-1, 0, 0),
      p + new Vector3Int(0, 1, 0),
      p + new Vector3Int(0, -1, 0),
      p + new Vector3Int(0, 0, 1),
      p + new Vector3Int(0, 0, -1),
    }.Count(q => group.Contains(q));
  }

  private static HashSet<Vector3Int> Combine(List<HashSet<Vector3Int>> groups) {
    HashSet<Vector3Int> all = new HashSet<Vector3Int>();
    foreach (HashSet<Vector3Int> group in groups) {
      all.UnionWith(group);
    }
    return all;
  }

  public static readonly Challenge Cube8 = new Challenge("Make a cube out of 8 crates.", (groups, initialBlocks) => {
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

    return Has(group,
      new Vector3Int(min.x, min.y, min.z),
      new Vector3Int(min.x + 1, min.y, min.z),
      new Vector3Int(min.x, min.y + 1, min.z),
      new Vector3Int(min.x + 1, min.y + 1, min.z),
      new Vector3Int(min.x, min.y, min.z + 1),
      new Vector3Int(min.x + 1, min.y, min.z + 1),
      new Vector3Int(min.x, min.y + 1, min.z + 1),
      new Vector3Int(min.x + 1, min.y + 1, min.z + 1));
  });

  public static readonly Challenge Plus3 = new Challenge("Create a structure out of 7 crates, with 6 of them touching only 1 other crate.", (groups, initialBlocks) => {
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

    return Has(group,
      mid,
      mid + Vector3Int.right,
      mid + Vector3Int.left,
      mid + Vector3Int.up,
      mid + Vector3Int.down,
      mid + new Vector3Int(0, 0, 1),
      mid + new Vector3Int(0, 0, -1));
  });

  public static readonly Challenge DoubledPrism = new Challenge("Create a rectangular prism that follows these rules: the height is twice the width, and the width and depth are the same.", (groups, initialBlocks) => {
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
  });

  public static readonly Challenge FloatingSquare = new Challenge("Create a floating square platform using more than 1 crate. Oh, by the way, you can delete a crate by right-clicking on it.", (groups, initialBlocks) => {
    if (groups.Count != 1) {
      return false;
    }

    HashSet<Vector3Int> group = groups[0];

    if (group.Count == 1) {
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

    int iFlat;
    if (min.x == max.x) {
      iFlat = 0;
    } else if (min.y == max.y) {
      iFlat = 1;
    } else if (min.z == max.z) {
      iFlat = 2;
    } else {
      return false;
    }

    int d0 = (iFlat + 1) % 3;
    int d1 = (iFlat + 2) % 3;

    if (max[d0] - min[d0] != max[d1] - min[d1]) {
      return false;
    }

    for (int r = min[d0]; r <= max[d0]; ++r) {
      for (int c = min[d1]; c <= max[d1]; ++c) {
        Vector3Int p = Vector3Int.zero;
        p[iFlat] = min[iFlat];
        p[d0] = r;
        p[d1] = c;
        if (!group.Contains(p)) {
          return false;
        }
      }
    }

    return true;
  });

  public static readonly Challenge MirrorElbow = new Challenge("Add 1 crate to create a symmetric structure.", (groups, initialBlocks) => {
    if (groups.Count != 1) {
      return false;
    }

    HashSet<Vector3Int> group = groups[0];

    if (group.Count != 4) {
      return false;
    }

    return
      initialBlocks.All(p => group.Contains(p)) &&
      (group.Contains(new Vector3Int(Constants.dimensions.x / 2 + 1, 0, Constants.dimensions.z / 2)) ||
       group.Contains(new Vector3Int(Constants.dimensions.x / 2 - 1, 1, Constants.dimensions.z / 2)) ||
       group.Contains(new Vector3Int(Constants.dimensions.x / 2, 2, Constants.dimensions.z / 2)));
  },
        new Vector3Int(Constants.dimensions.x / 2, 0, Constants.dimensions.z / 2),
        new Vector3Int(Constants.dimensions.x / 2, 1, Constants.dimensions.z / 2),
        new Vector3Int(Constants.dimensions.x / 2 + 1, 1, Constants.dimensions.z / 2));

  public static readonly Challenge Checker3 = new Challenge("Using 5 blocks, make a 3 by 3 checkerboard with all blocks touching the ground.", (groups, initialBlocks) => {
    if (groups.Count != 5) {
      return false;
    }

    HashSet<Vector3Int> group = Combine(groups);

    if (group.Count != 5) {
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

    if (min.y != 0) {
      return false;
    }

    return Has(group,
      min,
      new Vector3Int(min.x + 2, min.y, min.z),
      new Vector3Int(min.x, min.y, min.z + 2),
      new Vector3Int(min.x + 1, min.y, min.z + 1),
      new Vector3Int(min.x + 2, min.y, min.z + 2));
  });

  public static readonly Challenge Outline = new Challenge("Make a structure that uses 8 crates, with each touching 2 others.", (groups, initialCrates) => {
    if (groups.Count != 1) {
      return false;
    }

    HashSet<Vector3Int> group = groups[0];

    if (group.Count != 8) {
      return false;
    }

    return group.All(p => CountNeighbors(group, p) == 2);
  });

  public static readonly Challenge Template = new Challenge("...", (groups, initialBlocks) => {
    if (groups.Count != 1) {
      return false;
    }

    HashSet<Vector3Int> group = groups[0];

    if (group.Count != 4) {
      return false;
    }

    return
      initialBlocks.All(p => group.Contains(p)) &&
      (group.Contains(new Vector3Int(Constants.dimensions.x / 2 + 1, 0, Constants.dimensions.z / 2)) ||
       group.Contains(new Vector3Int(Constants.dimensions.x / 2 - 1, 1, Constants.dimensions.z / 2)) ||
       group.Contains(new Vector3Int(Constants.dimensions.x / 2, 2, Constants.dimensions.z / 2)));
  });
}
