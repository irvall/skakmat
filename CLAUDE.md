# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Working agreement

The author writes all chess engine code (everything in `src/**/*.cs`). For that code, Claude is only a guide and sparring partner:
- Never create, edit or delete `.cs` files under `src/`, not even through shell commands. `.claude/settings.json` enforces this for the edit tools.
- Explain concepts, review the author's code, point to the relevant file and line, ask questions, and discuss trade-offs.
- Code snippets in chat only when explicitly asked, and keep them minimal and illustrative, not drop-in solutions.

Claude may edit project setup and tooling: build and project files, `Makefile`, `.vscode/`, docs such as `README.md`/`CLAUDE.md`/`TODO`, and repo config.

## Project

skakmat is a small hobby chess engine in C# with a Raylib GUI. The author writes it "for the joy of it", so keep changes small and readable, and match the existing style instead of adding abstractions. It targets the current .NET LTS: `net10.0`, with the SDK pinned in `global.json`. The only dependency is `Raylib-cs`.

## Commands

```bash
dotnet build                     # from repo root (uses skakmat.slnx)
dotnet run --project src         # launches the GUI window
make run                         # build, then launch the debug binary
make clean                       # removes src/bin and src/obj
```

- In VS Code, F5 runs the `build` task (`.vscode/tasks.json`) and then launches the debugger.
- The repo has no test project and no linter. To verify a change, build it and play moves in the GUI. `GameController` prints the move history to the console after every move.
- `src/skakmat.csproj` copies `assets/` (sounds and the sprite sheet) into the output directory. Assets load from paths relative to the binary.

## Architecture

The layers are `Program` → `Engine/GameEngine` → `Engine/GameController` → `Game/Board` + `Game/MoveGenerator`, with `Rendering/BoardRenderer` used for drawing.

- **`GameEngine`** owns the Raylib loop: computer move, then input, then render. The opponent mode is the hardcoded const `OpposingPlayer`, set to `ComputerIsWhite` by default. The "AI" picks a random legal move, because search and evaluation are not implemented yet (`Game/Evaluation.cs` is a stub). Controls: `F` flips the board, and the left and right arrows step through the position history. Moves are blocked unless you are viewing the latest position.
- **`GameController`** holds the game state:
  - `boardPositions` is a list of immutable `Position` snapshots, one per ply, plus `positionIndex` for history browsing.
  - It caches the legal moves (`validMovesCache`, refreshed when `movesShouldUpdate` is set).
  - It works out check, checkmate and stalemate.
  - It raises `GameEventOccurred`, which `GameSoundHandler` subscribes to. Side effects such as sound should go through this event, not be called directly.
- **`Board`** is the single *mutable* state: `ulong[12]` bitboards, side to move, castling rights and the last move. `ApplyMove` handles special moves, castling rights, the move itself, pawn promotion (always to a queen) and the side swap, then returns a new `Position`.
- **`Position`** is an immutable snapshot (a readonly struct that clones the bitboards). Rendering and history use it.
- **`MoveGenerator`** wraps the *same* `Board` instance as `GameController`:
  - It builds pseudo-legal bitboards per piece (lookup tables in `Chess/MoveTables.cs` for knight, king and pawns; sliders are computed by ray loops, since magic bitboards are still a TODO).
  - It tests legality by calling `board.ExecuteMove` / `board.UndoMove` on the shared board and checking `IsKingUnderAttack()`.
  - Do not add code that reads `Board` while a legality test is running, and do not keep `Board` state across calls.
- **Special moves** are `Move` subclasses: `CastleMove` (king move plus `RookMove`) and `EnPassantMove` (with `PawnToRemove`). `Board.HandleSpecialMove` pattern-matches on them. `Move` equality compares only piece, origin and target. `GameController.MakeMove` therefore looks up the cached "actual" move so that a plain user click becomes the correct subclass.

### Bitboard conventions

- Piece indices are int constants in `Game/Piece.cs`: white is 0–5 (P, R, N, B, Q, K), black is 6–11, and `EmptySquare = -1`. They index directly into the bitboard array.
- **Square 0 is a8 and 63 is h1.** FEN rank 8 maps to the low bits, so `Masks.Rank8 = 0xFF` and `Masks.Rank1 = 0xFF00000000000000`. "Up" for white is `>> 8`. See the `Chess/BoardSquares.cs` enum.
- Bit helpers such as `Contains`, `Exclude` and `ForAll` live in `Extensions/`. Rank, file, castle-path and corner masks are in `Chess/Masks.cs`.
- `BoardHelper.BitboardFromFen` parses only the piece-placement field. Side to move, castling and en passant are not read from FEN, so `Board()` always starts with white to move and all castling rights. Test positions are in `Constants.FenPositions`.

### Rendering

`BoardRenderer` draws the board and pieces from `assets/spritesheets/classic.png`, using the coordinates in `Constants.PieceToSpriteCoords`. `Rendering/BitboardVisualiser.cs` is a separate debug tool for drawing and inspecting bitboards. It opens its own window and is not wired into `Program`.

## Roadmap

`TODO` lists the planned work: a game tree and search, FEN import/export, PGN-style move export, Zobrist hashing, and magic bitboards for sliding pieces.
