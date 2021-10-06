import { NgModule } from '@angular/core';
import { ExperimentsRoutingModule } from './experiments-routing.module';
import { CommonModule } from '@angular/common';
import { ExperimentsComponent } from './experiments.component';
import { NzTabsModule } from 'ng-zorro-antd/tabs';
import { OverviewComponent } from './overview/overview.component';
import { MetricsComponent } from './metrics/metrics.component';
import { FormsModule } from '@angular/forms';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzEmptyModule } from 'ng-zorro-antd/empty';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzInputModule } from 'ng-zorro-antd/input';
import { ShareModule } from 'src/app/share/share.module';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzSpaceModule } from 'ng-zorro-antd/space';

@NgModule({
  declarations: [
    ExperimentsComponent,
    OverviewComponent,
    MetricsComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    NzTabsModule,
    NzCardModule,
    NzSpinModule,
    NzSelectModule,
    NzEmptyModule,
    NzTableModule,
    NzButtonModule,
    NzIconModule,
    NzInputModule,
    NzTagModule,
    NzSpaceModule,
    ShareModule,
    ExperimentsRoutingModule
  ],
  providers: [
  ]
})
export class ExperimentsModule { }
