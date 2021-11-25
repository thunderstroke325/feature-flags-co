import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ZeroCodeSettingsRoutingModule } from './zero-code-settings-routing.module';
import { ComponentsModule } from '../components/components.module';
import { ZeroCodeSettingsComponent } from './zero-code-settings.component';
import { NzEmptyModule } from 'ng-zorro-antd/empty';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { FormsModule } from '@angular/forms';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzToolTipModule } from 'ng-zorro-antd/tooltip';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSpaceModule } from 'ng-zorro-antd/space';
import { NzCodeEditorModule } from 'ng-zorro-antd/code-editor';
import { NzDividerModule } from 'ng-zorro-antd/divider';

@NgModule({
  declarations: [ZeroCodeSettingsComponent],
  imports: [
    CommonModule,
    NzInputModule,
    FormsModule,
    NzEmptyModule,
    NzSelectModule,
    NzDatePickerModule,
    NzTableModule,
    NzButtonModule,
    NzIconModule,
    NzCardModule,
    NzSpinModule,
    NzToolTipModule,
    NzTagModule,
    NzPopconfirmModule,
    NzDropDownModule,
    NzSpaceModule,
    NzCodeEditorModule,
    NzDividerModule,
    ComponentsModule,
    ZeroCodeSettingsRoutingModule
  ]
})
export class ZeroCodeSettingsModule { }
