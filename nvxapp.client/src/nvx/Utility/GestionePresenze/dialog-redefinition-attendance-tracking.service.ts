import { Injectable, Type } from '@angular/core';
import { ApplicationType } from '../../ClientServer-Service/Infrastructure/Account/Models/user-load-model';
import { DialogName, DialogRedefinition } from '../infrastructure/dialog-redefinition';

/**
 * Dialog di Infrastructure sostituiti dalla gestione presenze (registrata in app.module con DIALOG_REDEFINITION).
 * I componenti si caricano solo quando servono.
 */
@Injectable()
export class DialogRedefinitionAttendanceTrackingService implements DialogRedefinition {

  public readonly applications = [ApplicationType.AttendanceTracking];
  public readonly priority = 10;

  public async redefineDialog(dialogName: string): Promise<Type<any> | null> {
    switch (dialogName) {
      case DialogName.AddUserCompany:
        return (await import('../../shared/components/GestionePresenze/user-attendance-wizard-dialog/user-attendance-wizard-dialog.component')).UserAttendanceWizardDialogComponent;
      default:
        return null;
    }
  }
}
