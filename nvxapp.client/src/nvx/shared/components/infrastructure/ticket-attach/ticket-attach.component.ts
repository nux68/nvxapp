import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { TicketAttachModel } from '../../../../ClientServer-Service/Infrastructure/SupportTicket/Models/ticket-model';

// stessi limiti del server (TicketRules): il server li ricontrolla comunque
export const TICKET_ATTACH_MAX_COUNT = 5;
export const TICKET_ATTACH_MAX_SIZE = 10 * 1024 * 1024;
export const TICKET_ATTACH_MAX_TOTAL_SIZE = 20 * 1024 * 1024;
export const TICKET_ATTACH_EXTENSIONS = [
  '.pdf', '.png', '.jpg', '.jpeg', '.gif', '.bmp', '.webp', '.txt', '.log', '.csv', '.json', '.xml',
  '.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx', '.odt', '.ods', '.zip', '.7z', '.eml', '.msg'
];

/*
 Allegati di un passaggio del ticket: scelta dei file (letti in base64) ed elenco con rimozione.
 Uso: <app-ticket-attach [attach]="allegati" (attachChange)="allegati = $event"></app-ticket-attach>
*/
@Component({
  selector: 'app-ticket-attach',
  templateUrl: './ticket-attach.component.html',
  styleUrls: ['./ticket-attach.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  standalone: false
})
export class TicketAttachComponent {

  @Input() attach: TicketAttachModel[] = [];
  @Output() attachChange: EventEmitter<TicketAttachModel[]> = new EventEmitter();

  public error: string | null = null;
  public readonly accept = TICKET_ATTACH_EXTENSIONS.join(',');

  constructor(private transloco: TranslocoService) { }

  async onFiles(event: Event) {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';
    this.error = null;

    const list = [...(this.attach ?? [])];
    for (const file of files) {
      const error = this.check(file, list);
      if (error) {
        this.error = error;
        continue;
      }
      list.push({
        id: 0,
        fileName: file.name,
        contentType: file.type || 'application/octet-stream',
        size: file.size,
        file: await this.toBase64(file)
      });
    }

    this.attach = list;
    this.attachChange.emit(list);
  }

  remove(index: number) {
    const list = [...this.attach];
    list.splice(index, 1);
    this.error = null;
    this.attach = list;
    this.attachChange.emit(list);
  }

  sizeText(size: number): string {
    return size >= 1024 * 1024 ? (size / (1024 * 1024)).toFixed(1) + ' MB' : Math.max(1, Math.round(size / 1024)) + ' KB';
  }

  private check(file: File, list: TicketAttachModel[]): string | null {
    const ext = file.name.includes('.') ? file.name.substring(file.name.lastIndexOf('.')).toLowerCase() : '';
    if (list.length >= TICKET_ATTACH_MAX_COUNT)
      return this.transloco.translate('components.ticketAttach.tooMany', { max: TICKET_ATTACH_MAX_COUNT });
    if (file.size === 0)
      return this.transloco.translate('components.ticketAttach.empty', { name: file.name });
    if (!TICKET_ATTACH_EXTENSIONS.includes(ext))
      return this.transloco.translate('components.ticketAttach.extension', { name: file.name });
    if (file.size > TICKET_ATTACH_MAX_SIZE)
      return this.transloco.translate('components.ticketAttach.tooLarge', { name: file.name, max: TICKET_ATTACH_MAX_SIZE / (1024 * 1024) });
    const total = list.reduce((s, a) => s + a.size, 0) + file.size;
    if (total > TICKET_ATTACH_MAX_TOTAL_SIZE)
      return this.transloco.translate('components.ticketAttach.totalTooLarge', { max: TICKET_ATTACH_MAX_TOTAL_SIZE / (1024 * 1024) });
    return null;
  }

  // contenuto in base64 (senza il prefisso "data:...;base64,")
  private toBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        const result = reader.result as string;
        resolve(result.substring(result.indexOf(',') + 1));
      };
      reader.onerror = () => reject(reader.error);
      reader.readAsDataURL(file);
    });
  }
}
