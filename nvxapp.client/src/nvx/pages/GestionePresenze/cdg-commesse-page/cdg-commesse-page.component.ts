import { Component, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { GenericRequest } from '../../../ClientServer-Service/ModelsBase/generic-request';
import { AzCommessaCdgService } from '../../../ClientServer-Service/GestionePresenze/Az_CommessaCdg/az-commessa-cdg.service';
import { Az_CommessaCdg_GetInModel, CdgParametri, CdgRisultato, CdgSubCommessaModel } from '../../../ClientServer-Service/GestionePresenze/Az_CommessaCdg/Models/az-commessa-cdg-model';
import { FabMenuService } from '../../../Utility/infrastructure/fab-menu.service';
import { ReportFabService } from '../../../Utility/GestionePresenze/report-fab.service';
import { ReportTipo } from '../../../ClientServer-Service/GestionePresenze/Report/Models/report-model';

/*
 Marginalita' commesse (ANALISI_ATTENDANCETRACKING_CONTROLLO_GESTIONE.md, AT-CDG.1-3): ore del timesheet per commessa e
 sottocommessa nel periodo, costo del personale (ore x costo orario), note spese ripartite, ricavo e margine.
 Costo orario predefinito, costi per dipendente e ricavi per sottocommessa si impostano qui e NON si salvano sul server
 (scelta provvisoria CDG1-2): il browser li ricorda e li invia a ogni calcolo e al report PDF (job).
*/
@Component({
  selector: 'app-cdg-commesse-page',
  templateUrl: './cdg-commesse-page.component.html',
  styleUrls: ['./cdg-commesse-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class CdgCommessePageComponent {

  public title: string;
  /** periodo come "yyyy-MM" (input mese) */
  public meseDal: string;
  public meseAl: string;
  public costoPredefinito: number | null = null;
  /** costi orari per dipendente e ricavi per sottocommessa impostati nella pagina */
  public costi: { [idAspNetUsers: string]: number | null } = {};
  public ricavi: { [idAz_SubCommessa: number]: number | null } = {};

  public risultato: CdgRisultato | null = null;
  public aperte = new Set<number>();
  public caricamento = false;

  constructor(private cdgService: AzCommessaCdgService,
              private fabMenuService: FabMenuService,
              private reportFabService: ReportFabService,
              private transloco: TranslocoService) {
    this.title = this.transloco.translate('attendance.pages.cdgCommesse.title');
    const oggi = new Date();
    this.meseDal = this.meseAl = `${oggi.getFullYear()}-${String(oggi.getMonth() + 1).padStart(2, '0')}`;

    const salvati = this.cdgService.ParametriSalvati();
    if (salvati) {
      this.costoPredefinito = salvati.costoOrarioPredefinito ?? null;
      (salvati.costiDipendenti ?? []).forEach(c => this.costi[c.idAspNetUsers] = c.costoOrario);
      (salvati.ricavi ?? []).forEach(r => this.ricavi[r.idAz_SubCommessa] = r.ricavo);
    }
  }

  ionViewWillEnter() {
    this.fabMenuService.fabMenuItem = [
      this.reportFabService.FabItem([
        { tipo: ReportTipo.MarginalitaCommesse, testo: 'attendance.services.report.marginalita' },
      ], () => {
        const p = this.parametri();
        return p ? { year: p.year, month: p.month, selectedUserId: [], repartiId: [], idPar_TipoSpesa: null, stati: null, cdg: p } : null;
      }),
    ];
    this.calcola();
  }

  ionViewWillLeave() {
    this.fabMenuService.fabMenuItem = [];
  }

  /** parametri della richiesta (null se il periodo non e' valido) */
  private parametri(): CdgParametri | null {
    if (!this.meseDal || !this.meseAl)
      return null;
    const p = new CdgParametri();
    [p.year, p.month] = this.meseDal.split('-').map(Number);
    [p.yearAl, p.monthAl] = this.meseAl.split('-').map(Number);
    p.costoOrarioPredefinito = this.numero(this.costoPredefinito);
    p.costiDipendenti = Object.entries(this.costi)
                              .filter(([, v]) => this.numero(v) !== null)
                              .map(([idAspNetUsers, v]) => ({ idAspNetUsers, costoOrario: this.numero(v)! }));
    p.ricavi = Object.entries(this.ricavi)
                     .filter(([, v]) => this.numero(v) !== null)
                     .map(([id, v]) => ({ idAz_SubCommessa: Number(id), ricavo: this.numero(v)! }));
    return p;
  }

  private numero(v: number | string | null | undefined): number | null {
    if (v === null || v === undefined || v === '') return null;
    const n = Number(v);
    return isNaN(n) ? null : n;
  }

  calcola() {
    const p = this.parametri();
    if (!p) return;
    this.cdgService.SalvaParametri(p);
    const request = new GenericRequest<Az_CommessaCdg_GetInModel>(Az_CommessaCdg_GetInModel);
    request.data.parametri = p;
    this.caricamento = true;
    this.cdgService.CdgGet(request).subscribe({
      next: res => {
        this.risultato = res.success && res.data ? res.data.risultato : null;
        this.caricamento = false;
      },
      error: () => this.caricamento = false
    });
  }

  toggle(s: CdgSubCommessaModel) {
    this.aperte.has(s.idAz_SubCommessa) ? this.aperte.delete(s.idAz_SubCommessa) : this.aperte.add(s.idAz_SubCommessa);
  }

  ore(minuti: number): string {
    return `${Math.floor(minuti / 60)}:${String(minuti % 60).padStart(2, '0')}`;
  }
}
