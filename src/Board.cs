using System.Numerics;
using Raylib_cs;

namespace skakmat;

class Board
{

    readonly ulong[] bitboards;

    internal ulong WhitePieces =>
        bitboards[WhitePawn]
        | bitboards[WhiteRook]
        | bitboards[WhiteKnight]
        | bitboards[WhiteBishop]
        | bitboards[WhiteQueen]
        | bitboards[WhiteKing];

    internal ulong BlackPieces =>
        bitboards[BlackPawn]
        | bitboards[BlackRook]
        | bitboards[BlackKnight]
        | bitboards[BlackBishop]
        | bitboards[BlackQueen]
        | bitboards[BlackKing];

    internal ulong AllPieces => WhitePieces | BlackPieces;

    // Indices
    internal const int WhitePawn = 0;
    internal const int WhiteRook = 1;
    internal const int WhiteKnight = 2;
    internal const int WhiteBishop = 3;
    internal const int WhiteQueen = 4;
    internal const int WhiteKing = 5;
    internal const int BlackPawn = 6;
    internal const int BlackRook = 7;
    internal const int BlackKnight = 8;
    internal const int BlackBishop = 9;
    internal const int BlackQueen = 10;
    internal const int BlackKing = 11;

    private const int SquareCount = 8;
    private readonly int _halfSideLength;
    private readonly int _sideLength;
    private readonly Vector2 _upperBounds;
    private readonly (int width, int height) _windowSize;

    private readonly Dictionary<string, int> _boardSquareToIndex;
    private readonly Dictionary<int, string> _indexToBoardSquare;

    private Texture2D _spriteTexture;
    private readonly MoveTables moveTables;
    private PieceSelection? pieceSelected;

    public Dictionary<PieceType, (int, int)> PieceToSpriteCoords = new()
    {
        { PieceType.WhitePawn, (5, 0) },
        { PieceType.WhiteKnight, (3, 0) },
        { PieceType.WhiteBishop, (2, 0) },
        { PieceType.WhiteRook, (4, 0) },
        { PieceType.WhiteQueen, (1, 0) },
        { PieceType.WhiteKing, (0, 0) },
        { PieceType.BlackPawn, (5, 1) },
        { PieceType.BlackKnight, (3, 1) },
        { PieceType.BlackBishop, (2, 1) },
        { PieceType.BlackRook, (4, 1) },
        { PieceType.BlackQueen, (1, 1) },
        { PieceType.BlackKing, (0, 1) },
    };

    public Board()
    {
        bitboards = new ulong[12];
        SetupStandardBoard(bitboards);
        moveTables = new MoveTables();
        _boardSquareToIndex = [];
        _indexToBoardSquare = [];

        int windowHeight = GetWindowHeightDynamically();
        _spriteTexture = LoadTextureChecked("assets/sprite.png");

        _sideLength = windowHeight / SquareCount;
        _halfSideLength = _sideLength / 2;
        _windowSize.width = windowHeight + _sideLength;
        _windowSize.height = windowHeight + _sideLength;
        _upperBounds = new Vector2(SquareCount - 1);

        InitializeBoardMaps();
    }

    public void Run()
    {
        DrawWindow();
    }

    private static void SetupStandardBoard(ulong[] bbs)
    {
        bbs[WhitePawn] = Masks.Rank2;
        bbs[WhiteRook] = 0x8100000000000000;
        bbs[WhiteKnight] = 0x4200000000000000;
        bbs[WhiteBishop] = 0x2400000000000000;
        bbs[WhiteQueen] = 0x800000000000000;
        bbs[WhiteKing] = 0x1000000000000000;

        bbs[BlackPawn] = Masks.Rank7;
        bbs[BlackKnight] = 0x42;
        bbs[BlackBishop] = 0x24;
        bbs[BlackRook] = 0x81;
        bbs[BlackQueen] = 0x8;
        bbs[BlackKing] = 0x10;
    }

