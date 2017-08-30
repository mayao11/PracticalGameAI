using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// Pos代替Vector2，代表位置。整数
public class Pos
{
    public int x = 0;
    public int y = 0;

    public Pos()
    {

    }

    public Pos(Pos p)
    {
        x = p.x;
        y = p.y;
    }

    public Pos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static float AStarDistance(Pos p1, Pos p2)
    {
        float d1 = Mathf.Abs(p1.x - p2.x);
        float d2 = Mathf.Abs(p1.y - p2.y);
        return d1 + d2;
    }

    public bool Equals(Pos p)
    {
        return x == p.x && y == p.y;
    }
}

public class AScore
{
    // G是从起点出发的步数
    public float G = 0;
    // H是估算的离终点距离
    public float H = 0;

    public bool closed = false;

    public Pos parent = null;

    public AScore(float g, float h)
    {
        G = g;
        H = h;
        closed = false;
    }

    public float F
    {
        get { return G + H; }
    }

    public int CompareTo(AScore a2)
    {
        if (F == a2.F)
        {
            return 0;
        }
        if (F > a2.F)
        {
            return 1;
        }
        return -1;
    }

    public bool Equals(AScore a)
    {
        if (a.F == F)
        {
            return true;
        }
        return false;
    }

}

public class GameMap : MonoBehaviour {

    int W = 30;
    int H = 20;

    int[,] map;
    public GameObject prefab_wall;
    public GameObject prefab_start;
    public GameObject prefab_end;
    public GameObject prefab_path;
    public GameObject prefab_way;

    public enum SearchWay
    {
        BFS,
        DFS,
        AStar,
        Link,
    }
    public SearchWay searchWay = SearchWay.BFS;

    GameObject pathParent;

    const int START = 8;
    const int END = 9;
    const int WALL = 1;

    enum GameState
    {
        SetBeginPoint,
        SetEndPoint,
        StartCalculation,
        Calculation,
        ShowPath,
        Finish,
    }


    GameState gameState = GameState.SetBeginPoint;

    public void ReadMapFile()
    {
        string path = Application.dataPath + "//" + "map.txt";
        if (!File.Exists(path))
        {
            return;
        }

        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        StreamReader read = new StreamReader(fs, Encoding.Default);

        string strReadline = "";
        int y = 0;

        // 跳过第一行
        read.ReadLine();
        strReadline = read.ReadLine();

        while (strReadline!=null && y<H)
        {
            for (int x=0; x<W && x<strReadline.Length; ++x)
            {
                int t;
                switch(strReadline[x])
                {
                    case '1':
                        t = 1;
                        break;
                    case '8':
                        t = 8;
                        break;
                    case '9':

                        t = 9;
                        break;
                    default:
                        t = 0;
                        break;
                }
//                Debug.Log("x, y"+ x +" " + y);
                map[y, x] = t;
            }
            y += 1;
            strReadline = read.ReadLine();
        }

        read.Dispose();//文件流释放  
        fs.Close();
    }

