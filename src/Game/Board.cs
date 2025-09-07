using skakmat.Chess;
using skakmat.Utilities;

namespace skakmat.Game;

public class Board
{

    private readonly ulong[] _bbs;

    internal ulong WhitePieces =>
        _bbs[Constants.WhitePawn]
        | _bbs[Constants.WhiteRook]
        | _bbs[Constants.WhiteKnight]
        | _bbs[Constants.WhiteBishop]
        | _bbs[Constants.WhiteQueen]
        | _bbs[Constants.WhiteKing];

    internal ulong BlackPieces =>
        _bbs[Constants.BlackPawn]
        | _bbs[Constants.BlackRook]
        | _bbs[Constants.BlackKnight]
        | _bbs[Constants.BlackBishop]
        | _bbs[Constants.BlackQueen]
        | _bbs[Constants.BlackKing];

    internal ulong AllPieces => WhitePieces | BlackPieces;

    private readonly MoveTables _moveTables;

    public Board()
    {
        _bbs = BoardUtility.BitboardFromFen(Constants.FenDefaultPosition);
        _moveTables = new MoveTables();
    }

    public int GetPieceTypeFromSquare(ulong square)
    {
        if (_bbs[Constants.WhitePawn].Contains(square)) return Constants.WhitePawn;
        if (_bbs[Constants.BlackPawn].Contains(square)) return Constants.BlackPawn;
        if (_bbs[Constants.WhiteRook].Contains(square)) return Constants.WhiteRook;
        if (_bbs[Constants.BlackRook].Contains(square)) return Constants.BlackRook;
        if (_bbs[Constants.WhiteKnight].Contains(square)) return Constants.WhiteKnight;
        if (_bbs[Constants.BlackKnight].Contains(square)) return Constants.BlackKnight;
        if (_bbs[Constants.WhiteBishop].Contains(square)) return Constants.WhiteBishop;
        if (_bbs[Constants.BlackBishop].Contains(square)) return Constants.BlackBishop;
        if (_bbs[Constants.WhiteQueen].Contains(square)) return Constants.WhiteQueen;
        if (_bbs[Constants.BlackQueen].Contains(square)) return Constants.BlackQueen;
        if (_bbs[Constants.WhiteKing].Contains(square)) return Constants.WhiteKing;
        if (_bbs[Constants.BlackKing].Contains(square)) return Constants.BlackKing;
        return Constants.EmptySquare;
    }

    public int GetPieceTypeAtIndex(int index)
    {
        return GetPieceTypeFromSquare(1UL << index);
    }

    public IEnumerable<(int square, int pieceType, ulong bit)> GetAllPieces()
    {
        for (var sq = 0; sq < 64; sq++)
        {
            var bit = 1UL << sq;
            var type = GetPieceTypeFromSquare(bit);
            if (type != Constants.EmptySquare)
                yield return (sq, type, bit);
        }
    }

    private static bool IsCorrectColor(int pieceType, bool isWhite)
    {
        return isWhite ?
            pieceType >= Constants.WhitePawn && pieceType <= Constants.WhiteKing
            : pieceType >= Constants.BlackPawn && pieceType <= Constants.BlackKing;
    }

    private ulong ControlledSquares(bool isWhite)
    {
        var control = 0UL;
        foreach (var (sq, type, _) in GetAllPieces())
        {
            if (IsCorrectColor(type, isWhite))
            {
                control |= GetPieceAttacks(type, sq, AllPieces);
            }
        }
        return control;
    }

