import { SharedParameterGestionePresenzeService } from '../../shared-parameter-gestione-presenze.service';
import { ParCausaliToLongTextPipe } from './par-causali-to-long-text.pipe';

describe('ParCausaliToLongTextPipe', () => {
  it('create an instance', () => {
    const pipe = new ParCausaliToLongTextPipe({} as SharedParameterGestionePresenzeService);
    expect(pipe).toBeTruthy();
  });
});
