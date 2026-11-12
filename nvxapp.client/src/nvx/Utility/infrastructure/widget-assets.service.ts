import { Injectable } from '@angular/core';
import { iWidgetProvider, WidgetDefinition } from './widget.model';
import { AstMyBookingsWidgetComponent } from '../../shared/components/Assets/widgets/ast-my-bookings-widget/ast-my-bookings-widget.component';
import { AstMineWidgetComponent } from '../../shared/components/Assets/widgets/ast-mine-widget/ast-mine-widget.component';
import { AstFleetWidgetComponent } from '../../shared/components/Assets/widgets/ast-fleet-widget/ast-fleet-widget.component';
import { AstToReturnWidgetComponent } from '../../shared/components/Assets/widgets/ast-to-return-widget/ast-to-return-widget.component';

// chi gestisce flotta e beni (come le pagine "Veicoli" e "Beni": il server lo ricontrolla)
const RUOLI_AMMINISTRATORI = ['CompanyPowerAdmin', 'CompanyAdmin'];
// chi lavora nell'azienda (prenotazioni e risorse proprie)
const RUOLI_AZIENDA = ['CompanyPowerAdmin', 'CompanyAdmin', 'User'];

/*
 Widget della dashboard di Risorse aziendali (ANALISI_INFRASTRUCTURE_WIDGET.md, dettaglio in ANALISI_ASSETS_WIDGET.md).
 Compaiono solo se Risorse aziendali e' attivo per l'azienda (WidgetService). Fascia di zorder 700000, come il menu.
*/
@Injectable({
  providedIn: 'root'
})
export class WidgetAssetsService implements iWidgetProvider {
  public get Widgets(): WidgetDefinition[] {
    return [
      {
        // prossime prenotazioni dell'utente (postazioni, sale, auto in pool)
        key: 'assets.myBookings',
        title: 'ast.widgets.myBookings.title',
        description: 'ast.widgets.myBookings.description',
        icon: 'calendar-outline',
        roles: RUOLI_AZIENDA,
        size: 'small',
        default: true,
        zorder: 700100,
        component: AstMyBookingsWidgetComponent
      },
      {
        // beni e veicolo in carico all'utente, verbali da confermare, multe da pagare
        key: 'assets.mine',
        title: 'ast.widgets.mine.title',
        description: 'ast.widgets.mine.description',
        icon: 'person-circle-outline',
        roles: RUOLI_AZIENDA,
        size: 'small',
        default: false,
        zorder: 700200,
        component: AstMineWidgetComponent
      },
      {
        // scadenze dei veicoli e dei contratti, multe da notificare
        key: 'assets.fleet',
        title: 'ast.widgets.fleet.title',
        description: 'ast.widgets.fleet.description',
        icon: 'car-outline',
        roles: RUOLI_AMMINISTRATORI,
        size: 'small',
        default: true,
        zorder: 700300,
        component: AstFleetWidgetComponent
      },
      {
        // beni e veicoli di dipendenti cessati ancora da restituire (AST-BENI.3)
        key: 'assets.toReturn',
        title: 'ast.widgets.toReturn.title',
        description: 'ast.widgets.toReturn.description',
        icon: 'return-down-back-outline',
        roles: RUOLI_AMMINISTRATORI,
        size: 'small',
        default: false,
        zorder: 700400,
        component: AstToReturnWidgetComponent
      }
    ];
  }
}
