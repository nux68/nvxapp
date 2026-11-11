import { AlertController, ModalController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { DipSelectorItem, DipSelectorModalComponent, DipSelectorMode, DipSelectorResult } from '../../shared/components/infrastructure/dip-selector-modal/dip-selector-modal.component';
import { HrDipendenteBreveModel } from '../../ClientServer-Service/HumanResources/Models/hr-model';

/*
 Piccoli aiuti condivisi dalle pagine di Risorse Umane:
   - dati del nuovo account del dipendente (HR-ONB.3), chiesti in un alert (nome utente, email, password)
   - scelta dei dipendenti con il dialog comune (DipSelectorModalComponent)
   - scelta di un file dal dispositivo
*/
export interface HrNuovoAccount { userName: string; mail: string | null; password: string; }

export async function hrChiediAccount(alertCtrl: AlertController, transloco: TranslocoService, mail: string | null): Promise<HrNuovoAccount | null> {
  const t = (k: string) => transloco.translate(k);
  const alert = await alertCtrl.create({
    header: t('hr.account.createTitle'),
    message: t('hr.account.createNote'),
    inputs: [
      { name: 'userName', type: 'text', placeholder: t('hr.account.userName') },
      { name: 'mail', type: 'email', placeholder: t('hr.account.mail'), value: mail ?? '' },
      { name: 'password', type: 'password', placeholder: t('hr.account.password') },
    ],
    buttons: [
      { text: t('common.buttons.cancel'), role: 'cancel' },
      { text: t('common.buttons.confirm'), role: 'confirm' },
    ]
  });
  await alert.present();
  const { data, role } = await alert.onDidDismiss();
  const v = data?.values;
  if (role !== 'confirm' || !v?.userName?.trim() || !v?.password)
    return null;
  return { userName: v.userName.trim(), mail: v.mail?.trim() || null, password: v.password };
}

export async function hrScegliDipendenti(modalCtrl: ModalController, dipendenti: HrDipendenteBreveModel[], mode: DipSelectorMode,
                                         preselected: number[] = []): Promise<number[] | null> {
  const items: DipSelectorItem[] = dipendenti.map(d => ({ id: d.id, cognome: d.cognome, nome: d.nome, matricola: d.matricola, dettaglio: d.codiceFiscale }));
  const modal = await modalCtrl.create({ component: DipSelectorModalComponent, componentProps: { mode, items, preselected } });
  await modal.present();
  const { data, role } = await modal.onDidDismiss<DipSelectorResult<number>>();
  return role === 'confirm' && data ? data.selected : null;
}

export function hrScegliFile(accept: string): Promise<File | null> {
  return new Promise(resolve => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = accept;
    input.onchange = () => resolve(input.files && input.files.length > 0 ? input.files[0] : null);
    input.click();
  });
}
