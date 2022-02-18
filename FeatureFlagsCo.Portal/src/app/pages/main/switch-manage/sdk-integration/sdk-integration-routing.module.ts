import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SDKIntegrationComponent } from './sdk-integration.component';

const routes: Routes = [
  {
    path: '',
    component: SDKIntegrationComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SDKIntegrationRoutingModule { }
