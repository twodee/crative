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

  public static readonly Challenge FourQueens =
    new Challenge("Place four crates such that no crate shares an x-coordinate, y-coordinate, or z-coordinate with another. Use as small a space as possible.", (groups, initialBlocks) =>
      {
        if (groups.Count != 4) {
          return false;
        }

        HashSet<Vector3Int> group = Combine(groups);

        if (group.Count != 4) {
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

        Vector3Int bounds = max - min;
        if (bounds.x != 3 || bounds.y != 3 || bounds.z != 3) {
          return false;
        }

        HashSet<int>[] buckets = {
          new HashSet<int>(),
          new HashSet<int>(),
          new HashSet<int>()
        };

        foreach (Vector3Int p in group) {
          buckets[0].Add(p.x);
          buckets[1].Add(p.y);
          buckets[2].Add(p.z);
        }

        return buckets[0].Count == 4 && buckets[1].Count == 4 && buckets[2].Count == 4;
      });



  public static readonly Challenge Loop3 =
    new Challenge("Create a structure using 16 blocks, such that every crate touches exactly 2 others, and crates appear in runs of 3.", (groups, initialBlocks) =>
      {
        if (groups.Count != 1) {
          return false;
        }

        HashSet<Vector3Int> group = groups[0];

        if (group.Count != 16) {
          return false;
        }

        if (group.Any(p => CountNeighbors(group, p) != 2)) {
          return false;
        }

        foreach (Vector3Int p in group) {
          Vector3Int left = p;
          while (group.Contains(new Vector3Int (left.x - 1, left.y, left.z))) {
            left = new Vector3Int (left.x - 1, left.y, left.z);
          }

          Vector3Int right = p;
          while (group.Contains(new Vector3Int (right.x + 1, right.y, right.z))) {
            right = new Vector3Int (right.x + 1, right.y, right.z);
          }

          Vector3Int down = p;
          while (group.Contains(new Vector3Int (down.x, down.y - 1, down.z))) {
            down = new Vector3Int (down.x, down.y - 1, down.z);
          }

          Vector3Int up = p;
          while (group.Contains(new Vector3Int (up.x, up.y + 1, up.z))) {
            up = new Vector3Int (up.x, up.y + 1, up.z);
          }

          Vector3Int backward = p;
          while (group.Contains(new Vector3Int (backward.x, backward.y, backward.z - 1))) {
            backward = new Vector3Int (backward.x, backward.y, backward.z - 1);
          }

          Vector3Int forward = p;
          while (group.Contains(new Vector3Int (forward.x, forward.y, forward.z + 1))) {
            forward = new Vector3Int (forward.x, forward.y, forward.z + 1);
          }

          int width = right.x - left.x + 1;
          int height = up.y - down.y + 1;
          int depth = forward.z - backward.z + 1;

          if (!(width == 1 || width == 3)) {
            return false;
          }

          if (!(depth == 1 || depth == 3)) {
            return false;
          }

          if (!(height == 1 || height == 3)) {
            return false;
          }
        }

        return true;
      });

  public static readonly Challenge MonotonicTower = new Challenge("Create a structure with 5 levels, with each level having more crates than the level below.", (groups, initialBlocks) => {
    if (groups.Count != 1) {
      return false;
    }

    HashSet<Vector3Int> group = Combine(groups);

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

    if (max.y - min.y + 1 != 5) {
      return false;
    }

    int belowCount = group.Count(p => p.y == min.y);
    for (int y = min.y + 1; y <= max.y; ++y) {
      int hereCount = group.Count(p => p.y == y);
      if (hereCount <= belowCount) {
        return false;
      }
      belowCount = hereCount;
    }

    return true;
  });

  public static readonly Challenge CupsideDown =
    new Challenge("Replace this box with the opposite of its inside.", (groups, initialBlocks) =>
      {
        if (groups.Count != 1) {
          return false;
        }

        HashSet<Vector3Int> group = Combine(groups);

        if (group.Count != 26){
          return false;
        }

        return Has(group,
          new Vector3Int(Constants.dimensions.x / 2 - 1, 0, Constants.dimensions.z / 2 - 1),
          new Vector3Int(Constants.dimensions.x / 2 - 0, 0, Constants.dimensions.z / 2 - 1),
          new Vector3Int(Constants.dimensions.x / 2 + 1, 0, Constants.dimensions.z / 2 - 1),

          new Vector3Int(Constants.dimensions.x / 2 - 1, 0, Constants.dimensions.z / 2 - 0),
          new Vector3Int(Constants.dimensions.x / 2 - 0, 0, Constants.dimensions.z / 2 - 0),
          new Vector3Int(Constants.dimensions.x / 2 + 1, 0, Constants.dimensions.z / 2 - 0),

          new Vector3Int(Constants.dimensions.x / 2 - 1, 0, Constants.dimensions.z / 2 + 1),
          new Vector3Int(Constants.dimensions.x / 2 - 0, 0, Constants.dimensions.z / 2 + 1),
          new Vector3Int(Constants.dimensions.x / 2 + 1, 0, Constants.dimensions.z / 2 + 1),

          new Vector3Int(Constants.dimensions.x / 2 - 1, 2, Constants.dimensions.z / 2 - 1),
          new Vector3Int(Constants.dimensions.x / 2 - 0, 2, Constants.dimensions.z / 2 - 1),
          new Vector3Int(Constants.dimensions.x / 2 + 1, 2, Constants.dimensions.z / 2 - 1),

          new Vector3Int(Constants.dimensions.x / 2 - 1, 2, Constants.dimensions.z / 2 - 0),
          new Vector3Int(Constants.dimensions.x / 2 - 0, 2, Constants.dimensions.z / 2 - 0),
          new Vector3Int(Constants.dimensions.x / 2 + 1, 2, Constants.dimensions.z / 2 - 0),

          new Vector3Int(Constants.dimensions.x / 2 - 1, 2, Constants.dimensions.z / 2 + 1),
          new Vector3Int(Constants.dimensions.x / 2 - 0, 2, Constants.dimensions.z / 2 + 1),
          new Vector3Int(Constants.dimensions.x / 2 + 1, 2, Constants.dimensions.z / 2 + 1),

          new Vector3Int(Constants.dimensions.x / 2 - 1, 1, Constants.dimensions.z / 2 - 1),
          new Vector3Int(Constants.dimensions.x / 2 - 0, 1, Constants.dimensions.z / 2 - 1),
          new Vector3Int(Constants.dimensions.x / 2 + 1, 1, Constants.dimensions.z / 2 - 1),

          new Vector3Int(Constants.dimensions.x / 2 - 1, 1, Constants.dimensions.z / 2 - 0),
          new Vector3Int(Constants.dimensions.x / 2 + 1, 1, Constants.dimensions.z / 2 - 0),

          new Vector3Int(Constants.dimensions.x / 2 - 1, 1, Constants.dimensions.z / 2 + 1),
          new Vector3Int(Constants.dimensions.x / 2 - 0, 1, Constants.dimensions.z / 2 + 1),
          new Vector3Int(Constants.dimensions.x / 2 + 1, 1, Constants.dimensions.z / 2 + 1)

        );
      },
      new Vector3Int(Constants.dimensions.x / 2, 1, Constants.dimensions.z / 2),

      // left
      new Vector3Int(Constants.dimensions.x / 2 - 2, 0, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 1, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 2, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 3, Constants.dimensions.z / 2 - 2),

      new Vector3Int(Constants.dimensions.x / 2 - 2, 0, Constants.dimensions.z / 2 - 1),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 1, Constants.dimensions.z / 2 - 1),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 2, Constants.dimensions.z / 2 - 1),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 3, Constants.dimensions.z / 2 - 1),

      new Vector3Int(Constants.dimensions.x / 2 - 2, 0, Constants.dimensions.z / 2 - 0),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 1, Constants.dimensions.z / 2 - 0),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 2, Constants.dimensions.z / 2 - 0),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 3, Constants.dimensions.z / 2 - 0),

      new Vector3Int(Constants.dimensions.x / 2 - 2, 0, Constants.dimensions.z / 2 + 1),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 1, Constants.dimensions.z / 2 + 1),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 2, Constants.dimensions.z / 2 + 1),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 3, Constants.dimensions.z / 2 + 1),

      new Vector3Int(Constants.dimensions.x / 2 - 2, 0, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 1, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 2, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 - 2, 3, Constants.dimensions.z / 2 + 2),

      // right
      new Vector3Int(Constants.dimensions.x / 2 + 2, 0, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 1, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 2, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 3, Constants.dimensions.z / 2 - 2),

      new Vector3Int(Constants.dimensions.x / 2 + 2, 0, Constants.dimensions.z / 2 - 1),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 1, Constants.dimensions.z / 2 - 1),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 2, Constants.dimensions.z / 2 - 1),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 3, Constants.dimensions.z / 2 - 1),

      new Vector3Int(Constants.dimensions.x / 2 + 2, 0, Constants.dimensions.z / 2 - 0),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 1, Constants.dimensions.z / 2 - 0),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 2, Constants.dimensions.z / 2 - 0),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 3, Constants.dimensions.z / 2 - 0),

      new Vector3Int(Constants.dimensions.x / 2 + 2, 0, Constants.dimensions.z / 2 + 1),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 1, Constants.dimensions.z / 2 + 1),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 2, Constants.dimensions.z / 2 + 1),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 3, Constants.dimensions.z / 2 + 1),

      new Vector3Int(Constants.dimensions.x / 2 + 2, 0, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 1, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 2, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 3, Constants.dimensions.z / 2 + 2),

      // front
      new Vector3Int(Constants.dimensions.x / 2 - 1, 0, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 - 1, 1, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 - 1, 2, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 - 1, 3, Constants.dimensions.z / 2 - 2),
      
      new Vector3Int(Constants.dimensions.x / 2 - 0, 0, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 - 0, 1, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 - 0, 2, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 - 0, 3, Constants.dimensions.z / 2 - 2),

      new Vector3Int(Constants.dimensions.x / 2 + 1, 0, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 1, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 2, Constants.dimensions.z / 2 - 2),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 3, Constants.dimensions.z / 2 - 2),

      // back
      new Vector3Int(Constants.dimensions.x / 2 - 1, 0, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 - 1, 1, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 - 1, 2, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 - 1, 3, Constants.dimensions.z / 2 + 2),
      
      new Vector3Int(Constants.dimensions.x / 2 - 0, 0, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 - 0, 1, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 - 0, 2, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 - 0, 3, Constants.dimensions.z / 2 + 2),

      new Vector3Int(Constants.dimensions.x / 2 + 1, 0, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 1, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 2, Constants.dimensions.z / 2 + 2),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 3, Constants.dimensions.z / 2 + 2),

      // top
      new Vector3Int(Constants.dimensions.x / 2 - 1, 3, Constants.dimensions.z / 2 - 1),
      new Vector3Int(Constants.dimensions.x / 2 - 0, 3, Constants.dimensions.z / 2 - 1),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 3, Constants.dimensions.z / 2 - 1),

      new Vector3Int(Constants.dimensions.x / 2 - 1, 3, Constants.dimensions.z / 2 - 0),
      new Vector3Int(Constants.dimensions.x / 2 - 0, 3, Constants.dimensions.z / 2 - 0),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 3, Constants.dimensions.z / 2 - 0),

      new Vector3Int(Constants.dimensions.x / 2 - 1, 3, Constants.dimensions.z / 2 + 1),
      new Vector3Int(Constants.dimensions.x / 2 - 0, 3, Constants.dimensions.z / 2 + 1),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 3, Constants.dimensions.z / 2 + 1)




      

    );

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

  public static readonly Challenge ConnectTowers = new Challenge("Turn this into one structure by placing only 3 blocks.", (groups, initialBlocks) => {
    if (groups.Count != 1) {
      return false;
    }

    HashSet<Vector3Int> group = groups[0];

    if (group.Count != 13) {
      return false;
    }

    return
      initialBlocks.All(p => group.Contains(p));
  },
      new Vector3Int(Constants.dimensions.x / 2 - 1, 0, Constants.dimensions.z / 2 - 1),
      //2
      new Vector3Int(Constants.dimensions.x / 2 + 1, 0, Constants.dimensions.z / 2 + 1),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 1, Constants.dimensions.z / 2 + 1),
      //3
      new Vector3Int(Constants.dimensions.x / 2 + 1, 0, Constants.dimensions.z / 2 - 1),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 1, Constants.dimensions.z / 2 - 1),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 2, Constants.dimensions.z / 2 - 1),
      //4
      new Vector3Int(Constants.dimensions.x / 2 - 1, 0, Constants.dimensions.z / 2 + 1),
      new Vector3Int(Constants.dimensions.x / 2 - 1, 1, Constants.dimensions.z / 2 + 1),
      new Vector3Int(Constants.dimensions.x / 2 - 1, 2, Constants.dimensions.z / 2 + 1),
      new Vector3Int(Constants.dimensions.x / 2 - 1, 3, Constants.dimensions.z / 2 + 1)
  );

  public static readonly Challenge FlipBoth =
    new Challenge("Flip this structure both horizontal and vertically.", (groups, initialBlocks) =>
      {
        if (groups.Count != 1) {
          return false;
        }

        HashSet<Vector3Int> group = groups[0];

        if (group.Count != initialBlocks.Length) {
          return false;
        }

        return Has(group,
          // Bottom row
          new Vector3Int(Constants.dimensions.x / 2 - 0, 4, Constants.dimensions.z / 2),
          new Vector3Int(Constants.dimensions.x / 2 - 1, 4, Constants.dimensions.z / 2),
          new Vector3Int(Constants.dimensions.x / 2 - 2, 4, Constants.dimensions.z / 2),
          new Vector3Int(Constants.dimensions.x / 2 - 3, 4, Constants.dimensions.z / 2),

          new Vector3Int(Constants.dimensions.x / 2 - 0, 3, Constants.dimensions.z / 2),

          // Middle row
          new Vector3Int(Constants.dimensions.x / 2 - 0, 2, Constants.dimensions.z / 2),
          new Vector3Int(Constants.dimensions.x / 2 - 1, 2, Constants.dimensions.z / 2),
          new Vector3Int(Constants.dimensions.x / 2 - 2, 2, Constants.dimensions.z / 2),

          new Vector3Int(Constants.dimensions.x / 2 - 0, 1, Constants.dimensions.z / 2),

          // Top row
          new Vector3Int(Constants.dimensions.x / 2 + 0, 0, Constants.dimensions.z / 2),
          new Vector3Int(Constants.dimensions.x / 2 + 1, 0, Constants.dimensions.z / 2)
        );
      },

      // Bottom row
      new Vector3Int(Constants.dimensions.x / 2 + 0, 0, Constants.dimensions.z / 2),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 0, Constants.dimensions.z / 2),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 0, Constants.dimensions.z / 2),
      new Vector3Int(Constants.dimensions.x / 2 + 3, 0, Constants.dimensions.z / 2),

      new Vector3Int(Constants.dimensions.x / 2 + 0, 1, Constants.dimensions.z / 2),

      // Middle row
      new Vector3Int(Constants.dimensions.x / 2 + 0, 2, Constants.dimensions.z / 2),
      new Vector3Int(Constants.dimensions.x / 2 + 1, 2, Constants.dimensions.z / 2),
      new Vector3Int(Constants.dimensions.x / 2 + 2, 2, Constants.dimensions.z / 2),

      new Vector3Int(Constants.dimensions.x / 2 + 0, 3, Constants.dimensions.z / 2),

      // Top row
      new Vector3Int(Constants.dimensions.x / 2 + 0, 4, Constants.dimensions.z / 2),
      new Vector3Int(Constants.dimensions.x / 2 - 1, 4, Constants.dimensions.z / 2)
    );

  public static readonly Challenge VolumeUnderArea =
    new Challenge("Create a solid rectangular prism whose volume-to-surface-area ratio is less than 1. Use more than 8 crates.", (groups, initialBlocks) =>
      {
        if (groups.Count != 1) {
          return false;
        }

        HashSet<Vector3Int> group = groups[0];

        if (group.Count <= 8) {
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

        Vector3Int dims = max - min + new Vector3Int(1, 1, 1);
        Debug.Log(dims);
        Debug.Log(group.Count);
        if (group.Count != dims.y * dims.x * dims.z) {
          return false;
        }

        int volume = dims.x * dims.y * dims.z;
        int area = 2 * (dims.x * dims.y + dims.y * dims.z + dims.x * dims.z);
        Debug.Log(volume);
        Debug.Log(area);

        return volume < area;
      });

  public static readonly Challenge VolumeOverArea =
    new Challenge("Create a solid rectangular prism whose volume-to-surface-area ratio exceeds 1. Use more than 8 crates.", (groups, initialBlocks) =>
    {
      if (groups.Count != 1) {
        return false;
      }

      HashSet<Vector3Int> group = groups[0];

      if (group.Count <= 8) {
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

      Vector3Int dims = max - min + new Vector3Int(1, 1, 1);
      if (group.Count != dims.y * dims.x * dims.z) {
        return false;
      }

      int volume = dims.x * dims.y * dims.z;
      int area = 2 * (dims.x * dims.y + dims.y * dims.z + dims.x * dims.z);

      return volume > area;
    });

  public static readonly Challenge VolumeEqualsArea =
    new Challenge("Create a solid rectangular prism whose volume-to-surface-area ratio is 1. Use more than 8 crates.", (groups, initialBlocks) => {
      if (groups.Count != 1) {
        return false;
      }

      HashSet<Vector3Int> group = groups[0];

      if (group.Count <= 8) {
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

      Vector3Int dims = max - min + new Vector3Int(1, 1, 1);
      if (group.Count != dims.y * dims.x * dims.z) {
        return false;
      }

      int volume = dims.x * dims.y * dims.z;
      int area = 2 * (dims.x * dims.y + dims.y * dims.z + dims.x * dims.z);

      return volume == area;
    });

  public static readonly Challenge Factor20 =
    new Challenge("Make a line of crates for each of the factors of 20, each being that number of crates long.", (groups, initialBlocks) => {
      HashSet<int> factors = new HashSet<int> {1, 2, 4, 5, 10, 20};
      HashSet<int> sizes = new HashSet<int> (groups.Select (group => group.Count));

      // Debug.Log(string.Join(", ", factors));
      // Debug.Log(string.Join(", ", sizes));

      if (!sizes.SetEquals(factors)) {
        return false;
      }

      foreach (HashSet<Vector3Int> group in groups) {
        if (group.Count == 1) {
          continue;
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
        
        Vector3Int diff = max - min;
        Vector3Int delta = Vector3Int.zero;
        if (diff.x > 0) {
          delta = Vector3Int.right;        
        } else if (diff.y > 0) {
          delta = Vector3Int.up;
        } else if (diff.z > 0) {
          delta = new Vector3Int(0, 0, 1);
        }

        // Debug.Log(d);

        Vector3Int q = min; 
        for (int i = 0; i < group.Count; ++i) {
          // Debug.LogFormat("checking for {0}: {1}", i, q);
          if (!group.Contains(q)) {
            return false;
          }
          q += delta;
        }
        // Debug.Log("----- ok -----");
      }

      return true;
    });

    public static readonly Challenge AddOpposite =
      new Challenge("Interlock this structure with its own replica, flipped upside down and turned to be perpendicular.", (groups, initialBlocks) => {
        if (groups.Count != 1) {
          return false;
        }

        HashSet<Vector3Int> group = groups[0];

        if (group.Count != 16) {
          return false;
        }
        HashSet<Vector3Int> newBlocks = new HashSet <Vector3Int> {
          new Vector3Int(Constants.dimensions.x / 2, 2, Constants.dimensions.z / 2),
          new Vector3Int(Constants.dimensions.x / 2, 3, Constants.dimensions.z / 2),
          new Vector3Int(Constants.dimensions.x / 2, 1, Constants.dimensions.z / 2 + 1),
          new Vector3Int(Constants.dimensions.x / 2, 2, Constants.dimensions.z / 2 + 1),
          new Vector3Int(Constants.dimensions.x / 2, 3, Constants.dimensions.z / 2 + 1),
          new Vector3Int(Constants.dimensions.x / 2, 1, Constants.dimensions.z / 2 - 1),
          new Vector3Int(Constants.dimensions.x / 2, 2, Constants.dimensions.z / 2 - 1),
          new Vector3Int(Constants.dimensions.x / 2, 3, Constants.dimensions.z / 2 - 1)
        };

        return
          initialBlocks.All(p => group.Contains(p)) &&
          newBlocks.All(p => group.Contains(p));
      },
        new Vector3Int(Constants.dimensions.x / 2, 0, Constants.dimensions.z / 2),
        new Vector3Int(Constants.dimensions.x / 2, 1, Constants.dimensions.z / 2),
        new Vector3Int(Constants.dimensions.x / 2 - 1, 0, Constants.dimensions.z / 2),
        new Vector3Int(Constants.dimensions.x / 2 - 1, 1, Constants.dimensions.z / 2),
        new Vector3Int(Constants.dimensions.x / 2 - 1, 2, Constants.dimensions.z / 2),
        new Vector3Int(Constants.dimensions.x / 2 + 1, 0, Constants.dimensions.z / 2),
        new Vector3Int(Constants.dimensions.x / 2 + 1, 1, Constants.dimensions.z / 2),
        new Vector3Int(Constants.dimensions.x / 2 + 1, 2, Constants.dimensions.z / 2)
    );
}
