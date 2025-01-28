import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  //{
  //  path: '',
  //  redirectTo: 'folder/inbox',
  //  pathMatch: 'full'
  //},
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'home', loadChildren: () => import('../nvx/pages/home-page/home-page.module').then(m => m.HomePageModule) },
  { path: 'login', loadChildren: () => import('../nvx/pages/login-page/login-page.module').then(m => m.LoginPageModule) },
  { path: 'logout', loadChildren: () => import('../nvx/pages/logout-page/logout-page.module').then(m => m.LogoutPageModule) },


  {
    path: 'folder/:id',
    loadChildren: () => import('./folder/folder.module').then( m => m.FolderPageModule)
  }
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules })
  ],
  exports: [RouterModule]
})
export class AppRoutingModule {}
