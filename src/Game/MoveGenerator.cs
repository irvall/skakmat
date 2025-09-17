
using skakmat.Chess;
using skakmat.Extensions;
using skakmat.Helpers;

namespace skakmat.Game;
internal class MoveGenerator(MoveTables moveTables, Board board)
{
    internal ulong GetLegalPawnMoves(int index, Position position)
    {
        var moveBits = position.WhiteToPlay ? moveTables.WhitePawnMoves[index] : moveTables.BlackPawnMoves[index];
        var attackBits = position.WhiteToPlay ? moveTables.WhitePawnAttacks[index] : moveTables.BlackPawnAttacks[index];
        var startRow = position.WhiteToPlay ? Masks.Rank2 : Masks.Rank7;
        var firstMoveBlokingRow = position.WhiteToPlay ? Masks.Rank3 : Masks.Rank6;

        var enemyPieces = position.GetEnemyPieces();
        attackBits &= enemyPieces;
        if (startRow.Contains(1UL << index) && firstMoveBlokingRow.Contains(moveBits & position.AllPieces))
        {
            return attackBits;
        }
        return moveBits.Exclude(position.AllPieces) | attackBits;
    }

    private static bool IsPathClear(Castling.Type castleType, Position position)
    {
        var path = castleType == Castling.Type.KingSide
            ? Masks.KingSideCastlePath(position.WhiteToPlay)
            : Masks.QueenSideCastlePath(position.WhiteToPlay);

        return position.EmptySquares.ForAll(path);
    }

    private bool IsPathSafe(Castling.Type castleType, Position position)
    {
        var path = castleType == Castling.Type.KingSide
        ? Masks.KingSideCastlePath(position.WhiteToPlay)
        : Masks.QueenSideCastlePath(position.WhiteToPlay);

        var enemyControl = SquaresUnderControl(position, !position.WhiteToPlay);
        return !path.Contains(enemyControl);
    }

    private Move ValidateCastling(Move move, Position position)
    {
        if (Piece.GetPieceIndex(PieceType.King, position) != move.PieceIndex) return move;
        if (IsKingUnderAttack()) return move;

        var castlingType = Castling.GetCastlingType(position.WhiteToPlay, move.TargetBit, position);
        if (castlingType == Castling.Type.None)
            return move;
        if (!IsPathClear(castlingType, position))
            return move;

        if (!IsPathSafe(castlingType, position))
            return move;

        return move.TryCreateCastleMove(position.WhiteToPlay);
    }

    bool IsMoveValid(Move move)
    {
        var valid = false;
        var possibleTargetType = board.GetPieceIndexAt(move.TargetBit);
        board.ExecuteMove(move);
        if (!IsKingUnderAttack())
        {
            valid = true;
        }
        board.UndoMove(move, possibleTargetType);
        return valid;
    }

    private (bool isLegal, Move) GetLegalMove(Move move, ulong pseudoLegalMoves, Position position)
    {
        if (pseudoLegalMoves.Contains(move.TargetBit) && IsMoveValid(move))
        {
            return (true, move);
        }
        else if (ValidateCastling(move, position) is CastleMove castleMove && IsMoveValid(castleMove))
        {
            return (true, castleMove);
        }
        else if (CanTakeEnPassant(move, position) is EnPassantMove enPassantMove && IsMoveValid(enPassantMove))
        {
            return (true, enPassantMove);
        }
        return (false, move);
    }

    internal List<Move> GenerateMovesForSquare(int index, Position position)
    {
        var pieceIndex = position.GetPieceIndexAt(index);
        var originBit = 1UL << index;
        var legalMoves = GetPseudoLegalMoves(pieceIndex, index, position);
        var validMoves = new List<Move>();
        foreach (var (_, targetBit) in BoardHelper.EnumerateSquares())
        {
            var possibleMove = new Move(pieceIndex, originBit, targetBit);
            var (ok, legalMove) = GetLegalMove(possibleMove, legalMoves, position);
            if (ok) validMoves.Add(legalMove);
        }
        return validMoves;
    }

