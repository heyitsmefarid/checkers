// Screens and game flow. Ports Form1 (login), Form2 (sign up), Form3 (menu),
// GameSetupForm, GameForm, LeaderboardForm, SettingsForm and MusicManager.
(function () {
  'use strict';

  const { PieceColor, Game } = window.CheckersEngine;
  const { AIPlayer } = window.CheckersAI;
  const store = window.CheckersStorage;

  const $ = (id) => document.getElementById(id);

  const app = {
    currentPlayer: null,
    settings: store.loadSettings()
  };

  // ---------------------------------------------------------------- screens

  const SCREENS = ['login', 'signup', 'menu', 'game', 'leaderboard', 'settings'];

  function showScreen(name, title) {
    for (const screen of SCREENS) {
      $(`screen-${screen}`).hidden = screen !== name;
    }

    document.title = title || $(`screen-${name}`).dataset.title || 'Checkers';
    window.scrollTo(0, 0);
  }

  function setCurrentPlayer(account) {
    app.currentPlayer = account;
    store.saveSession(account);
  }

  // ---------------------------------------------------------- message boxes

  function messageBox(title, text, buttons) {
    const dialog = $('message-dialog');
    $('message-title').textContent = title;
    $('message-text').textContent = text;

    const actions = $('message-actions');
    actions.replaceChildren(...buttons.map((b) => {
      const button = document.createElement('button');
      button.className = 'btn';
      button.value = b.value;
      button.textContent = b.label;
      button.style.setProperty('--btn', b.color);
      return button;
    }));

    return new Promise((resolve) => {
      dialog.addEventListener('close', () => resolve(dialog.returnValue), { once: true });
      dialog.returnValue = '';
      dialog.showModal();
      actions.firstElementChild.focus();
    });
  }

  function showMessage(title, text) {
    return messageBox(title, text, [{ label: 'OK', value: 'ok', color: '#2196f3' }]);
  }

  async function askYesNo(title, text) {
    const answer = await messageBox(title, text, [
      { label: 'Yes', value: 'yes', color: '#2196f3' },
      { label: 'No', value: 'no', color: '#607d8b' }
    ]);
    return answer === 'yes';
  }

  // ------------------------------------------------------------ login (Form1)

  $('login-form').addEventListener('submit', async (event) => {
    event.preventDefault();

    const { account, message } = await store.login($('login-username').value, $('login-password').value);
    if (!account) {
      await showMessage('Login Failed', message);
      return;
    }

    $('login-password').value = '';
    setCurrentPlayer(account);
    showMenu();
  });

  $('btn-goto-signup').addEventListener('click', () => {
    $('signup-form').reset();
    showScreen('signup');
    $('signup-username').focus();
  });

  function showLogin() {
    showScreen('login');
    $('login-username').focus();
  }

  // ---------------------------------------------------------- sign up (Form2)

  $('signup-form').addEventListener('submit', async (event) => {
    event.preventDefault();

    const result = await store.register(
      $('signup-username').value,
      $('signup-fullname').value,
      $('signup-password').value
    );

    if (!result.ok) {
      await showMessage('Sign Up Failed', result.message);
      return;
    }

    await showMessage('Success', 'Registration successful. You can now log in.');
    $('login-username').value = $('signup-username').value.trim();
    $('login-password').value = '';
    showLogin();
  });

  $('btn-signup-back').addEventListener('click', showLogin);

  // ------------------------------------------------------------- menu (Form3)

  function showMenu() {
    const player = app.currentPlayer;
    $('menu-welcome').textContent = player
      ? `Welcome, ${player.fullName} (@${player.username})`
      : 'Welcome to Checkers';
    showScreen('menu');
  }

  $('btn-play-2p').addEventListener('click', async () => {
    const setup = await openSetup(false);
    if (setup) {
      startMatch({ vsComputer: false, bestOf: setup.bestOf, difficulty: null });
    }
  });

  $('btn-play-ai').addEventListener('click', async () => {
    const setup = await openSetup(true);
    if (setup) {
      startMatch({ vsComputer: true, bestOf: setup.bestOf, difficulty: setup.difficulty });
    }
  });

  $('btn-leaderboard').addEventListener('click', showLeaderboard);
  $('btn-settings').addEventListener('click', showSettings);

  $('btn-logout').addEventListener('click', () => {
    setCurrentPlayer(null);
    showLogin();
  });

  // ------------------------------------------------- match setup (GameSetupForm)

  function openSetup(includeDifficulty) {
    const dialog = $('setup-dialog');
    $('setup-bestof').value = '1';
    $('setup-difficulty').value = 'Normal';
    $('setup-difficulty-row').hidden = !includeDifficulty;

    return new Promise((resolve) => {
      dialog.addEventListener('close', () => {
        if (dialog.returnValue !== 'start') {
          resolve(null);
          return;
        }

        resolve({
          bestOf: parseInt($('setup-bestof').value, 10),
          difficulty: includeDifficulty ? $('setup-difficulty').value : null
        });
      }, { once: true });

      dialog.returnValue = '';
      dialog.showModal();
    });
  }

  // ----------------------------------------------------------- game (GameForm)

  const boardEl = $('board');
  const squareEls = [];
  const fallbackAi = new AIPlayer();
  let match = null;

  function buildBoard() {
    for (let row = 0; row < 8; row++) {
      const rowEls = [];
      for (let col = 0; col < 8; col++) {
        const dark = (row + col) % 2 === 1;
        const square = document.createElement('button');
        square.type = 'button';
        square.className = dark ? 'square dark' : 'square';
        square.dataset.row = row;
        square.dataset.col = col;
        if (!dark) {
          square.disabled = true;
          square.tabIndex = -1;
          square.setAttribute('aria-hidden', 'true');
        }

        const piece = document.createElement('span');
        piece.className = 'piece';
        square.append(piece);

        boardEl.append(square);
        rowEls.push(square);
      }
      squareEls.push(rowEls);
    }

    boardEl.addEventListener('click', (event) => {
      const square = event.target.closest('.square');
      if (square && !square.disabled) {
        onSquareClick(Number(square.dataset.row), Number(square.dataset.col));
      }
    });
  }

  function startMatch({ vsComputer, bestOf, difficulty }) {
    const username = app.currentPlayer?.username;

    match = {
      game: new Game(),
      vsComputer,
      bestOf,
      difficulty,
      redName: username || 'Player 1',
      blackName: vsComputer ? 'Computer' : 'Player 2',
      selected: null,
      selectedMoves: [],
      locked: false,
      seriesWins: 0,
      seriesLosses: 0,
      redCaptured: 0,
      blackCaptured: 0,
      roundStart: 0,
      roundStopped: null,
      timerId: null
    };

    $('game-moves').replaceChildren();
    showScreen('game', vsComputer ? `Checkers - VS Computer (${difficulty})` : 'Checkers - 2 Players');

    match.game.startNew();
    resetRoundMetrics(match);
    renderGame(match);
  }

  function leaveMatch() {
    if (match) {
      stopRoundTimer(match);
      match = null;
    }

    cancelAiRequest();
    showMenu();
  }

  function onSquareClick(row, col) {
    const m = match;
    if (!m || m.locked) {
      return;
    }

    if (m.vsComputer && m.game.currentTurn === PieceColor.Black) {
      return;
    }

    const piece = m.game.board.squares[row][col];

    if (m.selected) {
      const selectedMove = m.selectedMoves
        .filter((move) => move.toRow === row && move.toCol === col)
        .sort((a, b) => b.captured.length - a.captured.length)[0];

      if (selectedMove) {
        applyAndContinue(m, selectedMove);
        return;
      }
    }

    if (!piece || piece.color !== m.game.currentTurn) {
      m.selected = null;
      m.selectedMoves = [];
      renderGame(m);
      return;
    }

    m.selected = { row, col };
    m.selectedMoves = m.game.getValidMovesFrom(row, col)
      .sort((a, b) => b.captured.length - a.captured.length);
    renderGame(m);
  }

  async function applyAndContinue(m, move) {
    const moverColor = m.game.currentTurn;

    if (!m.game.tryApplyMove(move)) {
      return;
    }

    registerCapture(m, moverColor, move.captured.length);

    m.selected = null;
    m.selectedMoves = [];

    addMoveToHistory(m, move, moverColor, false);
    renderGame(m);

    m.locked = true;
    try {
      if (await handleGameEndIfNeeded(m)) {
        return;
      }

      if (m.vsComputer && m.game.currentTurn === PieceColor.Black) {
        await delay(300);
        if (match !== m) {
          return;
        }

        const aiMove = await chooseAiMove(m);
        if (match !== m) {
          return;
        }

        if (aiMove) {
          m.game.tryApplyMove(aiMove);
          registerCapture(m, PieceColor.Black, aiMove.captured.length);
          addMoveToHistory(m, aiMove, PieceColor.Black, true);
          renderGame(m);
          await handleGameEndIfNeeded(m);
        }
      }
    } finally {
      m.locked = false;
    }
  }

  function addMoveToHistory(m, move, moverColor, isAi) {
    const actor = isAi
      ? m.blackName
      : moverColor === PieceColor.Red ? m.redName : m.blackName;

    let text = `${m.game.moveCount}. ${actor}: (${move.fromRow},${move.fromCol}) -> (${move.toRow},${move.toCol})`;
    if (move.captured.length > 0) {
      text += ` x${move.captured.length}`;
    }

    const list = $('game-moves');
    const item = document.createElement('li');
    item.textContent = text;
    item.title = text;
    list.append(item);
    list.scrollTop = list.scrollHeight;
  }

  async function handleGameEndIfNeeded(m) {
    if (m.game.isDraw()) {
      await showMessage('Match Draw', 'Draw by no-capture rule.');
      await finishGame(m, 'Draw');
      return true;
    }

    const winner = m.game.getWinner();
    if (!winner) {
      return false;
    }

    const userWon = winner === PieceColor.Red;

    if (m.vsComputer) {
      await showMessage('Round Over', userWon ? 'You won this round!' : 'Computer won this round.');
    } else {
      await showMessage('Round Over', `${winner} wins this round.`);
    }

    await finishGame(m, userWon ? 'Win' : 'Loss');
    return true;
  }

  async function finishGame(m, result, promptBeforeNextRound = false) {
    stopRoundTimer(m);

    if (result === 'Win') {
      m.seriesWins++;
    } else if (result === 'Loss') {
      m.seriesLosses++;
    }

    const player = app.currentPlayer;
    if (player) {
      const durationSeconds = m.vsComputer && result === 'Win' ? Math.round(elapsedSeconds(m)) : null;

      store.recordMatch(
        player.playerId,
        m.vsComputer ? 'Computer' : 'Player',
        m.vsComputer ? m.difficulty : null,
        result,
        m.game.moveCount,
        durationSeconds
      );
    }

    const neededWins = Math.floor(m.bestOf / 2) + 1;
    const score = `${m.seriesWins}-${m.seriesLosses}`;

    if (m.seriesWins >= neededWins || m.seriesLosses >= neededWins) {
      const finalResult = m.seriesWins > m.seriesLosses ? 'Win' : (m.seriesWins < m.seriesLosses ? 'Loss' : 'Draw');

      if (player) {
        store.recordSeries(player.playerId, m.bestOf, m.seriesWins, m.seriesLosses, finalResult);
      }

      const summary = `Series Complete\nScore: ${score}\nResult: ${finalResult}`;

      if (!promptBeforeNextRound) {
        await showMessage('Series Result', summary);
        startNextGame(m, true);
        return;
      }

      if (await askYesNo('Series Result', `${summary}\n\nPlay another series?`)) {
        startNextGame(m, true);
      } else {
        leaveMatch();
      }

      return;
    }

    if (!promptBeforeNextRound) {
      await showMessage('Series Progress', `Next round starts now.\nSeries score: ${score}`);
      startNextGame(m, false);
      return;
    }

    if (await askYesNo('Round Finished', `Series score: ${score}\n\nDo you want to play the next round?`)) {
      startNextGame(m, false);
    } else {
      leaveMatch();
    }
  }

  function startNextGame(m, resetSeries) {
    if (resetSeries) {
      m.seriesWins = 0;
      m.seriesLosses = 0;
      $('game-moves').replaceChildren();
    }

    m.selected = null;
    m.selectedMoves = [];
    m.game.startNew();
    resetRoundMetrics(m);
    renderGame(m);
  }

  $('btn-resign').addEventListener('click', async () => {
    const m = match;
    if (!m || m.locked) {
      return;
    }

    m.locked = true;
    try {
      if (await askYesNo('Resign', 'Do you want to resign this round?')) {
        await showMessage('Resigned', 'You resigned this round.');
        await finishGame(m, 'Loss', true);
      }
    } finally {
      m.locked = false;
    }
  });

  $('btn-leave').addEventListener('click', async () => {
    if (await askYesNo('Main Menu', 'Leave this match and return to the main menu?')) {
      leaveMatch();
    }
  });

  function renderGame(m) {
    const { game } = m;
    const highlight = m.selected && app.settings.highlightValidMoves;
    const targets = new Set(highlight ? m.selectedMoves.map((move) => move.toRow * 8 + move.toCol) : []);

    boardEl.className = `board theme-${app.settings.boardTheme}`;

    for (let row = 0; row < 8; row++) {
      for (let col = 0; col < 8; col++) {
        if ((row + col) % 2 === 0) {
          continue;
        }

        const square = squareEls[row][col];
        const piece = game.board.squares[row][col];
        const isSelected = highlight && m.selected.row === row && m.selected.col === col;

        square.classList.toggle('selected', Boolean(isSelected));
        square.classList.toggle('target', targets.has(row * 8 + col));
        square.firstElementChild.className = piece
          ? `piece piece-${piece.color.toLowerCase()}${piece.isKing ? ' king' : ''}`
          : 'piece';

        const contents = piece ? `${piece.color} ${piece.isKing ? 'king' : 'piece'}` : 'empty';
        square.setAttribute('aria-label', `Row ${row}, column ${col}: ${contents}`);
        square.setAttribute('aria-pressed', String(Boolean(isSelected)));
      }
    }

    const turnName = game.currentTurn === PieceColor.Red ? m.redName : m.blackName;
    $('game-turn').textContent = `Turn: ${turnName} (${game.currentTurn})`;
    $('game-status').textContent = m.vsComputer
      ? `Mode: VS Computer (${m.difficulty})\n${m.redName} vs ${m.blackName}`
      : `Mode: Local 2 Players\n${m.redName} vs ${m.blackName}`;
    $('game-series').textContent = `Best of ${m.bestOf} | Wins: ${m.seriesWins}  Losses: ${m.seriesLosses}`;
    $('game-captured').textContent = `Captured | ${m.redName}: ${m.redCaptured}   ${m.blackName}: ${m.blackCaptured}`;
    updateTimerLabel();
  }

  function registerCapture(m, moverColor, capturedCount) {
    if (capturedCount <= 0) {
      return;
    }

    if (moverColor === PieceColor.Red) {
      m.redCaptured += capturedCount;
    } else {
      m.blackCaptured += capturedCount;
    }
  }

  function resetRoundMetrics(m) {
    m.redCaptured = 0;
    m.blackCaptured = 0;
    m.roundStart = performance.now();
    m.roundStopped = null;
    clearInterval(m.timerId);
    m.timerId = setInterval(updateTimerLabel, 1000);
    updateTimerLabel();
  }

  function stopRoundTimer(m) {
    if (m.roundStopped === null) {
      m.roundStopped = performance.now();
    }
    clearInterval(m.timerId);
    m.timerId = null;
  }

  function elapsedSeconds(m) {
    return ((m.roundStopped ?? performance.now()) - m.roundStart) / 1000;
  }

  function updateTimerLabel() {
    if (!match) {
      return;
    }

    const total = Math.floor(elapsedSeconds(match));
    const minutes = String(Math.floor(total / 60) % 60).padStart(2, '0');
    const seconds = String(total % 60).padStart(2, '0');
    $('game-timer').textContent = `Round Time: ${minutes}:${seconds}`;
  }

  function delay(ms) {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }

  // ------------------------------------------------------------ computer move

  let aiWorker = createAiWorker();
  let aiPending = null;
  let aiRequestId = 0;

  // Workers are unavailable when the page is opened straight from disk (file://);
  // the AI then runs on the main thread like the desktop app did.
  function createAiWorker() {
    try {
      const worker = new Worker('js/ai-worker.js');

      worker.onmessage = (event) => {
        if (aiPending && event.data.id === aiPending.id) {
          const pending = aiPending;
          aiPending = null;
          pending.resolve(event.data.move);
        }
      };

      worker.onerror = (event) => {
        event.preventDefault();
        worker.terminate();
        aiWorker = null;

        if (aiPending) {
          const pending = aiPending;
          aiPending = null;
          pending.resolve(pending.runOnMainThread());
        }
      };

      return worker;
    } catch {
      return null;
    }
  }

  function chooseAiMove(m) {
    const runOnMainThread = () => fallbackAi.chooseMove(m.game, m.difficulty, PieceColor.Black);

    if (!aiWorker) {
      return Promise.resolve(runOnMainThread());
    }

    return new Promise((resolve) => {
      const id = ++aiRequestId;
      aiPending = { id, resolve, runOnMainThread };
      aiWorker.postMessage({
        id,
        squares: m.game.board.squares,
        turn: m.game.currentTurn,
        difficulty: m.difficulty
      });
    });
  }

  function cancelAiRequest() {
    if (!aiPending) {
      return;
    }

    aiPending = null;
    if (aiWorker) {
      aiWorker.terminate();
      aiWorker = createAiWorker();
    }
  }

  // ------------------------------------------------ leaderboard (LeaderboardForm)

  function formatHms(totalSeconds) {
    const h = String(Math.floor(totalSeconds / 3600)).padStart(2, '0');
    const m = String(Math.floor((totalSeconds % 3600) / 60)).padStart(2, '0');
    const s = String(totalSeconds % 60).padStart(2, '0');
    return `${h}:${m}:${s}`;
  }

  function loadLeaderboard() {
    const rows = store.getLeaderboard();

    $('lb-body').replaceChildren(...rows.map((r, i) => {
      const tr = document.createElement('tr');
      const cells = [
        [i + 1, true],
        [r.username, false],
        [r.fullName, false],
        [r.easyWins, true],
        [r.normalWins, true],
        [r.hardWins, true],
        [r.totalWins, true],
        [r.totalLosses, true],
        [r.winRate === null ? '' : `${r.winRate.toFixed(2)}%`, true],
        [r.bestWinSeconds === null ? '-' : formatHms(r.bestWinSeconds), false]
      ];

      for (const [value, numeric] of cells) {
        const td = document.createElement('td');
        td.textContent = value;
        if (numeric) {
          td.className = 'num';
        }
        tr.append(td);
      }

      return tr;
    }));

    $('lb-info').textContent = rows.length === 0 ? 'No players in leaderboard yet.' : `Loaded ${rows.length} players`;
  }

  function showLeaderboard() {
    loadLeaderboard();
    showScreen('leaderboard');
  }

  $('btn-lb-refresh').addEventListener('click', loadLeaderboard);
  $('btn-lb-close').addEventListener('click', showMenu);

  // -------------------------------------------------------- settings (SettingsForm)

  function showSettings() {
    const s = app.settings;
    $('set-music').checked = s.enableBackgroundMusic;
    $('set-volume').value = Math.min(100, Math.max(0, s.musicVolumePercent));
    $('set-volume-value').textContent = `${$('set-volume').value}%`;
    $('set-highlight').checked = s.highlightValidMoves;
    $('set-theme').value = s.boardTheme;

    const result = $('settings-result');
    result.textContent = '';
    result.className = 'settings-result';

    showScreen('settings');
  }

  $('set-volume').addEventListener('input', () => {
    $('set-volume-value').textContent = `${$('set-volume').value}%`;
  });

  $('settings-form').addEventListener('submit', async (event) => {
    event.preventDefault();

    app.settings = {
      enableBackgroundMusic: $('set-music').checked,
      musicVolumePercent: Number($('set-volume').value),
      highlightValidMoves: $('set-highlight').checked,
      boardTheme: $('set-theme').value || 'BlackWhite'
    };
    store.saveSettings(app.settings);

    const result = $('settings-result');
    const music = await applyMusicSettings(app.settings);
    if (music.ok) {
      result.className = 'settings-result ok';
      result.textContent = 'Settings saved.';
    } else {
      result.className = 'settings-result warn';
      result.textContent = `Settings saved, but music issue: ${music.error}`;
    }
  });

  $('btn-settings-close').addEventListener('click', showMenu);

  // ------------------------------------------------------- music (MusicManager)

  const musicEl = $('bg-music');

  async function applyMusicSettings(settings) {
    if (!settings.enableBackgroundMusic) {
      musicEl.pause();
      return { ok: true };
    }

    musicEl.volume = Math.min(1, Math.max(0, settings.musicVolumePercent / 100));

    if (!musicEl.paused) {
      return { ok: true };
    }

    try {
      await musicEl.play();
      return { ok: true };
    } catch (error) {
      return {
        ok: false,
        error: error.name === 'NotAllowedError'
          ? 'the browser blocked autoplay. Click anywhere on the page to start it.'
          : 'bgsound.mp3 could not be played.'
      };
    }
  }

  // Browsers only allow audio after the visitor interacts with the page.
  function startMusicOnFirstInteraction() {
    const start = () => {
      document.removeEventListener('pointerdown', start, true);
      document.removeEventListener('keydown', start, true);
      if (app.settings.enableBackgroundMusic && musicEl.paused) {
        applyMusicSettings(app.settings);
      }
    };

    document.addEventListener('pointerdown', start, true);
    document.addEventListener('keydown', start, true);
  }

  // ---------------------------------------------------------------- startup

  buildBoard();
  startMusicOnFirstInteraction();

  const restored = store.loadSession();
  if (restored) {
    app.currentPlayer = restored;
    showMenu();
  } else {
    showLogin();
  }
})();
