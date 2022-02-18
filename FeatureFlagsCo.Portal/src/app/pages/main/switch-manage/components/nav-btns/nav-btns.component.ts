import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { btnsConfig } from './btns';
import { FfcService } from 'src/app/services/ffc.service';
import { encodeURIComponentFfc } from 'src/app/utils';

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
    private ffcService: FfcService
  ){
    const experimentation  = this.ffcService.variation('experimentation', 'V2');
    if (experimentation === 'hide') {
      const idx = this.navConfig.findIndex(n => n.id === 'experimentations');
      if (idx > -1) {
        this.navConfig.splice(idx, 1);
      }
    }

    if (experimentation === 'V2') {
      this.navConfig = this.navConfig.map(n => {
        return Object.assign({}, n, {
          label: n.id === 'experimentations' ? `${n.label}(公测版)` : n.label
        });
      })
    }

    const zeroCodeEnabled = this.ffcService.variation('零代码', 'false') === 'true';
    if (!zeroCodeEnabled) {
      const idx = this.navConfig.findIndex(n => n.id === 'zero-code-settings');
      if (idx > -1) {
        this.navConfig.splice(idx, 1);
      }
    }
  }

  onCheck(id: string) {
    let url = `/switch-manage/${id}`;
    if(this.id) {
      url = `${url}/${encodeURIComponentFfc(this.id)}`;
    }
    this.router.navigateByUrl(url);
  }
}

