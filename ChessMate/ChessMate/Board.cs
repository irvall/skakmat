using System.Diagnostics;
using System.Reflection;

namespace ChessMate;

public class BoardMasks
{
    public static ulong FileA { get; } = 0x0101010101010101;
    public static ulong FileB { get; } = 0x0202020202020202;
    public static ulong FileC { get; } = 0x0404040404040404;
    public static ulong FileD { get; } = 0x0808080808080808;
    public static ulong FileE { get; } = 0x1010101010101010;
    public static ulong FileF { get; } = 0x2020202020202020;
    public static ulong FileG { get; } = 0x4040404040404040;
    public static ulong FileH { get; } = 0x8080808080808080;
    public static ulong Rank1 { get; } = 0xFF00000000000000;
    public static ulong Rank2 { get; } = 0x00FF000000000000;
    public static ulong Rank3 { get; } = 0x0000FF0000000000;
    public static ulong Rank4 { get; } = 0x000000FF00000000;
    public static ulong Rank5 { get; } = 0x00000000FF000000;
    public static ulong Rank6 { get; } = 0x0000000000FF0000;
    public static ulong Rank7 { get; } = 0x000000000000FF00;
    public static ulong Rank8 { get; } = 0x00000000000000FF;
    public static ulong Center { get; } = 0x0000001818000000;
    public static ulong Corners { get; } = 0x8100000000000081;
    public static ulong CornersAndCenter { get; } = 0x8100001818000081;
    public static ulong CornersAndCenterAndAdjacent { get; } = 0xFF000018181800FF;

    public struct Boxes
    {
        public static ulong A1G7 { get; } = 0x7f7f7f7f7f7f7f00;
        public static ulong A2G8 { get; } = 0x7f7f7f7f7f7f7f;
        public static ulong B1H7 { get; } = 0xfefefefefefefe00;
        public static ulong B2H8 { get; } = 0xfefefefefefefe;

        public static ulong A1F6 = 0x3f3f3f3f3f3f0000;
        public static ulong A3F8 = 0x3f3f3f3f3f3f;
        public static ulong C3H8 = 0xfcfcfcfcfcfc;
        public static ulong C1H6 = 0xfcfcfcfcfcfc0000;
    }





}

public class Board
{
    private enum BoardSquare: byte {
        A8, B8, C8, D8, E8, F8, G8, H8,
        A7, B7, C7, D7, E7, F7, G7, H7,
        A6, B6, C6, D6, E6, F6, G6, H6,
        A5, B5, C5, D5, E5, F5, G5, H5,
        A4, B4, C4, D4, E4, F4, G4, H4,
        A3, B3, C3, D3, E3, F3, G3, H3,
        A2, B2, C2, D2, E2, F2, G2, H2,
        A1, B1, C1, D1, E1, F1, G1, H1
    }

    private readonly Dictionary<string, int> _boardSquareToIndex;

    private ulong[] _whitePawnMoves;
    private ulong[] _whiteKingMoves;
    private ulong[] _whitePawnAttacks;
    private ulong[] _blackPawnMoves;
    private ulong[] _blackPawnAttacks;
    
    private const int RankOffset = 8;
    private const int FileOffset = 1;
    private const int DiagonalOffset = 7;
    private const int AntiDiagonalOffset = 9;
    
