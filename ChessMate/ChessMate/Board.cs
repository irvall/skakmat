using System.Diagnostics;
using System.Reflection;

namespace ChessMate;

public class Board
{
    private const int SquareNo = 8;
    private const int RankOffset = 8;
    private const int FileOffset = 1;
    private const int DiagonalOffset = 7;
    private const int AntiDiagonalOffset = 9;

    private readonly Dictionary<string, int> _boardSquareToIndex;
    private readonly Dictionary<int, string> _indexToBoardSquare;
    private ulong[] _blackPawnAttacks = new ulong[64];
    private ulong[] _blackPawnMoves = new ulong[64];
    private ulong[] _kingMoves = new ulong[64];
    private ulong[] _knightMoves = new ulong[64];

    private ulong[] _whitePawnAttacks = new ulong[64];

    private ulong[] _whitePawnMoves = new ulong[64];
    private readonly ulong[][] rayAttacks = new ulong[64][];

    public Board()
    {
        _boardSquareToIndex = [];
        _indexToBoardSquare = [];
        var sw = new Stopwatch();
        sw.Start();
        InitializeBoardMaps();
        InitializePawnMoves();
        InitializeKingMoves();
        InitializeKnightMoves();
        var elapsedMilliseconds = sw.ElapsedMilliseconds;
        Console.WriteLine($"Populating moves in {elapsedMilliseconds}ms");
    }

    private static ulong BishopAttacks(int square)
    {
        var attacks = 0UL;
        var directions = new[] { DiagonalOffset, -DiagonalOffset, AntiDiagonalOffset, -AntiDiagonalOffset };
        foreach (var direction in directions)
        {
            var targetSquare = square + direction;
            var targetBit = 1UL << targetSquare;
            while (!Masks.Edge.Contains(targetBit))
            {
                attacks |= targetBit;
                targetSquare += direction;
                targetBit = 1UL << targetSquare;
            }
        }

        return attacks;
    }

    private static ulong RookAttacks(int square)
    {
        var attacks = 0UL;
        var directions = new[] { RankOffset, -RankOffset, FileOffset, -FileOffset };
        var bit = 1UL << square;
        foreach (var direction in directions)
        {
            var targetSquare = square + direction;
            var targetBit = 1UL << targetSquare;
            var theEdge = Masks.Edge;
            if (Masks.Edge.Contains(bit))
            {
                // If the bit is on the edge, we need to remove the edge from the attack mask, but not the corners
                if (Masks.FileA.Contains(bit)) theEdge &= ~Masks.FileA | Masks.Corners;
                if (Masks.FileH.Contains(bit)) theEdge &= ~Masks.FileH | Masks.Corners;
                if (Masks.Rank1.Contains(bit)) theEdge &= ~Masks.Rank1 | Masks.Corners;
                if (Masks.Rank8.Contains(bit)) theEdge &= ~Masks.Rank8 | Masks.Corners;
            }
            while (!theEdge.Contains(targetBit))
            {
                attacks |= targetBit;
                targetSquare += direction;
                targetBit = 1UL << targetSquare;
            }
        }

        return attacks;
    }

