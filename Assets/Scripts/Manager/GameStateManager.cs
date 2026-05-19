using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public enum GameState
    {
        None,   // 디폴트 상태
        Prepare,    // 게임 시작 전 기물 배치 상태
        GamePlay,   // 게임 진행중 상태
        Win,    // 게임 승리 상태
        GameOver,    // 게임 패배 상태
        Reward,     // 보상 선택 상태
        Cleanup     // 승리 후 정리 연출/정리 처리 상태
    }

    [SerializeField] private GameState currentState;
    public GameState CurrentState => currentState;

    public delegate void StateChangeHandler(GameState newState);
    public event StateChangeHandler OnStateChanged;
    public Button gameStartButton;
    public GameObject gameOverPanel;

    private Dictionary<GameState, Action> stateActions = new Dictionary<GameState, Action>();

    public void RegisterStateAction(GameState state, Action action)
    {
        if (stateActions.ContainsKey(state))
        {
            stateActions[state] += action;
        }
        else
        {
            stateActions[state] = action;
        }
    }

    public void UnregisterStateAction(GameState state, Action action)
    {
        if (stateActions.ContainsKey(state))
        {
            stateActions[state] -= action;
        }
    }

    public void ChangeState(GameState newState)
    {
        if (currentState != newState)
        {
            if ((newState == GameState.Win || newState == GameState.GameOver) && currentState != GameState.GamePlay)
            {
                return;
            }

            currentState = newState;
            OnStateChanged?.Invoke(newState);

            UpdateGameStartButton(newState);

            if (newState == GameState.GameOver && gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (stateActions.TryGetValue(newState, out var action))
            {
                action?.Invoke();
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            // 인스펙터에서 상태를 변경했을 때 이벤트 발생
            OnStateChanged?.Invoke(currentState);
            if (stateActions.TryGetValue(currentState, out var action))
            {
                action?.Invoke();
            }
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        ChangeState(GameState.None);
        gameStartButton.onClick.AddListener(OnGameStartButtonClick);
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (gameStartButton == null)
        {
            GameObject btnObj = GameObject.Find("GameStartButton");
            if (btnObj != null)
            {
                gameStartButton = btnObj.GetComponent<Button>();
                gameStartButton.onClick.RemoveListener(OnGameStartButtonClick);
                gameStartButton.onClick.AddListener(OnGameStartButtonClick);
            }
        }

        if (gameOverPanel == null)
        {
            gameOverPanel = GameObject.Find("GameOverPanel");
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        UpdateGameStartButton(currentState);
    }

    private void OnGameStartButtonClick()
    {
        if (HasPlayerPiecesOnBoard())
        {
            // Capture placement snapshot at the moment the player confirms placement
            if (PieceManager.Instance != null)
            {
                PieceManager.Instance.CapturePlacementSnapshot();
            }

            ChangeState(GameState.GamePlay);
        }
        else
        {
            gameStartButton.transform.DOShakePosition(0.5f, new Vector3(5f, 1f, 0f), 30, 90f, false, true);
        }
    }

    private bool HasPlayerPiecesOnBoard()
    {
        if (PieceManager.Instance == null) return false;

        foreach (var piece in PieceManager.Instance.Pieces)
        {
            if (piece != null && !piece.IsEnemy) return true;
        }
        return false;
    }

    private void UpdateGameStartButton(GameState newState)
    {
        if (gameStartButton == null) return;

        gameStartButton.transform.DOKill();

        if (newState == GameState.Prepare)
        {
            gameStartButton.gameObject.SetActive(true);
            gameStartButton.transform.localScale = Vector3.zero;
            gameStartButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
        else
        {
            if (gameStartButton.gameObject.activeSelf)
            {
                gameStartButton.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        if (gameStartButton != null)
                            gameStartButton.gameObject.SetActive(false);
                    });
            }
        }
    }
}
