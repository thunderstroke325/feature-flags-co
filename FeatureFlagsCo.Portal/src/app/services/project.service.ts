import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { IProject, IProjectEnv } from '../config/types';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {

  baseUrl: string = environment.url + '/api/accounts/#accountId/projects';
  currentProjectEnvChanged$: Subject<void> = new Subject();

  projects: IProject[] = [];

  constructor(
    private http: HttpClient
  ) {}

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

  public getProjectList(accountId: number) {
    this.getProjects(accountId)
      .pipe()
      .subscribe(
        res => {
          this.projects = res as IProject[];
        }
      );
  }

  // resetCurrentProjectAndEnv(accountId: number): Observable<any> {
  //   localStorage.setItem('current-project', '');
  //   return this.getCurrentProjectAndEnv(accountId).pipe(
  //     map(() => this.currentProjectEnvChanged$.next())
  //   );
  // }

  changeCurrentProjectAndEnv(project: IProjectEnv) {
    localStorage.setItem('current-project', JSON.stringify(project));
    this.currentProjectEnvChanged$.next();
  }


  // projectEnvStr and projects must exist
  private getProjectEnv(projectEnvStr: string, projects: IProject[]): IProjectEnv {
    const projectEnv: IProjectEnv = JSON.parse(projectEnvStr);
    if (!!projectEnv && !!projects.find(x => projectEnv.projectId === x.id)) {
      return projectEnv;
    }

    const currentProject = this.projects[0];
    const currentEnv = currentProject.environments.slice(0, 1)[0];
    const result = { projectId: currentProject.id, projectName: currentProject.name, envId: currentEnv.id, envName: currentEnv.name, envSecret: currentEnv.secret };
    this.changeCurrentProjectAndEnv(result);
    return result;
  }

   // Observable version
   getCurrentProjectAndEnv(accountId: number): Observable<IProjectEnv> {
    return Observable.create(observer => {
      const projectEnvStr = localStorage.getItem('current-project');
      if (this.projects.length === 0 || !projectEnvStr) {
        this.getProjects(accountId).subscribe(res => {
          this.projects = res as IProject[];
          const projectEnv = this.getProjectEnv(!!projectEnvStr ? projectEnvStr : '{}', this.projects);
          observer.next(projectEnv);
        });
      } else {
        const projectEnv = this.getProjectEnv(projectEnvStr, this.projects);
        observer.next(projectEnv);
      }
    });
  }
}
