using System.Collections.Generic;
using UnityEngine;

public class SealManager : MonoBehaviour
{
    public static SealManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public List<SealBase> GetAttachedSeal(PieceController piece)
    {
        return piece.EquippedSeals;
    }
    // 기물에 인장 장착 시도
    public void TryAttachSealToPiece(PieceController piece, SealData seal)
    {
        // 1. 호환성 체크
        if (seal.compatiblePieces != null && seal.compatiblePieces.Count > 0)
        {
            if (!seal.compatiblePieces.Contains(piece.Type))
            {
                // TODO: 실패 피드백 (사운드, 텍스트 등)
                return;
            }
        }

        // 2. 이미 같은 인장이 있는지, 슬롯이 꽉 찼는지 등 추가 조건 체크 가능
        
        // 3. 인장 장착 (컴포넌트 추가)
        piece.EquipSeal(seal);
    }
}
