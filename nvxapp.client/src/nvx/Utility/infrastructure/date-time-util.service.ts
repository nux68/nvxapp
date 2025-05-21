import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class DateTimeUtilService {

  constructor() { }

  public timeToMinutes(time: Date): number {
    if (!time) return 0;
    const hours = time.getHours();
    const minutes = time.getMinutes();
    return hours * 60 + minutes;
  }

  public DateTo_ggmmyyyy(date: Date): string {
    const d = new Date(date);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}/${month}/${year}`;
  }

  public DateTo_ggmmyyyy_hhmm(date: Date): string {
    const d = new Date(date);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}`;
  }

}
