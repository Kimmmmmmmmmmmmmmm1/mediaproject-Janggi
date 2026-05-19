using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PieceInventory : MonoBehaviour
{
    public static PieceInventory Instance { get; private set; }
    [System.Serializable]
    public struct PieceInfo
    {
        public PieceType pieceType;
        [Tooltip("기물에 장착할 인장 목록")]
        public List<SealData> seals;
    }

    [Header("Owned Pieces")]
    [SerializeField] private List<PieceInfo> ownedPieces = new List<PieceInfo>();
    [Header("Initial Pieces")]
    [SerializeField] private InitialPieceData initialPieceData;
    private bool hasStageSnapshot = false;
    private string lastSuppliedScene = null;

    public IReadOnlyList<PieceInfo> OwnedPieces => ownedPieces;
    public InitialPieceData InitialPieceData => initialPieceData;
    public bool HasStageSnapshot => hasStageSnapshot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Scene-bound: do not persist across scenes to avoid stale UI references
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (lastSuppliedScene == scene.name)
        {
            return;
        }

        // Supply initial pieces once when a scene is loaded.
        SupplyInitialPieces();
        lastSuppliedScene = scene.name;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // Clear marker so reloading the same scene will re-supply.
        if (lastSuppliedScene == scene.name)
        {
            lastSuppliedScene = null;
        }
    }

    private void Start()
    {
        // 초기 기물은 TitleManager가 새 런 시작 시 InitialPieceData를 통해 공급합니다.
    }

    public void SupplyInitialPieces()
    {
        if (initialPieceData == null)
        {
            return;
        }

        ClearPieces();

        foreach (var pieceInfo in initialPieceData.initialPieces)
        {
            AddPiece(pieceInfo.pieceType, pieceInfo.seals != null ? new List<SealData>(pieceInfo.seals) : new List<SealData>());
        }
    }

    public void AddPiece(PieceType pieceType, List<SealData> seals = null)
    {
        List<SealData> pieceSeals = seals ?? new List<SealData>();
        ownedPieces.Add(new PieceInfo { pieceType = pieceType, seals = pieceSeals });

        if (pieceSeals.Count > 0)
        {
            CollectionManager.EnsureInstance().RecordSeals(pieceSeals);
        }
    }

    public bool RemovePiece(PieceType pieceType)
    {
        PieceInfo pieceToRemove = ownedPieces.Find(p => p.pieceType == pieceType);
        if (pieceToRemove.pieceType == pieceType)
        {
            ownedPieces.Remove(pieceToRemove);

            return true;
        }
        return false;
    }

    public void ClearPieces()
    {
        ownedPieces.Clear();
        // Clear the last supplied scene marker so a subsequent scene load
        // (even the same scene name) will re-supply initial pieces.
        lastSuppliedScene = null;
    }

    public void SaveBoardPieces()
    {
        if (PieceManager.Instance == null) return;

        foreach (var piece in PieceManager.Instance.Pieces)
        {
            // 적 기물이 아니고, 보드(Board) 상태인 기물만 저장
            if (piece != null && !piece.IsEnemy && piece.CurrentLocation == PieceLocation.Board)
            {
                List<SealData> seals = new List<SealData>();
                foreach (var sealBase in piece.EquippedSeals)
                {
                    if (sealBase != null && sealBase.Data != null)
                    {
                        seals.Add(sealBase.Data);
                    }
                }

                AddPiece(piece.HasPromotionSeal() ? PieceType.Soldier : piece.Type, seals);
            }
        }
    }

    public void SavePiecesForNextStage()
    {
        if (hasStageSnapshot)
        {
            return;
        }

        ClearPieces();

        PieceController[] allPieces = FindObjectsByType<PieceController>(FindObjectsSortMode.None);
        foreach (PieceController piece in allPieces)
        {
            if (piece == null || piece.IsEnemy)
            {
                continue;
            }

            if (piece.CurrentLocation != PieceLocation.Board && piece.CurrentLocation != PieceLocation.Inventory)
            {
                continue;
            }

            // 인장 정보를 수집합니다.
            List<SealData> seals = new List<SealData>();
            foreach (var sealBase in piece.EquippedSeals)
            {
                if (sealBase != null && sealBase.Data != null)
                {
                    seals.Add(sealBase.Data);
                }
            }

            AddPiece(piece.HasPromotionSeal() ? PieceType.Soldier : piece.Type, seals);
        }

        hasStageSnapshot = true;
    }

    public void ResetStageSnapshot()
    {
        hasStageSnapshot = false;
    }

    private void AddDefaultPieces()
    {
        // 기본 장기 기물 세트 예시 (로그라이크 특성에 맞춰 조절 가능)
        AddPiece(PieceType.King);
        AddPiece(PieceType.Chariot);
        AddPiece(PieceType.Cannon);
        AddPiece(PieceType.Horse);
        AddPiece(PieceType.Elephant);
        AddPiece(PieceType.Soldier);
        AddPiece(PieceType.Soldier);
        AddPiece(PieceType.Soldier);
    }
}
