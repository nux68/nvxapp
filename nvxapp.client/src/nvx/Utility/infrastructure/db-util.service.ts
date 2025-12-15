import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class DbUtilService {

  constructor() { }

  public GenerateCounterKey(maxNegative: number = 1000000): number {
      // Moltiplica per il limite massimo e arrotonda per difetto
      const randomPositive = Math.floor(Math.random() * maxNegative) + 1;
      // Rendi il numero negativo
      return -randomPositive;
  }

}
