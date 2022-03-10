import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EnvironmentUserService {

  currentUser;

  constructor(
    private http: HttpClient
  ) { }

  setCurrentUser(user) {
    this.currentUser = user;
  }

  // 获取开关用户列表
  getEnvUsers(params): Observable<any> {
    const url = environment.url + '/FeatureFlagsUsers/QueryEnvironmentFeatureFlagUsers';
    return this.http.get(url, { params });
  }

  // 获取单个用户详情
  getEnvUserDetail(params): Observable<any> {
    const url = environment.url + `/FeatureFlagsUsers/GetEnvironmentUser/${params.id}`;
    return this.http.get(url);
  }

  // 创建开关用户
  postCreateUser(params): Observable<any> {
    const url = environment.url + '/FeatureFlagsUsers/CreateFeatureFlagUser';
    return this.http.post(url, params);
  }

  // 获取开关用户自定义属性列表
  getUserProperties(params): Observable<any> {
    const url = environment.url + `/Environment/GetEnvironmentUserProperties/${params.id}`;
    return this.http.get(url);
  }

  // 修改开关用户自定义属性
  postUserProperties(params): Observable<any> {
    const url = environment.url + `/Environment/CreateOrUpdateCosmosDBEnvironmentUserProperties`;
    return this.http.post(url, params);
  }
}
