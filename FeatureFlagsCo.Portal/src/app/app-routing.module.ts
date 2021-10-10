import { NgModule } from '@angular/core';
import { Routes, RouterModule, PreloadAllModules } from '@angular/router';
import { AuthGuard } from './auth.guard';
import { LoginGuard } from './login.guard';
import { AccountProjectEnvResolver } from './account-preject-env-resolver.service';

const routes: Routes = [
  {
    path: 'login',
    canActivate: [LoginGuard],
    loadChildren: () => import("./pages/login/login.module").then(m => m.LoginModule)
  },{
    path: '',
    canActivate: [AuthGuard],
    loadChildren: () => import("./pages/main/main.module").then(m => m.MainModule),
    resolve: {
      _: AccountProjectEnvResolver // Ensure that the current account and project env are loaded before activate the main module
    },
  }
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, {
      preloadingStrategy: PreloadAllModules
    })
  ],
  exports: [RouterModule]
})
export class AppRoutingModule { }