    private PieceType? GetPieceTypeFromSquare(ulong square)
    {
        if (bitboards[WhitePawn].Contains(square)) return PieceType.WhitePawn;
        if (bitboards[BlackPawn].Contains(square)) return PieceType.BlackPawn;
        if (bitboards[WhiteRook].Contains(square)) return PieceType.WhiteRook;
        if (bitboards[BlackRook].Contains(square)) return PieceType.BlackRook;
        if (bitboards[WhiteKnight].Contains(square)) return PieceType.WhiteKnight;
        if (bitboards[BlackKnight].Contains(square)) return PieceType.BlackKnight;
        if (bitboards[WhiteBishop].Contains(square)) return PieceType.WhiteBishop;
        if (bitboards[BlackBishop].Contains(square)) return PieceType.BlackBishop;
        if (bitboards[WhiteQueen].Contains(square)) return PieceType.WhiteQueen;
        if (bitboards[BlackQueen].Contains(square)) return PieceType.BlackQueen;
        if (bitboards[WhiteKing].Contains(square)) return PieceType.WhiteKing;
        if (bitboards[BlackKing].Contains(square)) return PieceType.BlackKing;
        return null;
    }

    private void DrawPieces()
    {
        for (var sq = 0; sq < 64; sq++)
        {
            var type = GetPieceTypeFromSquare(1UL << sq);
            if (!type.HasValue) continue;
            DrawPiece(sq % 8, sq / 8, type.Value);
        }
    }


    private static int GetWindowHeightDynamically()
    {
        Raylib.InitWindow(0, 0, "Temporary 0x0 window to get screen size");
        var windowHeight = (int)(Raylib.GetScreenHeight() / 1.5);
        Raylib.CloseWindow();
        return windowHeight;
    }

