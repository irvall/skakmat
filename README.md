# skakmat ♟️

A tiny chess engine written in C# with a Raylib-powered graphical interface.

## Overview

skakmat (Danish for "checkmate") is a lightweight chess engine designed for learning and experimentation. It features a clean C# implementation with an intuitive visual interface built using Raylib.

## Features

<img width="712" height="740" alt="Screenshot 2025-09-11 at 23 54 24" src="https://github.com/user-attachments/assets/a1b2cce0-b718-42cd-9ebb-55a35bbee0c5" />

- **Chess Engine**: Complete chess rule implementation with move validation
    - [x] Board representation using bitboards
    - [x] Move generation
    - [ ] Search
    - [ ] Position evaluation

- **Graphical Interface**: Interactive board using Raylib for smooth rendering
- **Lightweight**: Minimal dependencies, easy to build and run
- **Educational**: Clean, readable code structure perfect for learning chess programming
- **Epic Sounds**: Chess sounds are normally so vague and flat; skakmat revolutionizes how sound and game can interplay

## Getting Started

### Prerequisites

- .NET 6.0 or later
- Raylib-cs NuGet package

### Building

```bash
git clone https://github.com/irvall/skakmat.git
cd skakmat
dotnet build
```

### Running

```bash
dotnet run
```

## How to Play

- Click-and-drop to move pieces
- 'F' flips boards
- The engine validates all moves according to chess rules
- Enjoy a game against yourself or use it as a foundation for AI development

## Project Structure

The codebase is organized for clarity and modularity, making it easy to understand and extend the engine's capabilities.

## Contributing

Feel free to open issues or submit pull requests. This is a learning project, and contributions are welcome!

## License

This project is open source. See the repository for license details.

---
