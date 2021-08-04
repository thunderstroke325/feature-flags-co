import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { IAuthProps, IAccount, IProject, IEnvironment, IProjectEnv } from 'src/app/config/types';
import { IMenuItem } from './menu';
import { getAuth } from 'src/app/utils';
import { AccountService } from 'src/app/services/account.service';
import { ProjectService } from 'src/app/services/project.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.less']
})
export class MenuComponent implements OnInit {


  @Input() menus: IMenuItem[];
  @Output() logout = new EventEmitter();

  get projects() {
    return this.projectService.projects || [];
  }

  selectedProject: IProject;
  selectedEnv: IEnvironment;
  auth: IAuthProps;
  currentProjectEnv: IProjectEnv;
  currentAccount: IAccount;
  envModalVisible: boolean = false;

  constructor(
    private router: Router,
    private accountService: AccountService,
    private projectService: ProjectService,
  ) {
  }

  ngOnInit(): void {
    this.auth = getAuth();
    this.projectService.currentProjectEnvChanged$
      .pipe()
      .subscribe(
        res => {
          const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
          this.currentAccount = currentAccountProjectEnv.account;
          this.currentProjectEnv = currentAccountProjectEnv.projectEnv;
          this.setSelectedProjectAndEnv();
        }
      );

    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
    this.currentAccount = currentAccountProjectEnv.account;
    this.currentProjectEnv = currentAccountProjectEnv.projectEnv;
    this.setSelectedProjectAndEnv();
  }

  private setSelectedProjectAndEnv() {
    this.selectedProject = { id: this.currentProjectEnv.projectId, name: this.currentProjectEnv.projectName } as IProject;
    this.selectedEnv = { id: this.currentProjectEnv.envId, name: this.currentProjectEnv.envName } as IEnvironment;
  }

  get availableProjects() {
    return this.projects;
  }

  get availableEnvs() {
    return this.projects.find(x => x.id === this.selectedProject.id)?.environments;
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
    };

    this.projectService.changeCurrentProjectAndEnv(projectEnv);
    this.currentProjectEnv = projectEnv;
    this.envModalVisible = false;

    if (this.router.url.indexOf("/switch-manage") > -1) {
      this.router.navigateByUrl("/switch-manage");
    }

    setTimeout(() => window.location.reload(), 200);
  }

  onSelectProject(project: IProject) {
    this.selectedProject = project;
  }

  onSelectEnv(env: IEnvironment) {
    this.selectedEnv = env;
  }

  onMenuItemSelected(menu: IMenuItem) {
    if (menu.path) return;
    window.open(menu.target);
  }
}
