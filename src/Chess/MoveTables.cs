using System.Reflection;
using skakmat.Extensions;
using skakmat.Game;
using skakmat.Helpers;

namespace skakmat.Chess;

internal partial class MoveTables
{
    internal const int rankOffset = 8;
    private const int fileOffset = 1;
    private const int diagonalOffset = 7;
    private const int antiDiagonalOffset = 9;

    private readonly Dictionary<string, int> boardSquareToIndex;
    private readonly Dictionary<int, string> indexToBoardSquare;
    internal ulong[] BlackPawnAttacks = new ulong[64];
    internal ulong[] BlackPawnMoves = new ulong[64];
    internal ulong[] KingMoves = new ulong[64];
    internal ulong[] KnightMoves = new ulong[64];

    internal ulong[] WhitePawnAttacks = new ulong[64];

    internal ulong[] WhitePawnMoves = new ulong[64];

    internal MoveTables()
    {
        boardSquareToIndex = [];
        indexToBoardSquare = [];
        InitializeBoardMaps();
        InitializePawnMoves();
        InitializeKingMoves();
        InitializeKnightMoves();
    }

    internal static ulong RookAttacks(int square, ulong blockers)
    {
        var attacks = 0UL;
        var lowerY = square % 8;
        var upperY = square + (64 - ((square / 8 + 1) * 8));
        var lowerX = square - lowerY;
        var upperX = lowerX + 8;
        for (int s = square + fileOffset; s < upperX; s += fileOffset)
        {
            var bit = 1UL << s;
            attacks |= bit;
            if (blockers.Contains(bit)) break;
        }
        for (int s = square - fileOffset; s >= lowerX; s -= fileOffset)
        {
            var bit = 1UL << s;
            attacks |= bit;
            if (blockers.Contains(bit)) break;
        }

        for (int s = square + rankOffset; s <= upperY; s += rankOffset)
        {
            var bit = 1UL << s;
            attacks |= bit;
            if (blockers.Contains(bit)) break;
        }

        for (int s = square - rankOffset; s >= lowerY; s -= rankOffset)
        {
            var bit = 1UL << s;
            attacks |= bit;
            if (blockers.Contains(bit)) break;
        }

        return attacks;
    }

    internal static ulong BishopAttacks(int square, ulong blockers)
    {
        var attacks = 0UL;
        var targetRank = square / 8;
        var targetFile = square % 8;

        int currentRank, currentFile;

        for (currentRank = targetRank + 1, currentFile = targetFile + 1; currentRank < squareNo && currentFile < squareNo; currentRank++, currentFile++)
        {
            var bit = 1UL << (currentRank * 8 + currentFile);
            attacks |= bit;
            if (blockers.Contains(bit)) break;
        }
        for (currentRank = targetRank + 1, currentFile = targetFile - 1; currentRank < squareNo && currentFile >= 0; currentRank++, currentFile--)
        {
            var bit = 1UL << (currentRank * 8 + currentFile);
            attacks |= bit;
            if (blockers.Contains(bit)) break;
        }
        for (currentRank = targetRank - 1, currentFile = targetFile + 1; currentRank >= 0 && currentFile < squareNo; currentRank--, currentFile++)
        {
            var bit = 1UL << (currentRank * 8 + currentFile);
            attacks |= bit;
            if (blockers.Contains(bit)) break;
        }
        for (currentRank = targetRank - 1, currentFile = targetFile - 1; currentRank >= 0 && currentFile >= 0; currentRank--, currentFile--)
        {
            var bit = 1UL << (currentRank * 8 + currentFile);
            attacks |= bit;
            if (blockers.Contains(bit)) break;
        }

        return attacks;
    }


