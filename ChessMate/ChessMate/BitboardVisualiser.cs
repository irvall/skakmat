using System.Numerics;
using Raylib_cs;
using Color = Raylib_cs.Color;

namespace ChessMate;

public class BitboardVisualiser
{
    private const int SquareCount = 8;
    private readonly List<HighlightSquare> _boundingBoxes;
    private readonly int _halfSideLength;
    private readonly int _sideLength;
    private readonly Vector2 _upperBounds;
    private readonly (int width, int height) _windowSize;
    private Color _boundingBoxColor;

    private bool _isDragging;
    private Vector2? _rectBeginPos;
    private Vector2? _rectEndPos;


    public BitboardVisualiser(int windowHeight)
    {
        _sideLength = windowHeight / SquareCount;
        _halfSideLength = _sideLength / 2;
        _windowSize.width = windowHeight + _sideLength;
        _windowSize.height = windowHeight + _sideLength;
        _boundingBoxes = new List<HighlightSquare>();
        _upperBounds = new Vector2(SquareCount - 1);
        _rectBeginPos = null;
        _rectEndPos = null;
        _boundingBoxColor = Palette.GetNextColor();
        DrawWindow();
    }

    private static ulong BoundingBoxToBitboard(HighlightSquare bb)
    {
        var bitboard = 0UL;
        var isXGreater = bb.EndPosition.X > bb.StartPosition.X;
        var isYGreater = bb.EndPosition.Y > bb.StartPosition.Y;
        for (var x = (int)bb.StartPosition.X;
             isXGreater ? x <= bb.EndPosition.X : x >= bb.EndPosition.X;
             x += isXGreater ? 1 : -1)
        for (var y = (int)bb.StartPosition.Y;
             isYGreater ? y <= bb.EndPosition.Y : y >= bb.EndPosition.Y;
             y += isYGreater ? 1 : -1)
            bitboard |= 1UL << GridIndex(x, y);

        return bitboard;
    }

    private Vector2 ScreenToGrid(int screenX, int screenY)
    {
        var gridX = (screenX - _halfSideLength) / _sideLength;
        var gridY = (screenY - _halfSideLength) / _sideLength;
        return new Vector2(gridX, gridY);
    }

