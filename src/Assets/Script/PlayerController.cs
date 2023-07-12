using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    enum RotState
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,

        Invalid = -1,
    }
    const int FALL_COUNT_UNNIT = 120;
    const int FALL_COUNT_SPD = 10;
    const int FALL_COUNT_FAST_SPD = 20;
    const int GROUND_FLAMES = 50;

    int _fallCount = 0;
    int _groundFrame = GROUND_FLAMES;
    [SerializeField] PuyoController[] _puyoController = new PuyoController[2] { default!, default! };
    [SerializeField] BoadController boardController = default!;
    Vector2Int _position;
    RotState _rotate = RotState.Up;
    AnimationController _animationController = new AnimationController();
    Vector2Int _last_poaition;
    RotState _last_rotate = RotState.Up;
    const int TRANS_TIME = 3;
    const int ROT_TIME =   3;
    LogialInput _logicalInput = new();
    void SetTransition(Vector2Int pos, RotState rot, int time)
    {
        _last_poaition = _position;
        _last_rotate = _rotate;

        _position = pos;
        _rotate = rot;

        _animationController.Set(time);
    }
    void Start()
    {
        gameObject.SetActive(false);
    }
    static readonly Vector2Int[] rotate_tbl = new Vector2Int[] {
    Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
    public bool Spawn(PuyoType axis,PuyoType child)
    {
        Vector2Int position = new Vector2Int(2, 12);
        RotState rotate = RotState.Up;
        if (!CanMove(position, rotate)) return false;

        _position = _last_poaition = position;
        _rotate = _last_rotate = rotate;
        _animationController.Set(1);
        _fallCount = 1;
        _groundFrame = GROUND_FLAMES;

        _puyoController[0].SetPuyoType(axis);
        _puyoController[1].SetPuyoType(child);

        _puyoController[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoController[1].SetPos(new Vector3((float)posChild.x,(float)posChild.y,0.0f));

        gameObject.SetActive(true);

        return true;
    }
    public void setLogicalInput(LogialInput reference)
    {
        _logicalInput = reference;
    }
    private static Vector2Int CalcChildPuyoPos(Vector2Int pos, RotState rot)
    {
        return pos + rotate_tbl[(int)rot];
    }
    private bool CanMove(Vector2Int pos, RotState rot)
    {
        if (!boardController.CanSettle(pos)) return false;
        if (!boardController.CanSettle(CalcChildPuyoPos(pos, rot))) return false;

        return true;
    }
    private bool Translate(bool is_right)
    {
        Vector2Int pos = _position + (is_right ? Vector2Int.right : Vector2Int.left);
        if (!CanMove(pos, _rotate)) return false;


        SetTransition(pos, _rotate, TRANS_TIME);

        return true;
    }
    bool Rotate(bool is_right)
    {
        RotState rot = (RotState)(((int)_rotate + (is_right ? 1 : +3)) & 3);
        Vector2Int pos = _position;
        switch (rot)
        {
            case RotState.Down:
                if (!boardController.CanSettle(pos + Vector2Int.down) ||
                   !boardController.CanSettle(pos + new Vector2Int(is_right ? 1 : -1, -1)))
                {
                    pos += Vector2Int.up;
                }
                break;
            case RotState.Right:
                if (!boardController.CanSettle(pos + Vector2Int.right)) pos += Vector2Int.left;
                break;
            case RotState.Left:
                if (!boardController.CanSettle(pos + Vector2Int.left)) pos += Vector2Int.right;
                break;
            case RotState.Up:
                break;
            default:
                Debug.Assert(false);
                break;
        }
        if (!CanMove(pos, rot)) return false;
        SetTransition(pos, rot, ROT_TIME);
        return true;
    }
    void Settle()
    {
        bool is_set0 = boardController.Settle(_position,
            (int)_puyoController[0].GetPuyoType());
        Debug.Assert(is_set0);

        bool is_set1 = boardController.Settle(CalcChildPuyoPos(_position, _rotate),
            (int)_puyoController[1].GetPuyoType());
        Debug.Assert(is_set1);

        gameObject.SetActive(false);
    }
    void QuickDrop()
    {
        Vector2Int pos = _position;
        do
        {
            pos += Vector2Int.down;
        } while (CanMove(pos, _rotate));
        pos -= Vector2Int.down;
        _position = pos;
      
        Settle();
    }
   bool Fall(bool is_fall)
    {
        _fallCount -= is_fall ? FALL_COUNT_FAST_SPD:FALL_COUNT_SPD;
        while(_fallCount < 0)
        {
            if(!CanMove(_position + Vector2Int.down, _rotate))
            {
                _fallCount = 0;
                if (0 < --_groundFrame) return true;
                Settle();
                return false;
            }
            _position += Vector2Int.down;
            _last_poaition += Vector2Int.down;
            _fallCount += FALL_COUNT_UNNIT;
        }
        return true;
    }
    void Control()
    {
        if (!Fall(_logicalInput.IsRaw(LogialInput.Key.Down))) return;

        if (_animationController.Update()) return;

        if (_logicalInput.IsRepeat(LogialInput.Key.Right))
        {
            if (Translate(true)) return;
        }
        if (_logicalInput.IsRepeat(LogialInput.Key.Left))
        {
            if (Translate(false)) return;
        }
        if (_logicalInput.IsTrigger(LogialInput.Key.RotR))
        {
            if (Rotate(true)) return;
        }
        if (_logicalInput.IsTrigger(LogialInput.Key.RotL))
        {
            if (Rotate(false)) return;
        }
        if (_logicalInput.IsRelease(LogialInput.Key.QuickDrop))
        {
            QuickDrop();
        }
    }
    
   
   
    void FixedUpdate()
    {
     
      
            Control();

        Vector3 dy = Vector3.up * (float)_fallCount / (float)FALL_COUNT_UNNIT;
        float anim_rate = _animationController.GetNormalized();
        _puyoController[0].SetPos(dy+Interpolate(_position, RotState.Invalid, _last_poaition, RotState.Invalid, anim_rate));
        _puyoController[1].SetPos(dy+Interpolate(_position, _rotate, _last_poaition, _last_rotate, anim_rate));
    }
    static Vector3 Interpolate(Vector2Int pos, RotState rot, Vector2Int pos_last, RotState rot_last, float rate)
    {
        Vector3 p = Vector3.Lerp(
            new Vector3((float)pos.x, (float)pos.y, 0.0f),
            new Vector3((float)pos_last.x, (float)pos_last.y, 0.0f), rate);

        if (rot == RotState.Invalid) return p;
        float theta0 = 0.5f * Mathf.PI * (float)(int)rot;
        float theta1 = 0.5f * Mathf.PI * (float)(int)rot_last;
        float theta = theta1 - theta0;

        if (+Mathf.PI < theta) theta = theta - 2.0f * Mathf.PI;
        if (theta < -Mathf.PI) theta = theta + 2.0f * Mathf.PI;

        theta = theta0 + rate * theta;

        return p + new Vector3(Mathf.Sin(theta), Mathf.Cos(theta), 0.0f);
    }
}