    private ulong GetPieceAttacks(int pieceType, int square, ulong allPieces) => pieceType switch
    {
        Constants.WhitePawn => _moveTables.WhitePawnAttacks[square],
        Constants.BlackPawn => _moveTables.BlackPawnAttacks[square],
        Constants.WhiteKnight or Constants.BlackKnight => _moveTables.KnightMoves[square],
        Constants.WhiteBishop => MoveTables.BishopAttacks(square, allPieces).Exclude(WhitePieces),
        Constants.BlackBishop => MoveTables.BishopAttacks(square, allPieces).Exclude(BlackPieces),
        Constants.WhiteRook => MoveTables.RookAttacks(square, allPieces).Exclude(WhitePieces),
        Constants.BlackRook => MoveTables.RookAttacks(square, allPieces).Exclude(BlackPieces),
        Constants.WhiteQueen =>
            GetPieceAttacks(Constants.WhiteRook, square, allPieces)
            | GetPieceAttacks(Constants.WhiteBishop, square, allPieces),
        Constants.BlackQueen =>
            GetPieceAttacks(Constants.BlackRook, square, allPieces)
            | GetPieceAttacks(Constants.BlackBishop, square, allPieces),
        Constants.WhiteKing or Constants.BlackKing => _moveTables.KingMoves[square],
        _ => 0UL
    };


    public ulong LegalPawnMoves(int index, bool isWhite)
    {
        var moveBits = isWhite ? _moveTables.WhitePawnMoves[index] : _moveTables.BlackPawnMoves[index];
        var attackBits = isWhite ? _moveTables.WhitePawnAttacks[index] : _moveTables.BlackPawnAttacks[index];
        var startRow = isWhite ? Masks.Rank2 : Masks.Rank7;
        var firstMoveBlokingRow = isWhite ? Masks.Rank3 : Masks.Rank6;
        attackBits &= isWhite ? BlackPieces : WhitePieces;
        if (startRow.Contains(1UL << index) && firstMoveBlokingRow.Contains(moveBits & AllPieces))
        {
            return attackBits;
        }
        return moveBits.Exclude(AllPieces) | attackBits;
    }

    public ulong LegalMoves(int pieceType, int index)
    {
        return pieceType switch
        {
            Constants.WhitePawn => LegalPawnMoves(index, true),
            Constants.BlackPawn => LegalPawnMoves(index, false),
            Constants.WhiteBishop => MoveTables.BishopAttacks(index, AllPieces).Exclude(WhitePieces),
            Constants.BlackBishop => MoveTables.BishopAttacks(index, AllPieces).Exclude(BlackPieces),
            Constants.WhiteKnight => _moveTables.KnightMoves[index].Exclude(WhitePieces),
            Constants.BlackKnight => _moveTables.KnightMoves[index].Exclude(BlackPieces),
            Constants.WhiteRook => MoveTables.RookAttacks(index, AllPieces).Exclude(WhitePieces),
            Constants.BlackRook => MoveTables.RookAttacks(index, AllPieces).Exclude(BlackPieces),
            Constants.WhiteQueen => LegalMoves(Constants.WhiteRook, index) | LegalMoves(Constants.WhiteBishop, index),
            Constants.BlackQueen => LegalMoves(Constants.BlackRook, index) | LegalMoves(Constants.BlackBishop, index),
            Constants.WhiteKing => _moveTables.KingMoves[index].Exclude(ControlledSquares(false) | WhitePieces),
            Constants.BlackKing => _moveTables.KingMoves[index].Exclude(ControlledSquares(true) | BlackPieces),
            _ => 0UL,
        };
    }

    public static bool IsValidMove(PieceSelection selection, ulong targetBit)
    {
        return selection.LegalMoves.Contains(targetBit);
    }

    public bool MakeMove(PieceSelection selection, ulong targetBit)
    {
        if (!IsValidMove(selection, targetBit))
            return false;

        var optCapturedPiece = GetPieceTypeFromSquare(targetBit);
        if (optCapturedPiece != Constants.EmptySquare)
        {
            _bbs[optCapturedPiece] ^= targetBit;
        }
        var originBit = 1UL << selection.SquareIndex;
        _bbs[selection.PieceType] ^= originBit;
        _bbs[selection.PieceType] |= targetBit;
        return true;

    }

    public PieceSelection? TrySelectPiece(int index, bool whiteToPlay)
    {
        var pieceType = GetPieceTypeAtIndex(index);
        if (pieceType == Constants.EmptySquare || !IsCorrectColor(pieceType, whiteToPlay)) return null;
        var legalMoves = LegalMoves(pieceType, index);
        return new PieceSelection(pieceType, index, legalMoves);
    }

}