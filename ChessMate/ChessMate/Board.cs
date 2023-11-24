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
    


}

public class Board
{
    private enum BoardSquare: byte {
        a8, b8, c8, d8, e8, f8, g8, h8,
        a7, b7, c7, d7, e7, f7, g7, h7,
        a6, b6, c6, d6, e6, f6, g6, h6,
        a5, b5, c5, d5, e5, f5, g5, h5,
        a4, b4, c4, d4, e4, f4, g4, h4,
        a3, b3, c3, d3, e3, f3, g3, h3,
        a2, b2, c2, d2, e2, f2, g2, h2,
        a1, b1, c1, d1, e1, f1, g1, h1
    }

    private readonly Dictionary<string, int> _boardSquareToIndex;

    private ulong[] _whitePawnMoves;
    private ulong[] _whiteKingMoves;
    private ulong[] _whitePawnAttacks;
    private ulong[] _blackPawnMoves;
    private ulong[] _blackPawnAttacks;
    
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
                _blackPawnMoves[idx] |= bit << 16;
            if ((bit & BoardMasks.Rank1) == 0)
            {
                _blackPawnMoves[idx] |= bit << 8;

                if ((bit & BoardMasks.FileA) == 0)
                    _blackPawnAttacks[idx] |= bit << 7;

                if ((bit & BoardMasks.FileH) == 0)
                    _blackPawnAttacks[idx] |= bit << 9;
            }

            // White Pawns
            if ((bit & BoardMasks.Rank2) != 0)
                _whitePawnMoves[idx] |= bit >> 16;

            if ((bit & BoardMasks.Rank8) == 0)
            {
                _whitePawnMoves[idx] |= bit >> 8;

                if ((bit & BoardMasks.FileA) == 0)
                    _whitePawnAttacks[idx] |= bit >> 9;

                if ((bit & BoardMasks.FileH) == 0)
                    _whitePawnAttacks[idx] |= bit >> 7;
            }
        }
    }
    
    public Board()
    {
        _boardSquareToIndex = new Dictionary<string, int>();
        InitializeBoardSquareToIndex();
        InitializePawnMoves();
        if (_whitePawnMoves == null || _whitePawnAttacks == null || _blackPawnMoves == null ||
            _blackPawnAttacks == null) return;
        
        Console.WriteLine("MOVES");
        
        PrintBoard(_blackPawnMoves[(int)BoardSquare.e2]);
        PrintBoard(_blackPawnMoves[(int)BoardSquare.e7]);
        PrintBoard(_blackPawnMoves[(int)BoardSquare.a1]);
        PrintBoard(_blackPawnMoves[(int)BoardSquare.h8]);
        PrintBoard(_blackPawnMoves[(int)BoardSquare.d5]);

        Console.WriteLine("ATTACKS");
        
        PrintBoard(_blackPawnAttacks[(int)BoardSquare.e2]);
        PrintBoard(_blackPawnAttacks[(int)BoardSquare.e7]);
        PrintBoard(_blackPawnAttacks[(int)BoardSquare.a1]);
        PrintBoard(_blackPawnAttacks[(int)BoardSquare.h8]);
        PrintBoard(_blackPawnAttacks[(int)BoardSquare.d5]);
        
    }
    
    public void PrintBoard(ulong bitboard)
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