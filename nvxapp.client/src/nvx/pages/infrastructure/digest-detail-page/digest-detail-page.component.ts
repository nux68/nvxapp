import { Component, ChangeDetectionStrategy } from '@angular/core';
import { formatDate } from '@angular/common';
import { NavController } from '@ionic/angular';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { DigestDetailModel, DigestGetInModel, DigestSection } from '../../../ClientServer-Service/Infrastructure/Digest/Models/digest-model';
import { DigestService } from '../../../ClientServer-Service/Infrastructure/Digest/digest.service';
import { LanguageService } from '../../../Utility/infrastructure/language.service';
import { ButtonItem, UserInterfaceService } from '../../../Utility/infrastructure/user-interface.service';

/*
 Dettaglio di un riepilogo (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-AI.3), in sola lettura: testo (scritto dall'AI o
 generato dai fatti) e fatti strutturati forniti dai moduli da cui e' stato scritto. Si apre dall'elenco o dalla notifica
 (stato { id }).
*/
@Component({
  selector: 'app-digest-detail-page',
  templateUrl: './digest-detail-page.component.html',
  styleUrls: ['./digest-detail-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class DigestDetailPageComponent {

  public digest: DigestDetailModel | null = null;
  public readonly buttonbar: ButtonItem[];
  private idDigest = 0;

  constructor(private navCtrl: NavController,
              private userInterfaceService: UserInterfaceService,
              private digestService: DigestService,
              private languageService: LanguageService,
              private transloco: TranslocoService) {
    const elenco = this.userInterfaceService.Btn_Elenco;
    elenco.event = () => this.navCtrl.navigateBack('/digests');
    this.buttonbar = [elenco];
  }

  get title(): string {
    return this.digest?.titolo ?? this.transloco.translate('pages.digestDetail.title');
  }

  ionViewWillEnter() {
    const state = history.state;
    this.idDigest = state && state.id ? state.id : 0;
    if (!this.idDigest) {
      this.navCtrl.navigateRoot('/digests');
      return;
    }
    this.load();
  }

  load() {
    const request = new GenericRequest<DigestGetInModel>(DigestGetInModel);
    request.data.id = this.idDigest;
    this.digestService.DigestGet(request).subscribe(res => {
      this.digest = res.success && res.data ? res.data.digest : null;
    });
  }

  /** sezioni del riepilogo dell'azienda o, per lo studio, gruppi per azienda */
  get gruppi(): { azienda: string | null, sezioni: DigestSection[] }[] {
    const fatti = this.digest?.fatti;
    if (!fatti)
      return [];
    if (fatti.aziende)
      return fatti.aziende.map(a => ({ azienda: a.azienda, sezioni: a.sezioni }));
    return [{ azienda: null, sezioni: fatti.sezioni ?? [] }];
  }

  public periodo(d: DigestDetailModel): string {
    return this.formatData(d.dal) + ' - ' + this.formatData(d.al);
  }

  public generato(d: DigestDetailModel): string {
    const quando = d.data ? formatDate(d.data, 'short', this.languageService.locale) : '';
    return this.transloco.translate(d.generatoConAI ? 'pages.digestDetail.generatedAi' : 'pages.digestDetail.generatedPlain', { date: quando })
         + (d.manuale ? ' ' + this.transloco.translate('pages.digestDetail.manual') : '');
  }

  private formatData(value: string | null): string {
    return value ? formatDate(value, 'shortDate', this.languageService.locale) : '';
  }
}
