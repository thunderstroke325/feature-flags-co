import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ExperimentationComponent } from './experimentation.component';

const routes: Routes = [{
  path: '',
  component: ExperimentationComponent
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ExperimentationRoutingModule { }
