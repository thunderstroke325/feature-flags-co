import { getLocalStorageKey } from "./index";

export const LOGIN_REDIRECT_URL = 'login-redirect-url';
export const IDENTITY_TOKEN = 'token';
export const USER_PROFILE = 'auth';

export function CURRENT_PROJECT() {
  return getLocalStorageKey('current-project');
}

export function CURRENT_ACCOUNT() {
  return getLocalStorageKey('current-account');
}
