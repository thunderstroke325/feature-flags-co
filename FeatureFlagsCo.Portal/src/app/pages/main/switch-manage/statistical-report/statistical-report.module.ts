import { NgModule } from '@angular/core';

import { StatisticalReportRoutingModule } from './statistical-report-routing.module';
import { StatisticalReportComponent } from './statistical-report.component';
import { ComponentsModule } from '../components/components.module';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { DatePipe } from '@angular/common';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import {ShareModule} from "../../../../share/share.module";


@NgModule({
  declarations: [StatisticalReportComponent],
    imports: [
        ComponentsModule,
        NzSpinModule,
        NzButtonModule,
        StatisticalReportRoutingModule,
        ShareModule
    ],
  providers: [DatePipe]
})
export class StatisticalReportModule { }