    internal static Move CanTakeEnPassant(Move possibleMove, Position position)
    {
        if (possibleMove.PieceIndex != position.GetPieceIndex(PieceType.Pawn))
            return possibleMove;
        var prevMove = position.LastMovePlayed;
        if (prevMove is null)
            return possibleMove;
        if (!prevMove.IsPawnDoublePush())
            return possibleMove;
        if (!BoardHelper.AreHorizontalNeighbors(possibleMove.OriginBit, prevMove.TargetBit))
            return possibleMove;
        var behindBit = possibleMove.IsWhite() ?
            prevMove.TargetBit >> MoveTables.RankOffset :
            prevMove.TargetBit << MoveTables.RankOffset;
        if (behindBit != possibleMove.TargetBit)
            return possibleMove;
        return new EnPassantMove(possibleMove, prevMove);
    }

    internal List<Move> GenerateMoves()
    {
        var validMoves = new List<Move>();
        var position = board.CreatePosition();
        foreach (var (pieceIndex, idx, _) in board.GetAllPieces())
        {
            if (!Piece.IsCorrectColor(pieceIndex, position.WhiteToPlay)) continue;
            validMoves.AddRange(GenerateMovesForSquare(idx, position));
        }
        return validMoves;
    }

    internal ulong SquaresUnderControl(Position position, bool isWhite)
    {
        var control = 0UL;
        foreach (var (type, idx, _) in position.GetAllPieces())
        {
            if (Piece.IsCorrectColor(type, isWhite))
            {
                control |= moveTables.GetPieceAttacks(type, idx, position);
            }
        }
        return control;
    }

    private ulong SquaresUnderWhiteControl() => SquaresUnderControl(board.CreatePosition(), true);
    private ulong SquaresUnderBlackControl() => SquaresUnderControl(board.CreatePosition(), false);

    internal ulong GetPseudoLegalMoves(int pieceIndex, int index, Position position)
    {
        return pieceIndex switch
        {
            Piece.WhitePawn => GetLegalPawnMoves(index, position),
            Piece.BlackPawn => GetLegalPawnMoves(index, position),
            Piece.WhiteBishop => MoveTables.BishopAttacks(index, position.AllPieces).Exclude(position.WhitePieces),
            Piece.BlackBishop => MoveTables.BishopAttacks(index, position.AllPieces).Exclude(position.BlackPieces),
            Piece.WhiteKnight => moveTables.KnightMoves[index].Exclude(position.WhitePieces),
            Piece.BlackKnight => moveTables.KnightMoves[index].Exclude(position.BlackPieces),
            Piece.WhiteRook => MoveTables.RookAttacks(index, position.AllPieces).Exclude(position.WhitePieces),
            Piece.BlackRook => MoveTables.RookAttacks(index, position.AllPieces).Exclude(position.BlackPieces),
            Piece.WhiteQueen => GetPseudoLegalMoves(Piece.WhiteRook, index, position) | GetPseudoLegalMoves(Piece.WhiteBishop, index, position),
            Piece.BlackQueen => GetPseudoLegalMoves(Piece.BlackRook, index, position) | GetPseudoLegalMoves(Piece.BlackBishop, index, position),
            Piece.WhiteKing => moveTables.KingMoves[index].Exclude(SquaresUnderBlackControl() | position.WhitePieces),
            Piece.BlackKing => moveTables.KingMoves[index].Exclude(SquaresUnderWhiteControl() | position.BlackPieces),
            _ => 0UL,
        };
    }

    internal bool IsKingUnderAttack(Position position)
    {
        var enemyControl = SquaresUnderControl(position, !position.WhiteToPlay);

        if (position.WhiteToPlay)
        {
            return position.Bitboards[Piece.WhiteKing].Contains(enemyControl);
        }
        else
        {
            return position.Bitboards[Piece.BlackKing].Contains(enemyControl);
        }
    }

    internal bool IsKingUnderAttack() => IsKingUnderAttack(board.CreatePosition());

}