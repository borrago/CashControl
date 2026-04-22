const ACCESS_TOKEN_KEY = "cashcontrol.access-token";
const REFRESH_TOKEN_KEY = "cashcontrol.refresh-token";
const REFRESH_EXPIRES_KEY = "cashcontrol.refresh-expires-at";

export interface StoredSession {
  accessToken: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
}

export const sessionStorageService = {
  read(): StoredSession | null {
    const accessToken = localStorage.getItem(ACCESS_TOKEN_KEY);
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    const refreshTokenExpiresAtUtc = localStorage.getItem(REFRESH_EXPIRES_KEY);

    if (!accessToken || !refreshToken || !refreshTokenExpiresAtUtc) {
      return null;
    }

    return {
      accessToken,
      refreshToken,
      refreshTokenExpiresAtUtc,
    };
  },
  write(session: StoredSession) {
    localStorage.setItem(ACCESS_TOKEN_KEY, session.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, session.refreshToken);
    localStorage.setItem(REFRESH_EXPIRES_KEY, session.refreshTokenExpiresAtUtc);
  },
  clear() {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(REFRESH_EXPIRES_KEY);
  },
};
