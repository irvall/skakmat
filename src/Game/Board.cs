using skakmat.Chess;
using skakmat.Utilities;

namespace skakmat.Game;

public class Board(bool debug)
{

    private bool _whiteToPlay = true;
    private readonly ulong[] _bbs = BoardUtility.BitboardFromFen(Constants.FenBothSidesCanCastle);
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
    internal ulong EmptySquares => ~AllPieces;

    private readonly MoveTables _moveTables = new();

    private readonly bool _debug = debug;

    private bool whiteKingMoved = false;
    private bool blackKingMoved = false;

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

    public IEnumerable<(int pieceType, int index, ulong bit)> GetAllPieces()
    {
        foreach (var (idx, bit) in BoardUtility.EnumerateSquares())
        {
            var type = GetPieceTypeFromSquare(bit);
            if (type != Constants.EmptySquare)
                yield return (type, idx, bit);
        }
    }

    private static bool IsCorrectColor(int pieceType, bool isWhite)
    {
        return isWhite ?
            pieceType >= Constants.WhitePawn && pieceType <= Constants.WhiteKing
            : pieceType >= Constants.BlackPawn && pieceType <= Constants.BlackKing;
    }

    public ulong ControlledSquares()
    {
        return ControlledSquares(_whiteToPlay);
    }

    private ulong ControlledSquares(bool isWhite)
    {
        var control = 0UL;
        var allPieces = AllPieces;
        foreach (var (type, idx, _) in GetAllPieces())
        {
            if (IsCorrectColor(type, isWhite))
            {
                control |= GetPieceAttacks(type, idx, allPieces);
            }
        }
        return control;
    }

