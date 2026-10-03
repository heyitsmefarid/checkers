// Browser replacement for Checkers/DatabaseManager.cs and AuthManager.cs.
// Data lives in this browser's localStorage, just like the desktop app kept it
// in a MySQL server on the local machine.
(function (global) {
  'use strict';

  const DB_KEY = 'checkers.db.v1';
  const SETTINGS_KEY = 'checkers.settings.v1';
  const SESSION_KEY = 'checkers.session.v1';

  const DEFAULT_SETTINGS = Object.freeze({
    enableBackgroundMusic: true,
    musicVolumePercent: 45,
    highlightValidMoves: true,
    boardTheme: 'BlackWhite'
  });

  // Falls back to memory when storage is blocked (private mode, disabled site data).
  const memory = {};

  function readJson(storage, key) {
    try {
      const raw = storage.getItem(key);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return memory[key] ?? null;
    }
  }

  function writeJson(storage, key, value) {
    memory[key] = value;
    try {
      storage.setItem(key, JSON.stringify(value));
    } catch {
      // Keep the in-memory copy.
    }
  }

  function removeKey(storage, key) {
    delete memory[key];
    try {
      storage.removeItem(key);
    } catch {
      // Nothing persisted to remove.
    }
  }

  function loadDb() {
    const db = readJson(global.localStorage, DB_KEY);
    if (db && Array.isArray(db.players)) {
      return db;
    }

    return { nextPlayerId: 1, players: [], stats: {}, matchHistory: [], matchSeries: [] };
  }

  function saveDb(db) {
    writeJson(global.localStorage, DB_KEY, db);
  }

  async function hashPassword(username, password) {
    const data = new TextEncoder().encode(`checkers:${username.toLowerCase()}:${password}`);
    const digest = await global.crypto.subtle.digest('SHA-256', data);
    return Array.from(new Uint8Array(digest), (b) => b.toString(16).padStart(2, '0')).join('');
  }

  function findPlayer(db, username) {
    const key = username.toLowerCase();
    return db.players.find((p) => p.username.toLowerCase() === key) || null;
  }

  function toAccount(player) {
    return {
      playerId: player.playerId,
      username: player.username,
      fullName: player.fullName,
      lastLogin: player.lastLogin
    };
  }

  function isBlank(value) {
    return !value || !value.trim();
  }

  async function register(username, fullName, password) {
    if (isBlank(username) || isBlank(fullName) || isBlank(password)) {
      return { ok: false, message: 'All fields are required.' };
    }

    username = username.trim();
    fullName = fullName.trim();

    const db = loadDb();
    if (findPlayer(db, username)) {
      return { ok: false, message: 'Username already exists.' };
    }

    const playerId = db.nextPlayerId++;
    db.players.push({
      playerId,
      username,
      fullName,
      passwordHash: await hashPassword(username, password),
      dateCreated: new Date().toISOString(),
      lastLogin: null
    });
    db.stats[playerId] = {
      easyWins: 0,
      normalWins: 0,
      hardWins: 0,
      totalWins: 0,
      totalLosses: 0,
      bestWinSeconds: null
    };
    saveDb(db);

    return { ok: true, message: '' };
  }

  async function login(username, password) {
    if (isBlank(username) || isBlank(password)) {
      return { account: null, message: 'Username and password are required.' };
    }

    username = username.trim();

    const db = loadDb();
    const player = findPlayer(db, username);
    if (!player || player.passwordHash !== await hashPassword(player.username, password)) {
      return { account: null, message: 'Invalid username or password.' };
    }

    const account = toAccount(player);
    player.lastLogin = new Date().toISOString();
    saveDb(db);

    return { account, message: '' };
  }

  function recordMatch(playerId, opponentType, difficulty, result, movesCount, durationSeconds) {
    const db = loadDb();
    const stats = db.stats[playerId];
    if (!stats) {
      return false;
    }

    db.matchHistory.push({
      playerId,
      opponentType,
      difficulty,
      result,
      movesCount,
      durationSeconds,
      datePlayed: new Date().toISOString()
    });

    if (result === 'Win') {
      if (difficulty === 'Easy') stats.easyWins++;
      if (difficulty === 'Normal') stats.normalWins++;
      if (difficulty === 'Hard') stats.hardWins++;
      stats.totalWins++;

      const trackTime = opponentType === 'Computer' && durationSeconds !== null;
      if (trackTime && (stats.bestWinSeconds === null || durationSeconds < stats.bestWinSeconds)) {
        stats.bestWinSeconds = durationSeconds;
      }
    } else if (result === 'Loss') {
      stats.totalLosses++;
    }

    saveDb(db);
    return true;
  }

  function recordSeries(playerId, bestOf, wins, losses, finalResult) {
    const db = loadDb();
    db.matchSeries.push({ playerId, bestOf, wins, losses, finalResult, datePlayed: new Date().toISOString() });
    saveDb(db);
    return true;
  }

  // Same ordering as the MySQL query: HardWins DESC, TotalWins DESC,
  // WinRate DESC (no games sorts last), BestWinSeconds ASC (none sorts first).
  function getLeaderboard() {
    const db = loadDb();
    const rows = db.players
      .filter((p) => db.stats[p.playerId])
      .map((p) => {
        const s = db.stats[p.playerId];
        const played = s.totalWins + s.totalLosses;
        return {
          username: p.username,
          fullName: p.fullName,
          easyWins: s.easyWins,
          normalWins: s.normalWins,
          hardWins: s.hardWins,
          totalWins: s.totalWins,
          totalLosses: s.totalLosses,
          winRate: played === 0 ? null : Math.round((s.totalWins / played) * 10000) / 100,
          bestWinSeconds: s.bestWinSeconds
        };
      });

    const nullsLast = (a, b) => (a === null ? (b === null ? 0 : 1) : b === null ? -1 : 0);

    rows.sort((a, b) =>
      b.hardWins - a.hardWins ||
      b.totalWins - a.totalWins ||
      nullsLast(a.winRate, b.winRate) || (b.winRate ?? 0) - (a.winRate ?? 0) ||
      -nullsLast(a.bestWinSeconds, b.bestWinSeconds) || (a.bestWinSeconds ?? 0) - (b.bestWinSeconds ?? 0)
    );

    return rows;
  }

  function loadSettings() {
    return { ...DEFAULT_SETTINGS, ...(readJson(global.localStorage, SETTINGS_KEY) || {}) };
  }

  function saveSettings(settings) {
    writeJson(global.localStorage, SETTINGS_KEY, settings);
  }

  function loadSession() {
    const session = readJson(global.sessionStorage, SESSION_KEY);
    if (!session) {
      return null;
    }

    const player = loadDb().players.find((p) => p.playerId === session.playerId);
    return player ? toAccount(player) : null;
  }

  function saveSession(account) {
    if (account) {
      writeJson(global.sessionStorage, SESSION_KEY, { playerId: account.playerId });
    } else {
      removeKey(global.sessionStorage, SESSION_KEY);
    }
  }

  global.CheckersStorage = {
    register,
    login,
    recordMatch,
    recordSeries,
    getLeaderboard,
    loadSettings,
    saveSettings,
    loadSession,
    saveSession
  };
})(window);
