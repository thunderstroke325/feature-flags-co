import { IRulePercentageRollout } from "../pages/main/switch-manage/types/switch-new";

export function getAuth() {
    const auth = localStorage.getItem('auth');
    if (!auth) return null;
    return JSON.parse(auth);
}

export function uuidv4() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

export function isNotPercentageRollout(rulePercentageRollouts: IRulePercentageRollout[]) : boolean {
  return rulePercentageRollouts.length === 0 || (rulePercentageRollouts.length === 1 && rulePercentageRollouts[0].rolloutPercentage.length === 2 && rulePercentageRollouts[0].rolloutPercentage[0] === 0 && rulePercentageRollouts[0].rolloutPercentage[1] === 1);
}

export function getPercentageFromRolloutPercentageArray(arr: number[]): number {
  const diff = arr[1] - arr[0];
  return Number((Number(diff.toFixed(2)) * 100).toFixed(0));
}

export function encodeURIComponentFfc(url: string): string {
  return encodeURIComponent(url).replace(/\(/g, "%28").replace(/\)/g, '%29');
}

// determine if a rule operation is single operater
export function isSingleOperator(operationType: string): boolean {
  return !['string', 'number', 'regex', 'multi'].includes(operationType);
}
