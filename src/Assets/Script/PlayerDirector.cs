using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDirector : MonoBehaviour
{
    interface IState
    {
        public enum E_State
        {
            Control = 0,
            GameOver = 1,

            MAX,

            UnChanged,

        }

        E_State Initialize(PlayerDirector parent);

        E_State Update(PlayerDirector parent);
    }

    // Start is called before the first frame update
    [SerializeField] GameObject player = default!;
    PlayerController _playerController = null;
    LogialInput _logicalInput = new();
    NextQueue _nextQueue = new();
    [SerializeField] PuyoPair[] nextPuyoPairs = { default!, default! };
    IState.E_State _current_state = IState.E_State.Control;
    static readonly IState[] states = new IState[(int)IState.E_State.MAX]
     {
         new ControlState(),
         new GameoverState(),
     };

    void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
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
        if(!player.activeSelf)
        {
            Spawn(_nextQueue.Update());
            UpdateNextsView();
        }
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
            return parent.player.activeSelf ?IState.E_State.UnChanged :IState.E_State.Control;
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
}


