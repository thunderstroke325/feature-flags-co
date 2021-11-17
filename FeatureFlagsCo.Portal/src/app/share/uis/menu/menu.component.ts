import { Component, Input, OnInit } from '@angular/core';
import { IMenuItem } from './menu';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.less']
})
export class MenuComponent implements OnInit {
  @Input() menus: IMenuItem[];

  constructor() {
  }

  ngOnInit(): void {
  }
  onMenuItemSelected(menu: IMenuItem) {
    if (menu.path) return;
    window.open(menu.target);
  }
}