    public void PrintBoardMasks()
    {
        var boardMasksType = typeof(Masks);

        foreach (var property in boardMasksType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic))
        {
            var propertyName = property.Name;
            var propertyValue = (ulong?)property.GetValue(null);
            Console.WriteLine($"{propertyName}");
            PrintBoard(propertyValue ?? 0UL);
        }
    }

    private void InitializeBoardMaps()
    {
        var index = 0;
        for (var i = 8; i >= 1; i--)
            for (var c = 'a'; c <= 'h'; c++)
            {
                var boardSquare = $"{c}{i}";
                _boardSquareToIndex.Add(boardSquare, index++);
            }

        foreach (var kvp in _boardSquareToIndex)
            _indexToBoardSquare[kvp.Value] = kvp.Key;
    }

    private void InitializeKingMoves()
    {
        _kingMoves = new ulong[64];
        foreach (var idx in _boardSquareToIndex.Values)
        {
            var bit = 1UL << idx;
            if (!Masks.Rank8.Contains(bit))
                _kingMoves[idx] |= bit >> RankOffset;
            if (!Masks.Rank1.Contains(bit))
                _kingMoves[idx] |= bit << RankOffset;
            if (!Masks.FileH.Contains(bit))
                _kingMoves[idx] |= bit << FileOffset;
            if (!Masks.FileA.Contains(bit))
                _kingMoves[idx] |= bit >> FileOffset;
            if (Masks.Boxes.A1G7.Contains(bit))
                _kingMoves[idx] |= bit >> DiagonalOffset;
            if (Masks.Boxes.B2H8.Contains(bit))
                _kingMoves[idx] |= bit << DiagonalOffset;
            if (Masks.Boxes.A2G8.Contains(bit))
                _kingMoves[idx] |= bit << AntiDiagonalOffset;
            if (Masks.Boxes.B1H7.Contains(bit))
                _kingMoves[idx] |= bit >> AntiDiagonalOffset;
        }
    }

    private void InitializeKnightMoves()
    {
        _knightMoves = new ulong[64];
        foreach (var idx in _boardSquareToIndex.Values)
        {
            var bit = 1UL << idx;
            if (Masks.Boxes.A1G6.Contains(bit))
                _knightMoves[idx] |= bit >> (RankOffset * 2 - FileOffset);
            if (Masks.Boxes.B1H6.Contains(bit))
                _knightMoves[idx] |= bit >> (RankOffset * 2 + FileOffset);
            if (Masks.Boxes.A1F7.Contains(bit))
                _knightMoves[idx] |= bit >> (RankOffset - FileOffset * 2);
            if (Masks.Boxes.A2F8.Contains(bit))
                _knightMoves[idx] |= bit << (RankOffset + FileOffset * 2);
            if (Masks.Boxes.C1H7.Contains(bit))
                _knightMoves[idx] |= bit >> (RankOffset + FileOffset * 2);
            if (Masks.Boxes.C2H8.Contains(bit))
                _knightMoves[idx] |= bit << (RankOffset - FileOffset * 2);
            if (Masks.Boxes.B3H8.Contains(bit))
                _knightMoves[idx] |= bit << (RankOffset * 2 - FileOffset);
            if (Masks.Boxes.A3G8.Contains(bit))
                _knightMoves[idx] |= bit << (RankOffset * 2 + FileOffset);

        }
    }

    private static ulong MoveBit(ulong bitboard, int x, int y)
    {
        var xOffset = Math.Abs(x) * FileOffset;
        var yOffset = Math.Abs(y) * RankOffset;
        var shiftedX = x > 0 ? bitboard << xOffset : bitboard >> xOffset;
        var shiftedY = y > 0 ? shiftedX << yOffset : shiftedX >> yOffset;
        return shiftedY;
    }

    private void InitializePawnMoves()
    {
        _whitePawnMoves = new ulong[64];
        _blackPawnMoves = new ulong[64];
        _whitePawnAttacks = new ulong[64];
        _blackPawnAttacks = new ulong[64];

        foreach (var idx in _boardSquareToIndex.Values)
        {
            var bit = 1UL << idx;

            // Black Pawns
            if (Masks.Rank7.Contains(bit))
                _blackPawnMoves[idx] |= bit << (RankOffset * 2);
            if (!Masks.Rank1.Contains(bit))
            {
                _blackPawnMoves[idx] |= bit << RankOffset;

                if (!Masks.FileA.Contains(bit))
                    _blackPawnAttacks[idx] |= bit << DiagonalOffset;

                if (!Masks.FileH.Contains(bit))
                    _blackPawnAttacks[idx] |= bit << AntiDiagonalOffset;
            }

            // White Pawns
            if (Masks.Rank2.Contains(bit))
                _whitePawnMoves[idx] |= bit >> (RankOffset * 2);

            if (!Masks.Rank8.Contains(bit))
            {
                _whitePawnMoves[idx] |= bit >> RankOffset;

                if (!Masks.FileA.Contains(bit))
                    _whitePawnAttacks[idx] |= bit >> AntiDiagonalOffset;

                if (!Masks.FileH.Contains(bit))
                    _whitePawnAttacks[idx] |= bit >> DiagonalOffset;
            }
        }
    }

    private void PrintBoard(ulong bitBoard, string? optional = null, int? optIndex = null)
    {
        if (optional != null)
        {
            var optMessage = optIndex is { } i and >= 0 and < 64 ? _indexToBoardSquare[i] : optional;
            LogUtility.WriteColor(LogUtility.BoldText(optMessage), ConsoleColor.Green);
        }

        for (var i = 0; i < 64; i++)
        {
            if (i % 8 == 0) Console.Write($"{(i > 0 ? Environment.NewLine : string.Empty)}");
            var theBit = (1UL << i) & bitBoard;
            if (theBit != 0)
                Console.Write(LogUtility.BoldText(_indexToBoardSquare[i].PadLeft(3)));
            else
            {
                if (optIndex != null && i == optIndex)
                    LogUtility.WriteColor(_indexToBoardSquare[i].PadLeft(3), ConsoleColor.Red, false);
                else LogUtility.WriteColor(_indexToBoardSquare[i].PadLeft(3), ConsoleColor.DarkGray, false);
            }
        }

        Console.WriteLine(Environment.NewLine);
    }

    private enum BoardSquare : byte
    {
        A8, B8, C8, D8, E8, F8, G8, H8,
        A7, B7, C7, D7, E7, F7, G7, H7,
        A6, B6, C6, D6, E6, F6, G6, H6,
        A5, B5, C5, D5, E5, F5, G5, H5,
        A4, B4, C4, D4, E4, F4, G4, H4,
        A3, B3, C3, D3, E3, F3, G3, H3,
        A2, B2, C2, D2, E2, F2, G2, H2,
        A1, B1, C1, D1, E1, F1, G1, H1
    }
}