    private void DrawWindow()
    {
        LogUtility.IgnoreRaylibLogs();
        Raylib.InitWindow(_windowSize.width, _windowSize.height, "Bitboard Helper");
        var bgColor = new Color(4, 15, 15, 1);
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(bgColor);
            DrawBoard();
            HandleInteractions();
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    private static int GridIndex(int x, int y)
    {
        return y * SquareCount + x;
    }

    private void DrawBoard()
    {
        for (var i = 0; i < SquareCount; i++)
        for (var j = 0; j < SquareCount; j++)
        {
            if (j == 0)
            {
                var posX = _halfSideLength / 3;
                var posY = (int)(_sideLength * .85);
                Raylib.DrawText(8 - i + "", posX, i * _sideLength + posY, _halfSideLength, Color.WHITE);
            }

            var draw = (i + j) % 2 == 0;
            DrawTile(j, i, draw ? Color.BROWN : Color.RAYWHITE);
        }

        for (var c = 'A'; c <= 'H'; c++)
        {
            var posX = (int)(_sideLength * .85);
            var posY = _windowSize.height - _halfSideLength;
            Raylib.DrawText(c + "", (c - 'A') * _sideLength + posX, posY, _halfSideLength, Color.WHITE);
        }


        _boundingBoxes.ForEach(bb => DrawRect(bb.StartPosition, bb.EndPosition, bb.Color));
    }

    private static string GetAreaAsVariable(HighlightSquare bb)
    {
        var bitboard = BoundingBoxToBitboard(bb);
        var lowerBound = Math.Min(bb.StartPosition.X, bb.EndPosition.X);
        var upperBound = Math.Max(bb.StartPosition.X, bb.EndPosition.X);
        var leftBound = Math.Max(bb.StartPosition.Y, bb.EndPosition.Y);
        var rightBound = Math.Min(bb.StartPosition.Y, bb.EndPosition.Y);
        var lowerVector = new Vector2(lowerBound, leftBound);
        var upperVector = new Vector2(upperBound, rightBound);
        var startSquare = VectorToSquare(lowerVector);
        var endSquare = VectorToSquare(upperVector);
        var bitboardAsHex = Convert.ToString((long)bitboard, 16);
        return $"public static ulong {startSquare}{endSquare} = 0x{bitboardAsHex};";
    }

    private static string VectorToSquare(Vector2 vector)
    {
        var x = (char)('A' + vector.X);
        var y = (char)('8' - vector.Y);
        return $"{x}{y}";
    }

    private void HandleInteractions()
    {
        var mouseScreenPos = Raylib.GetMousePosition();
        var mouseGridPos = ScreenToGrid((int)mouseScreenPos.X, (int)mouseScreenPos.Y);
        var isMouseOnBoard = mouseGridPos is { X: >= 0 and < SquareCount, Y: >= 0 and < SquareCount };
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        {
            if (isMouseOnBoard)
            {
                _isDragging = true;
                mouseGridPos = Vector2.Clamp(mouseGridPos, Vector2.Zero, _upperBounds);
                _rectBeginPos ??= mouseGridPos;
                _rectEndPos = mouseGridPos;
            }
        }
        else if (Raylib.IsMouseButtonUp(MouseButton.MOUSE_BUTTON_LEFT) && _isDragging)
        {
            if (_rectBeginPos != null && _rectEndPos != null)
            {
                _boundingBoxes.Add(new HighlightSquare(_rectBeginPos.Value, _rectEndPos.Value, _boundingBoxColor));
                _rectBeginPos = null;
                _rectEndPos = null;
                _boundingBoxColor = Palette.GetNextColor();
            }

            var variables = string.Join("\n", _boundingBoxes.Select(GetAreaAsVariable));
            Console.WriteLine("\nCopied to clipboard:");
            Console.WriteLine(variables);
            Clipboard.SetText(variables);
            _isDragging = false;
        }

        if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT)) _boundingBoxes.Clear();

        if (_rectBeginPos != null && _rectEndPos != null)
            DrawRect(_rectBeginPos.Value, _rectEndPos.Value, _boundingBoxColor);
        if (!isMouseOnBoard) return;
        DrawTile(mouseGridPos, _boundingBoxColor);
    }

    private void DrawTile(Vector2 tilePosition, Color tileColor)
    {
        DrawTile((int)tilePosition.X, (int)tilePosition.Y, tileColor);
    }

    private void DrawTile(int col, int row, Color tileColor)
    {
        var posX = col * _sideLength + _halfSideLength;
        var posY = row * _sideLength + _halfSideLength;
        Raylib.DrawRectangle(posX, posY, _sideLength, _sideLength, tileColor);
    }

    private void DrawRect(Vector2 rectBeginPos, Vector2 rectEndPos, Color tileColor)
    {
        var isXGreater = rectEndPos.X > rectBeginPos.X;
        var isYGreater = rectEndPos.Y > rectBeginPos.Y;
        for (var x = (int)rectBeginPos.X;
             isXGreater ? x <= rectEndPos.X : x >= rectEndPos.X;
             x += isXGreater ? 1 : -1)
        for (var y = (int)rectBeginPos.Y;
             isYGreater ? y <= rectEndPos.Y : y >= rectEndPos.Y;
             y += isYGreater ? 1 : -1)
            DrawTile(x, y, tileColor);
    }

    private struct HighlightSquare
    {
        public readonly Color Color;
        public readonly Vector2 StartPosition;
        public readonly Vector2 EndPosition;

        public HighlightSquare(Vector2 startPosition, Vector2 endPosition, Color color)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Color = color;
        }
    }
}