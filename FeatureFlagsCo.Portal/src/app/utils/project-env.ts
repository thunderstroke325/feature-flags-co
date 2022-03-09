import { getLocalStorageKey } from ".";
import { IProjectEnv } from "../config/types";

export function getCurrentProjectEnv(): IProjectEnv {
  const json = localStorage.getItem(getLocalStorageKey('current-project'));
  if (json) {
    return JSON.parse(json);
  }

  return undefined;
}
