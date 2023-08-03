using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

interface IState
{
    public enum E_State
    {
        Control = 0,
        GameOver = 1,
        Falling = 2,
        Erasing = 3,

        MAX,

        UnChanged,

    }
    E_State Initialize(PlayerDirector parent);

    E_State Update(PlayerDirector parent);
}
[RequireComponent(typeof(BoadController))]
public class PlayerDirector : MonoBehaviour
{
   
    // Start is called before the first frame update
    [SerializeField] TextMeshProUGUI TextScore = default!;
    uint _score = 0;
    int _chainCount = -1;
    [SerializeField] GameObject player = default!;
    PlayerController _playerController = null;
    LogialInput _logicalInput = new();
    NextQueue _nextQueue = new();
    BoadController _boardController = default!;
    [SerializeField] PuyoPair[] nextPuyoPairs = { default!, default! };
    IState.E_State _current_state = IState.E_State.Falling;
    static readonly IState[] states = new IState[(int)IState.E_State.MAX]
     {
         new ControlState(),
         new GameoverState(),
         new FallingState(),
         new ErasingState(),
     };

    void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _boardController = GetComponent<BoadController>();
        _logicalInput.Clear();
        _playerController.setLogicalInput(_logicalInput);

        _nextQueue.Initialize();
        InitializeState();
        Spawn(_nextQueue.Update());
        UpdateNextsView();
    }
    void UpdateNextsView()
    {
        _nextQueue.Each((int idx, Vector2Int n) =>{
            nextPuyoPairs[idx++].SetPuyoType((PuyoType)n.x, (PuyoType)n.y);
        });
    }
    static readonly KeyCode[] key_code_tbl = new KeyCode[(int)LogialInput.Key.Max]
    {
        KeyCode.RightArrow,
        KeyCode.LeftArrow,
        KeyCode.X,
        KeyCode.Z,
        KeyCode.UpArrow,
        KeyCode.DownArrow
    };
    void UpdateInput()
    {
        LogialInput.Key inputDev = 0;
        for (int i = 0; i < (int)LogialInput.Key.Max; i++)
        {
            if (Input.GetKey(key_code_tbl[i]))
            {
                inputDev |= (LogialInput.Key)(1 << i);
            }
        }

        _logicalInput.Update(inputDev);

    }
    void FixedUpdate()
    {
        UpdateInput();

        UpdateState();

        AddScore(_playerController.popScore());
        AddScore(_boardController.popScore());

    }
    bool Spawn(Vector2Int next) => _playerController.Spawn((PuyoType)next[0], (PuyoType)next[1]);

    class ControlState : IState
    {
        public IState.E_State Initialize(PlayerDirector parent)
        {
            if(!parent.Spawn(parent._nextQueue.Update()))
                return IState.E_State.GameOver;

            parent.UpdateNextsView();
            return IState.E_State.UnChanged;
            
        }
        public IState.E_State Update(PlayerDirector parent)
        {
            return parent.player.activeSelf ?IState.E_State.UnChanged :IState.E_State.Falling;
        }
    }
    class GameoverState : IState
    {
        public IState.E_State Initialize(PlayerDirector parent)
        {
            SceneManager.LoadScene(0);
            return IState.E_State.UnChanged;
        }
        public IState.E_State Update(PlayerDirector parent)
        {
            return IState.E_State.UnChanged;
        }
    }
    class FallingState : IState
    {
        public IState.E_State Initialize(PlayerDirector parent)
        {
            return parent._boardController.CheckFall() ? IState.E_State.UnChanged : IState.E_State.Erasing;
        }
        public IState.E_State Update(PlayerDirector parent)
        {
            return parent._boardController.Fall() ? IState.E_State.UnChanged : IState.E_State.Erasing;
        }
    }
    class ErasingState : IState
    {
        public IState.E_State Initialize(PlayerDirector parent)
        {
            if(parent._boardController.CheckErase(parent._chainCount++))
            {
                return IState.E_State.UnChanged;
            }
            parent._chainCount = 0;
            return IState.E_State.Control;
        }
       
        public IState.E_State Update(PlayerDirector parent)
        {
            return parent._boardController.Erase() ? IState.E_State.UnChanged : IState.E_State.Falling;
        }
    }
    void InitializeState()
    {
        Debug.Assert(condition: _current_state is >= 0 and < IState.E_State.MAX);
        var next_state = states[(int)_current_state].Initialize(this);

        if (next_state != IState.E_State.UnChanged)
        {
            _current_state = next_state;
            InitializeState();
        }
    }
    void UpdateState()
    {
        Debug.Assert(condition: _current_state is >= 0 and < IState.E_State.MAX);
        var next_state = states[(int)_current_state].Update(this);
        if (next_state != IState.E_State.UnChanged)
        {
            _current_state = next_state;
            InitializeState();
        }
    }
    void SetScore(uint score)
    {
        _score = score;
        TextScore.text = _score.ToString();
    }
    void AddScore(uint score)
    {
        if(0<score)SetScore(_score + score);
    }
}


