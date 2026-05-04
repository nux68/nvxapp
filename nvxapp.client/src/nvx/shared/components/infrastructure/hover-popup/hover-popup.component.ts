import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';


@Component({
  selector: 'app-hover-popup',
  templateUrl: './hover-popup.component.html',
  styleUrls: ['./hover-popup.component.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class HoverPopupComponent {
  @Input() data!: HoverPopupData;
  @Input() style: Record<string, string> = {};
}

/*
1)
    <div [appHoverPopupEnabled]="true" [appHoverPopup]="{ title: 'Dipendente', content: 'Mario Rossi', extraInfo: { Reparto: 'IT', Turno: 'Mattina' } }">
      Passa il mouse qui
    </div>

2)
    popupInfo: HoverPopupData = {
      title: 'Dettaglio presenze',
      content: 'Ore lavorate: 7h 30m',
      extraInfo: { Ingresso: '08:00', Uscita: '15:30' }
    };
    <ion-item [appHoverPopupEnabled]="true" [nvxHoverPopup]="popupInfo">...</ion-item>

*/



export interface HoverPopupData {
  title?: string;
  content: string;
  extraInfo?: Record<string, string>;
}
