import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SegmentsIndexComponent } from './segments-index.component';

const routes: Routes = [{
  path: '',
  data: { title: 'segments index' },
  component: SegmentsIndexComponent
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SegmentsIndexRoutingModule { }
