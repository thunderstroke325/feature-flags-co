import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { IAuthProps, IAccount, IProject, IEnvironment, IProjectEnv } from 'src/app/config/types';
import { AccountService } from 'src/app/services/account.service';
import { ProjectService } from 'src/app/services/project.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.less']
})
export class HeaderComponent implements OnInit {

  @Input() auth: IAuthProps;
  @Output() logout = new EventEmitter();

  currentProjectEnv: IProjectEnv;
  currentAccount: IAccount;

  allProjects: IProject[];
  selectedProject: IProject;
  selectedEnv: IEnvironment;
  envModalVisible: boolean = false;

  constructor(
    private router: Router,
    private accountService: AccountService,
    private projectService: ProjectService,
  ) {
  }

  ngOnInit(): void {
    this.selectCurrentProjectEnv();
    this.setAllProjects();

    this.projectService.projectListChanged$
      .subscribe(_ => {
        this.setAllProjects();
        this.selectCurrentProjectEnv();
      });

    this.projectService.currentProjectEnvChanged$
      .subscribe(_ => this.selectCurrentProjectEnv());
  }

  get availableProjects() {
    return this.allProjects;
  }

  get availableEnvs() {
    return this.allProjects.find(x => x.id === this.selectedProject.id)?.environments;
  }

  envModelCancel() {
    this.envModalVisible = false;
  }

  envModalConfirm() {
    const projectEnv = {
      projectId: this.selectedProject.id,
      projectName: this.selectedProject.name,
      envId: this.selectedEnv.id,
      envName: this.selectedEnv.name,
      envSecret: this.selectedEnv.secret
    };

    this.projectService.upsertCurrentProjectEnvLocally(projectEnv);
    this.currentProjectEnv = projectEnv;
    this.envModalVisible = false;

    if (this.router.url.indexOf("/switch-manage") > -1) {
      this.router.navigateByUrl("/switch-manage");
    }

    setTimeout(() => window.location.reload(), 200);
  }

  onSelectProject(project: IProject) {
    this.selectedProject = project;
    this.selectedEnv = project.environments.length > 0 ? project.environments[0] : null;
  }

  onSelectEnv(env: IEnvironment) {
    this.selectedEnv = env;
  }

  private selectCurrentProjectEnv() {
    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();

    this.currentAccount = currentAccountProjectEnv.account;
    this.currentProjectEnv = currentAccountProjectEnv.projectEnv;

    this.selectedProject = {
      id: this.currentProjectEnv.projectId,
      name: this.currentProjectEnv.projectName
    } as IProject;
    this.selectedEnv = {
      id: this.currentProjectEnv.envId,
      name: this.currentProjectEnv.envName
    } as IEnvironment;
  }

  private setAllProjects() {
    this.projectService.getProjects(this.currentAccount.id)
      .subscribe(projects => this.allProjects = projects);
  }
}
