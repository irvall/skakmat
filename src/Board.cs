using System.Numerics;
using Raylib_cs;

namespace skakmat;

class Board
{

    readonly ulong[] bitboards;

    internal ulong WhitePieces =>
        bitboards[PieceConstants.WhitePawn]
        | bitboards[PieceConstants.WhiteRook]
        | bitboards[PieceConstants.WhiteKnight]
        | bitboards[PieceConstants.WhiteBishop]
        | bitboards[PieceConstants.WhiteQueen]
        | bitboards[PieceConstants.WhiteKing];

    internal ulong BlackPieces =>
        bitboards[PieceConstants.BlackPawn]
        | bitboards[PieceConstants.BlackRook]
        | bitboards[PieceConstants.BlackKnight]
        | bitboards[PieceConstants.BlackBishop]
        | bitboards[PieceConstants.BlackQueen]
        | bitboards[PieceConstants.BlackKing];

    internal ulong AllPieces => WhitePieces | BlackPieces;


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
        bbs[PieceConstants.WhitePawn] = Masks.Rank2;
        bbs[PieceConstants.WhiteRook] = 0x8100000000000000;
        bbs[PieceConstants.WhiteKnight] = 0x4200000000000000;
        bbs[PieceConstants.WhiteBishop] = 0x2400000000000000;
        bbs[PieceConstants.WhiteQueen] = 0x800000000000000;
        bbs[PieceConstants.WhiteKing] = 0x1000000000000000;

