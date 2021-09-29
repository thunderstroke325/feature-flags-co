import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SwitchUserRoutingModule } from './switch-user-routing.module';
import { SwitchUserComponent } from './switch-user.component';
import { UserDetailComponent } from './user-detail/user-detail.component';
import { UserListComponent } from './user-list/user-list.component';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzInputModule } from 'ng-zorro-antd/input';
import { FormsModule } from '@angular/forms';
import { NzDrawerModule } from 'ng-zorro-antd/drawer';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { ShareModule } from 'src/app/share/share.module';
import { NzIconModule } from 'ng-zorro-antd/icon';


@NgModule({
  declarations: [SwitchUserComponent, UserDetailComponent, UserListComponent],
  imports: [
    CommonModule,
    FormsModule,
    ShareModule,
    NzTableModule,
    NzInputModule,
    NzDrawerModule,
    NzButtonModule,
    NzSpinModule,
    SwitchUserRoutingModule,
    NzIconModule
  ]
})
export class SwitchUserModule { }
