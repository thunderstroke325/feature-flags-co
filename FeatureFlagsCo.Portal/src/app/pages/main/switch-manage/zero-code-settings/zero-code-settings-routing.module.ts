import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ZeroCodeSettingsComponent } from './zero-code-settings.component';

const routes: Routes = [{
  path: '',
  component: ZeroCodeSettingsComponent
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ZeroCodeSettingsRoutingModule { }
