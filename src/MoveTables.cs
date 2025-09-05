using System.Diagnostics;
using System.Reflection;

namespace skakmat;

public partial class MoveTables
{
    private const int SquareNo = 8;
    private const int RankOffset = 8;
    private const int FileOffset = 1;
    private const int DiagonalOffset = 7;
    private const int AntiDiagonalOffset = 9;

    private readonly Dictionary<string, int> _boardSquareToIndex;
    private readonly Dictionary<int, string> _indexToBoardSquare;
    public ulong[] BlackPawnAttacks = new ulong[64];
    public ulong[] BlackPawnMoves = new ulong[64];
    public ulong[] KingMoves = new ulong[64];
    public ulong[] KnightMoves = new ulong[64];

    public ulong[] WhitePawnAttacks = new ulong[64];

    public ulong[] WhitePawnMoves = new ulong[64];

    public MoveTables()
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
        Console.WriteLine($"Precomputed moves in {elapsedMilliseconds}ms");
    }

    public static ulong RookAttacks(int square, ulong blockers)
    {
        var attacks = 0UL;
        var lowerY = square % 8;
        var upperY = square + (64 - ((square / 8 + 1) * 8));
        var lowerX = square - lowerY;
        var upperX = lowerX + 8;
        for (int s = square + FileOffset; s < upperX; s += FileOffset)
        {
            var bit = 1UL << s;
            if (blockers.Contains(bit)) break;
            attacks |= bit;
        }
        for (int s = square - FileOffset; s >= lowerX; s -= FileOffset)
        {
            var bit = 1UL << s;
            if (blockers.Contains(bit)) break;
            attacks |= bit;
        }

        for (int s = square + RankOffset; s <= upperY; s += RankOffset)
        {
            var bit = 1UL << s;
            if (blockers.Contains(bit)) break;
            attacks |= bit;
        }

        for (int s = square - RankOffset; s >= lowerY; s -= RankOffset)
        {
            var bit = 1UL << s;
            if (blockers.Contains(bit)) break;
            attacks |= bit;
        }

        return attacks;
    }

    public static ulong BishopAttacks(int square, ulong blockers)
    {
        var attacks = 0UL;
        var targetRank = square / 8;
        var targetFile = square % 8;

        int currentRank, currentFile;

        for (currentRank = targetRank + 1, currentFile = targetFile + 1; currentRank < SquareNo && currentFile < SquareNo; currentRank++, currentFile++)
        {
            var bit = 1UL << (currentRank * 8 + currentFile);
            if (blockers.Contains(bit)) break;
            attacks |= bit;
        }
        for (currentRank = targetRank + 1, currentFile = targetFile - 1; currentRank < SquareNo && currentFile >= 0; currentRank++, currentFile--)
        {
            var bit = 1UL << (currentRank * 8 + currentFile);
            if (blockers.Contains(bit)) break;
            attacks |= bit;
        }
        for (currentRank = targetRank - 1, currentFile = targetFile + 1; currentRank >= 0 && currentFile < SquareNo; currentRank--, currentFile++)
        {
            var bit = 1UL << (currentRank * 8 + currentFile);
            if (blockers.Contains(bit)) break;
            attacks |= bit;
        }
        for (currentRank = targetRank - 1, currentFile = targetFile - 1; currentRank >= 0 && currentFile >= 0; currentRank--, currentFile--)
        {
            var bit = 1UL << (currentRank * 8 + currentFile);
            if (blockers.Contains(bit)) break;
            attacks |= bit;
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
            Console.WriteLine($"{LogUtility.BoldText(propertyName)}\n");
            PrintBoard(propertyValue ?? 0UL);
            System.Console.WriteLine("///\n");
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
        KingMoves = new ulong[64];
        foreach (var idx in _boardSquareToIndex.Values)
        {
            var bit = 1UL << idx;
            if (!Masks.Rank8.Contains(bit))
                KingMoves[idx] |= bit >> RankOffset;
            if (!Masks.Rank1.Contains(bit))
                KingMoves[idx] |= bit << RankOffset;
            if (!Masks.FileH.Contains(bit))
                KingMoves[idx] |= bit << FileOffset;
            if (!Masks.FileA.Contains(bit))
                KingMoves[idx] |= bit >> FileOffset;
            if (Masks.Boxes.A1G7.Contains(bit))
                KingMoves[idx] |= bit >> DiagonalOffset;
            if (Masks.Boxes.B2H8.Contains(bit))
                KingMoves[idx] |= bit << DiagonalOffset;
            if (Masks.Boxes.A2G8.Contains(bit))
                KingMoves[idx] |= bit << AntiDiagonalOffset;
            if (Masks.Boxes.B1H7.Contains(bit))
                KingMoves[idx] |= bit >> AntiDiagonalOffset;
        }
    }

    private void InitializeKnightMoves()
    {
        KnightMoves = new ulong[64];
        foreach (var idx in _boardSquareToIndex.Values)
        {
            var bit = 1UL << idx;
            if (Masks.Boxes.A1G6.Contains(bit))
                KnightMoves[idx] |= bit >> (RankOffset * 2 - FileOffset);
            if (Masks.Boxes.B1H6.Contains(bit))
                KnightMoves[idx] |= bit >> (RankOffset * 2 + FileOffset);
            if (Masks.Boxes.A1F7.Contains(bit))
                KnightMoves[idx] |= bit >> (RankOffset - FileOffset * 2);
            if (Masks.Boxes.A2F8.Contains(bit))
                KnightMoves[idx] |= bit << (RankOffset + FileOffset * 2);
            if (Masks.Boxes.C1H7.Contains(bit))
                KnightMoves[idx] |= bit >> (RankOffset + FileOffset * 2);
            if (Masks.Boxes.C2H8.Contains(bit))
                KnightMoves[idx] |= bit << (RankOffset - FileOffset * 2);
            if (Masks.Boxes.B3H8.Contains(bit))
                KnightMoves[idx] |= bit << (RankOffset * 2 - FileOffset);
            if (Masks.Boxes.A3G8.Contains(bit))
                KnightMoves[idx] |= bit << (RankOffset * 2 + FileOffset);

        }
    }

    private void InitializePawnMoves()
    {
        WhitePawnMoves = new ulong[64];
        BlackPawnMoves = new ulong[64];
        WhitePawnAttacks = new ulong[64];
        BlackPawnAttacks = new ulong[64];

        foreach (var idx in _boardSquareToIndex.Values)
        {
            var bit = 1UL << idx;

            // Black Pawns
            if (Masks.Rank7.Contains(bit))
                BlackPawnMoves[idx] |= bit << (RankOffset * 2);
            if (!Masks.Rank1.Contains(bit))
            {
                BlackPawnMoves[idx] |= bit << RankOffset;

                if (!Masks.FileA.Contains(bit))
                    BlackPawnAttacks[idx] |= bit << DiagonalOffset;

                if (!Masks.FileH.Contains(bit))
                    BlackPawnAttacks[idx] |= bit << AntiDiagonalOffset;
            }

            // White Pawns
            if (Masks.Rank2.Contains(bit))
                WhitePawnMoves[idx] |= bit >> (RankOffset * 2);

            if (!Masks.Rank8.Contains(bit))
            {
                WhitePawnMoves[idx] |= bit >> RankOffset;

                if (!Masks.FileA.Contains(bit))
                    WhitePawnAttacks[idx] |= bit >> AntiDiagonalOffset;

                if (!Masks.FileH.Contains(bit))
                    WhitePawnAttacks[idx] |= bit >> DiagonalOffset;
            }
        }
    }

    public void PrintBoard(ulong bitBoard, string? optional = null, int? optIndex = null)
    {
        if (optional != null && optIndex != null)
        {
            LogUtility.WriteColor(LogUtility.BoldText(optional + " " + _indexToBoardSquare[optIndex.Value]), ConsoleColor.Green);
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
}