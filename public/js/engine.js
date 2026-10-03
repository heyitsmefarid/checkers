// Port of Checkers/GameEngine.cs and Checkers/Models.cs.
(function (global) {
  'use strict';

  const PieceColor = Object.freeze({ Red: 'Red', Black: 'Black' });

  function opponentOf(color) {
    return color === PieceColor.Red ? PieceColor.Black : PieceColor.Red;
  }

  function createMove(fromRow, fromCol, toRow, toCol, captured) {
    return { fromRow, fromCol, toRow, toCol, captured: captured || [] };
  }

  function cloneMove(move) {
    return createMove(
      move.fromRow,
      move.fromCol,
      move.toRow,
      move.toCol,
      move.captured.map((c) => ({ row: c.row, col: c.col }))
    );
  }

  function isCapture(move) {
    return move.captured.length > 0;
  }

  function movesMatch(a, b) {
    if (a.fromRow !== b.fromRow || a.fromCol !== b.fromCol || a.toRow !== b.toRow || a.toCol !== b.toCol) {
      return false;
    }

    if (a.captured.length !== b.captured.length) {
      return false;
    }

    for (let i = 0; i < a.captured.length; i++) {
      if (a.captured[i].row !== b.captured[i].row || a.captured[i].col !== b.captured[i].col) {
        return false;
      }
    }

    return true;
  }

  function isInBounds(row, col) {
    return row >= 0 && row < 8 && col >= 0 && col < 8;
  }

  const KING_DIRECTIONS = [[-1, -1], [-1, 1], [1, -1], [1, 1]];
  const RED_DIRECTIONS = [[-1, -1], [-1, 1]];
  const BLACK_DIRECTIONS = [[1, -1], [1, 1]];

  function getDirections(color, isKing) {
    if (isKing) {
      return KING_DIRECTIONS;
    }

    return color === PieceColor.Red ? RED_DIRECTIONS : BLACK_DIRECTIONS;
  }

  function emptySquares() {
    const squares = [];
    for (let row = 0; row < 8; row++) {
      squares.push(new Array(8).fill(null));
    }
    return squares;
  }

  // Piece objects are never mutated once placed (promotion swaps in a new
  // object), so clones can share them and only copy the row arrays.
  class Board {
    constructor() {
      this.squares = emptySquares();
    }

    initialize() {
      this.squares = emptySquares();

      for (let row = 0; row < 3; row++) {
        for (let col = 0; col < 8; col++) {
          if ((row + col) % 2 === 1) {
            this.squares[row][col] = { color: PieceColor.Black, isKing: false };
          }
        }
      }

      for (let row = 5; row < 8; row++) {
        for (let col = 0; col < 8; col++) {
          if ((row + col) % 2 === 1) {
            this.squares[row][col] = { color: PieceColor.Red, isKing: false };
          }
        }
      }
    }

    clone() {
      const board = new Board();
      board.squares = this.squares.map((row) => row.slice());
      return board;
    }

    countPieces(color) {
      let count = 0;
      for (let row = 0; row < 8; row++) {
        for (let col = 0; col < 8; col++) {
          if (this.squares[row][col]?.color === color) {
            count++;
          }
        }
      }
      return count;
    }

    getValidMoves(turn) {
      const captures = [];
      const nonCaptures = [];

      for (let row = 0; row < 8; row++) {
        for (let col = 0; col < 8; col++) {
          const piece = this.squares[row][col];
          if (!piece || piece.color !== turn) {
            continue;
          }

          captures.push(...this.getCaptureMovesFrom(row, col));
          nonCaptures.push(...this.getNormalMovesFrom(row, col));
        }
      }

      return captures.concat(nonCaptures);
    }

    getValidMovesFrom(row, col, turn) {
      return this.getValidMoves(turn).filter((m) => m.fromRow === row && m.fromCol === col);
    }

    applyMove(move) {
      const piece = this.squares[move.fromRow][move.fromCol];
      if (!piece) {
        throw new Error('No piece at source.');
      }

      this.squares[move.fromRow][move.fromCol] = null;
      this.squares[move.toRow][move.toCol] = piece;

      for (const captured of move.captured) {
        this.squares[captured.row][captured.col] = null;
      }

      if (!piece.isKing) {
        if ((piece.color === PieceColor.Red && move.toRow === 0) || (piece.color === PieceColor.Black && move.toRow === 7)) {
          this.squares[move.toRow][move.toCol] = { color: piece.color, isKing: true };
        }
      }
    }

    getNormalMovesFrom(row, col) {
      const piece = this.squares[row][col];
      if (!piece) {
        return [];
      }

      const moves = [];

      if (piece.isKing) {
        for (const [dr, dc] of getDirections(piece.color, piece.isKing)) {
          let toRow = row + dr;
          let toCol = col + dc;

          while (isInBounds(toRow, toCol) && this.squares[toRow][toCol] === null) {
            moves.push(createMove(row, col, toRow, toCol));
            toRow += dr;
            toCol += dc;
          }
        }

        return moves;
      }

      for (const [dr, dc] of getDirections(piece.color, piece.isKing)) {
        const toRow = row + dr;
        const toCol = col + dc;
        if (isInBounds(toRow, toCol) && this.squares[toRow][toCol] === null) {
          moves.push(createMove(row, col, toRow, toCol));
        }
      }

      return moves;
    }

    getCaptureMovesFrom(row, col) {
      const piece = this.squares[row][col];
      if (!piece) {
        return [];
      }

      // exploreCaptures only reads the board it is given, so no defensive clone.
      const captures = [];
      exploreCaptures(this, row, col, row, col, piece.color, piece.isKing, [], captures);
      return captures;
    }

    hasAnyMoves(color) {
      return this.getValidMoves(color).length > 0;
    }
  }

  function exploreCaptures(board, startRow, startCol, currentRow, currentCol, color, isKing, captured, results) {
    let foundNextCapture = false;

    if (isKing) {
      foundNextCapture = exploreKingCaptures(board, startRow, startCol, currentRow, currentCol, color, captured, results);
    } else {
      for (const [dr, dc] of getDirections(color, false)) {
        const enemyRow = currentRow + dr;
        const enemyCol = currentCol + dc;
        const landRow = currentRow + 2 * dr;
        const landCol = currentCol + 2 * dc;

        if (!isInBounds(enemyRow, enemyCol) || !isInBounds(landRow, landCol)) {
          continue;
        }

        const enemyPiece = board.squares[enemyRow][enemyCol];
        if (!enemyPiece || enemyPiece.color === color || board.squares[landRow][landCol] !== null) {
          continue;
        }

        foundNextCapture = true;

        const nextBoard = board.clone();
        const movingPiece = nextBoard.squares[currentRow][currentCol];
        nextBoard.squares[currentRow][currentCol] = null;
        nextBoard.squares[enemyRow][enemyCol] = null;

        const nextCaptured = captured.map((c) => ({ row: c.row, col: c.col }));
        nextCaptured.push({ row: enemyRow, col: enemyCol });

        const becomesKing = !movingPiece.isKing &&
          ((movingPiece.color === PieceColor.Red && landRow === 0) || (movingPiece.color === PieceColor.Black && landRow === 7));
        nextBoard.squares[landRow][landCol] = becomesKing ? { color: movingPiece.color, isKing: true } : movingPiece;

        exploreCaptures(nextBoard, startRow, startCol, landRow, landCol, color, movingPiece.isKing || becomesKing, nextCaptured, results);
      }
    }

    if (!foundNextCapture && captured.length > 0) {
      results.push(createMove(startRow, startCol, currentRow, currentCol, captured));
    }
  }

  function exploreKingCaptures(board, startRow, startCol, currentRow, currentCol, color, captured, results) {
    let found = false;

    for (const [dr, dc] of getDirections(color, true)) {
      let scanRow = currentRow + dr;
      let scanCol = currentCol + dc;

      while (isInBounds(scanRow, scanCol) && board.squares[scanRow][scanCol] === null) {
        scanRow += dr;
        scanCol += dc;
      }

      if (!isInBounds(scanRow, scanCol)) {
        continue;
      }

      const enemyPiece = board.squares[scanRow][scanCol];
      if (!enemyPiece || enemyPiece.color === color) {
        continue;
      }

      let landRow = scanRow + dr;
      let landCol = scanCol + dc;

      while (isInBounds(landRow, landCol) && board.squares[landRow][landCol] === null) {
        found = true;

        const nextBoard = board.clone();
        const movingPiece = nextBoard.squares[currentRow][currentCol];
        nextBoard.squares[currentRow][currentCol] = null;
        nextBoard.squares[scanRow][scanCol] = null;
        nextBoard.squares[landRow][landCol] = movingPiece;

        const nextCaptured = captured.map((c) => ({ row: c.row, col: c.col }));
        nextCaptured.push({ row: scanRow, col: scanCol });

        exploreCaptures(nextBoard, startRow, startCol, landRow, landCol, color, true, nextCaptured, results);

        landRow += dr;
        landCol += dc;
      }
    }

    return found;
  }

  class Game {
    constructor() {
      this.board = new Board();
      this.currentTurn = PieceColor.Red;
      this.moveCount = 0;
      this.noCaptureCounter = 0;
    }

    startNew() {
      this.board.initialize();
      this.currentTurn = PieceColor.Red;
      this.moveCount = 0;
      this.noCaptureCounter = 0;
    }

    getValidMoves() {
      return this.board.getValidMoves(this.currentTurn);
    }

    getValidMovesFrom(row, col) {
      return this.board.getValidMovesFrom(row, col, this.currentTurn);
    }

    tryApplyMove(move) {
      const validMove = this.getValidMoves().find((m) => movesMatch(m, move));
      if (!validMove) {
        return false;
      }

      this.board.applyMove(validMove);
      this.moveCount++;

      if (isCapture(validMove)) {
        this.noCaptureCounter = 0;
      } else {
        this.noCaptureCounter++;
      }

      this.currentTurn = opponentOf(this.currentTurn);
      return true;
    }

    getWinner() {
      if (this.board.countPieces(PieceColor.Red) === 0) {
        return PieceColor.Black;
      }

      if (this.board.countPieces(PieceColor.Black) === 0) {
        return PieceColor.Red;
      }

      if (!this.board.hasAnyMoves(this.currentTurn)) {
        return opponentOf(this.currentTurn);
      }

      return null;
    }

    isDraw() {
      return this.noCaptureCounter >= 40;
    }
  }

  const api = { PieceColor, Board, Game, opponentOf, cloneMove, isCapture, movesMatch };

  if (typeof module !== 'undefined' && module.exports) {
    module.exports = api;
  } else {
    global.CheckersEngine = api;
  }
})(typeof window !== 'undefined' ? window : globalThis);
