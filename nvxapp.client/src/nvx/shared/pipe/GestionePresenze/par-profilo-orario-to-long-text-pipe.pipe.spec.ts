import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { ParProfiloOrarioToLongTextPipePipe } from './par-profilo-orario-to-long-text-pipe.pipe';

describe('ParProfiloOrarioToLongTextPipePipe', () => {
  it('create an instance', () => {
    const pipe = new ParProfiloOrarioToLongTextPipePipe({} as SharedParameterGestionePresenzeService);
    expect(pipe).toBeTruthy();
  });
});