    internal void PrintBoardMasks()
    {
        var boardMasksType = typeof(Masks);

        foreach (var property in boardMasksType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic))
        {
            var propertyName = property.Name;
            var propertyValue = (ulong?)property.GetValue(null);
            Console.WriteLine($"{RaylibHelper.BoldText(propertyName)}\n");
            PrintBoard(propertyValue ?? 0UL);
            Console.WriteLine("///\n");
        }
    }

    private void InitializeBoardMaps()
    {
        var index = 0;
        for (var i = 8; i >= 1; i--)
            for (var c = 'a'; c <= 'h'; c++)
            {
                var boardSquare = $"{c}{i}";
                boardSquareToIndex.Add(boardSquare, index++);
            }

        foreach (var kvp in boardSquareToIndex)
            indexToBoardSquare[kvp.Value] = kvp.Key;
    }

    private void InitializeKingMoves()
    {
        KingMoves = new ulong[64];
        foreach (var idx in boardSquareToIndex.Values)
        {
            var bit = 1UL << idx;
            if (!Masks.Rank8.Contains(bit))
                KingMoves[idx] |= bit >> rankOffset;
            if (!Masks.Rank1.Contains(bit))
                KingMoves[idx] |= bit << rankOffset;
            if (!Masks.FileH.Contains(bit))
                KingMoves[idx] |= bit << fileOffset;
            if (!Masks.FileA.Contains(bit))
                KingMoves[idx] |= bit >> fileOffset;
            if (Masks.Boxes.A1G7.Contains(bit))
                KingMoves[idx] |= bit >> diagonalOffset;
            if (Masks.Boxes.B2H8.Contains(bit))
                KingMoves[idx] |= bit << diagonalOffset;
            if (Masks.Boxes.A2G8.Contains(bit))
                KingMoves[idx] |= bit << antiDiagonalOffset;
            if (Masks.Boxes.B1H7.Contains(bit))
                KingMoves[idx] |= bit >> antiDiagonalOffset;
        }
    }

    private void InitializeKnightMoves()
    {
        KnightMoves = new ulong[64];
        foreach (var idx in boardSquareToIndex.Values)
        {
            var bit = 1UL << idx;
            if (Masks.Boxes.A1G6.Contains(bit))
                KnightMoves[idx] |= bit >> (rankOffset * 2 - fileOffset);
            if (Masks.Boxes.B1H6.Contains(bit))
                KnightMoves[idx] |= bit >> (rankOffset * 2 + fileOffset);
            if (Masks.Boxes.A1F7.Contains(bit))
                KnightMoves[idx] |= bit >> (rankOffset - fileOffset * 2);
            if (Masks.Boxes.A2F8.Contains(bit))
                KnightMoves[idx] |= bit << (rankOffset + fileOffset * 2);
            if (Masks.Boxes.C1H7.Contains(bit))
                KnightMoves[idx] |= bit >> (rankOffset + fileOffset * 2);
            if (Masks.Boxes.C2H8.Contains(bit))
                KnightMoves[idx] |= bit << (rankOffset - fileOffset * 2);
            if (Masks.Boxes.B3H8.Contains(bit))
                KnightMoves[idx] |= bit << (rankOffset * 2 - fileOffset);
            if (Masks.Boxes.A3G8.Contains(bit))
                KnightMoves[idx] |= bit << (rankOffset * 2 + fileOffset);

        }
    }

    private void InitializePawnMoves()
    {
        WhitePawnMoves = new ulong[64];
        BlackPawnMoves = new ulong[64];
        WhitePawnAttacks = new ulong[64];
        BlackPawnAttacks = new ulong[64];

        foreach (var idx in boardSquareToIndex.Values)
        {
            var bit = 1UL << idx;

            // Black Pawns
            if (Masks.Rank7.Contains(bit))
                BlackPawnMoves[idx] |= bit << (rankOffset * 2);
            if (!Masks.Rank1.Contains(bit))
            {
                BlackPawnMoves[idx] |= bit << rankOffset;

                if (!Masks.FileA.Contains(bit))
                    BlackPawnAttacks[idx] |= bit << diagonalOffset;

                if (!Masks.FileH.Contains(bit))
                    BlackPawnAttacks[idx] |= bit << antiDiagonalOffset;
            }

            // White Pawns
            if (Masks.Rank2.Contains(bit))
                WhitePawnMoves[idx] |= bit >> (rankOffset * 2);

            if (!Masks.Rank8.Contains(bit))
            {
                WhitePawnMoves[idx] |= bit >> rankOffset;

                if (!Masks.FileA.Contains(bit))
                    WhitePawnAttacks[idx] |= bit >> antiDiagonalOffset;

                if (!Masks.FileH.Contains(bit))
                    WhitePawnAttacks[idx] |= bit >> diagonalOffset;
            }
        }
    }

    internal void PrintBoard(ulong bitBoard, string? optional = null, int? optIndex = null)
    {
        if (optional != null && optIndex != null)
        {
            RaylibHelper.WriteColor(RaylibHelper.BoldText(optional + " " + indexToBoardSquare[optIndex.Value]), ConsoleColor.Green);
        }

        foreach (var (i, bit) in BoardHelper.EnumerateSquares())
        {
            if (i % 8 == 0) Console.Write($"{(i > 0 ? Environment.NewLine : string.Empty)}");
            var theBit = bit & bitBoard;
            if (theBit != 0)
                Console.Write(RaylibHelper.BoldText(indexToBoardSquare[i].PadLeft(3)));
            else
            {
                if (optIndex != null && i == optIndex)
                    RaylibHelper.WriteColor(indexToBoardSquare[i].PadLeft(3), ConsoleColor.Red, false);
                else RaylibHelper.WriteColor(indexToBoardSquare[i].PadLeft(3), ConsoleColor.DarkGray, false);
            }
        }

        Console.WriteLine(Environment.NewLine);
    }

    internal ulong GetPieceAttacks(int pieceIndex, int index, Position position) => pieceIndex switch
    {
        Piece.WhitePawn => WhitePawnAttacks[index],
        Piece.BlackPawn => BlackPawnAttacks[index],
        Piece.WhiteKnight => KnightMoves[index].Exclude(position.WhitePieces),
        Piece.BlackKnight => KnightMoves[index].Exclude(position.BlackPieces),
        Piece.WhiteBishop => BishopAttacks(index, position.AllPieces).Exclude(position.WhitePieces),
        Piece.BlackBishop => BishopAttacks(index, position.AllPieces).Exclude(position.BlackPieces),
        Piece.WhiteRook => RookAttacks(index, position.AllPieces).Exclude(position.WhitePieces),
        Piece.BlackRook => RookAttacks(index, position.AllPieces).Exclude(position.BlackPieces),
        Piece.WhiteQueen => GetPieceAttacks(Piece.WhiteRook, index, position) | GetPieceAttacks(Piece.WhiteBishop, index, position),
        Piece.BlackQueen => GetPieceAttacks(Piece.BlackRook, index, position) | GetPieceAttacks(Piece.BlackBishop, index, position),
        Piece.WhiteKing => KingMoves[index].Exclude(position.WhitePieces),
        Piece.BlackKing => KingMoves[index].Exclude(position.BlackPieces),
        _ => 0UL
    };

}
