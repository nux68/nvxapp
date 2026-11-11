import { Injectable } from '@angular/core';
import { Routes } from '@angular/router';
import { RoleGuard4CompanyAdmin } from '../../pages/RoleGuard/infrastructure/role-guard-4-company-admin';
import { RoleGuard4CompanyList } from '../../pages/RoleGuard/infrastructure/role-guard-4-company-list';
import { RoleGuard4User } from '../../pages/RoleGuard/infrastructure/role-guard-4-user';
import { RoleGuard4CompanyPowerAdmin } from '../../pages/RoleGuard/infrastructure/role-guard-4-company-power-admin';
import { RoleGuard4JustClockRequest } from '../../pages/RoleGuard/infrastructure/role-guard-4-just-clock-request';
import { licenseLevelGuard } from '../../pages/RoleGuard/infrastructure/license-level-guard';
import { ApplicationType, LivelloLicenza } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';

@Injectable({
  providedIn: 'root'
})
export class RouteAttendanceTrackingService {

  constructor() { }

  public getRoutes(): Routes {
    return [
      //user
      // note spese: richiesta e consultazione dell'utente, esame del CompanyPowerAdmin, tipi di spesa (parametri)
      { path: 'notaspesarequestuser', loadChildren: () => import('../../pages/GestionePresenze/nota-spesa-request-user-page/nota-spesa-request-user-page.module').then(m => m.NotaSpesaRequestUserPageModule), canActivate: [RoleGuard4User] },
      { path: 'notaspeseuser', loadChildren: () => import('../../pages/GestionePresenze/nota-spese-user-page/nota-spese-user-page.module').then(m => m.NotaSpeseUserPageModule), canActivate: [RoleGuard4User] },
      { path: 'notaspeseadmin', loadChildren: () => import('../../pages/GestionePresenze/nota-spese-admin-page/nota-spese-admin-page.module').then(m => m.NotaSpeseAdminPageModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'tipospesalist', loadChildren: () => import('../../pages/GestionePresenze/tipo-spesa-list-page/tipo-spesa-list-page.module').then(m => m.TipoSpesaListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      // formule personalizzate del calcolo giornata: CompanyPowerAdmin e piano Pro
      { path: 'formulelist', loadChildren: () => import('../../pages/GestionePresenze/formule-list-page/formule-list-page.module').then(m => m.FormuleListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin, licenseLevelGuard(ApplicationType.AttendanceTracking, LivelloLicenza.Pro)] },
      { path: 'formuleedit', loadChildren: () => import('../../pages/GestionePresenze/formule-edit-page/formule-edit-page.module').then(m => m.FormuleEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin, licenseLevelGuard(ApplicationType.AttendanceTracking, LivelloLicenza.Pro)] },
      { path: 'profiloformulelist', loadChildren: () => import('../../pages/GestionePresenze/profilo-formule-list-page/profilo-formule-list-page.module').then(m => m.ProfiloFormuleListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin, licenseLevelGuard(ApplicationType.AttendanceTracking, LivelloLicenza.Pro)] },
      { path: 'profiloformuleedit', loadChildren: () => import('../../pages/GestionePresenze/profilo-formule-edit-page/profilo-formule-edit-page.module').then(m => m.ProfiloFormuleEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin, licenseLevelGuard(ApplicationType.AttendanceTracking, LivelloLicenza.Pro)] },
      { path: 'tipospesaedit', loadChildren: () => import('../../pages/GestionePresenze/tipo-spesa-edit-page/tipo-spesa-edit-page.module').then(m => m.TipoSpesaEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'usertimesheet', loadChildren: () => import('../../pages/GestionePresenze/time-sheet-user-page/time-sheet-user-page.module').then(m => m.TimeSheetUserModule), canActivate: [RoleGuard4User] },
      { path: 'timeclockuser', loadChildren: () => import('../../pages/GestionePresenze/time-clock-user-page/time-clock-user-page.module').then(m => m.TimeClockUserPageModule), canActivate: [RoleGuard4User] },
      { path: 'requestjustificationuser', loadChildren: () => import('../../pages/GestionePresenze/request-justification-user-page/request-justification-user-page.module').then(m => m.RequestJustificationUserPageModule), canActivate: [RoleGuard4JustClockRequest] },
      { path: 'requestclockinguser', loadChildren: () => import('../../pages/GestionePresenze/request-clocking-user-page/request-clocking-user-page.module').then(m => m.RequestClockingUserPageModule), canActivate: [RoleGuard4JustClockRequest] },
      { path: 'requestlistuser', loadChildren: () => import('../../pages/GestionePresenze/request-list-user-page/request-list-user-page.module').then(m => m.RequestListUserPageModule), canActivate: [RoleGuard4User] },
      { path: 'timbraturaqr', loadChildren: () => import('../../pages/GestionePresenze/timbratura-qr-page/timbratura-qr-page.module').then(m => m.TimbraturaQrPageModule), canActivate: [RoleGuard4User] },
      { path: 'lavoroagile', loadChildren: () => import('../../pages/GestionePresenze/lavoro-agile-page/lavoro-agile-page.module').then(m => m.LavoroAgilePageModule), canActivate: [RoleGuard4User] },
      { path: 'mieiturni', loadChildren: () => import('../../pages/GestionePresenze/miei-turni-page/miei-turni-page.module').then(m => m.MieiTurniPageModule), canActivate: [RoleGuard4User] },

      //power admin
      { path: 'exportcausali', loadChildren: () => import('../../pages/GestionePresenze/export-causali-page/export-causali-page.module').then(m => m.TimeSheetPowerAdminPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'activitystatistics', loadChildren: () => import('../../pages/GestionePresenze/activity-statistics-page/activity-statistics-page.module').then(m => m.ActivityStatisticsPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'presentstaff', loadChildren: () => import('../../pages/GestionePresenze/present-staff-page/present-staff-page.module').then(m => m.PresentStaffPageModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'vacationplan', loadChildren: () => import('../../pages/GestionePresenze/vacation-plan-page/vacation-plan-page.module').then(m => m.VacationPlanPageModule), canActivate: [RoleGuard4CompanyAdmin] },
      // turni e pianificazione (AT-TURNI)
      { path: 'pianificazioneturni', loadChildren: () => import('../../pages/GestionePresenze/pianificazione-turni-page/pianificazione-turni-page.module').then(m => m.PianificazioneTurniPageModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'turnolist', loadChildren: () => import('../../pages/GestionePresenze/turno-list-page/turno-list-page.module').then(m => m.TurnoListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'turnoedit', loadChildren: () => import('../../pages/GestionePresenze/turno-edit-page/turno-edit-page.module').then(m => m.TurnoEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      // smart working (AT-SW)
      { path: 'accordiswlist', loadChildren: () => import('../../pages/GestionePresenze/accordi-sw-list-page/accordi-sw-list-page.module').then(m => m.AccordiSwListPageModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'accordoswedit', loadChildren: () => import('../../pages/GestionePresenze/accordo-sw-edit-page/accordo-sw-edit-page.module').then(m => m.AccordoSwEditPageModule), canActivate: [RoleGuard4CompanyAdmin] },
      // timbratura evoluta (AT-TIMB)
      { path: 'timbraturasede', loadChildren: () => import('../../pages/GestionePresenze/timbratura-sede-page/timbratura-sede-page.module').then(m => m.TimbraturaSedePageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'timbraturapin', loadChildren: () => import('../../pages/GestionePresenze/timbratura-pin-page/timbratura-pin-page.module').then(m => m.TimbraturaPinPageModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'timbraturapostazione', loadChildren: () => import('../../pages/GestionePresenze/timbratura-postazione-page/timbratura-postazione-page.module').then(m => m.TimbraturaPostazionePageModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'timbraturakiosk', loadChildren: () => import('../../pages/GestionePresenze/timbratura-kiosk-page/timbratura-kiosk-page.module').then(m => m.TimbraturaKioskPageModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'turnocoperture', loadChildren: () => import('../../pages/GestionePresenze/turno-coperture-page/turno-coperture-page.module').then(m => m.TurnoCoperturePageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },

      { path: 'attendanceanomalies', loadChildren: () => import('../../pages/GestionePresenze/anomalie-page/anomalie-page.module').then(m => m.AnomaliePageModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'jobmargins', loadChildren: () => import('../../pages/GestionePresenze/cdg-commesse-page/cdg-commesse-page.module').then(m => m.CdgCommessePageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'quadraturamensile', loadChildren: () => import('../../pages/GestionePresenze/quadratura-mensile-page/quadratura-mensile-page.module').then(m => m.QuadraturaMensilePageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'festivitalist', loadChildren: () => import('../../pages/GestionePresenze/festivita-list-page/festivita-list-page.module').then(m => m.FestivitaListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'festivitaedit', loadChildren: () => import('../../pages/GestionePresenze/festivita-edit-page/festivita-edit-page.module').then(m => m.FestivitaEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },

      { path: 'poweradmintimesheet', loadChildren: () => import('../../pages/GestionePresenze/time-sheet-power-admin-page/time-sheet-power-admin-page.module').then(m => m.TimeSheetPowerAdminPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'justificationlist', loadChildren: () => import('../../pages/GestionePresenze/justification-list-page/justification-list-page.module').then(m => m.JustificationListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'justificationedit', loadChildren: () => import('../../pages/GestionePresenze/justification-edit-page/justification-edit-page.module').then(m => m.JustificationEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'causalilist', loadChildren: () => import('../../pages/GestionePresenze/causali-list-page/causali-list-page.module').then(m => m.CausaliListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'causaliedit', loadChildren: () => import('../../pages/GestionePresenze/causali-edit-page/causali-edit-page.module').then(m => m.CausaliEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'departmentlist', loadChildren: () => import('../../pages/GestionePresenze/department-list-page/department-list-page.module').then(m => m.DepartmentListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'departmentedit', loadChildren: () => import('../../pages/GestionePresenze/department-edit-page/department-edit-page.module').then(m => m.DepartmentEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'companycfgedit', loadChildren: () => import('../../pages/GestionePresenze/company-cfg-page/company-cfg-page.module').then(m => m.CompanyCfgPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'userdepartmentlist', loadChildren: () => import('../../pages/GestionePresenze/user-department-list-page/user-department-list-page.module').then(m => m.UserDepartmentListPageModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'competencelist', loadChildren: () => import('../../pages/GestionePresenze/competence-list-page/competence-list-page.module').then(m => m.CompetenceListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'competenceedit', loadChildren: () => import('../../pages/GestionePresenze/competence-edit-page/competence-edit-page.module').then(m => m.CompetenceEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'activitylist', loadChildren: () => import('../../pages/GestionePresenze/activity-list-page/activity-list-page.module').then(m => m.ActivityListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'activityedit', loadChildren: () => import('../../pages/GestionePresenze/activity-edit-page/activity-edit-page.module').then(m => m.ActivityEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'commessalist', loadChildren: () => import('../../pages/GestionePresenze/commessa-list-page/commessa-list-page.module').then(m => m.CommessaListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'commessaedit', loadChildren: () => import('../../pages/GestionePresenze/commessa-edit-page/commessa-edit-page.module').then(m => m.CommessaEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'mytemplate1list', loadChildren: () => import('../../pages/GestionePresenze/mytemplate1-list-page/mytemplate1-list-page.module').then(m => m.MyTemplate1ListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'mytemplate1edit', loadChildren: () => import('../../pages/GestionePresenze/mytemplate1-edit-page/mytemplate1-edit-page.module').then(m => m.MyTemplate1EditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'azsedilist', loadChildren: () => import('../../pages/GestionePresenze/az-sedi-list-page/az-sedi-list-page.module').then(m => m.AzSediListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'azsediedit', loadChildren: () => import('../../pages/GestionePresenze/az-sedi-edit-page/az-sedi-edit-page.module').then(m => m.AzSediEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'orarilist', loadChildren: () => import('../../pages/GestionePresenze/orari-list-page/orari-list.module').then(m => m.OrariListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'orariedit', loadChildren: () => import('../../pages/GestionePresenze/orari-edit-page/orari-edit-page.module').then(m => m.OrariEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'profiliorarilist', loadChildren: () => import('../../pages/GestionePresenze/profilo-orario-list-page/profilo-orario-list-page.module').then(m => m.ProfiloOrarioListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'profiliorariedit', loadChildren: () => import('../../pages/GestionePresenze/profilo-orario-edit-page/profilo-orario-edit-page.module').then(m => m.ProfiloOrarioEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },

      { path: 'exportcaulist', loadChildren: () => import('../../pages/GestionePresenze/export-cau-list-page/export-cau-list-page.module').then(m => m.ExportCauListPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },
      { path: 'exportcauedit', loadChildren: () => import('../../pages/GestionePresenze/export-cau-edit-page/export-cau-edit-page.module').then(m => m.ExportCauEditPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },


      { path: 'seletionsedirepartopage', loadChildren: () => import('../../pages/GestionePresenze/_selection/seletion-sedi-reparto-page/seletion-sedi-reparto-page.module').then(m => m.SeletionSediRepartoPageModule), canActivate: [RoleGuard4CompanyPowerAdmin] },


      { path: 'userdepartmentedit', loadChildren: () => import('../../pages/GestionePresenze/user-department-edit-page/user-department-edit-page.module').then(m => m.UserDepartmentEditPageModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'userdepartmentwizard', loadChildren: () => import('../../pages/GestionePresenze/user-department-wizard-page/user-department-wizard-page.module').then(m => m.UserDepartmentWizardPageModule), canActivate: [RoleGuard4CompanyAdmin] },

      

{ path: 'companyattendancewizard', loadChildren: () => import('../../pages/GestionePresenze/company-attendance-wizard-page/company-attendance-wizard-page.module').then(m => m.CompanyAttendanceWizardPageModule), canActivate: [RoleGuard4CompanyList] },

      

      //admin
      { path: 'admintimesheet', loadChildren: () => import('../../pages/GestionePresenze/time-sheet-admin-page/time-sheet-admin-page.module').then(m => m.TimeSheetAdminModule), canActivate: [RoleGuard4CompanyAdmin] },
      { path: 'requestlistadmin', loadChildren: () => import('../../pages/GestionePresenze/request-list-admin-page/request-list-admin-page.module').then(m => m.RequestListAdminPageModule), canActivate: [RoleGuard4CompanyAdmin] },
    ];
  }
}
