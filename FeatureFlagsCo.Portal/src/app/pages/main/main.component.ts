import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { IAuthProps } from 'src/app/config/types';
import { AuthService } from 'src/app/services/auth.service';
import { UserService } from 'src/app/services/user.service';
import { IMenuItem } from 'src/app/share/uis/menu/menu';
import { QUICK_COMBAT_DOCUMENT} from 'src/app/config';
import { getAuth } from 'src/app/utils';
import { ProjectService } from 'src/app/services/project.service';
import { SwitchService } from 'src/app/services/switch.service';
import { FfcService } from 'src/app/services/ffc.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.less']
})
export class MainComponent implements OnInit, OnDestroy {

  public menus: IMenuItem[] = [];
  public auth: IAuthProps;

  private destory$: Subject<void> = new Subject();

  constructor(
    private router: Router,
    private authService: AuthService,
    private projectService: ProjectService,
    private switchService: SwitchService,
    private userService: UserService,
    private ffcService: FfcService,
  ) {

    if (environment.name === 'Production') {
      // setup the microsoft insights
      const angularPlugin = new AngularPlugin();
    }

    this.setMenus();
  }

  ngOnInit(): void {
    this.auth = getAuth();
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  private setMenus(): void {
    // 菜单 path 和 target 互斥，优先匹配 path

    this.menus = [
      {
        level: 1,
        title: '开关管理',
        path: '/switch-manage'
      }, {
        level: 1,
        title: '开关用户管理',
        path: '/switch-user'
      }, {
        level: 1,
        title: '开关存档',
        path: '/switch-archive'
      }, {
        level: 1,
        title: '数据同步',
        path: '/data-sync'
      }, {
        level: 1,
        line: true
      }, {
        level: 1,
        title: '快速实战文档',
        target: QUICK_COMBAT_DOCUMENT
      },
      {
        level: 1,
        line: true
      }, {
        level: 1,
        title: '账户管理',
        path: '/account-settings'
      }
    ];

    if (this.ffcService.variation('experimentation', 'V2') === 'V2') {
      const experimentationItem = {
        level: 1,
        title: '数据实验',
        path: '/experiments'
      }
      this.menus.splice(3, 0, experimentationItem);
    }

    if (this.ffcService.variation('数据看板', 'false') === 'true') {
      const dataBoardItems = [{
        level: 1,
        line: true
      },{
        level: 1,
        title: '数据看板',
        path: '/analytics'
      }];

      const idx = this.menus.findIndex(m => m.path === '/data-sync');
      if (idx > -1) {
        this.menus.splice(idx + 1, 0, ...dataBoardItems);
      }
    }
  }

  public logout() {
    this.authService.logout();
  }
}
