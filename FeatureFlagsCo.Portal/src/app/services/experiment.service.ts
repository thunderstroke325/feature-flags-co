import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ExperimentService {

  baseUrl: string = environment.url + '/api/experiments';
  currentProjectEnvChanged$: Subject<void> = new Subject();

  constructor(
    private http: HttpClient
  ) {}

  // 获取 custom events 列表
  public getCustomEvents(envId: number, lastItem?: string, searchText?: string): Observable<string[]> {
    let url = this.baseUrl + '/Events/#envId'.replace(/#envId/ig, `${envId}`);
    let queryStr = '';
    if (lastItem !== undefined && lastItem !== null && lastItem.trim().length > 0) {
      queryStr = `?lastItem=${lastItem}`;
    }

    if (searchText !== undefined && searchText !== null && searchText.trim().length > 0) {
      if (queryStr !== '') {
        queryStr += '&';
      } else {
        queryStr = '?';
      }

      queryStr += `searchText=${searchText}`;
    }

    if (queryStr !== '') {
      url += queryStr;
    }

    return this.http.get<string[]>(url);
  }

    // 获取 experiment 结果
    getExperimentResult(envId: number, params): Observable<any> {
      const url = this.baseUrl + `/${envId}`;
      return this.http.post(url, params);
    }
}
