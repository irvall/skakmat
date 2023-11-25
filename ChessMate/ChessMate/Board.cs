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
    private ulong[] _blackPawnAttacks;
    private ulong[] _blackPawnMoves;
    private ulong[] _kingMoves;
    private ulong[] _knightMoves;

    private ulong[] _whitePawnAttacks;

    private ulong[] _whitePawnMoves;

    public Board()
    {
        _boardSquareToIndex = new Dictionary<string, int>();
        _indexToBoardSquare = new Dictionary<int, string>();
        var sw = new Stopwatch();
        sw.Start();
        InitializeBoardSquareToIndex();
        InitializePawnMoves();
        InitializeKingMoves();
        InitializeKnightMoves();
        var elapsedMilliseconds = sw.ElapsedMilliseconds;
        Console.WriteLine($"Populating moves in {elapsedMilliseconds}ms");
        for (var i = 0; i < 64; i++)
        {
            PrintBoard(_knightMoves[i], "Knight at ", i);
            PrintBoard(_whitePawnMoves[i], "White Pawn at ", i);
            PrintBoard(_blackPawnMoves[i], "Black Pawn at ", i);
            PrintBoard(_kingMoves[i], "King at ", i);
        }
    }

    private void TestKnightMoves()
    {
        /*PrintBoard(_knightMoves[(int)BoardSquare.A8], (int?)BoardSquare.A8);
        PrintBoard(_knightMoves[(int)BoardSquare.A1], (int?)BoardSquare.A1);
        PrintBoard(_knightMoves[(int)BoardSquare.H8], (int?)BoardSquare.H8);
        PrintBoard(_knightMoves[(int)BoardSquare.H1], (int?)BoardSquare.H1);
        PrintBoard(_knightMoves[(int)BoardSquare.D5], (int?)BoardSquare.D5);*/
    }

    private void TestKingMoves()
    {
        /*PrintBoard(_kingMoves[(int)BoardSquare.A8], (int?)BoardSquare.A8);
        PrintBoard(_kingMoves[(int)BoardSquare.A1], (int?)BoardSquare.A1);
        PrintBoard(_kingMoves[(int)BoardSquare.H8], (int?)BoardSquare.H8);
        PrintBoard(_kingMoves[(int)BoardSquare.H1], (int?)BoardSquare.H1);
        PrintBoard(_kingMoves[(int)BoardSquare.D5], (int?)BoardSquare.D5);*/
    }


    public void PrintBoardMasks()
    {
        var boardMasksType = typeof(Masks);

        foreach (var property in boardMasksType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic))
        {
            var propertyName = property.Name;
            var propertyValue = (ulong)property.GetValue(null);
            Console.WriteLine($"{propertyName}");
            PrintBoard(propertyValue);
        }
    }

    private void InitializeBoardSquareToIndex()
    {
        var index = 0;
        for (var i = 8; i >= 1; i--)
            for (var c = 'a'; c <= 'h'; c++)
            {
                var boardSquare = $"{c}{i}";
                _boardSquareToIndex.Add(boardSquare, index++);
            }

        foreach (var kvp in _boardSquareToIndex) _indexToBoardSquare[kvp.Value] = kvp.Key;
    }

    private void InitializeKingMoves()
    {
        _kingMoves = new ulong[64];
        foreach (var idx in _boardSquareToIndex.Values)
        {
            // TODO: Broken king
            var bit = 1UL << idx;
            if (!Masks.Rank8.Contains(bit))
                _kingMoves[idx] |= bit >> RankOffset;
            if (!Masks.Rank1.Contains(bit))
                _kingMoves[idx] |= bit << RankOffset;
            if (!Masks.FileH.Contains(bit))
                _kingMoves[idx] |= bit << FileOffset;
            if (!Masks.FileA.Contains(bit))
                _kingMoves[idx] |= bit >> FileOffset;
            if (!Masks.Boxes.A1G7.Contains(bit))
                _kingMoves[idx] |= bit >> DiagonalOffset;
            if (!Masks.Boxes.B2H8.Contains(bit))
                _kingMoves[idx] |= bit << DiagonalOffset;
            if (!Masks.Boxes.A2G8.Contains(bit))
                _kingMoves[idx] |= bit << AntiDiagonalOffset;
            if (!Masks.Boxes.B1H7.Contains(bit))
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


    private void PawnTests()
    {
        Console.WriteLine("MOVES");

        PrintBoard(_blackPawnMoves[(int)BoardSquare.E2]);
        PrintBoard(_blackPawnMoves[(int)BoardSquare.E7]);
        PrintBoard(_blackPawnMoves[(int)BoardSquare.A1]);
        PrintBoard(_blackPawnMoves[(int)BoardSquare.H8]);
        PrintBoard(_blackPawnMoves[(int)BoardSquare.D5]);

        Console.WriteLine("ATTACKS");

        PrintBoard(_blackPawnAttacks[(int)BoardSquare.E2]);
        PrintBoard(_blackPawnAttacks[(int)BoardSquare.E7]);
        PrintBoard(_blackPawnAttacks[(int)BoardSquare.A1]);
        PrintBoard(_blackPawnAttacks[(int)BoardSquare.H8]);
        PrintBoard(_blackPawnAttacks[(int)BoardSquare.D5]);
    }


    private void PrintBoard(ulong bitBoard, string? optional = null, int? optIndex = null)
    {
        if (optional != null)
        {
            var optMessage = optIndex is { } i and >= 0 and < 64 ? _indexToBoardSquare[i] : optional;
            LogUtility.WriteColor(optional + LogUtility.BoldText(optMessage), ConsoleColor.Green);
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