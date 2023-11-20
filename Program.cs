// See https://aka.ms/new-console-template for more information

using chess_rts;

var puzzle = "r1b1k1r1/pp3pPp/2n1p3/1B2q3/3p4/4B3/P2N1PPP/R2Q1RK1";
var standard = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
var coolPawns = "8/2ppP2p/p4p1K/1P1Pp1p1/3P1Pp1/k1P1p3/P1P1P1Pp/8";
const string enPassantGalore = "8/pppppppp/8/P1P1P1P1/1p1p1p1p/8/PPPPPPPP/8";
var board = BoardHelpers.FromFen(enPassantGalore);
board.Run();