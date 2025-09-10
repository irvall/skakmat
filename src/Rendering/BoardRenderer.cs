using System.Numerics;
using Raylib_cs;
using skakmat.Game;
using skakmat.Utilities;

namespace skakmat.Rendering;

internal class BoardRenderer
{
    private readonly int _sideLength;
    private readonly int _halfSideLength;
    private Texture2D _spriteTexture;
    private readonly (int width, int height) _windowSize;

    internal BoardRenderer(int windowHeight, int sideLength)
    {
        _sideLength = sideLength;
        _halfSideLength = _sideLength / 2;
        _windowSize.width = windowHeight + _sideLength;
        _windowSize.height = windowHeight + _sideLength;
    }

    internal void Initialize()
    {
        _spriteTexture = RaylibUtility.LoadTextureChecked("assets/sprite.png");
    }

    internal void DrawBoard()
    {
        var primaryTileColor = Palette.FromHex("C7D59F");
        var whiteTiles = Palette.WhiteVersion(primaryTileColor);
        for (var i = 0; i < Constants.SquareCount; i++)
            for (var j = 0; j < Constants.SquareCount; j++)
            {
                if (j == 0)
                {
                    var posX = _halfSideLength / 3;
                    var posY = (int)(_sideLength * .85);
                    Raylib.DrawText(Constants.SquareCount - i + "", posX, i * _sideLength + posY, _halfSideLength, Color.WHITE);
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

    internal void DrawPieces(Board board)
    {
        foreach (var (pieceIndex, index, _) in board.GetAllPieces())
        {
            var col = index % Constants.SquareCount;
            var row = index / Constants.SquareCount;
            DrawPiece(col, row, pieceIndex);
        }
    }

    private void DrawPiece(int col, int row, int pieceIndex)
    {
        int cellWidth = _spriteTexture.Width / 6;
        int cellHeight = _spriteTexture.Height / 2;

        if (!Constants.PieceToSpriteCoords.TryGetValue(pieceIndex, out var tup))
            throw new Exception("Piece type not supported " + pieceIndex);

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
        // string id = (row * 8 + col).ToString();
        // Raylib.DrawText(id, (int)dest.X, (int)dest.Y, 12, Color.BLACK);
        Raylib.DrawTexturePro(_spriteTexture, src, dest, Vector2.Zero, 0f, Color.WHITE);
    }

    private void DrawTile(int col, int row, Color tileColor, double alpha = 1.0)
    {
        var posX = col * _sideLength + _halfSideLength;
        var posY = row * _sideLength + _halfSideLength;
        tileColor.A = (byte)(255.0 * alpha);
        Raylib.DrawRectangle(posX, posY, _sideLength, _sideLength, tileColor);
    }

    internal void HighlightSquares(ulong squares, Color color)
    {
        for (var idx = 63; idx >= 0; idx--)
        {
            var bit = 1UL << idx;
            if (squares.Contains(bit))
            {
                DrawTile(idx % Constants.SquareCount, idx / Constants.SquareCount, color, 0.5f);
            }
        }
    }

    internal void DrawBigMessage(string text)
    {
        Raylib.DrawText(text, _windowSize.width / 8, _windowSize.height / 2, 72, Color.BLACK);
    }

}
