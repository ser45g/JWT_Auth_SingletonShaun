// src/api/tokenStore.ts

let _access_token: string | null = null;
let _refresh_token: string | null = null;

export interface Tokens {
  access_token: string |null;
  refresh_token: string | null;
}

export const tokenStore = {
  get(): Tokens {
    return {access_token:_access_token, refresh_token:_refresh_token};
  },
  set: (access_token: string | null, refresh_token: string | null): void => {
    _access_token = access_token;
    _refresh_token = refresh_token;

  },
  clear: (): void => {
    _access_token = null;
    _refresh_token = null;
  },
};