import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { RoleGuard4Admin } from '../../pages/RoleGuard/infrastructure/role-guard-4-admin';
import { RoleGuard4CompanyAdmin } from '../../pages/RoleGuard/infrastructure/role-guard-4-company-admin';
import { RoleGuard4CompanyEdit } from '../../pages/RoleGuard/infrastructure/role-guard-4-company-edit';
import { RoleGuard4CompanyList } from '../../pages/RoleGuard/infrastructure/role-guard-4-company-list';
import { RoleGuard4CompanyPowerAdmin } from '../../pages/RoleGuard/infrastructure/role-guard-4-company-power-admin';
import { RoleGuard4DealerAdmin } from '../../pages/RoleGuard/infrastructure/role-guard-4-dealer-admin';
import { RoleGuard4DealerEdit } from '../../pages/RoleGuard/infrastructure/role-guard-4-dealer-edit';
import { RoleGuard4DealerList } from '../../pages/RoleGuard/infrastructure/role-guard-4-dealer-list';
import { RoleGuard4DealerPowerAdmin } from '../../pages/RoleGuard/infrastructure/role-guard-4-dealer-power-admin';
import { RoleGuard4FinancialAdvisorAdmin } from '../../pages/RoleGuard/infrastructure/role-guard-4-financial-advisor-admin';
import { RoleGuard4FinancialAdvisorEdit } from '../../pages/RoleGuard/infrastructure/role-guard-4-financial-advisor-edit';
import { RoleGuard4FinancialAdvisorList } from '../../pages/RoleGuard/infrastructure/role-guard-4-financial-advisor-list';
import { RoleGuard4FinancialAdvisorPowerAdmin } from '../../pages/RoleGuard/infrastructure/role-guard-4-financial-advisor-power-admin';
import { RoleGuard4SuperUser } from '../../pages/RoleGuard/infrastructure/role-guard-4-super-user';
import { RoleGuard4User } from '../../pages/RoleGuard/infrastructure/role-guard-4-user';
import { RoleGuard4UserCompanyList } from '../../pages/RoleGuard/infrastructure/role-guard-4-user-company-list';
import { RoleGuard4UserImpersonate } from '../../pages/RoleGuard/infrastructure/role-guard-4-user-impersonate';

@Injectable({
  providedIn: 'root'
})
export class RouteInfrastructureService {

  constructor() { }

