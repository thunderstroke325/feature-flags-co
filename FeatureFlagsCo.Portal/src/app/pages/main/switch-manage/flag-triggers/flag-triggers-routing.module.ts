import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FlagTriggersComponent } from './flag-triggers.component';

const routes: Routes = [{
  path: '',
  component: FlagTriggersComponent
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FlagTriggersRoutingModule { }
