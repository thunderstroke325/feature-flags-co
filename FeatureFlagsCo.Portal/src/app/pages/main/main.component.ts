import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import { AngularPlugin } from '@microsoft/applicationinsights-angularplugin-js';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { IAuthProps } from 'src/app/config/types';
import { IMenuItem } from 'src/app/share/uis/menu/menu';
import { QUICK_COMBAT_DOCUMENT} from 'src/app/config';
import { getAuth } from 'src/app/utils';
import { FfcService } from 'src/app/services/ffc.service';
import { environment } from 'src/environments/environment';
import { CURRENT_ACCOUNT, CURRENT_PROJECT } from "@utils/localstorage-keys";

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
    private ffcService: FfcService,
  ) {

    if (environment.name === 'Production') {
      // setup the microsoft insights
      const angularPlugin = new AngularPlugin();
      const appInsights = new ApplicationInsights({ config: {
      instrumentationKey: 'c706330b-e4e1-cf9b-9aef-ff26397502d7',
      endpointUrl: 'https://dc.applicationinsights.azure.cn/v2/track', // this points to azure china, ref https://docs.microsoft.com/en-us/azure/azure-monitor/app/custom-endpoints?tabs=nodejs
      extensions: [angularPlugin],
      extensionConfig: {
          [angularPlugin.identifier]: { router: this.router }
      }
      } });
      appInsights.loadAppInsights();
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

    if (this.ffcService.boolVariation('segments', false)) {
      const segmentsItem = {
        level: 1,
        title: '用户组',
        path: '/segments'
      }

      const idx = this.menus.findIndex(m => m.path === '/switch-user');
      if (idx > -1) {
        this.menus.splice(idx + 1, 0, segmentsItem);
      }
    }

    if (this.ffcService.variation('experimentation', 'V2') === 'V2') {
      const experimentationItem = {
        level: 1,
        title: '数据实验',
        path: '/experiments'
      }

      const idx = this.menus.findIndex(m => m.path === '/switch-archive');
      if (idx > -1) {
        this.menus.splice(idx + 1, 0, experimentationItem);
      }
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


  public async logout() {
    const anonymousUser = await this.ffcService.logout();
    const storageToKeep = {
      // restore account and project, so when user login, he would always see the same project & env
      [CURRENT_ACCOUNT()]: localStorage.getItem(CURRENT_ACCOUNT()),
      [CURRENT_PROJECT()]: localStorage.getItem(CURRENT_PROJECT()),

      // restore guid of ffc-js-client-side-sdk, this would keep the same anonymous user
      'ffc-guid': anonymousUser.id
    };

    localStorage.clear();
    Object.keys(storageToKeep).forEach(k => localStorage.setItem(k, storageToKeep[k]))
    this.router.navigateByUrl('/login');
  }
}
