using skakmat.Chess;
using skakmat.Utilities;

namespace skakmat.Game;

internal class Board
{
    internal BoardState GetBoardState() => new(_bbs, _whiteToPlay, castlingRights, lastMovePlayed);
    public bool WhiteToPlay => _whiteToPlay;
    private bool _whiteToPlay = true;
    private readonly ulong[] _bbs = BoardUtility.BitboardFromFen(Constants.Fen.DefaultPosition);
    private Castling.Rights castlingRights = Castling.Rights.All;
    private Move? lastMovePlayed = null;

    internal int GetPieceIndexAt(ulong square)
    {
        if (_bbs[Piece.WhitePawn].Contains(square)) return Piece.WhitePawn;
        if (_bbs[Piece.BlackPawn].Contains(square)) return Piece.BlackPawn;
        if (_bbs[Piece.WhiteRook].Contains(square)) return Piece.WhiteRook;
        if (_bbs[Piece.BlackRook].Contains(square)) return Piece.BlackRook;
        if (_bbs[Piece.WhiteKnight].Contains(square)) return Piece.WhiteKnight;
        if (_bbs[Piece.BlackKnight].Contains(square)) return Piece.BlackKnight;
        if (_bbs[Piece.WhiteBishop].Contains(square)) return Piece.WhiteBishop;
        if (_bbs[Piece.BlackBishop].Contains(square)) return Piece.BlackBishop;
        if (_bbs[Piece.WhiteQueen].Contains(square)) return Piece.WhiteQueen;
        if (_bbs[Piece.BlackQueen].Contains(square)) return Piece.BlackQueen;
        if (_bbs[Piece.WhiteKing].Contains(square)) return Piece.WhiteKing;
        if (_bbs[Piece.BlackKing].Contains(square)) return Piece.BlackKing;
        return Piece.EmptySquare;
    }

    internal int GetPieceIndexAt(int squareIndex)
    {
        return GetPieceIndexAt(1UL << squareIndex);
    }

    internal IEnumerable<(int pieceIndex, int index, ulong bit)> GetAllPieces()
    {
        foreach (var (idx, bit) in BoardUtility.EnumerateSquares())
        {
            var type = GetPieceIndexAt(bit);
            if (type != Piece.EmptySquare)
                yield return (type, idx, bit);
        }
    }

    private void PromotePawns()
    {
        var lastRank = _whiteToPlay ? Masks.Rank8 : Masks.Rank1;
        var pawnType = GetPieceIndex(PieceType.Pawn);
        var queenType = _whiteToPlay ? Piece.WhiteQueen : Piece.BlackQueen;
        _bbs[queenType] |= lastRank & _bbs[pawnType];
        _bbs[pawnType] ^= lastRank & _bbs[pawnType];
    }

    private void HandleSpecialMove(Move move)
    {
        switch (move)
        {
            case CastleMove castle:
                ApplyMove(castle.RookMove, false);
                UpdateCastlingRights(move);
                break;
            case EnPassantMove ep: RemovePiece(ep.PawnToRemove.PieceIndex, ep.PawnToRemove.TargetBit); break;
        }
    }

    internal void ApplyMove(Move move, bool swapSide = true)
    {
        System.Console.WriteLine($"[DEBUG] Before move: _whiteToPlay={{_whiteToPlay}}, move={{move}} swapSide={{swapSide}}");
        HandleSpecialMove(move);
        ExecuteMove(move);
        PromotePawns();
        if (swapSide)
        {
            _whiteToPlay = !_whiteToPlay;
            System.Console.WriteLine($"[DEBUG] Swapped side. Now _whiteToPlay={{_whiteToPlay}} after move={{move}}");
        }
        else
        {
            System.Console.WriteLine($"[DEBUG] Did not swap side. _whiteToPlay still {{_whiteToPlay}} after move={{move}}");
        }
        lastMovePlayed = move;
    }

    private void RemoveCastlingRight(Castling.Type type)
    {
        if (type == Castling.Type.KingSide)
        {
            castlingRights &= ~(_whiteToPlay ? Castling.Rights.WhiteKingSide : Castling.Rights.BlackKingSide);
        }
        else
        {
            castlingRights &= ~(_whiteToPlay ? Castling.Rights.WhiteQueenSide : Castling.Rights.BlackQueenSide);
        }
    }

    private int GetPieceIndex(PieceType type)
    {
        return Piece.GetPieceIndex(type, _whiteToPlay);
    }

    private void UpdateCastlingRights(Move move)
    {
        if (move.PieceIndex == GetPieceIndex(PieceType.Rook))
        {
            if (move.OriginBit.Contains(Masks.RookRightCorner(_whiteToPlay)))
                RemoveCastlingRight(Castling.Type.KingSide);
            if (move.OriginBit.Contains(Masks.RookLeftCorner(_whiteToPlay)))
                RemoveCastlingRight(Castling.Type.QueenSide);
        }
        if (move.PieceIndex == GetPieceIndex(PieceType.King))
        {
            var whiteCastlingRights = Castling.Rights.WhiteKingSide | Castling.Rights.WhiteQueenSide;
            var blackCastlingRights = Castling.Rights.BlackKingSide | Castling.Rights.BlackQueenSide;
            castlingRights &= ~(_whiteToPlay ? whiteCastlingRights : blackCastlingRights);
        }
    }

    private void RemovePiece(int pieceIndexToRemove, ulong targetBit)
    {
        if (pieceIndexToRemove == Piece.EmptySquare)
            return;
        _bbs[pieceIndexToRemove] ^= targetBit;
    }

    public void ExecuteMove(Move move)
    {
        var optCapturedPiece = GetPieceIndexAt(move.TargetBit);
        RemovePiece(optCapturedPiece, move.TargetBit);
        _bbs[move.PieceIndex] ^= move.OriginBit;
        _bbs[move.PieceIndex] |= move.TargetBit;
    }

    internal void UndoMove(Move move, int possibleTargetType)
    {
        RemovePiece(possibleTargetType, move.TargetBit);
        _bbs[move.PieceIndex] ^= move.TargetBit;
        _bbs[move.PieceIndex] |= move.OriginBit;
    }

}