    public static Texture2D LoadTextureChecked(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Asset not found: {path}");
        return Raylib.LoadTexture(path);
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

    private void DrawWindow()
    {
        Raylib.InitWindow(_windowSize.width, _windowSize.height, "Skakmat");
        _spriteTexture = LoadTextureChecked("assets/sprite.png");
        var bgColor = new Color(4, 15, 15, 1);
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(bgColor);
            DrawBoard();
            HandleInteractions();
            DrawPieces();

            if (pieceSelected.HasValue)
            {
                HighlightSquares(pieceSelected.Value.LegalMoves, Color.GREEN);
            }
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    private void DrawBoard()
    {
        var primaryTileColor = Palette.FromHex("C7D59F");
        var whiteTiles = Palette.WhiteVersion(primaryTileColor);
        for (var i = 0; i < SquareCount; i++)
            for (var j = 0; j < SquareCount; j++)
            {
                if (j == 0)
                {
                    var posX = _halfSideLength / 3;
                    var posY = (int)(_sideLength * .85);
                    Raylib.DrawText(8 - i + "", posX, i * _sideLength + posY, _halfSideLength, Color.WHITE);
                }

                var draw = (i + j) % 2 != 0;
                DrawTile(j, i, draw ? primaryTileColor : whiteTiles);
            }

        for (var c = 'A'; c <= 'H'; c++)
        {
            var posX = (int)(_sideLength * .85);
            var posY = _windowSize.height - _halfSideLength;
            Raylib.DrawText(c + "", (c - 'A') * _sideLength + posX, posY, _halfSideLength, Color.WHITE);
        }

    }

    private void DrawPiece(int col, int row, PieceType pieceType)
    {
        int cellWidth = _spriteTexture.Width / 6;  // 6 columns now
        int cellHeight = _spriteTexture.Height / 2; // 2 rows (white/black)

        if (!PieceToSpriteCoords.TryGetValue(pieceType, out var tup))
            throw new Exception("Piece type not supported " + pieceType);

        var (pieceIndexX, pieceIndexY) = tup;
        var src = new Rectangle(
            pieceIndexX * cellWidth,
            pieceIndexY * cellHeight,
            cellWidth,
            cellHeight
        );

        var dest = new Rectangle(
            col * _sideLength + _halfSideLength,
            row * _sideLength + _halfSideLength,
            _sideLength,
            _sideLength
        );

        Raylib.DrawTexturePro(_spriteTexture, src, dest, Vector2.Zero, 0f, Color.WHITE);
    }

    private void DrawTile(Vector2 tilePosition, Color tileColor, double alpha = 1.0)
    {
        DrawTile((int)tilePosition.X, (int)tilePosition.Y, tileColor, alpha);
    }

    private void DrawTile(int col, int row, Color tileColor, double alpha = 1.0)
    {
        var posX = col * _sideLength + _halfSideLength;
        var posY = row * _sideLength + _halfSideLength;
        tileColor.A = (byte)(255.0 * alpha);
        Raylib.DrawRectangle(posX, posY, _sideLength, _sideLength, tileColor);
    }

    private void HighlightSquares(ulong legalMoves, Color color)
    {
        for (var idx = 63; idx >= 0; idx--)
        {
            var bit = 1UL << idx;
            if (legalMoves.Contains(bit))
            {
                DrawTile(idx % 8, idx / 8, color, 0.5f);
            }
        }
    }

    private static (int, ulong) IndexAndBitUnderMouse(Vector2 mousePosition)
    {
        var idx = (int)(mousePosition.X + mousePosition.Y * 8);
        return (idx, 1UL << idx);
    }

    private PieceSelection HandlePawnMove(int index, ulong bit, bool isWhite)
    {
        var moveBits = isWhite ? moveTables.WhitePawnMoves[index] : moveTables.BlackPawnMoves[index];
        var startRow = isWhite ? Masks.Rank2 : Masks.Rank7;
        var firstMoveBlokingRow = isWhite ? Masks.Rank3 : Masks.Rank6;
        if (startRow.Contains(bit))
        {
            // It's the first move of pawn, check for blocking pieces
            if (firstMoveBlokingRow.Contains(moveBits & AllPieces))
            {
                moveBits = 0;
            }

            // TODO: Attacks are ok, despite blocking piece
        }
        return new PieceSelection(bit, moveBits & ~AllPieces, isWhite ? PieceType.WhitePawn : PieceType.BlackPawn, isWhite ? WhitePawn : BlackPawn, index);
    }

    private PieceSelection? DetectPieceSelection(Vector2 mouseGridPos)
    {
        var (idx, bit) = IndexAndBitUnderMouse(mouseGridPos);
        if (bitboards[WhitePawn].Contains(bit))
        {
            return HandlePawnMove(idx, bit, true);
        }
        if (bitboards[BlackPawn].Contains(bit))
        {
            return HandlePawnMove(idx, bit, false);
        }
        if (bitboards[WhiteKnight].Contains(bit))
        {
            var moveBits = moveTables.KnightMoves[idx] & ~WhitePieces;
            return new PieceSelection(bit, moveBits, PieceType.WhiteKnight, WhiteKnight, idx);
        }
        if (bitboards[BlackKnight].Contains(bit))
        {
            var moveBits = moveTables.KnightMoves[idx] & ~BlackPieces;
            return new PieceSelection(bit, moveBits, PieceType.BlackKnight, BlackKnight, idx);
        }

        if (bitboards[WhiteBishop].Contains(bit))
        {
            var moveBits = MoveTables.BishopAttacks(idx, AllPieces);
            return new PieceSelection(bit, moveBits, PieceType.WhiteBishop, WhiteBishop, idx);
        }
        if (bitboards[BlackBishop].Contains(bit))
        {
            var moveBits = MoveTables.BishopAttacks(idx, AllPieces);
            return new PieceSelection(bit, moveBits, PieceType.BlackBishop, BlackBishop, idx);
        }
        if (bitboards[WhiteRook].Contains(bit))
        {
            var moveBits = MoveTables.RookAttacks(idx, AllPieces);
            return new PieceSelection(bit, moveBits, PieceType.WhiteRook, WhiteRook, idx);
        }
        if (bitboards[BlackRook].Contains(bit))
        {
            var moveBits = MoveTables.RookAttacks(idx, AllPieces);
            return new PieceSelection(bit, moveBits, PieceType.BlackRook, BlackRook, idx);
        }
        if (bitboards[WhiteQueen].Contains(bit))
        {
            var moveBits = MoveTables.RookAttacks(idx, AllPieces) | MoveTables.BishopAttacks(idx, AllPieces);
            return new PieceSelection(bit, moveBits, PieceType.WhiteQueen, WhiteQueen, idx);
        }
        if (bitboards[BlackQueen].Contains(bit))
        {
            var moveBits = MoveTables.RookAttacks(idx, AllPieces) | MoveTables.BishopAttacks(idx, AllPieces);
            return new PieceSelection(bit, moveBits, PieceType.BlackQueen, BlackQueen, idx);
        }
        if (bitboards[BlackKing].Contains(bit))
        {
            var moveBits = moveTables.KingMoves[idx] & ~AllPieces;
            return new PieceSelection(bit, moveBits, PieceType.BlackKing, BlackKing, idx);
        }

        if (bitboards[WhiteKing].Contains(bit))
        {
            var moveBits = moveTables.KingMoves[idx] & ~AllPieces;
            return new PieceSelection(bit, moveBits, PieceType.WhiteKing, WhiteKing, idx);
        }
        if (bitboards[BlackKing].Contains(bit))
        {
            var moveBits = moveTables.KingMoves[idx] & ~AllPieces;
            return new PieceSelection(bit, moveBits, PieceType.BlackKing, BlackKing, idx);
        }
        return null;
    }

    private enum InteractionResult
    {
        None,
        MoveMade,
        SwitchedPiece,
        Deselected
    }

    private InteractionResult CheckPiece(Vector2 mouseGridPos)
    {
        var (_, bit) = IndexAndBitUnderMouse(mouseGridPos);

        if (!pieceSelected.HasValue)
            return InteractionResult.None;

        if (pieceSelected.Value.LegalMoves.Contains(bit))
        {
            bitboards[pieceSelected.Value.PieceIndex] ^= pieceSelected.Value.Bit;
            bitboards[pieceSelected.Value.PieceIndex] |= bit;
            pieceSelected = null;
            return InteractionResult.MoveMade;
        }

        var maybeNewSelection = DetectPieceSelection(mouseGridPos);
        if (maybeNewSelection.HasValue)
        {
            pieceSelected = maybeNewSelection;
            return InteractionResult.SwitchedPiece;
        }
        pieceSelected = null;
        return InteractionResult.Deselected;
    }

    private void HandleInteractions()
    {
        var mouseScreenPos = Raylib.GetMousePosition();
        var mouseGridPos = ScreenToGrid((int)mouseScreenPos.X, (int)mouseScreenPos.Y);
        var isMouseOnBoard = mouseGridPos is { X: >= 0 and < SquareCount, Y: >= 0 and < SquareCount };
        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
        {
            if (isMouseOnBoard)
            {
                var result = CheckPiece(mouseGridPos);

                if (result == InteractionResult.None)
                {
                    // No move, no selection yet -> try selecting
                    mouseGridPos = Vector2.Clamp(mouseGridPos, Vector2.Zero, _upperBounds);
                    pieceSelected = DetectPieceSelection(mouseGridPos);
                }
            }
        }

        if (!isMouseOnBoard) return;

        DrawTile(mouseGridPos, Color.RED, 0.75);
    }

    private Vector2 ScreenToGrid(int screenX, int screenY)
    {
        var gridX = (screenX - _halfSideLength) / _sideLength;
        var gridY = (screenY - _halfSideLength) / _sideLength;
        return new Vector2(gridX, gridY);
    }

}