    private ulong GetPieceAttacks(int pieceType, int index, ulong allPieces) => pieceType switch
    {
        Constants.WhitePawn => _moveTables.WhitePawnAttacks[index],
        Constants.BlackPawn => _moveTables.BlackPawnAttacks[index],
        Constants.WhiteKnight => _moveTables.KnightMoves[index].Exclude(WhitePieces),
        Constants.BlackKnight => _moveTables.KnightMoves[index].Exclude(BlackPieces),
        Constants.WhiteBishop => MoveTables.BishopAttacks(index, allPieces).Exclude(WhitePieces),
        Constants.BlackBishop => MoveTables.BishopAttacks(index, allPieces).Exclude(BlackPieces),
        Constants.WhiteRook => MoveTables.RookAttacks(index, allPieces).Exclude(WhitePieces),
        Constants.BlackRook => MoveTables.RookAttacks(index, allPieces).Exclude(BlackPieces),
        Constants.WhiteQueen =>
            GetPieceAttacks(Constants.WhiteRook, index, allPieces)
            | GetPieceAttacks(Constants.WhiteBishop, index, allPieces),
        Constants.BlackQueen =>
            GetPieceAttacks(Constants.BlackRook, index, allPieces)
            | GetPieceAttacks(Constants.BlackBishop, index, allPieces),
        Constants.WhiteKing => _moveTables.KingMoves[index].Exclude(WhitePieces),
        Constants.BlackKing => _moveTables.KingMoves[index].Exclude(BlackPieces),
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

    public ulong GetPseudoLegalMoves(int pieceType, int index)
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
            Constants.WhiteQueen => GetPseudoLegalMoves(Constants.WhiteRook, index) | GetPseudoLegalMoves(Constants.WhiteBishop, index),
            Constants.BlackQueen => GetPseudoLegalMoves(Constants.BlackRook, index) | GetPseudoLegalMoves(Constants.BlackBishop, index),
            Constants.WhiteKing => _moveTables.KingMoves[index].Exclude(ControlledSquares(false) | WhitePieces),
            Constants.BlackKing => _moveTables.KingMoves[index].Exclude(ControlledSquares(true) | BlackPieces),
            _ => 0UL,
        };
    }

    public bool UndoMove()
    {
        if (MovesPlayed == 0)
            return false;
        var latestMove = _movesPlayed[^1];
        var invertedMove = BoardUtility.InvertMove(latestMove);
        MakeMove(invertedMove);
        return true;
    }

    private void ApplyMove(Move move)
    {
        ApplyMove(move.OriginBit, move.TargetBit, move.PieceType);
    }

    private void PromotePawns()
    {
        var lastRank = _whiteToPlay ? Masks.Rank8 : Masks.Rank1;
        var pawnType = _whiteToPlay ? Constants.WhitePawn : Constants.BlackPawn;
        var queenType = _whiteToPlay ? Constants.WhiteQueen : Constants.BlackQueen;
        _bbs[queenType] |= lastRank & _bbs[pawnType];
        _bbs[pawnType] ^= lastRank & _bbs[pawnType];
    }

    private void HandleCastling(Move move)
    {
        var whiteCanCastle = move.PieceType == Constants.WhiteKing && !whiteKingMoved;
        var blackCanCastle = move.PieceType == Constants.BlackKing && !blackKingMoved;
        if (!whiteCanCastle && !blackCanCastle)
            return;

        var kingStartPosition = _whiteToPlay ? BoardSquares.Squares.E1.AsBit() : BoardSquares.Squares.E8.AsBit();
        var kingCastlingMoves = _whiteToPlay ? Masks.WhiteKingTryCastleShort : Masks.BlackKingTryCastleShort;
        if (move.OriginBit.Contains(kingStartPosition) && kingCastlingMoves.Contains(move.TargetBit))
        {
            // Valid short castle
            var rookStartPosition = _whiteToPlay ? BoardSquares.Squares.H1.AsBit() : BoardSquares.Squares.H8.AsBit();
            var rookEndPosition = _whiteToPlay ? BoardSquares.Squares.F1.AsBit() : BoardSquares.Squares.F8.AsBit();
            var rookType = _whiteToPlay ? Constants.WhiteRook : Constants.BlackRook;
            var rookMove = new Move(rookType, rookStartPosition, rookEndPosition);
            MakeMove(rookMove, swapSide: false);
        }
    }


    public void MakeMove(Move move, bool swapSide = true)
    {
        ApplyMove(move.OriginBit, move.TargetBit, move.PieceType);
        _movesPlayed.Add(move);
        PromotePawns();
        HandleCastling(move);
        if (_whiteToPlay && move.PieceType == Constants.WhiteKing)
            whiteKingMoved = true;
        if (!_whiteToPlay && move.PieceType == Constants.BlackKing)
            blackKingMoved = true;
        if (swapSide)
            _whiteToPlay = !_whiteToPlay;
    }

    private void ApplyMove(ulong originBit, ulong targetBit, int pieceType)
    {
        var optCapturedPiece = GetPieceTypeFromSquare(targetBit);
        if (optCapturedPiece != Constants.EmptySquare)
        {
            _bbs[optCapturedPiece] ^= targetBit;
        }
        _bbs[pieceType] ^= originBit;
        _bbs[pieceType] |= targetBit;
    }

    private void UndoMove(Move move, int possibleTargetType)
    {
        if (possibleTargetType != Constants.EmptySquare)
        {
            _bbs[possibleTargetType] ^= move.TargetBit;
        }
        _bbs[move.PieceType] ^= move.TargetBit;
        _bbs[move.PieceType] |= move.OriginBit;
    }

    public PieceSelection? TrySelectPiece(int index)
    {
        var pieceType = GetPieceTypeAtIndex(index);
        if (pieceType == Constants.EmptySquare || !IsCorrectColor(pieceType, _whiteToPlay)) return null;
        var validMoves = GenerateMovesFromIndex(index);
        return new PieceSelection(pieceType, index, validMoves);
    }

    private bool IsKingUnderAttack()
    {
        var controlledSquares = ControlledSquares(!_whiteToPlay);
        return _whiteToPlay && _bbs[Constants.WhiteKing].Contains(controlledSquares)
           || !_whiteToPlay && _bbs[Constants.BlackKing].Contains(controlledSquares);
    }

    private bool IsValidCastling(Move move)
    {
        if (move.PieceType != Constants.WhiteKing && move.PieceType != Constants.BlackKing)
            return false;

        var emptySquares = EmptySquares;
        var kingTriesToCastle = _whiteToPlay ? Masks.WhiteKingShortGap.Contains(move.TargetBit) : Masks.BlackKingShortGap.Contains(move.TargetBit);
        var emptyGap = _whiteToPlay ? emptySquares.Contains(Masks.WhiteKingShortGap) : emptySquares.Contains(Masks.BlackKingShortGap);
        return kingTriesToCastle && emptyGap;
    }

    public List<Move> GenerateMovesFromIndex(int index)
    {
        var pieceType = GetPieceTypeAtIndex(index);
        var originBit = 1UL << index;
        var legalMoves = GetPseudoLegalMoves(pieceType, index);
        var validMoves = new List<Move>();
        foreach (var (_, targetBit) in BoardUtility.EnumerateSquares())
        {
            var possibleMove = new Move(pieceType, originBit, targetBit);
            if (legalMoves.Contains(targetBit) || IsValidCastling(possibleMove))
            {
                var possibleTargetType = GetPieceTypeFromSquare(targetBit);
                ApplyMove(possibleMove);
                if (!IsKingUnderAttack())
                {
                    validMoves.Add(possibleMove);
                }
                UndoMove(possibleMove, possibleTargetType);
            }
        }
        return validMoves;
    }


    public List<Move> GenerateMoves()
    {
        var validMoves = new List<Move>();
        foreach (var (pieceType, idx, _) in GetAllPieces())
        {
            if (!IsCorrectColor(pieceType, _whiteToPlay)) continue;
            validMoves.AddRange(GenerateMovesFromIndex(idx));
        }
        return validMoves;
    }

    private void DumpBoardData()
    {
        Console.WriteLine("=== BOARD DUMP ===");
        Console.WriteLine($"{(_whiteToPlay ? "White" : "Black")} to play");
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