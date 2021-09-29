import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SwitchArchiveComponent } from './switch-archive.component';
import { SwitchArchiveRoutingModule } from './switch-archive-routing.module';
import { FormsModule } from '@angular/forms';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzMessageModule } from 'ng-zorro-antd/message';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import {NzTableModule} from "ng-zorro-antd/table";
import {NzSwitchModule} from "ng-zorro-antd/switch";


@NgModule({
  declarations: [SwitchArchiveComponent],
  imports: [
    CommonModule,
    FormsModule,
    NzSpinModule,
    NzButtonModule,
    NzMessageModule,
    SwitchArchiveRoutingModule,
    NzTableModule,
    NzSwitchModule
  ]
})
export class SwitchArchiveModule { }
