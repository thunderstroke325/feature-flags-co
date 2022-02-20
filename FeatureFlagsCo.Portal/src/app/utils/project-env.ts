import { IProjectEnv } from "../config/types";

const projectEnvKey: string = 'current-project';

export function getCurrentProjectEnv(): IProjectEnv {
  const json = localStorage.getItem(projectEnvKey);
  if (json) {
    return JSON.parse(json);
  }

  return undefined;
}