  public getRoutes(): Routes {
    return [
      { path: '', redirectTo: '/home', pathMatch: 'full' },
      { path: 'home', loadChildren: () => import('../../../nvx/pages/infrastructure/home-page/home-page.module').then(m => m.HomePageModule) },
      { path: 'login', loadChildren: () => import('../../pages/infrastructure/login-page/login-page.module').then(m => m.LoginPageModule) },
      { path: 'logout', loadChildren: () => import('../../pages/infrastructure/logout-page/logout-page.module').then(m => m.LogoutPageModule) },
      { path: 'userimpersonate', loadChildren: () => import('../../pages/infrastructure/user-impersonate-page/user-impersonate-page.module').then(m => m.UserImpersonatePageModule), canActivate: [RoleGuard4UserImpersonate] },
      { path: 'user', loadChildren: () => import('../../pages/infrastructure/user-page/user-page.module').then(m => m.UserPageModule), canActivate: [RoleGuard4User] },


      { path: 'superuser', loadChildren: () => import('../../pages/infrastructure/super-user-page/super-user-page.module').then(m => m.SuperUserPageModule), canActivate: [RoleGuard4SuperUser] },
      { path: 'poweradmin', loadChildren: () => import('../../pages/infrastructure/power-admin-page/power-admin-page.module').then(m => m.PowerAdminPageModule), canActivate: [RoleGuard4Admin] },
      { path: 'admin', loadChildren: () => import('../../pages/infrastructure/admin-page/admin-page.module').then(m => m.AdminPageModule), canActivate: [RoleGuard4Admin] },

      { path: 'companypoweradmin', loadChildren: () => import('../../pages/infrastructure/company-power-admin-page/company-power-admin-page.module').then(m => m.CompanyPowerAdminPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'companyadmin', loadChildren: () => import('../../pages/infrastructure/company-admin-page/company-admin-page.module').then(m => m.CompanyAdminPageModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'companylist', loadChildren: () => import('../../pages/infrastructure/company-list-page/company-list-page.module').then(m => m.CompanyListPageModule), canActivate: [RoleGuard4CompanyList] },
      { path: 'companyedit', loadChildren: () => import('../../pages/infrastructure/company-edit-page/company-edit-page.module').then(m => m.CompanyEditPageModule), canActivate: [RoleGuard4CompanyEdit] },

      

      { path: 'dealerpoweradmin', loadChildren: () => import('../../pages/infrastructure/dealer-power-admin-page/dealer-power-admin-page.module').then(m => m.DealerPowerAdminPageModule), canActivate: [RoleGuard4DealerPowerAdmin] },
      { path: 'dealeradmin', loadChildren: () => import('../../pages/infrastructure/dealer-admin-page/dealer-admin-page.module').then(m => m.DealerAdminPageModule), canActivate: [RoleGuard4DealerAdmin] },
      { path: 'dealerlist', loadChildren: () => import('../../pages/infrastructure/dealer-list-page/dealer-list-page.module').then(m => m.DealerListPageModule), canActivate: [RoleGuard4DealerList] },
      { path: 'dealeredit', loadChildren: () => import('../../pages/infrastructure/dealer-edit-page/dealer-edit-page.module').then(m => m.DealerEditPageModule), canActivate: [RoleGuard4DealerEdit] },

      { path: 'financialadvisorpoweradmin', loadChildren: () => import('../../pages/infrastructure/financial-advisor-power-admin-page/financial-advisor-power-admin-page.module').then(m => m.FinancialAdvisorPowerAdminPageModule), canActivate: [RoleGuard4FinancialAdvisorPowerAdmin] },
      { path: 'financialadvisoradmin', loadChildren: () => import('../../pages/infrastructure/financial-advisor-admin-page/financial-advisor-admin-page.module').then(m => m.FinancialAdvisorAdminPageModule), canActivate: [RoleGuard4FinancialAdvisorAdmin] },
      { path: 'financialadvisorlist', loadChildren: () => import('../../pages/infrastructure/financial-advisor-list-page/financial-advisor-list-page.module').then(m => m.FinancialAdvisorListPageModule), canActivate: [RoleGuard4FinancialAdvisorList] },
      { path: 'financialadvisoredit', loadChildren: () => import('../../pages/infrastructure/financial-advisor-edit-page/financial-advisor-edit-page.module').then(m => m.FinancialAdvisorEditPageModule), canActivate: [RoleGuard4FinancialAdvisorEdit] },

      { path: 'userlist', loadChildren: () => import('../../pages/infrastructure/user-list-page/user-list-page.module').then(m => m.UserListPageModule), canActivate: [RoleGuard4Admin] },
      { path: 'useredit', loadChildren: () => import('../../pages/infrastructure/user-edit-page/user-edit-page.module').then(m => m.UserEditPageModule), canActivate: [RoleGuard4Admin] },
      { path: 'userdealerlist', loadChildren: () => import('../../pages/infrastructure/user-dealer-list-page/user-dealer-list-page.module').then(m => m.UserDealerListPageModule), canActivate: [RoleGuard4DealerAdmin] },
      { path: 'userdealeredit', loadChildren: () => import('../../pages/infrastructure/user-dealer-edit-page/user-dealer-edit-page.module').then(m => m.UserDealerEditPageModule), canActivate: [RoleGuard4DealerAdmin] },

      { path: 'userfinancialadvisorlist', loadChildren: () => import('../../pages/infrastructure/user-financial-advisor-list-page/user-financial-advisor-list-page.module').then(m => m.UserFinancialAdvisorListPageModule), canActivate: [RoleGuard4FinancialAdvisorAdmin] },
      { path: 'userfinancialadvisoredit', loadChildren: () => import('../../pages/infrastructure/user-financial-advisor-edit-page/user-financial-advisor-edit-page.module').then(m => m.UserFinancialAdvisorEditPageModule), canActivate: [RoleGuard4FinancialAdvisorAdmin] },

      { path: 'usercompanylist', loadChildren: () => import('../../pages/infrastructure/user-company-list-page/user-company-list-page.module').then(m => m.UserCompanyListPageModule), canActivate: [RoleGuard4UserCompanyList] },
      { path: 'usercompanyedit', loadChildren: () => import('../../pages/infrastructure/user-company-edit-page/user-company-edit-page.module').then(m => m.UserCompanyEditPageModule), canActivate: [RoleGuard4UserCompanyList] },
      { path: 'usercompanywizard', loadChildren: () => import('../../pages/infrastructure/user-company-wizard-page/user-company-wizard-page.module').then(m => m.UserCompanyWizardPageModule), canActivate: [RoleGuard4UserCompanyList] },


    ];
  }

}
