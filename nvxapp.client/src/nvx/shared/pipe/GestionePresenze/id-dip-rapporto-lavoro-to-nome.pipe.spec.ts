import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { IdDipRapportoLavoroToNomePipe } from './id-dip-rapporto-lavoro-to-nome.pipe';

describe('IdDipRapportoLavoroToNomePipe', () => {
  it('create an instance', () => {
    const pipe = new IdDipRapportoLavoroToNomePipe({} as SharedParameterGestionePresenzeService);
    expect(pipe).toBeTruthy();
  });
});
