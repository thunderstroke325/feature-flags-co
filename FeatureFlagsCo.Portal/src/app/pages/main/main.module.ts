import { NgModule, ErrorHandler } from '@angular/core';
import { ApplicationinsightsAngularpluginErrorService } from '@microsoft/applicationinsights-angularplugin-js';
import { MainRoutingModule } from './main-routing.module';
import { MainComponent } from './main.component';
import { ShareModule } from 'src/app/share/share.module';
import { NzBreadCrumbModule } from 'ng-zorro-antd/breadcrumb';

@NgModule({
  declarations: [MainComponent],
  imports: [
    ShareModule,
    NzBreadCrumbModule,
    MainRoutingModule
  ],
  providers: [
    {
      provide: ErrorHandler,
      useClass: ApplicationinsightsAngularpluginErrorService
    }
  ]
})
export class MainModule { }
