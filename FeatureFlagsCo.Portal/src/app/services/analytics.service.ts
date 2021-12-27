import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { updataReportParam, UpsertDimensionVm } from '../pages/main/analytics/types/analytics';
import { sameTimeGroup } from '../pages/main/analytics/types/data-grouping';
import moment from "moment";

@Injectable({
  providedIn: "root"
})
export class AnalyticsService {

  baseUrl: string = environment.url + '/api/analytics';

  constructor(
    private http: HttpClient
  ) {}

  // 获取看板数据
  public getAnalyticBoardData(envID: number): Observable<any> {
    const url = `${this.baseUrl}/${envID}`;
    return this.http.get(url);
  }

  // 添加数据源
  public addDataSourece(param: any): Observable<any> {
    const url = `${this.baseUrl}/data-source`;
    return this.http.post(url, param)
  }

  // 保存报表
  public saveReport(param: updataReportParam): Observable<any> {
    const url = `${this.baseUrl}/data-group`;
    return this.http.post(url, param);
  }

  // 删除报表
  public deleteReport(envId: number, boardId: string, groupId: string): Observable<any> {
    const url = `${this.baseUrl}/data-group`;
    const params = {
      envId, boardId, groupId
    }
    return this.http.delete(url, { params });
  }

  // 删除数据源
  public deleteDateSource(envId: number, boardId: string, dataSourceId: string): Observable<any> {
    const url = `${this.baseUrl}/data-source`;
    const params = {
      envId, boardId, dataSourceId
    }
    return this.http.delete(url, { params });
  }

  // 新增/更新 维度
  public upsertDimension(dimensionVm: UpsertDimensionVm) {
    const url = `${this.baseUrl}/dimension`;
    return this.http.post(url, dimensionVm);
  }

  // 删除维度
  public deleteDimension(envId: number, boardId: string, dimensionId: string) {
    const url = `${this.baseUrl}/dimension`;
    const params = {envId, boardId, dimensionId}
    return this.http.delete(url, {params});
  }

  // 计算结果
  public computeResult(param: sameTimeGroup): Observable<any> {
    const url = `${this.baseUrl}/results`;

    // use hour time to compute result
    const payload = Object.assign({}, param, {
      startTime: moment(param.startTime).startOf('hour').toDate(),
      endTime: moment(param.endTime).startOf('hour').toDate()
    });

    return this.http.post(url, payload);
  }
}