    IEnumerator InitMap()
    {
        var walls = new GameObject();
        walls.name = "walls";

        for (int i = 0; i < H; i++)

        {
            for (int j = 0; j < W; j++)
            {
                if (map[i,j] == WALL)
                {
                    var go = Instantiate(prefab_wall, new Vector3(j*1, 0.5f, i*1), Quaternion.identity);
                    go.transform.parent = walls.transform;
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }

    void InitMap0()
    {
        var walls = new GameObject();
        walls.name = "walls";

        for (int i = 0; i < H; i++)
        {
            for (int j = 0; j < W; j++)
            {
                if (map[i, j] == WALL)
                {
                    var go = Instantiate(prefab_wall, new Vector3(j * 1, 0.5f, i * 1), Quaternion.identity, walls.transform);
                }
            }
        }
    }

    // Use this for initialization
    void Start () {
        pathParent = GameObject.Find("PathParent");
        map = new int[H, W];

        ReadMapFile();

        // StartCoroutine(InitMap());
        InitMap0();
    }

    Pos startPos;
    Pos endPos;

    bool SetPoint(int n)
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // We need to actually hit an object
            RaycastHit hitt = new RaycastHit();
            Physics.Raycast(ray, out hitt, 100);
            if (hitt.transform != null && hitt.transform.name == "Ground")
            {
                int x = (int)hitt.point.x;
                int y = (int)hitt.point.z;

                map[y, x] = n;
                if (n == START)
                {
                    startPos = new Pos( x, y );
                }
                else if (n == END)
                {
                    endPos = new Pos( x, y );
                }
                return true;
            }
        }
        return false;
    }

    delegate bool Func(Pos cur, int ox, int oy);

    int cur_depth = 0;
    // Breadth First Search, 广度优先搜索
    short[,] bfs_search = null;
    IEnumerator BFS()
    {
        bfs_search = new short[map.GetLength(0),map.GetLength(1)];

        // map_search和map一样大小，每个元素的值：32767(short.MaxValue)代表不可通过或者未探索，其他值代表移动的步数

        for (int i=0; i<H; ++i)
        {
            for (int j=0; j<W; ++j)
            {
                bfs_search[i, j] = short.MaxValue;
            }
        }

        Queue<Pos> queue = new Queue<Pos>();

        Func func = (Pos cur, int ox, int oy) =>
        {
            if (map[cur.y + oy, cur.x + ox] == END)
            {
                bfs_search[cur.y + oy, cur.x+ox] = (short)(bfs_search[cur.y, cur.x] + 1);
                Debug.Log("寻路完成");
                return true;
            }
            if (map[cur.y + oy, cur.x + ox] == 0)
            {
                if (bfs_search[cur.y + oy, cur.x + ox] > bfs_search[cur.y, cur.x] + 1)
                {
                    bfs_search[cur.y + oy, cur.x + ox] = (short)(bfs_search[cur.y, cur.x] + 1);
                    queue.Enqueue(new Pos(cur.x+ox, cur.y+oy));
                }
            }
            return false;
        };

        bfs_search[startPos.y, startPos.x] = 0;
        queue.Enqueue(startPos);

        while (queue.Count > 0)
        {
            Pos cur = queue.Dequeue();
            // 上
            if (cur.y > 0 )
            {
                if (func(cur, 0, -1)) { break; }
            }
            // 下
            if (cur.y<H-1)
            {
                if (func(cur, 0, 1)) { break; }
            }
            // 左
            if (cur.x>0)
            {
                if (func(cur, -1, 0)) { break; }
            }
            // 右
            if (cur.x<W-1)
            {
                if (func(cur, 1, 0)) { break; }
            }

            if (bfs_search[cur.y, cur.x] > cur_depth)
            {
                cur_depth = bfs_search[cur.y, cur.x];
                RefreshPath(bfs_search);
                yield return new WaitForSeconds(0.1f);
            }
        }

        gameState = GameState.ShowPath;
        yield return null;
    }

    IEnumerator DFS()
    {
        bfs_search = new short[map.GetLength(0), map.GetLength(1)];

        // map_search和map一样大小，每个元素的值：32767(short.MaxValue)代表不可通过或者未探索，其他值代表移动的步数

        for (int i = 0; i < H; ++i)
        {
            for (int j = 0; j < W; ++j)
            {
                bfs_search[i, j] = short.MaxValue;
            }
        }

        List<Pos> queue = new List<Pos>();

        Func func = (Pos cur, int ox, int oy) =>
        {
            if (map[cur.y + oy, cur.x + ox] == END)
            {
                bfs_search[cur.y + oy, cur.x + ox] = (short)(bfs_search[cur.y, cur.x] + 1);
                Debug.Log("寻路完成");
                return true;
            }
            if (map[cur.y + oy, cur.x + ox] == 0)
            {
                if (bfs_search[cur.y + oy, cur.x + ox] > bfs_search[cur.y, cur.x] + 1)
                {
                    bfs_search[cur.y + oy, cur.x + ox] = (short)(bfs_search[cur.y, cur.x] + 1);
                    queue.Add(new Pos(cur.x + ox, cur.y + oy));
                }
            }
            return false;
        };

        bfs_search[startPos.y, startPos.x] = 0;
        queue.Add(startPos);

        while (queue.Count > 0)
        {
            int min_i = 0;
            Pos cur = queue[min_i];
            float min_dist = Pos.AStarDistance(cur, endPos);
            for(int i=0; i<queue.Count; ++i)
            {
                float d = Pos.AStarDistance(queue[i], endPos);
                if (d < min_dist)
                {
                    min_i = i;
                    cur = queue[i];
                    min_dist = d;
                }
            }

            queue.RemoveAt(min_i);

            int last_count = queue.Count;
            // 上
            bool flag = false;
            if (cur.y > 0)
            {
                if (func(cur, 0, -1)) { break; }
            }
            // 下
            if (cur.y < H - 1)
            {
                if (func(cur, 0, 1)) { break; }
            }
            // 左
            if (cur.x > 0)
            {
                if (func(cur, -1, 0)) { break; }
            }
            // 右
            if (cur.x < W - 1)
            {
                if (func(cur, 1, 0)) { break; }
            }

            RefreshPath(bfs_search);
            yield return new WaitForSeconds(0.05f);
        }

        gameState = GameState.ShowPath;
        yield return null;
    }

    AScore[,] astar_search;
    IEnumerator AStar()
    {
        astar_search = new AScore[map.GetLength(0), map.GetLength(1)];

        List<Pos> list = new List<Pos>();

        astar_search[startPos.y, startPos.x] = new AScore(0,0);
        list.Add(startPos);

        // 每一个点的处理函数
        Func func = (Pos cur, int ox, int oy) =>
        {
            var o_score = astar_search[cur.y + oy, cur.x + ox];
            if (o_score!=null && o_score.closed)
            {
                return false;
            }
            var cur_score = astar_search[cur.y, cur.x];
            Pos o_pos = new Pos(cur.x + ox, cur.y + oy);
            if (map[cur.y + oy, cur.x + ox] == END)
            {
                var a = new AScore(cur_score.G + 1, 0);
                a.parent = cur;
                astar_search[cur.y + oy, cur.x + ox] = a;
                Debug.Log("寻路完成");
                return true;
            }
            if (map[cur.y + oy, cur.x + ox] == 0)
            {
                if (o_score==null)
                {
                    var a = new AScore(cur_score.G + 1, Pos.AStarDistance(o_pos, endPos));
                    a.parent = cur;
                    astar_search[cur.y + oy, cur.x + ox] = a;
                    list.Add(o_pos);
                }
                else if (o_score.G > cur_score.G+1)
                {
                    o_score.G = cur_score.G + 1;
                    o_score.parent = cur;
                    if (!list.Contains(o_pos))
                    {
                        list.Add(o_pos);
                    }
                }
            }
            return false;
        };


        while (list.Count > 0)
        {
            list.Sort((Pos p1, Pos p2) =>
            {
                AScore a1 = astar_search[p1.y, p1.x];
                AScore a2 = astar_search[p2.y, p2.x];
                //return a1.H.CompareTo(a2.H);
                return a1.CompareTo(a2);
            });
            Pos cur = list[0];
            list.RemoveAt(0);
            astar_search[cur.y, cur.x].closed = true;

            // 上
            if (cur.y > 0)
            {
                if (func(cur, 0, -1)) { break; }
            }
            // 下
            if (cur.y < H - 1)
            {
                if (func(cur, 0, 1)) { break; }
            }
            // 左
            if (cur.x > 0)
            {
                if (func(cur, -1, 0)) { break; }
            }
            // 右
            if (cur.x < W - 1)
            {
                if (func(cur, 1, 0)) { break; }
            }

            short[,] temp_map = new short[map.GetLength(0), map.GetLength(1)];
            for (int i=0; i<H; ++i)
            {
                for (int j=0; j<W; ++j)
                {
                    temp_map[i, j] = short.MaxValue;
                    //if (map_search[i,j] != null && map_search[i,j].closed)
                    if (astar_search[i,j] != null)
                    {
                        temp_map[i, j] = (short)astar_search[i, j].F;
                    }
                }
            }
            RefreshPath(temp_map);
            yield return 0;
        }
        gameState = GameState.ShowPath;
        yield return null;
    }

    IEnumerator BFSShowPath()
    {
        Pos p = endPos;
        while (true)
        {
            int cur_step = bfs_search[p.y, p.x];
            if (cur_step == 0)
            {
                break;
            }
            if (p.y>0 && bfs_search[p.y-1, p.x] == cur_step - 1)
            {
                p.y -= 1;
            }
            else if (p.y<bfs_search.GetLength(0) && bfs_search[p.y+1, p.x] == cur_step - 1)
            {
                p.y += 1;
            }
            else if (p.x>0 && bfs_search[p.y, p.x-1] == cur_step - 1)
            {
                p.x -= 1;
            }
            else if (p.x>0 && bfs_search[p.y, p.x+1] == cur_step - 1)
            {
                p.x += 1;
            }
            if (!p.Equals(startPos))
            {
                var go = Instantiate(prefab_way, new Vector3(p.x * 1, 0.5f, p.y * 1), Quaternion.identity, pathParent.transform);
                yield return new WaitForSeconds(0.2f);
            }
        }
        yield return null;
    }

    void AStarShowPath()
    {
        Pos pos = endPos;
        while (!pos.Equals(startPos))
        {
            var go = Instantiate(prefab_end, new Vector3(pos.x * 1, 0.5f, pos.y * 1), Quaternion.identity, pathParent.transform);
            pos = astar_search[pos.y, pos.x].parent;
        }
    }

    class LinkData
    {
        public bool verticle;
        public byte count;

        public LinkData(bool verticle, byte count)
        {
            this.verticle = verticle;
            this.count = count;
        }
    }

    class LinkLine
    {
        public int start;
        public int end;
        public bool verticle;

        public int other;
    }

    LinkLine HLine(Pos p)
    {
        int left = p.x;
        int right = p.x;
        while (left>0 && map[p.y, left-1]!=1)
        {
            left -= 1;
        }
        while (right<W-1 && map[p.y, right+1]!=1)
        {
            right += 1;
        }
        LinkLine line = new LinkLine();
        line.verticle = false;
        line.start = left;
        line.end = right;
        line.other = p.y;
        return line;
    }

    LinkLine VLine(Pos p)
    {
        int up = p.y;
        int down = p.y;
        while (up>0 && map[up-1, p.x]!=1)
        {
            up -= 1;
        }
        while (down<H-1 && map[down+1, p.x]!=1)
        {
            down += 1;
        }
        LinkLine line = new LinkLine();
        line.verticle = true;
        line.start = up;
        line.end = down;
        line.other = p.x;
        return line;
    }

    bool LineCross(LinkLine l1, LinkLine l2)
    {
        if (l1.verticle == l2.verticle)
        {
            // 平行线
            if (l1.other == l2.other)
            {
                if (l1.end < l2.start || l2.end < l1.start)
                {
                    return false;
                }
                return true;
            }
        }
        else
        {
            if ((l1.start <= l2.other && l1.end >= l2.other) && (l2.start <= l1.other && l2.end >= l1.other))
            {
                return true;
            }
        }
        return false;
    }

    // 寻找另一条线让两直线和它相交
    int LineCrossOther(LinkLine l1, LinkLine l2)
    {
        for (int i=Mathf.Max(l1.start,l2.start); i<=Mathf.Min(l1.end, l2.end); ++i)
        {
            bool ok = true;
            for (int j=Mathf.Min(l1.other, l2.other); j<=Mathf.Max(l1.other, l2.other); ++j)
            {
                if (l1.verticle)
                {
                    if (map[i, j] == 1)
                    {
                        ok = false;
                        break;
                    }
                }
                else
                {
                    if (map[j, i] == 1)
                    {
                        ok = false;
                        break;
                    }
                }
            }
            if (ok)
            {
                return i;
            }
        }
        return -1;
    }

    List<LinkLine> linkResult;
    void LinkSearch()
    {
        linkResult = new List<LinkLine>();

        LinkLine line_h1 = HLine(startPos);
        LinkLine line_v1 = VLine(startPos);
        LinkLine line_h2 = HLine(endPos);
        LinkLine line_v2 = VLine(endPos);

        if (LineCross(line_h1, line_h2))
        {
            // 0折
            Debug.Log("========h" + 0);
            var line = new LinkLine();
            line.verticle = false;
            line.other = startPos.y;
            line.start = Mathf.Min(startPos.x, endPos.x);
            line.end = Mathf.Max(startPos.x, endPos.x);
            linkResult.Add(line);
        }
        else if (LineCross(line_v1, line_v2))
        {
            // 0折
            Debug.Log("========v" + 0);
            var line = new LinkLine();
            line.verticle = true;
            line.other = startPos.x;
            line.start = Mathf.Min(startPos.y, endPos.y);
            line.end = Mathf.Max(startPos.y, endPos.y);
            linkResult.Add(line);
        }
        else if (LineCross(line_h1, line_v2))
        {
            // 1折
            Debug.Log("========h" + 1);
            var line1 = new LinkLine();
            line1.verticle = false;
            line1.other = startPos.y;
            line1.start = Mathf.Min(startPos.x, endPos.x);
            line1.end = Mathf.Max(startPos.x, endPos.x);

            var line2 = new LinkLine();
            line2.verticle = true;
            line2.other = endPos.x;
            line2.start = Mathf.Min(startPos.y, endPos.y);
            line2.end = Mathf.Max(startPos.y, endPos.y);

            linkResult.Add(line1);
            linkResult.Add(line2);
        }
        else if (LineCross(line_v1, line_h2))
        {
            // 1折
            Debug.Log("========v" + 1);
            var line1 = new LinkLine();
            line1.verticle = true;
            line1.other = startPos.x;
            line1.start = Mathf.Min(startPos.y, endPos.y);
            line1.end = Mathf.Max(startPos.y, endPos.y);

            var line2 = new LinkLine();
            line2.verticle = false;
            line2.other = endPos.y;
            line2.start = Mathf.Min(startPos.x, endPos.x);
            line2.end = Mathf.Max(startPos.x, endPos.x);

            linkResult.Add(line1);
            linkResult.Add(line2);
        }
        else
        {
            // 2折以上
            int p = LineCrossOther(line_v1, line_v2);
            if (p != -1)
            {
                Debug.Log("========v2");
                var line1 = new LinkLine();
                line1.verticle = true;
                line1.other = startPos.x;
                line1.start = Mathf.Min(startPos.y,p);
                line1.end = Mathf.Max(startPos.y,p);

                var line2 = new LinkLine();
                line2.verticle = false;
                line2.other = p;
                line2.start = Mathf.Min(startPos.x, endPos.x);
                line2.end = Mathf.Max(startPos.x, endPos.x);

                var line3 = new LinkLine();
                line3.verticle = true;
                line3.other = endPos.x;
                line3.start = Mathf.Min(endPos.y, p);
                line3.end = Mathf.Max(endPos.y, p);

                linkResult.Add(line1);
                linkResult.Add(line2);
                linkResult.Add(line3);
            }
            else
            {
                p = LineCrossOther(line_h1, line_h2);
                if (p != -1)
                {
                    Debug.Log("========h2");
                    var line1 = new LinkLine();
                    line1.verticle = false;
                    line1.other = startPos.y;
                    line1.start = Mathf.Min(startPos.x, p);
                    line1.end = Mathf.Max(startPos.x, p);

                    var line2 = new LinkLine();
                    line2.verticle = true;
                    line2.other = p;
                    line2.start = Mathf.Min(startPos.y, endPos.y);
                    line2.end = Mathf.Max(startPos.y, endPos.y);

                    var line3 = new LinkLine();
                    line3.verticle = false;
                    line3.other = endPos.y;
                    line3.start = Mathf.Min(endPos.x, p);
                    line3.end = Mathf.Max( endPos.x, p);

                    linkResult.Add(line1);
                    linkResult.Add(line2);
                    linkResult.Add(line3);
                }
                else
                {
                    Debug.Log("========3!");
                    return;
                }
            }
        }

        return;
    }

    void DrawLinkLine(LinkLine line)
    {
        if (line.verticle)
        {
            for (int i=line.start; i<=line.end; ++i)
            {
                Instantiate(prefab_end, new Vector3(line.other, 1.0f, i), Quaternion.identity);
            }
        }
        else
        {
            for (int i=line.start; i<=line.end; ++i)
            {
                Instantiate(prefab_end, new Vector3(i, 1.0f, line.other), Quaternion.identity);
            }
        }
    }

    void ShowLinkPath()
    {
        foreach (var line in linkResult)
        {
            DrawLinkLine(line);
        }
    }


    // Update is called once per frame
    void Update () {
        switch(gameState)
        {
            case GameState.SetBeginPoint:
                if (SetPoint(START))
                {
                    Refresh();
                    gameState = GameState.SetEndPoint;
                }
                break;
            case GameState.SetEndPoint:
                if (SetPoint(END))
                {
                    Refresh();
                    gameState = GameState.StartCalculation;
                }
                break;
            case GameState.StartCalculation:
                if (searchWay == SearchWay.BFS)
                {
                    StartCoroutine(BFS());
                }
                else if (searchWay == SearchWay.DFS)
                {
                    StartCoroutine(DFS());
                }
                else if (searchWay == SearchWay.AStar)
                {
                    StartCoroutine(AStar());
                }
                else if (searchWay == SearchWay.Link)
                {
                    LinkSearch();
                }
                gameState = GameState.Calculation;
                break;
            case GameState.Calculation:
                if (searchWay == SearchWay.Link)
                {
                    gameState = GameState.ShowPath;
                }
                break;
            case GameState.ShowPath:
                if (searchWay == SearchWay.BFS)
                {
                    StartCoroutine(BFSShowPath());
                    gameState = GameState.Finish;
                }
                else if (searchWay == SearchWay.DFS)
                {
                }
                else if (searchWay == SearchWay.AStar)
                {
                    AStarShowPath();
                    gameState = GameState.Finish;
                }
                else if (searchWay == SearchWay.Link)
                {
                    ShowLinkPath();
                    gameState = GameState.Finish;
                }

                break;
            case GameState.Finish:
                break;
        }
	}

    void Refresh()
    {
        // 删除所有格子
        GameObject[] all_go = GameObject.FindGameObjectsWithTag("Path");
        foreach (var go in all_go)
        {
            Destroy(go);
        }

        // 创建起点和终点
        for (int i = 0; i < H; i++)
        {
            for (int j = 0; j < W; j++)
            {
                if (map[i, j] == START)
                {
                    Debug.Log("START "+ prefab_start);
                    var go = Instantiate(prefab_start, new Vector3(j * 1, 0.5f, i * 1), Quaternion.identity, pathParent.transform);
                    go.tag = "Path";
                }
                if (map[i, j] == END)
                {
                    var go = Instantiate(prefab_end, new Vector3(j * 1, 0.5f, i * 1), Quaternion.identity, pathParent.transform);
                    go.tag = "Path";
                }
            }
        }
    }

    void RefreshPath(short[,] temp_map)
    {
        Refresh();
        // 显示探索过的部分
        for (int i = 0; i < H; i++)
        {
            string line = "";
            for (int j = 0; j < W; j++)
            {
                line += temp_map[i, j] + " ";
                if (map[i,j]==0 && temp_map[i, j] != short.MaxValue)
                {
                    var go = Instantiate(prefab_path, new Vector3(j * 1, 0.1f, i * 1), Quaternion.identity, pathParent.transform);
                    go.tag = "Path";
                    if (searchWay == SearchWay.AStar)
                    {
                        PathData pathData = go.GetComponent<PathData>();
                        if (pathData != null)
                        {
                            pathData.G = astar_search[i, j].G;
                            pathData.H = astar_search[i, j].H;
                        }
                    }
                }
            }
        }
    }
}
