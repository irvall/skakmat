using System.Numerics;
using Raylib_cs;
using skakmat.Extensions;
using skakmat.Game;
using skakmat.Helpers;

namespace skakmat.Rendering;

internal class BoardRenderer
{
    private readonly int _squareSize;
    private Texture2D _spriteTexture;
    private readonly (int width, int height) _windowSize;
    private bool useStandardOrientation;

    internal BoardRenderer(int windowHeight, int squareSize, bool useStandardOrientation)
    {
        this.useStandardOrientation = useStandardOrientation;

        _squareSize = squareSize;
        _windowSize.width = windowHeight;
        _windowSize.height = windowHeight;
    }

    internal void Initialize()
    {
        var windowHeight = RaylibHelper.GetWindowHeightDynamically();
        Raylib.InitWindow(windowHeight, windowHeight, "skakmat");
        _spriteTexture = RaylibHelper.LoadSpritesheet("classic.png");
    }

    internal void DrawBoard()
    {
        var primaryTileColor = Palette.Coolors.TiffanyBlue;
        var whiteTiles = Palette.WhiteVersion(primaryTileColor);

        for (var i = 0; i < Constants.SquareCount; i++)
        {
            for (var j = 0; j < Constants.SquareCount; j++)
            {
                var draw = (i + j) % 2 != 0;
                var tileColor = draw ? primaryTileColor : whiteTiles;
                var textColor = !draw ? primaryTileColor : whiteTiles;
                DrawTile(j, i, tileColor);

                var digit = useStandardOrientation ? Constants.SquareCount - i : i + 1;

                int fontSize = 10;
                int padding = 2;

                int posX = j * _squareSize + _squareSize - fontSize + padding;
                int posY = i * _squareSize + padding;
                Raylib.DrawText(digit.ToString(), posX, posY, fontSize, textColor);

                char letter = useStandardOrientation
                    ? (char)('A' + j)
                    : (char)('H' - j);

                int letterPosX = j * _squareSize + padding;
                int letterPosY = (i + 1) * _squareSize - fontSize - padding;
                Raylib.DrawText(letter.ToString(), letterPosX, letterPosY, fontSize, textColor);
            }
        }
    }

    internal void DrawPieces(Position position)
    {
        for (var pieceIndex = 0; pieceIndex < position.Bitboards.Length; pieceIndex++)
        {
            for (var i = 0; i < 64; i++)
            {
                var bit = 1UL << i;
                if (!position.Bitboards[pieceIndex].Contains(bit))
                    continue;

                int row = i / 8;
                int col = i % 8;

                if (!useStandardOrientation)
                {
                    row = 7 - row;
                    col = 7 - col;
                }

                DrawPiece(row, col, pieceIndex);
            }
        }
    }


    private void DrawPiece(int row, int col, int pieceIndex)
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
            col * _squareSize,
            row * _squareSize,
            _squareSize,
            _squareSize
        );
        Raylib.DrawTexturePro(_spriteTexture, src, dest, Vector2.Zero, 0f, Color.WHITE);
    }

    private void DrawTile(int col, int row, Color tileColor, double alpha = 1.0)
    {
        var posX = col * _squareSize;
        var posY = row * _squareSize;
        tileColor.A = (byte)(255.0 * alpha);
        Raylib.DrawRectangle(posX, posY, _squareSize, _squareSize, tileColor);
    }

    internal void HighlightSquares(ulong squares, Color color)
    {
        for (var idx = 63; idx >= 0; idx--)
        {
            var bit = 1UL << idx;
            if (squares.Contains(bit))
            {
                var col = idx % Constants.SquareCount;
                var row = idx / Constants.SquareCount;
                if (!useStandardOrientation)
                {
                    col = 7 - col;
                    row = 7 - row;
                }
                DrawTile(col, row, color, 0.5f);
            }
        }
    }

    internal void UpdateOrientation(bool usingStandard)
    {
        useStandardOrientation = usingStandard;
    }
}
