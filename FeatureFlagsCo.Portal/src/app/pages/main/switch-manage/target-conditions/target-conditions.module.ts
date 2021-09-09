import { NgModule } from '@angular/core';

import { TargetConditionsRoutingModule } from './target-conditions-routing.module';
import { TargetConditionsComponent } from './target-conditions.component';
import { ComponentsModule } from '../components/components.module';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzMessageModule } from 'ng-zorro-antd/message';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { SafeHtmlPipe } from 'src/app/share/pipes/safe-html.pipe';
import { NzSelectModule } from 'ng-zorro-antd/select';

@NgModule({
  declarations: [TargetConditionsComponent, SafeHtmlPipe ],
  imports: [
    ComponentsModule,
    NzButtonModule,
    NzIconModule,
    NzMessageModule,
    NzSpinModule,
    NzDropDownModule,
    NzModalModule,
    NzSelectModule,
    TargetConditionsRoutingModule
  ]
})
export class TargetConditionsModule { }
