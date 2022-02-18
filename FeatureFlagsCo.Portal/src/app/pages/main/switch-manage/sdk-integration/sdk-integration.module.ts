import { NgModule } from '@angular/core';

import { SDKIntegrationRoutingModule } from './sdk-integration-routing.module';
import { SDKIntegrationComponent } from './sdk-integration.component';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { ComponentsModule } from '../components/components.module';
import { NzInputModule } from 'ng-zorro-antd/input';
import { FormsModule } from '@angular/forms';
import { NzMessageModule } from 'ng-zorro-antd/message';
import { NzTypographyModule } from 'ng-zorro-antd/typography';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';;
import { NzTabsModule } from 'ng-zorro-antd/tabs';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { SDKJSComponent } from './sdk-js/sdk-js.component';
import { SDKJavaComponent } from './sdk-java/sdk-java.component';
import { NzEmptyModule } from 'ng-zorro-antd/empty';

@NgModule({
  declarations: [
    SDKIntegrationComponent,
    SDKJSComponent,
    SDKJavaComponent,
  ],
  imports: [
    NzButtonModule,
    NzInputModule,
    NzMessageModule,
    NzTypographyModule,
    NzDividerModule,
    NzTableModule,
    NzIconModule,
    NzPopconfirmModule,
    NzTabsModule,
    NzRadioModule,
    NzEmptyModule,
    FormsModule,
    ComponentsModule,
    SDKIntegrationRoutingModule
  ]
})
export class SDKIntegrationModule { }
