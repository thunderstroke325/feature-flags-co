import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsComponent } from './analytics.component';
import { AnalyticsRoutingModule } from './analytics-routing.module';
import { FormsModule } from '@angular/forms';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzMessageModule } from 'ng-zorro-antd/message';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzListModule } from 'ng-zorro-antd/list';
import { NzSpaceModule } from 'ng-zorro-antd/space';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { ShareModule } from 'src/app/share/share.module';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzStatisticModule } from 'ng-zorro-antd/statistic';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NewReportComponent } from './components/new-report/new-report.component';
import { DataSourcesComponent } from './components/data-sources/data-sources.component';
import { NzTableModule } from 'ng-zorro-antd/table';

@NgModule({
  declarations: [AnalyticsComponent, NewReportComponent, DataSourcesComponent],
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
    NzGridModule,
    NzCardModule,
    NzStatisticModule,
    NzPopconfirmModule,
    NzDatePickerModule,
    NzModalModule,
    NzFormModule,
    NzInputModule,
    NzSelectModule,
    NzTableModule,
    ShareModule,
    AnalyticsRoutingModule
  ]
})
export class AnalyticsModule { }
