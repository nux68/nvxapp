import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';
import { RoleGuard4SuperUser } from '../nvx/pages/RoleGuard/role-guard-4-super-user';
import { RoleGuard4Admin } from '../nvx/pages/RoleGuard/role-guard-4-admin';
import { RoleGuard4CompanyPowerAdmin } from '../nvx/pages/RoleGuard/role-guard-4-company-power-admin';
import { RoleGuard4CompanyAdmin } from '../nvx/pages/RoleGuard/role-guard-4-company-admin';
import { RoleGuard4DealerPowerAdmin } from '../nvx/pages/RoleGuard/role-guard-4-dealer-power-admin';
import { RoleGuard4DealerAdmin } from '../nvx/pages/RoleGuard/role-guard-4-dealer-admin';
import { RoleGuard4UserImpersonate } from '../nvx/pages/RoleGuard/role-guard-4-user-impersonate';
import { RoleGuard4DealerList } from '../nvx/pages/RoleGuard/role-guard-4-dealer-list';
import { RoleGuard4CompanyList } from '../nvx/pages/RoleGuard/role-guard-4-company-list';
import { RoleGuard4UserCompanyList } from '../nvx/pages/RoleGuard/role-guard-4-user-company-list';
import { RoleGuard4FinancialAdvisorPowerAdmin } from '../nvx/pages/RoleGuard/role-guard-4-financial-advisor-power-admin';
import { RoleGuard4FinancialAdvisorAdmin } from '../nvx/pages/RoleGuard/role-guard-4-financia-ladvisor-admin';
import { RoleGuard4FinancialAdvisorList } from '../nvx/pages/RoleGuard/role-guard-4-financial-advisor-list';
import { RoleGuard4User } from '../nvx/pages/RoleGuard/role-guard-4-user';



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
  { path: 'userimpersonate', loadChildren: () => import('../nvx/pages/user-impersonate-page/user-impersonate-page.module').then(m => m.UserImpersonatePageModule), canActivate: [RoleGuard4UserImpersonate] },
  { path: 'user', loadChildren: () => import('../nvx/pages/user-page/user-page.module').then(m => m.UserPageModule), canActivate: [RoleGuard4User] },
  

  { path: 'superuser', loadChildren: () => import('../nvx/pages/super-user-page/super-user-page.module').then(m => m.SuperUserPageModule), canActivate: [RoleGuard4SuperUser] },
  { path: 'poweradmin', loadChildren: () => import('../nvx/pages/power-admin-page/power-admin-page.module').then(m => m.PowerAdminPageModule), canActivate: [RoleGuard4Admin] },
  { path: 'admin', loadChildren: () => import('../nvx/pages/admin-page/admin-page.module').then(m => m.AdminPageModule), canActivate: [RoleGuard4Admin] },

  { path: 'companypoweradmin', loadChildren: () => import('../nvx/pages/company-power-admin-page/company-power-admin-page.module').then(m => m.CompanyPowerAdminPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
  { path: 'companyadmin', loadChildren: () => import('../nvx/pages/company-admin-page/company-admin-page.module').then(m => m.CompanyAdminPageModule), canActivate: [RoleGuard4CompanyAdmin] },
  { path: 'companylist', loadChildren: () => import('../nvx/pages/company-list-page/company-list-page.module').then(m => m.CompanyListPageModule), canActivate: [RoleGuard4CompanyList] },
  { path: 'usercompanylist', loadChildren: () => import('../nvx/pages/user-company-list-page/user-company-list-page.module').then(m => m.UserCompanyListPageModule), canActivate: [RoleGuard4UserCompanyList] },

  { path: 'dealerpoweradmin', loadChildren: () => import('../nvx/pages/dealer-power-admin-page/dealer-power-admin-page.module').then(m => m.DealerPowerAdminPageModule), canActivate: [RoleGuard4DealerPowerAdmin] },
  { path: 'dealeradmin', loadChildren: () => import('../nvx/pages/dealer-admin-page/dealer-admin-page.module').then(m => m.DealerAdminPageModule), canActivate: [RoleGuard4DealerAdmin] },
  { path: 'dealerlist', loadChildren: () => import('../nvx/pages/dealer-list-page/dealer-list-page.module').then(m => m.DealerListPageModule), canActivate: [RoleGuard4DealerList] },

  { path: 'financialadvisorpoweradmin', loadChildren: () => import('../nvx/pages/financial-advisor-power-admin-page/financial-advisor-power-admin-page.module').then(m => m.FinancialAdvisorPowerAdminPageModule), canActivate: [RoleGuard4FinancialAdvisorPowerAdmin] },
  { path: 'financialadvisoradmin', loadChildren: () => import('../nvx/pages/financial-advisor-admin-page/financial-advisor-admin-page.module').then(m => m.FinancialAdvisorAdminPageModule), canActivate: [RoleGuard4FinancialAdvisorAdmin] },
  { path: 'financialadvisorlist', loadChildren: () => import('../nvx/pages/financial-advisor-list-page/financial-advisor-list-page.module').then(m => m.FinancialAdvisorListPageModule), canActivate: [RoleGuard4FinancialAdvisorList] },


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
