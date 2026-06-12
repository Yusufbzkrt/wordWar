export const API_BASE = 'http://localhost:5025';
export const GAME_HUB_URL = `${API_BASE}/hubs/game`;
export const CHAT_HUB_URL = `${API_BASE}/hubs/chat`;

export const COLORS = {
  correct: '#22C55E',
  wrong: '#EF4444',
  gold: '#F59E0B',
  diamond: '#06B6D4',
  accent: '#8B5CF6',
};

export const GAME = {
  ROUND_DURATION: 30,
  ROUNDS_TO_WIN: 2,
  POPULAR_ANSWER_BONUS: 10,
  MATCH_WIN_GOLD: 10,
  MATCH_WIN_DIAMONDS: 5,
  EXTRA_TIME_COST: 10,
  HINT_COST: 15,
  AD_COOLDOWN_HOURS: 2,
};
