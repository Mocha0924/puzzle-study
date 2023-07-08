using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct FallDate
{
    public readonly int X { get; }
    public readonly int Y { get; }
    public readonly int Dest { get; }

    public FallDate(int x, int y, int dest)
    {
        X = x;
        Y = y;
        Dest = dest;
    }

}
public class BoadController : MonoBehaviour
{

    // Start is called before the first frame update
    public const int FALL_FLAME_PER_CELL = 5;
    public const int BOARD_WIDTH = 6;
    public const int BOARD_HEIGHT = 14;
    [SerializeField] GameObject prefabPuyo = default!;
    int[,] _board = new int[BOARD_HEIGHT,BOARD_WIDTH];
    GameObject[,] _Puyos = new GameObject[BOARD_HEIGHT,BOARD_WIDTH];
    List<FallDate> _falls = new ();
    int _fallFlames = 0;
    private void ClearAll()
    {
        for (int y = 0; y < BOARD_HEIGHT; y++)
        {
            for(int x = 0; x < BOARD_WIDTH; x++)
            {
                _board[y,x] = 0;
                if(_Puyos[y,x] != null)Destroy(_Puyos[y,x]);
                _Puyos[y,x] = null;
            }
        }
    }
    public void Start()
    {
        ClearAll();
       
    }
    public static bool IsValidated(Vector2Int pos)
    {
        return 0<=pos.x && pos.x < BOARD_WIDTH
            && 0<=pos.y && pos.y < BOARD_HEIGHT;
    }
    public bool CanSettle(Vector2Int pos)
    {
        if(!IsValidated(pos))return false;
        return 0 == _board[pos.y,pos.x];
    }
    public bool Settle(Vector2Int pos, int val)
    {
        if(!CanSettle(pos))return false;
        _board[pos.y,pos.x] = val;
        Debug.Assert(_Puyos[pos.y, pos.x] == null);
        Vector3 World_position = transform.position + new Vector3(pos.x, pos.y, 0.0f);
        _Puyos[pos.y, pos.x] = Instantiate(prefabPuyo, World_position, Quaternion.identity, transform);
        _Puyos[pos.y, pos.x].GetComponent<PuyoController>().SetPuyoType((PuyoType)val);
        return true;
    }
  
    public bool CheckFall()
    {
        _falls.Clear();
        _fallFlames = 0;
        int[] dsts = new int[BOARD_WIDTH];
        for(int i = 0; i < BOARD_WIDTH; i++) dsts[i] = 0;

        int max_check_line = BOARD_HEIGHT - 1;
        for(int y = 0; y < max_check_line; y++)
        {
            for(int x = 0; x < BOARD_WIDTH; x++)
            {
                if (_board[y, x] == 0) continue;

                int dst = dsts[x];
                dsts[x] = y + 1;

                if (y == 0) continue;

                if(_board[y - 1, x] != 0) continue;

                _falls.Add(new FallDate(x, y, dst));

                _board[dst, x] = _board[y, x];
                _board[y, x] = 0;
                _Puyos[dst, x] = _Puyos[y, x];
                _Puyos[y, x] = null;

                dsts[x] = dst + 1;
            }
        }
        return _falls.Count != 0;
    }
    public bool Fall()
    {
        _fallFlames++;

        float dy = _fallFlames / (float)FALL_FLAME_PER_CELL;
        int di = (int)dy;   

        for(int i = _falls.Count - 1; i >= 0; i--)
        {
            FallDate f = _falls[i];

            Vector3 pos = _Puyos[f.Dest, f.X].transform.localPosition;
            pos.y = f.Y - dy;

            if(f.Y <= f.Dest + di)
            {
                pos.y = f.Dest;
                _falls.RemoveAt(i);
            }
            _Puyos[f.Dest,f.X].transform.localPosition = pos;
        }
        return _falls.Count != 0;
    }
}
