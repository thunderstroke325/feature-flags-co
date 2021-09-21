import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { btnsConfig } from './btns';
import { FfcAngularSdkService } from 'ffc-angular-sdk';

@Component({
  selector: 'app-nav-btns',
  templateUrl: './nav-btns.component.html',
  styleUrls: ['./nav-btns.component.less']
})
export class NavBtnsComponent {

  @Input() routeUrl: string;
  @Input() id: string;

  public navConfig = btnsConfig;

  constructor(
    private router: Router,
    private ffcAngularSdkService: FfcAngularSdkService
  ){
    const flagTriggersEnabled = this.ffcAngularSdkService.variation('flag-trigger') === 'true';
    if (!flagTriggersEnabled) {
      const idx = this.navConfig.findIndex(n => n.id === 'triggers');
      if (idx > -1) {
        this.navConfig.splice(idx, 1);
      }
    }

    const experimentation  = this.ffcAngularSdkService.variation('experimentation');
    if (experimentation === 'hide') {
      const idx = this.navConfig.findIndex(n => n.id === 'experimentations');
      if (idx > -1) {
        this.navConfig.splice(idx, 1);
      }
    }

    if (experimentation === 'temporary version') {
      this.navConfig = this.navConfig.map(n => {
        return Object.assign({}, n, {
          label: n.id === 'experimentations' ? `${n.label}(公测版)` : n.label
        });
      })
    }
  }

  onCheck(id: string) {
    let url = `/switch-manage/${id}`;
    if(this.id) {
      url = `${url}/${encodeURIComponent(this.id)}`;
    }
    this.router.navigateByUrl(url);
  }
}
