// Runs the computer player off the main thread so the page stays responsive
// while the Hard (minimax) AI is thinking.
importScripts('engine.js', 'ai.js');

const { Game } = self.CheckersEngine;
const { AIPlayer } = self.CheckersAI;
const ai = new AIPlayer();

self.onmessage = (event) => {
  const { id, squares, turn, difficulty } = event.data;

  const game = new Game();
  game.board.squares = squares;
  game.currentTurn = turn;

  self.postMessage({ id, move: ai.chooseMove(game, difficulty, turn) });
};
