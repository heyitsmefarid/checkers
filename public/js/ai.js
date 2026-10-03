// Port of Checkers/AIPlayer.cs.
(function (global) {
  'use strict';

  const engine = typeof module !== 'undefined' && module.exports ? require('./engine.js') : global.CheckersEngine;
  const { PieceColor, opponentOf, cloneMove, isCapture } = engine;

  const DifficultyLevel = Object.freeze({ Easy: 'Easy', Normal: 'Normal', Hard: 'Hard' });

  class AIPlayer {
    constructor(random = Math.random) {
      this.random = random;
    }

    nextIndex(count) {
      return Math.floor(this.random() * count);
    }

    chooseMove(game, difficulty, aiColor) {
      const validMoves = game.getValidMoves();
      if (validMoves.length === 0) {
        return null;
      }

      switch (difficulty) {
        case DifficultyLevel.Normal:
          return this.chooseNormalMove(validMoves, game.board);
        case DifficultyLevel.Hard:
          return this.chooseMinimaxMove(game.board, aiColor, 5);
        default:
          return cloneMove(validMoves[this.nextIndex(validMoves.length)]);
      }
    }

    chooseNormalMove(validMoves, board) {
      const captures = validMoves.filter(isCapture);
      if (captures.length > 0) {
        let best = captures[0];
        for (const capture of captures) {
          if (capture.captured.length > best.captured.length) {
            best = capture;
          }
        }
        return cloneMove(best);
      }

      const kingPromotion = validMoves.filter((m) => {
        const piece = board.squares[m.fromRow][m.fromCol];
        if (!piece || piece.isKing) {
          return false;
        }

        return piece.color === PieceColor.Red ? m.toRow === 0 : m.toRow === 7;
      });

      if (kingPromotion.length > 0) {
        return cloneMove(kingPromotion[this.nextIndex(kingPromotion.length)]);
      }

      const distance = (m) => Math.abs(3 - m.toRow) + Math.abs(3 - m.toCol);
      let closest = validMoves[0];
      for (const move of validMoves) {
        if (distance(move) < distance(closest)) {
          closest = move;
        }
      }
      return cloneMove(closest);
    }

    chooseMinimaxMove(board, aiColor, depth) {
      const moves = board.getValidMoves(aiColor);
      let bestMove = moves[0];
      let bestScore = -Infinity;

      for (const move of moves) {
        const nextBoard = board.clone();
        nextBoard.applyMove(move);
        const score = minimax(nextBoard, depth - 1, false, aiColor, -Infinity, Infinity);

        if (score > bestScore) {
          bestScore = score;
          bestMove = move;
        }
      }

      return cloneMove(bestMove);
    }
  }

  function minimax(board, depth, maximizing, aiColor, alpha, beta) {
    const current = maximizing ? aiColor : opponentOf(aiColor);
    const moves = board.getValidMoves(current);

    if (depth === 0 || moves.length === 0) {
      return evaluateBoard(board, aiColor, moves.length === 0 ? current : null, current, moves.length);
    }

    if (maximizing) {
      let maxEval = -Infinity;
      for (const move of moves) {
        const child = board.clone();
        child.applyMove(move);
        const evaluation = minimax(child, depth - 1, false, aiColor, alpha, beta);
        maxEval = Math.max(maxEval, evaluation);
        alpha = Math.max(alpha, evaluation);
        if (beta <= alpha) {
          break;
        }
      }
      return maxEval;
    }

    let minEval = Infinity;
    for (const move of moves) {
      const child = board.clone();
      child.applyMove(move);
      const evaluation = minimax(child, depth - 1, true, aiColor, alpha, beta);
      minEval = Math.min(minEval, evaluation);
      beta = Math.min(beta, evaluation);
      if (beta <= alpha) {
        break;
      }
    }
    return minEval;
  }

  // `current` / `currentMoveCount` let the caller reuse the move list it already
  // generated, instead of generating it again for the mobility term.
  function evaluateBoard(board, aiColor, noMovesFor, current, currentMoveCount) {
    if (noMovesFor !== null) {
      return noMovesFor === aiColor ? -10000 : 10000;
    }

    let score = 0;

    for (let row = 0; row < 8; row++) {
      for (let col = 0; col < 8; col++) {
        const piece = board.squares[row][col];
        if (!piece) {
          continue;
        }

        let value = piece.isKing ? 5 : 3;
        value += Math.floor((piece.color === PieceColor.Red ? 7 - row : row) / 2);

        if (piece.color === aiColor) {
          score += value;
        } else {
          score -= value;
        }
      }
    }

    const opponent = opponentOf(aiColor);
    score += current === aiColor ? currentMoveCount : board.getValidMoves(aiColor).length;
    score -= current === opponent ? currentMoveCount : board.getValidMoves(opponent).length;

    return score;
  }

  const api = { AIPlayer, DifficultyLevel };

  if (typeof module !== 'undefined' && module.exports) {
    module.exports = api;
  } else {
    global.CheckersAI = api;
  }
})(typeof window !== 'undefined' ? window : globalThis);
