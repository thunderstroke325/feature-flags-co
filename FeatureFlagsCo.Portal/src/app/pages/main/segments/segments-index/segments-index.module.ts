import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SegmentsIndexRoutingModule } from './segments-index-routing.module';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NzInputModule } from 'ng-zorro-antd/input';
import { SegmentsIndexComponent } from './segments-index.component';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzModalModule  } from 'ng-zorro-antd/modal';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzMessageModule } from 'ng-zorro-antd/message';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzSwitchModule } from 'ng-zorro-antd/switch';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzTreeViewModule } from "ng-zorro-antd/tree-view";
import { NzTreeSelectModule } from "ng-zorro-antd/tree-select";
import { NzTransferModule } from "ng-zorro-antd/transfer";
import { NzTagModule } from "ng-zorro-antd/tag";
import { NzToolTipModule } from "ng-zorro-antd/tooltip";
import { NzFormModule } from "ng-zorro-antd/form";
import { ShareModule } from "../../../../share/share.module";


@NgModule({
  declarations: [SegmentsIndexComponent],
  imports: [
    CommonModule,
    NzSelectModule,
    NzInputModule,
    NzButtonModule,
    NzGridModule,
    NzModalModule,
    NzMessageModule,
    FormsModule,
    NzTableModule,
    NzSpinModule,
    SegmentsIndexRoutingModule,
    NzSwitchModule,
    NzIconModule,
    NzTreeViewModule,
    NzTreeSelectModule,
    NzTransferModule,
    NzTagModule,
    NzToolTipModule,
    NzFormModule,
    ReactiveFormsModule,
    ShareModule
  ]
})
export class SegmentsIndexModule { }
