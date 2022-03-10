import { IProjectEnv } from "../config/types";
import { CURRENT_PROJECT } from "./localstorage-keys";

export function getCurrentProjectEnv(): IProjectEnv {
  const json = localStorage.getItem(CURRENT_PROJECT());
  if (json) {
    return JSON.parse(json);
  }

  return undefined;
}
