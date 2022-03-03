import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { IProject, IProjectEnv } from '../config/types';
import { FfcService } from "./ffc.service";

@Injectable({
  providedIn: 'root'
})
export class ProjectService {

  readonly projectEnvKey: string = 'current-project';
  currentProjectEnvChanged$: Subject<void> = new Subject<void>();
  projectListChanged$: Subject<void> = new Subject<void>();

  private _baseUrl: string;
  public get baseUrl(): string {
    if (!this._baseUrl) {
      const apiVersion = this.ffcService.variation('backend-api-version', 'v2');
      this._baseUrl = `${environment.url}/api/${apiVersion}/accounts/#accountId/projects`;
    }

    return this._baseUrl;
  }

  constructor(
    private http: HttpClient,
    private ffcService: FfcService
  ) {
  }

  // 获取 project 列表
  public getProjects(accountId: number): Observable<IProject[]> {
    const url = this.baseUrl.replace(/#accountId/ig, `${accountId}`);
    return this.http.get<IProject[]>(url);
  }

  // 创建 project
  postCreateProject(accountId: number, params): Observable<any> {
    const url = this.baseUrl.replace(/#accountId/ig, `${accountId}`);
    return this.http.post(url, params);
  }

  // 更新 project
  putUpdateProject(accountId: number, params): Observable<any> {
    const url = this.baseUrl.replace(/#accountId/ig, `${accountId}`);
    return this.http.put(url, params);
  }

  // 删除 project
  removeProject(accountId: number, projectId: number): Observable<any> {
    const url = this.baseUrl.replace(/#accountId/ig, `${accountId}`) + `/${projectId}`;
    return this.http.delete(url);
  }

  // update or set current project env
  upsertCurrentProjectEnvLocally(project: IProjectEnv) {
    localStorage.setItem(this.projectEnvKey, JSON.stringify(project));
    this.currentProjectEnvChanged$.next();
  }

  // update current project env by partial object
  updateCurrentProjectEnvLocally(partialUpdated: Partial<IProjectEnv>) {
    const projectEnvJson = localStorage.getItem(this.projectEnvKey);
    if (!projectEnvJson) {
      return;
    }

    const projectEnv = JSON.parse(projectEnvJson);
    const updatedProject = Object.assign(projectEnv, partialUpdated);

    this.upsertCurrentProjectEnvLocally(updatedProject);
  }

  // get local project env
  getLocalCurrentProjectEnv(): IProjectEnv {
    const projectEnvJson = localStorage.getItem(this.projectEnvKey);
    return projectEnvJson ? JSON.parse(projectEnvJson) : undefined;
  }

  // get current project env for account
  getCurrentProjectEnv(accountId: number): Observable<IProjectEnv> {
    return new Observable(observer => {
      const localCurrentProjectEnv = this.getLocalCurrentProjectEnv();
      if (localCurrentProjectEnv) {
        observer.next(localCurrentProjectEnv);
      } else {
        this.getProjects(accountId).subscribe(projects => {
          // chose first project first env as default value
          const firstProject = projects[0];
          const firstProjectEnv = firstProject.environments[0];

          const projectEnv: IProjectEnv = {
            projectId: firstProject.id,
            projectName: firstProject.name,
            envId: firstProjectEnv.id,
            envName: firstProjectEnv.name,
            envSecret: firstProjectEnv.secret
          };

          this.upsertCurrentProjectEnvLocally(projectEnv);
          observer.next(projectEnv);
        });
      }
    })
  };

  // reset current project env
  clearCurrentProjectEnv() {
    localStorage.removeItem(this.projectEnvKey);
  }
}
