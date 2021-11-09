import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { IProject, IEnvironment, IProjectEnv } from 'src/app/config/types';
import { ProjectService } from 'src/app/services/project.service';
import { AccountService } from 'src/app/services/account.service';
import { EnvService } from 'src/app/services/env.service';

@Component({
  selector: 'app-project',
  templateUrl: './project.component.html',
  styleUrls: ['./project.component.less']
})
export class ProjectComponent implements OnInit {

  @ViewChild('listRef', { static: false }) listRef: ElementRef;c

  creatEditProjectFormVisible: boolean = false;
  creatEditEnvFormVisible: boolean = false;
  currentAccountId: number;

  project: IProject; // the project being deleting or editing
  env: IEnvironment;

  searchValue: string;

  currentProjectEnv: IProjectEnv; // the project on the top left corner of the page

  projects: IProject[] = [];

  doSearch() {
    const projects = this.projectService.projects || [];

    if (!this.searchValue) {
      this.projects = [...projects];
      return;
    }
    this.projects = [...projects.filter(project => project.name.toLowerCase().indexOf(this.searchValue.toLowerCase()) >= 0)];
  }

  get listHeight() {
    return document.body.clientHeight - 129 - 74 - 20;
    // return (this.listRef && this.listRef.nativeElement.clientHeight) ?? 0;
  }

  constructor(
    private projectService: ProjectService,
    private accountService: AccountService,
    private envService: EnvService,
  ) { }

  ngOnInit(): void {
    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
    this.currentAccountId = currentAccountProjectEnv.account.id;
    this.currentProjectEnv = currentAccountProjectEnv.projectEnv;
    this.doSearch();
  }

  isEnvDeleteBtnVisible(env: IEnvironment): boolean {
    return this.currentProjectEnv?.envId !== env.id;
  }

  fetchList(accountId: number) {
    this.projectService.getProjectList(accountId);
  }

  onCreateProjectClick() {
    this.project = undefined;
    this.creatEditProjectFormVisible = true;
  }

  onCreateEnvClick(project: IProject) {
    this.project = project;
    this.env = { projectId: project.id } as IEnvironment;
    this.creatEditEnvFormVisible = true;
  }

  onEditProjectClick(project: IProject) {
    this.project = project;
    this.creatEditProjectFormVisible = true;
  }

  onEditEnvClick(project: IProject, env: IEnvironment) {
    this.project = project;
    this.env = env;
    this.creatEditEnvFormVisible = true;
  }

  onDeleteEnvClick(project: IProject, env: IEnvironment) {
    this.envService.removeEnv(this.currentAccountId, project.id, env.id).subscribe(() => {
      this.envService.getEnvs(this.currentAccountId, env.projectId).subscribe((envs: IEnvironment[]) => {
        project.environments = envs;
      });
    })
  }

  onDeleteProjectClick(project: IProject) {
    this.projectService.removeProject(this.currentAccountId, project.id).subscribe(() => {
      this.fetchList(this.currentAccountId);
    });
  }

  projectClosed(data: any) {
    this.creatEditProjectFormVisible = false;
    this.fetchList(this.currentAccountId);
    if (data.isEditing && this.currentProjectEnv.projectId == this.project.id) { // is editing current project
      this.currentProjectEnv.projectName = data.project.name;
      this.projectService.changeCurrentProjectAndEnv(this.currentProjectEnv);
    }
  }

  envClosed(data: any) {
    this.creatEditEnvFormVisible = false;
    this.envService.getEnvs(this.currentAccountId, this.env.projectId).subscribe((envs: IEnvironment[]) => {
      this.project.environments = envs;
    });
    if (data.isEditing && this.currentProjectEnv.envId == this.env.id) { // is editing current env
      this.currentProjectEnv.envName = data.env.name;
      this.projectService.changeCurrentProjectAndEnv(this.currentProjectEnv);
    }
  }
}
