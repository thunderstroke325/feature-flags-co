import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataSyncComponent } from './data-sync.component';
import { DataSyncRoutingModule } from './data-sync-routing.module';
import { FormsModule } from '@angular/forms';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzMessageModule } from 'ng-zorro-antd/message';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzListModule } from 'ng-zorro-antd/list';
import { NzSpaceModule } from 'ng-zorro-antd/space';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { ShareModule } from 'src/app/share/share.module';


@NgModule({
  declarations: [DataSyncComponent],
  imports: [
    CommonModule,
    FormsModule,
    NzSpinModule,
    NzButtonModule,
    NzMessageModule,
    NzListModule,
    NzSpaceModule,
    NzDividerModule,
    NzIconModule,
    ShareModule,
    DataSyncRoutingModule
  ]
})
export class DataSyncModule { }
