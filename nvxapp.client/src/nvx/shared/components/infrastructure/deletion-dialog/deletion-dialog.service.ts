import { Injectable } from '@angular/core';
import { ModalController } from '@ionic/angular';
import { DeletionDialogComponent } from './deletion-dialog.component';
import { DeletionNodeType, DeletionTargetModel } from '../../../../ClientServer-Service/Infrastructure/Deletion/Models/deletion-model';

// Apre il dialog di cancellazione fisica; true se l'elemento e' stato eliminato (la pagina ricarica la lista)
@Injectable({
  providedIn: 'root'
})
export class DeletionDialogService {

  constructor(private modalCtrl: ModalController) { }

  async open(nodeType: DeletionNodeType, id: number | null, idAspNetUsers: string | null = null): Promise<boolean> {
    const target = new DeletionTargetModel();
    target.nodeType = nodeType;
    target.id = id;
    target.idAspNetUsers = idAspNetUsers;

    const modal = await this.modalCtrl.create({
      component: DeletionDialogComponent,
      componentProps: { target },
      backdropDismiss: false
    });
    await modal.present();

    const { role } = await modal.onDidDismiss();
    return role === 'confirm';
  }
}