        bbs[PieceConstants.BlackPawn] = Masks.Rank7;
        bbs[PieceConstants.BlackKnight] = 0x42;
        bbs[PieceConstants.BlackBishop] = 0x24;
        bbs[PieceConstants.BlackRook] = 0x81;
        bbs[PieceConstants.BlackQueen] = 0x8;
        bbs[PieceConstants.BlackKing] = 0x10;
    }

    private int GetPieceTypeFromSquare(ulong square)
    {
        if (bitboards[PieceConstants.WhitePawn].Contains(square)) return PieceConstants.WhitePawn;
        if (bitboards[PieceConstants.BlackPawn].Contains(square)) return PieceConstants.BlackPawn;
        if (bitboards[PieceConstants.WhiteRook].Contains(square)) return PieceConstants.WhiteRook;
        if (bitboards[PieceConstants.BlackRook].Contains(square)) return PieceConstants.BlackRook;
        if (bitboards[PieceConstants.WhiteKnight].Contains(square)) return PieceConstants.WhiteKnight;
        if (bitboards[PieceConstants.BlackKnight].Contains(square)) return PieceConstants.BlackKnight;
        if (bitboards[PieceConstants.WhiteBishop].Contains(square)) return PieceConstants.WhiteBishop;
        if (bitboards[PieceConstants.BlackBishop].Contains(square)) return PieceConstants.BlackBishop;
        if (bitboards[PieceConstants.WhiteQueen].Contains(square)) return PieceConstants.WhiteQueen;
        if (bitboards[PieceConstants.BlackQueen].Contains(square)) return PieceConstants.BlackQueen;
        if (bitboards[PieceConstants.WhiteKing].Contains(square)) return PieceConstants.WhiteKing;
        if (bitboards[PieceConstants.BlackKing].Contains(square)) return PieceConstants.BlackKing;
        return -1;
    }

    private void DrawPieces()
    {
        for (var sq = 0; sq < 64; sq++)
        {
            var type = GetPieceTypeFromSquare(1UL << sq);
            if (type == -1) continue;
            DrawPiece(sq % 8, sq / 8, type);
        }
    }


    private static int GetWindowHeightDynamically()
    {
        Raylib.InitWindow(0, 0, "Temporary 0x0 window to get screen size");
        var windowHeight = (int)(Raylib.GetScreenHeight() / 1.5);
        Raylib.CloseWindow();
        return windowHeight;
    }

    private ulong OpponentsAttacks(bool isWhite)
    {
        var attacks = 0UL;
        for (var i = 0; i < 64; i++)
        {
            var sq = 1UL << i;
            if (bitboards[isWhite ? PieceConstants.BlackPawn : PieceConstants.WhitePawn].Contains(sq))
                attacks |= isWhite ? moveTables.BlackPawnAttacks[i] : moveTables.WhitePawnAttacks[i];
            if (bitboards[isWhite ? PieceConstants.BlackKnight : PieceConstants.WhiteKnight].Contains(sq))
                attacks |= moveTables.KnightMoves[i];

        }
        return attacks;
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

    private void DrawPiece(int col, int row, int pieceType)
    {
        int cellWidth = _spriteTexture.Width / 6;
        int cellHeight = _spriteTexture.Height / 2;

        if (!PieceConstants.PieceToSpriteCoords.TryGetValue(pieceType, out var tup))
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
        var attackBits = isWhite ? moveTables.WhitePawnAttacks[index] : moveTables.BlackPawnAttacks[index];
        var startRow = isWhite ? Masks.Rank2 : Masks.Rank7;
        var firstMoveBlokingRow = isWhite ? Masks.Rank3 : Masks.Rank6;
        if (startRow.Contains(bit) && firstMoveBlokingRow.Contains(moveBits & AllPieces))
        {
            moveBits = 0;
        }
        attackBits &= isWhite ? BlackPieces : WhitePieces;
        return new PieceSelection(bit, moveBits & ~AllPieces | attackBits, isWhite ? PieceConstants.WhitePawn : PieceConstants.BlackPawn, index);
    }

    private PieceSelection? DetectPieceSelection(Vector2 mouseGridPos)
    {
        var (idx, bit) = IndexAndBitUnderMouse(mouseGridPos);
        if (bitboards[PieceConstants.WhitePawn].Contains(bit))
        {
            return HandlePawnMove(idx, bit, true);
        }
        if (bitboards[PieceConstants.BlackPawn].Contains(bit))
        {
            return HandlePawnMove(idx, bit, false);
        }
        if (bitboards[PieceConstants.WhiteKnight].Contains(bit))
        {
            var moveBits = moveTables.KnightMoves[idx] & ~WhitePieces;
            return new PieceSelection(bit, moveBits, PieceConstants.WhiteKnight, idx);
        }
        if (bitboards[PieceConstants.BlackKnight].Contains(bit))
        {
            var moveBits = moveTables.KnightMoves[idx] & ~BlackPieces;
            return new PieceSelection(bit, moveBits, PieceConstants.BlackKnight, idx);
        }

        if (bitboards[PieceConstants.WhiteBishop].Contains(bit))
        {
            var moveBits = MoveTables.BishopAttacks(idx, AllPieces);
            return new PieceSelection(bit, moveBits & ~WhitePieces, PieceConstants.WhiteBishop, idx);
        }
        if (bitboards[PieceConstants.BlackBishop].Contains(bit))
        {
            var moveBits = MoveTables.BishopAttacks(idx, AllPieces);
            return new PieceSelection(bit, moveBits & ~BlackPieces, PieceConstants.BlackBishop, idx);
        }
        if (bitboards[PieceConstants.WhiteRook].Contains(bit))
        {
            var moveBits = MoveTables.RookAttacks(idx, AllPieces);
            return new PieceSelection(bit, moveBits & ~WhitePieces, PieceConstants.WhiteRook, idx);
        }
        if (bitboards[PieceConstants.BlackRook].Contains(bit))
        {
            var moveBits = MoveTables.RookAttacks(idx, AllPieces);
            return new PieceSelection(bit, moveBits & ~BlackPieces, PieceConstants.BlackRook, idx);
        }
        if (bitboards[PieceConstants.WhiteQueen].Contains(bit))
        {
            var moveBits = MoveTables.RookAttacks(idx, AllPieces) | MoveTables.BishopAttacks(idx, AllPieces);
            return new PieceSelection(bit, moveBits & ~WhitePieces, PieceConstants.WhiteQueen, idx);
        }
        if (bitboards[PieceConstants.BlackQueen].Contains(bit))
        {
            var moveBits = MoveTables.RookAttacks(idx, AllPieces) | MoveTables.BishopAttacks(idx, AllPieces);
            return new PieceSelection(bit, moveBits & ~BlackPieces, PieceConstants.BlackQueen, idx);
        }
        if (bitboards[PieceConstants.WhiteKing].Contains(bit))
        {
            var moveBits = moveTables.KingMoves[idx] & ~WhitePieces;
            return new PieceSelection(bit, moveBits & ~OpponentsAttacks(true), PieceConstants.WhiteKing, idx);
        }
        if (bitboards[PieceConstants.BlackKing].Contains(bit))
        {
            var moveBits = moveTables.KingMoves[idx] & ~BlackPieces;
            return new PieceSelection(bit, moveBits & ~OpponentsAttacks(false), PieceConstants.BlackKing, idx);
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
            var capturedPieceType = GetPieceTypeFromSquare(bit);
            if (capturedPieceType != -1)
            {
                bitboards[capturedPieceType] ^= bit;
            }
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