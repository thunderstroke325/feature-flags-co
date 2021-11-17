import { Component, OnInit} from '@angular/core';
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

  creatEditProjectFormVisible: boolean = false;
  creatEditEnvFormVisible: boolean = false;

  // the project being deleting or editing
  project: IProject;
  env: IEnvironment;

  searchValue: string;

  // current project env
  currentAccountId: number;
  currentProjectEnv: IProjectEnv;

  projects: IProject[] = [];

  constructor(
    private projectService: ProjectService,
    private accountService: AccountService,
    private envService: EnvService,
  ) { }

  ngOnInit(): void {
    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
    this.currentAccountId = currentAccountProjectEnv.account.id;
    this.currentProjectEnv = currentAccountProjectEnv.projectEnv;
    this.projectService
      .getProjects(this.currentAccountId)
      .subscribe(projects => this.projects = projects);
  }

  isEnvDeleteBtnVisible(env: IEnvironment): boolean {
    return this.currentProjectEnv?.envId !== env.id;
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

      // emit project list change event
      this.projectService.projectListChanged$.next();
    })
  }

  onDeleteProjectClick(project: IProject) {
    this.projectService.removeProject(this.currentAccountId, project.id).subscribe(() => {
      // remove the deleted project from list
      this.projects = this.projects.filter(item => item.id !== project.id);

      // emit project list change event
      this.projectService.projectListChanged$.next();
    });
  }

  projectClosed(data: any) {
    this.creatEditProjectFormVisible = false;

    // close after edit project name
    if (data.isEditing) {
      const newName = data.project.name;

      const oldProject = this.projects.find(item => item.id == data.project.id);
      oldProject.name = newName;

      // if is editing current project
      if (this.currentProjectEnv.projectId == this.project.id) {
        this.currentProjectEnv.projectName = newName;
        this.projectService.upsertCurrentProjectEnvLocally(this.currentProjectEnv);
      }
    }

    // close after create project
    else if (data.project) {
      // put the newly created project at the first place
      this.projects.unshift(data.project);
    }

    // close after do nothing
    else {

    }

    // emit project list change event
    this.projectService.projectListChanged$.next();
  }

  envClosed(data: any) {
    this.creatEditEnvFormVisible = false;
    this.envService
      .getEnvs(this.currentAccountId, this.env.projectId)
      .subscribe((envs: IEnvironment[]) => {
        this.project.environments = envs;
      });

    // if is editing current env
    if (data.isEditing && this.currentProjectEnv.envId == this.env.id) {
      this.currentProjectEnv.envName = data.env.name;
      this.projectService.upsertCurrentProjectEnvLocally(this.currentProjectEnv);
    }

    // emit project list change event
    this.projectService.projectListChanged$.next();
  }
}
