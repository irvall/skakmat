
using skakmat.Chess;
using skakmat.Utilities;

namespace skakmat.Game;
internal class MoveGenerator(MoveTables moveTables, Board board)
{
    internal ulong GetLegalPawnMoves(int index, BoardState state)
    {
        var moveBits = state.WhiteToPlay ? moveTables.WhitePawnMoves[index] : moveTables.BlackPawnMoves[index];
        var attackBits = state.WhiteToPlay ? moveTables.WhitePawnAttacks[index] : moveTables.BlackPawnAttacks[index];
        var startRow = state.WhiteToPlay ? Masks.Rank2 : Masks.Rank7;
        var firstMoveBlokingRow = state.WhiteToPlay ? Masks.Rank3 : Masks.Rank6;

        var enemyPieces = state.GetEnemyPieces();
        attackBits &= enemyPieces;
        if (startRow.Contains(1UL << index) && firstMoveBlokingRow.Contains(moveBits & state.AllPieces))
        {
            return attackBits;
        }
        return moveBits.Exclude(state.AllPieces) | attackBits;
    }

    private static bool IsPathClear(Castling.Type castleType, BoardState state)
    {
        var path = castleType == Castling.Type.KingSide
            ? Masks.KingSideCastlePath(state.WhiteToPlay)
            : Masks.QueenSideCastlePath(state.WhiteToPlay);

        return state.EmptySquares.ForAll(path);
    }

    private bool IsPathSafe(Castling.Type castleType, BoardState state)
    {
        var path = castleType == Castling.Type.KingSide
        ? Masks.KingSideCastlePath(state.WhiteToPlay)
        : Masks.QueenSideCastlePath(state.WhiteToPlay);

        var enemyControl = SquaresUnderControl(state, !state.WhiteToPlay);
        return !path.Contains(enemyControl);
    }

    private Move ValidateCastling(Move move, BoardState state)
    {
        if (Piece.GetPieceIndex(PieceType.King, state) != move.PieceIndex) return move;
        if (IsKingUnderAttack()) return move;

        var castlingType = Castling.GetCastlingType(state.WhiteToPlay, move.TargetBit, state);
        if (castlingType == Castling.Type.None)
            return move;
        if (!IsPathClear(castlingType, state))
            return move;

        if (!IsPathSafe(castlingType, state))
            return move;

        return move.TryCreateCastleMove(state.WhiteToPlay);
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

    private (bool isLegal, Move) GetLegalMove(Move move, ulong pseudoLegalMoves, BoardState state)
    {
        if (pseudoLegalMoves.Contains(move.TargetBit) && IsMoveValid(move))
        {
            return (true, move);
        }
        else if (ValidateCastling(move, state) is CastleMove castleMove && IsMoveValid(castleMove))
        {
            return (true, castleMove);
        }
        else if (CanTakeEnPassant(move, state) is EnPassantMove enPassantMove && IsMoveValid(enPassantMove))
        {
            return (true, enPassantMove);
        }
        return (false, move);
    }

    internal List<Move> GenerateMovesForSquare(int index, BoardState state)
    {
        var pieceIndex = state.GetPieceIndexAtIndex(index);
        var originBit = 1UL << index;
        var legalMoves = GetPseudoLegalMoves(pieceIndex, index, state);
        var validMoves = new List<Move>();
        foreach (var (_, targetBit) in BoardUtility.EnumerateSquares())
        {
            var possibleMove = new Move(pieceIndex, originBit, targetBit);
            var (ok, legalMove) = GetLegalMove(possibleMove, legalMoves, state);
            if (ok) validMoves.Add(legalMove);
        }
        return validMoves;
    }

    internal static Move CanTakeEnPassant(Move possibleMove, BoardState state)
    {
        if (possibleMove.PieceIndex != state.GetPieceIndex(PieceType.Pawn))
            return possibleMove;
        var prevMove = state.LastMovePlayed;
        if (prevMove is null)
            return possibleMove;
        if (!prevMove.IsPawnDoublePush())
            return possibleMove;
        if (!BoardUtility.AreHorizontalNeighbors(possibleMove.OriginBit, prevMove.TargetBit))
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
        var state = board.GetBoardState();
        foreach (var (pieceIndex, idx, _) in board.GetAllPieces())
        {
            if (!Piece.IsCorrectColor(pieceIndex, state.WhiteToPlay)) continue;
            validMoves.AddRange(GenerateMovesForSquare(idx, state));
        }
        return validMoves;
    }

    internal ulong SquaresUnderControl(BoardState state, bool isWhite)
    {
        var control = 0UL;
        foreach (var (type, idx, _) in state.GetAllPieces())
        {
            if (Piece.IsCorrectColor(type, isWhite))
            {
                control |= moveTables.GetPieceAttacks(type, idx, state);
            }
        }
        return control;
    }

    private ulong SquaresUnderWhiteControl() => SquaresUnderControl(board.GetBoardState(), true);
    private ulong SquaresUnderBlackControl() => SquaresUnderControl(board.GetBoardState(), false);

    internal ulong GetPseudoLegalMoves(int pieceIndex, int index, BoardState state)
    {
        return pieceIndex switch
        {
            Piece.WhitePawn => GetLegalPawnMoves(index, state),
            Piece.BlackPawn => GetLegalPawnMoves(index, state),
            Piece.WhiteBishop => MoveTables.BishopAttacks(index, state.AllPieces).Exclude(state.WhitePieces),
            Piece.BlackBishop => MoveTables.BishopAttacks(index, state.AllPieces).Exclude(state.BlackPieces),
            Piece.WhiteKnight => moveTables.KnightMoves[index].Exclude(state.WhitePieces),
            Piece.BlackKnight => moveTables.KnightMoves[index].Exclude(state.BlackPieces),
            Piece.WhiteRook => MoveTables.RookAttacks(index, state.AllPieces).Exclude(state.WhitePieces),
            Piece.BlackRook => MoveTables.RookAttacks(index, state.AllPieces).Exclude(state.BlackPieces),
            Piece.WhiteQueen => GetPseudoLegalMoves(Piece.WhiteRook, index, state) | GetPseudoLegalMoves(Piece.WhiteBishop, index, state),
            Piece.BlackQueen => GetPseudoLegalMoves(Piece.BlackRook, index, state) | GetPseudoLegalMoves(Piece.BlackBishop, index, state),
            Piece.WhiteKing => moveTables.KingMoves[index].Exclude(SquaresUnderBlackControl() | state.WhitePieces),
            Piece.BlackKing => moveTables.KingMoves[index].Exclude(SquaresUnderWhiteControl() | state.BlackPieces),
            _ => 0UL,
        };
    }

    internal bool IsKingUnderAttack(BoardState state)
    {
        var enemyControl = SquaresUnderControl(state, !state.WhiteToPlay);

        if (state.WhiteToPlay)
        {
            return state.Bitboards[Piece.WhiteKing].Contains(enemyControl);
        }
        else
        {
            return state.Bitboards[Piece.BlackKing].Contains(enemyControl);
        }
    }

    internal bool IsKingUnderAttack() => IsKingUnderAttack(board.GetBoardState());

}