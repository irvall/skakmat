using skakmat.Chess;
using skakmat.Extensions;
using skakmat.Helpers;

namespace skakmat.Game;

internal class Board
{
    internal Position CreatePosition() => new(bbs, whiteToPlay, castlingRights, lastMovePlayed);
    private bool whiteToPlay;
    private readonly ulong[] bbs;
    private Castling.Rights castlingRights;
    private Move? lastMovePlayed;

    public Board()
    {
        bbs = BoardHelper.BitboardFromFen(Constants.FenPositions.Default);
        whiteToPlay = true;
        castlingRights = Castling.Rights.All;
        lastMovePlayed = null;
    }

    public Board(Position position)
    {
        bbs = (ulong[])position.Bitboards.Clone();
        whiteToPlay = position.WhiteToPlay;
        castlingRights = position.CastlingRights;
        lastMovePlayed = position.LastMovePlayed;
    }

    internal int GetPieceIndexAt(ulong square)
    {
        if (bbs[Piece.WhitePawn].Contains(square)) return Piece.WhitePawn;
        if (bbs[Piece.BlackPawn].Contains(square)) return Piece.BlackPawn;
        if (bbs[Piece.WhiteRook].Contains(square)) return Piece.WhiteRook;
        if (bbs[Piece.BlackRook].Contains(square)) return Piece.BlackRook;
        if (bbs[Piece.WhiteKnight].Contains(square)) return Piece.WhiteKnight;
        if (bbs[Piece.BlackKnight].Contains(square)) return Piece.BlackKnight;
        if (bbs[Piece.WhiteBishop].Contains(square)) return Piece.WhiteBishop;
        if (bbs[Piece.BlackBishop].Contains(square)) return Piece.BlackBishop;
        if (bbs[Piece.WhiteQueen].Contains(square)) return Piece.WhiteQueen;
        if (bbs[Piece.BlackQueen].Contains(square)) return Piece.BlackQueen;
        if (bbs[Piece.WhiteKing].Contains(square)) return Piece.WhiteKing;
        if (bbs[Piece.BlackKing].Contains(square)) return Piece.BlackKing;
        return Piece.EmptySquare;
    }

    internal int GetPieceIndexAt(int squareIndex)
    {
        return GetPieceIndexAt(1UL << squareIndex);
    }

    internal IEnumerable<(int pieceIndex, int index, ulong bit)> GetAllPieces()
    {
        foreach (var (idx, bit) in BoardHelper.EnumerateSquares())
        {
            var type = GetPieceIndexAt(bit);
            if (type != Piece.EmptySquare)
                yield return (type, idx, bit);
        }
    }

    private void PromotePawns()
    {
        var lastRank = whiteToPlay ? Masks.Rank8 : Masks.Rank1;
        var pawnType = GetPieceIndex(PieceType.Pawn);
        var queenType = whiteToPlay ? Piece.WhiteQueen : Piece.BlackQueen;
        bbs[queenType] |= lastRank & bbs[pawnType];
        bbs[pawnType] ^= lastRank & bbs[pawnType];
    }

    private void HandleSpecialMove(Move move)
    {
        switch (move)
        {
            case CastleMove castle:
                ApplyMove(castle.RookMove, false);
                break;
            case EnPassantMove ep:
                RemovePiece(ep.PawnToRemove.PieceIndex, ep.PawnToRemove.TargetBit);
                break;
        }
    }

    internal Position ApplyMove(Move move, bool swapSide = true)
    {
        HandleSpecialMove(move);
        UpdateCastlingRights(move);
        ExecuteMove(move);
        PromotePawns();
        if (swapSide)
            whiteToPlay = !whiteToPlay;
        lastMovePlayed = move;
        return CreatePosition();
    }

    private void RemoveCastlingRight(Castling.Type type)
    {
        if (type == Castling.Type.KingSide)
        {
            castlingRights &= ~(whiteToPlay ? Castling.Rights.WhiteKingSide : Castling.Rights.BlackKingSide);
        }
        else
        {
            castlingRights &= ~(whiteToPlay ? Castling.Rights.WhiteQueenSide : Castling.Rights.BlackQueenSide);
        }
    }

    private int GetPieceIndex(PieceType type)
    {
        return Piece.GetPieceIndex(type, whiteToPlay);
    }

    private void UpdateCastlingRights(Move move)
    {
        if (move.PieceIndex == GetPieceIndex(PieceType.Rook))
        {
            if (move.OriginBit.Contains(Masks.RookRightCorner(whiteToPlay)))
                RemoveCastlingRight(Castling.Type.KingSide);
            if (move.OriginBit.Contains(Masks.RookLeftCorner(whiteToPlay)))
                RemoveCastlingRight(Castling.Type.QueenSide);
        }
        if (move.PieceIndex == GetPieceIndex(PieceType.King))
        {
            var whiteCastlingRights = Castling.Rights.WhiteKingSide | Castling.Rights.WhiteQueenSide;
            var blackCastlingRights = Castling.Rights.BlackKingSide | Castling.Rights.BlackQueenSide;
            castlingRights &= ~(whiteToPlay ? whiteCastlingRights : blackCastlingRights);
        }
    }

    private void RemovePiece(int pieceIndexToRemove, ulong targetBit)
    {
        if (pieceIndexToRemove == Piece.EmptySquare)
            return;
        bbs[pieceIndexToRemove] ^= targetBit;
    }

    public void ExecuteMove(Move move)
    {
        var optCapturedPiece = GetPieceIndexAt(move.TargetBit);
        RemovePiece(optCapturedPiece, move.TargetBit);
        bbs[move.PieceIndex] ^= move.OriginBit;
        bbs[move.PieceIndex] |= move.TargetBit;
    }

    internal void UndoMove(Move move, int possibleTargetType)
    {
        RemovePiece(possibleTargetType, move.TargetBit);
        bbs[move.PieceIndex] ^= move.TargetBit;
        bbs[move.PieceIndex] |= move.OriginBit;
    }

}