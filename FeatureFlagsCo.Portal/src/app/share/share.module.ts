import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MenuComponent } from './uis/menu/menu.component';
import { OverlayModule } from '@angular/cdk/overlay';

import { NzIconModule } from 'ng-zorro-antd/icon';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { RouterModule } from '@angular/router';
import { NzDrawerModule } from 'ng-zorro-antd/drawer';
import { ProjectDrawerComponent } from './uis/project-drawer/project-drawer.component';
import { EnvDrawerComponent } from './uis/env-drawer/env-drawer.component';
import { MemberDrawerComponent } from './uis/member-drawer/member-drawer.component';
import { AccountDrawerComponent } from './uis/account-drawer/account-drawer.component';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzMessageModule } from 'ng-zorro-antd/message';
import { XuBreadCrumbComponent } from './uis/custom-breadcrumb/breadcrumb.component';
import { XuBreadCrumbItemComponent } from './uis/custom-breadcrumb/breadcrumb-item.component';
import { XuBreadCrumbSeparatorComponent } from './uis/custom-breadcrumb/breadcrumb-separator.component';
import { NzOverlayModule } from 'ng-zorro-antd/core/overlay';
import { NzOutletModule } from 'ng-zorro-antd/core/outlet';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { PropsDrawerComponent } from './uis/props-drawer/props-drawer.component';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzSpaceModule } from 'ng-zorro-antd/space';
import { NzSpinModule } from 'ng-zorro-antd/spin';


@NgModule({
  declarations: [MenuComponent, XuBreadCrumbComponent, XuBreadCrumbItemComponent, XuBreadCrumbSeparatorComponent, PropsDrawerComponent, ProjectDrawerComponent, EnvDrawerComponent, MemberDrawerComponent, AccountDrawerComponent],
  imports: [
    CommonModule,
    FormsModule,
    OverlayModule,
    ReactiveFormsModule,
    RouterModule,
    NzFormModule,
    NzIconModule,
    NzMenuModule,
    NzModalModule,
    NzInputModule,
    NzOutletModule,
    NzButtonModule,
    NzDrawerModule,
    NzMessageModule,
    NzOverlayModule,
    NzDropDownModule,
    NzTableModule,
    NzSelectModule,
    NzDividerModule,
    NzSpaceModule,
    NzSpinModule,
    ReactiveFormsModule
  ],
  exports: [
    CommonModule,
    MenuComponent,
    ReactiveFormsModule,
    FormsModule,
    XuBreadCrumbComponent,
    XuBreadCrumbItemComponent,
    XuBreadCrumbSeparatorComponent,
    PropsDrawerComponent,
    ProjectDrawerComponent,
    EnvDrawerComponent,
    MemberDrawerComponent,
    AccountDrawerComponent
  ]
})
export class ShareModule { }
