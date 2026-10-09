import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { IdDipRapportoLavoroToCognomePipe } from './id-dip-rapporto-lavoro-to-cognome.pipe';

describe('IdDipRapportoLavoroToCognomePipe', () => {
  it('create an instance', () => {
    const pipe = new IdDipRapportoLavoroToCognomePipe({} as SharedParameterGestionePresenzeService);
    expect(pipe).toBeTruthy();
  });
});
