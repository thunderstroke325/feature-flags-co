import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SegmentsComponent } from './segments.component';
// import { SwicthSettingResolver } from './switch-setting/swicth-setting-resolver.service';
// import { TargetConditionsResolver } from './target-conditions/target-conditions-resolver.service';

const routes: Routes = [
  {
    path: '',
    data: {
      breadcrumb: '用户组'
    },
    component: SegmentsComponent,
    children: [
      {
        path: '',
        loadChildren: () => import("./segments-index/segments-index.module").then(m => m.SegmentsIndexModule)
      },
      // {
      //   path: ':id/setting',
      //   resolve: { switchInfo: SwicthSettingResolver },
      //   loadChildren: () => import("./segments-index/switch-setting.module").then(m => m.SwitchSettingModule),
      //   data: {
      //     breadcrumb: '用户组'
      //   }
      // },
      // {
      //   path: ':id/targeting',
      //   resolve: { switchInfo: TargetConditionsResolver },
      //   loadChildren: () => import("./segments-index/target-conditions.module").then(m => m.TargetConditionsModule),
      //   data: {
      //     breadcrumb: '用户组'
      //   }
      // },
      {
        path: '',
        redirectTo: '/segments'
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
  providers: [
    // SwicthSettingResolver,
    // TargetConditionsResolver
  ]
})
export class SegmentsRoutingModule { }
