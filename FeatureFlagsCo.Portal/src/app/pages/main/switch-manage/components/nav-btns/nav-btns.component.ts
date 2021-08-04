import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { btnsConfig } from './btns';

@Component({
  selector: 'app-nav-btns',
  templateUrl: './nav-btns.component.html',
  styleUrls: ['./nav-btns.component.less']
})
export class NavBtnsComponent {

  @Input() routeUrl: string;
  @Input() id: string;

  constructor(
    private router: Router
  ){}

  public navConfig = btnsConfig;

  onCheck(id: string) {
    let url = `/switch-manage/${id}`;
    if(this.id) {
      url = `${url}/${encodeURIComponent(this.id)}`;
    }
    this.router.navigateByUrl(url);
  }
}
