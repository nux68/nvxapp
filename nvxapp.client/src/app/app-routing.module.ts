import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';
import { RoleGuard4SuperUser } from '../nvx/pages/RoleGuard/role-guard-4super-user';
import { RoleGuard4PowerAdmin } from '../nvx/pages/RoleGuard/role-guard-4power-admin';
import { RoleGuard4Admin } from '../nvx/pages/RoleGuard/role-guard-4-admin';
import { RoleGuard4DomainPowerAdmin } from '../nvx/pages/RoleGuard/role-guard-4-domain-power-admin';
import { RoleGuard4DomainAdmin } from '../nvx/pages/RoleGuard/role-guard-4-domain-admin';

const routes: Routes = [
  //{
  //  path: '',
  //  redirectTo: 'folder/inbox',
  //  pathMatch: 'full'
  //},
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'home', loadChildren: ()   => import('../nvx/pages/home-page/home-page.module').then(m => m.HomePageModule) },
  { path: 'login', loadChildren: ()  => import('../nvx/pages/login-page/login-page.module').then(m => m.LoginPageModule) },
  { path: 'logout', loadChildren: () => import('../nvx/pages/logout-page/logout-page.module').then(m => m.LogoutPageModule) },

  { path: 'superuser', loadChildren: () => import('../nvx/pages/super-user-page/super-user-page.module').then(m => m.SuperUserPageModule), canActivate: [RoleGuard4SuperUser] },
  { path: 'poweradmin', loadChildren: () => import('../nvx/pages/power-admin-page/power-admin-page.module').then(m => m.PowerAdminPageModule), canActivate: [RoleGuard4PowerAdmin] },
  { path: 'admin', loadChildren: () => import('../nvx/pages/admin-page/admin-page.module').then(m => m.AdminPageModule), canActivate: [RoleGuard4Admin] },

  { path: 'domainpoweradmin', loadChildren: () => import('../nvx/pages/domain-power-admin-page/domain-power-admin-page.module').then(m => m.DomainPowerAdminPageModule), canActivate: [RoleGuard4DomainPowerAdmin] },
  { path: 'domainadmin', loadChildren: () => import('../nvx/pages/domain-admin-page/domain-admin-page.module').then(m => m.DomainAdminPageModule), canActivate: [RoleGuard4DomainAdmin] },


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
