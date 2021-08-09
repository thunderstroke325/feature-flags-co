import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { FormsModule } from '@angular/forms';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSliderModule } from 'ng-zorro-antd/slider';
import { NzPaginationModule } from 'ng-zorro-antd/pagination';

import { NavBtnsComponent } from './nav-btns/nav-btns.component';
import { LayoutComponent } from './layout/layout.component';
import { UpperSwitchComponent } from './upper-switch/upper-switch.component';
import { TargetUserComponent } from './target-user/target-user.component';
import { FindRuleComponent } from './find-rule/find-rule.component';
import { RuleComponent } from './find-rule/rule/rule.component';
import { ServeComponent } from './find-rule/serve/serve.component';
import { ServeMultistatesComponent } from './find-rule/serve-multistates/serve-multistates.component';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';

@NgModule({
  declarations: [
    NavBtnsComponent,
    LayoutComponent,
    UpperSwitchComponent,
    TargetUserComponent,
    FindRuleComponent,
    RuleComponent,
    ServeComponent ,
    ServeMultistatesComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    NzButtonModule,
    NzIconModule,
    NzInputModule,
    NzSelectModule,
    NzSliderModule,
    NzPaginationModule,
    NzPopconfirmModule
  ],
  exports: [
    NavBtnsComponent,
    LayoutComponent,
    CommonModule,
    UpperSwitchComponent,
    TargetUserComponent,
    FindRuleComponent,
    ServeComponent,
    ServeMultistatesComponent
  ]
})
export class ComponentsModule { }
