using skakmat.Chess;
using skakmat.Utilities;

namespace skakmat.Game;

public class Board
{

    private bool _whiteToPlay = true;
    private readonly ulong[] _bbs;
    private int _moveIndex = 0;
    private readonly List<Move> _movesPlayed = [];

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

    public Move GetMoveAt(int index)
    {
        if (index < 0 || index >= _movesPlayed.Count)
            throw new IndexOutOfRangeException("Move index invalid: " + index);
        return _movesPlayed[index];
    }

    public bool WhiteToPlay => _whiteToPlay;
    public int MovesPlayed => _movesPlayed.Count;

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


    public ulong GetLegalPawnMoves(int index, bool isWhite)
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

    public ulong GetLegalMoves(int pieceType, int index)
    {
        return pieceType switch
        {
            Constants.WhitePawn => GetLegalPawnMoves(index, true),
            Constants.BlackPawn => GetLegalPawnMoves(index, false),
            Constants.WhiteBishop => MoveTables.BishopAttacks(index, AllPieces).Exclude(WhitePieces),
            Constants.BlackBishop => MoveTables.BishopAttacks(index, AllPieces).Exclude(BlackPieces),
            Constants.WhiteKnight => _moveTables.KnightMoves[index].Exclude(WhitePieces),
            Constants.BlackKnight => _moveTables.KnightMoves[index].Exclude(BlackPieces),
            Constants.WhiteRook => MoveTables.RookAttacks(index, AllPieces).Exclude(WhitePieces),
            Constants.BlackRook => MoveTables.RookAttacks(index, AllPieces).Exclude(BlackPieces),
            Constants.WhiteQueen => GetLegalMoves(Constants.WhiteRook, index) | GetLegalMoves(Constants.WhiteBishop, index),
            Constants.BlackQueen => GetLegalMoves(Constants.BlackRook, index) | GetLegalMoves(Constants.BlackBishop, index),
            Constants.WhiteKing => _moveTables.KingMoves[index].Exclude(ControlledSquares(false) | WhitePieces),
            Constants.BlackKing => _moveTables.KingMoves[index].Exclude(ControlledSquares(true) | BlackPieces),
            _ => 0UL,
        };
    }

    public static bool IsValidMove(PieceSelection selection, ulong targetBit)
    {
        return selection.LegalMoves.Contains(targetBit);
    }

    public bool UndoMove()
    {
        if (MovesPlayed == 0)
            return false;
        var latestMove = _movesPlayed[^1];
        var invertedMove = BoardUtility.InvertMove(latestMove);
        return MakeMove(invertedMove, false);
    }

    public bool UndoFullMove()
    {
        if (MovesPlayed == 0 || MovesPlayed % 2 != 0)
            return false;

        return UndoMove() && UndoMove();
    }

    public bool MakeMove(Move move, bool incrementIndex = true)
    {
        var moveMade = MakeMove(move.OriginBit, move.TargetBit, move.PieceType);
        if (moveMade)
        {
            if (incrementIndex)
                _moveIndex++;
            _movesPlayed.Add(move);
        }
        return moveMade;
    }

    private bool MakeMove(ulong originBit, ulong targetBit, int pieceType)
    {
        var optCapturedPiece = GetPieceTypeFromSquare(targetBit);
        if (optCapturedPiece != Constants.EmptySquare)
        {
            _bbs[optCapturedPiece] ^= targetBit;
        }
        _bbs[pieceType] ^= originBit;
        _bbs[pieceType] |= targetBit;
        _whiteToPlay = !_whiteToPlay;
        return true;
    }

    public PieceSelection? TrySelectPiece(int index)
    {
        var pieceType = GetPieceTypeAtIndex(index);
        if (pieceType == Constants.EmptySquare || !IsCorrectColor(pieceType, _whiteToPlay)) return null;
        var legalMoves = GetLegalMoves(pieceType, index);
        return new PieceSelection(pieceType, index, legalMoves);
    }


    public List<Move> GenerateMoves()
    {
        var validMoves = new List<Move>();
        for (var idx = 0; idx < 64; idx++)
        {
            var originBit = 1UL << idx;
            var pieceType = GetPieceTypeFromSquare(originBit);
            if (pieceType == Constants.EmptySquare) continue;
            if (!IsCorrectColor(pieceType, _whiteToPlay)) continue;

            var legalMoves = GetLegalMoves(pieceType, idx);
            for (var j = 0; j < 64; j++)
            {
                var targetBit = 1UL << j;
                if (legalMoves.Contains(targetBit))
                {
                    validMoves.Add(new Move(pieceType, originBit, targetBit));
                }
            }
        }

        return validMoves;
    }

    public void DumpBoardData()
    {
        Console.WriteLine("=== BOARD DUMP ===");
        Console.WriteLine($"{(_whiteToPlay ? "White" : "Black")} to play");
        Console.WriteLine("Move index: " + _moveIndex);
        Console.WriteLine("Moves:");
        var moveIndex = 1;
        for (var i = 0; i < _movesPlayed.Count; i += 2)
        {
            var whiteMove = _movesPlayed[i];
            var blackMove = _movesPlayed[i + 1];
            Console.WriteLine($"{moveIndex++}. {whiteMove.ToSanNotation()} {blackMove.ToSanNotation()}");
        }
    }

}