    public void PrintBoardMasks()
    {
        Type boardMasksType = typeof(BoardMasks);

        foreach (PropertyInfo property in boardMasksType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic))
        {
            string propertyName = property.Name;
            ulong propertyValue = (ulong)property.GetValue(null);
            Console.WriteLine($"{propertyName}");
            PrintBoard(propertyValue);
        }
    }

    private void InitializeBoardSquareToIndex()
    {
        var index = 0;
        for (int i = 8; i >= 1; i--)
        {
            for (char c = 'a'; c <= 'h'; c++)
            {
                var boardSquare = $"{c}{i}";
                _boardSquareToIndex.Add(boardSquare, index++);
            }
        }
    }
    
    private void InitializeKingMoves()
    {
        _whiteKingMoves = new ulong[64];
        foreach (var idx in _boardSquareToIndex.Values)
        {
            var bit = 1UL << idx;
            if((bit & BoardMasks.Rank8) == 0)
                _whiteKingMoves[idx] |= bit >> RankOffset;
            if((bit & BoardMasks.Rank1) == 0)
                _whiteKingMoves[idx] |= bit << RankOffset;
            if((bit & BoardMasks.FileH) == 0)    
                _whiteKingMoves[idx] |= bit << FileOffset;
            if ((bit & BoardMasks.FileA) == 0)
                _whiteKingMoves[idx] |= bit >> FileOffset;
            if ((bit & BoardMasks.Boxes.A1G7) != 0)
                _whiteKingMoves[idx] |= bit >> DiagonalOffset;
            if((bit & BoardMasks.Boxes.B2H8) != 0)
                _whiteKingMoves[idx] |= bit << DiagonalOffset;
            if((bit & BoardMasks.Boxes.A2G8) != 0)
                _whiteKingMoves[idx] |= bit << AntiDiagonalOffset;
            if((bit & BoardMasks.Boxes.B1H7) != 0)
                _whiteKingMoves[idx] |= bit >> AntiDiagonalOffset;
        }
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
            if ((bit & BoardMasks.Rank7) != 0)
                _blackPawnMoves[idx] |= bit << (RankOffset * 2);
            if ((bit & BoardMasks.Rank1) == 0)
            {
                _blackPawnMoves[idx] |= bit << RankOffset;

                if ((bit & BoardMasks.FileA) == 0)
                    _blackPawnAttacks[idx] |= bit << DiagonalOffset;

                if ((bit & BoardMasks.FileH) == 0)
                    _blackPawnAttacks[idx] |= bit << AntiDiagonalOffset;
            }

            // White Pawns
            if ((bit & BoardMasks.Rank2) != 0)
                _whitePawnMoves[idx] |= bit >> (RankOffset*2);

            if ((bit & BoardMasks.Rank8) == 0)
            {
                _whitePawnMoves[idx] |= bit >> RankOffset;

                if ((bit & BoardMasks.FileA) == 0)
                    _whitePawnAttacks[idx] |= bit >> AntiDiagonalOffset;

                if ((bit & BoardMasks.FileH) == 0)
                    _whitePawnAttacks[idx] |= bit >> DiagonalOffset;
            }
        }
    }
    
    public Board()
    {
        _boardSquareToIndex = new Dictionary<string, int>();
        InitializeBoardSquareToIndex();
        InitializePawnMoves();
        InitializeKingMoves();
        if (_whiteKingMoves == null) return;
        PrintBoard(_whiteKingMoves[(int)BoardSquare.A8], (int?)BoardSquare.A8);
        PrintBoard(_whiteKingMoves[(int)BoardSquare.A1], (int?)BoardSquare.A1);
        PrintBoard(_whiteKingMoves[(int)BoardSquare.H8], (int?)BoardSquare.H8);
        PrintBoard(_whiteKingMoves[(int)BoardSquare.H1], (int?)BoardSquare.H1);
        PrintBoard(_whiteKingMoves[(int)BoardSquare.D5], (int?)BoardSquare.D5);
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

    public void PrintBoard(ulong bitboard, int? optIndexToHighlight = null)
    {
        var row = 8;
        for (int i = 0; i < 64; i++)
        {
            if (i % 8 == 0)
            {
                Console.Write($"{row--} | "); 
            }
            var bit = 1UL << i;
            var bitboardValue = bitboard & bit;
            var value = bitboardValue > 0 ? 1 : 0;
            if(optIndexToHighlight != null && i == optIndexToHighlight)
                Console.Write($"x ");
            else
                Console.Write($"{value} ");
            if (i % 8 == 7)
            {
                Console.WriteLine();
            }
        }
        Console.WriteLine("    a b c d e f g h");
        Console.WriteLine();
    }



}