# Server.Service

Questo documento fornisce linee guida per contribuire ai dati di nvxapp. Seguire queste istruzioni per garantire che i vostri contributi siano conformi agli standard del progetto.


La solution Service , definische la logica di buisnes  che verrà chiamata dagli end point


Le classi che definiscono i Service derivano da ServiceBase


Ecco un esempio di come definire un service che gestisce l'accesso alla tabella MyTabella.

```linguaggio

    public class MyTabellaService : ServiceBase, IMyTabellaService
    {
        private readonly IMyTabellaRepository _myTabellaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public MyTabellaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IMyTabellaRepository myTabellaRepository,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _myTabellaRepository = MyTabellaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<MyTabellaGetOutAllModel>> MyTabellaGetAll(GenericRequest<MyTabellaGetInAllModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                MyTabellaGetOutAllModel retVal = new MyTabellaGetOutAllModel();

                int idCompany;
                int.TryParse(this.CurrentCompany, out idCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
                {
                    var myTabella_all = _myTabellaRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id).ToList();
                    retVal.MyTabella = _mapper.Map<List<MyTabellaModel>>(myTabella_all);
                }

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<MyTabellaGetOutModel>> MyTabellaGet(GenericRequest<MyTabellaGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                MyTabellaGetOutModel retVal = new MyTabellaGetOutModel();

                var myTabella = await _myTabellaRepository.FindByIdAsync(model.Data.Id);
                if (myTabella != null)
                {
                    retVal.MyTabella = _mapper.Map<MyTabellaModel>(myTabella);
                }
                else
                {
                    retVal.MyTabella = new MyTabellaModel { };
                }

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<MyTabellaPutOutModel>> MyTabellaPut(GenericRequest<MyTabellaPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                MyTabellaPutOutModel retVal = new MyTabellaPutOutModel();
                retVal.MyTabella = model.Data.MyTabella;

                int idCompany;
                int.TryParse(this.CurrentCompany, out idCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
                {
                    MyTabella? myTabella = await _myTabellaRepository.FindByIdAsync(model.Data.MyTabella.Id);
                    if (myTabella == null)
                    {
                        myTabella = _mapper.Map<MyTabella>(model.Data.MyTabella);
                        myTabella.IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id;
                    }
                    else
                    {
                        //update 
                        myTabella = _mapper.Map<MyTabella>(model.Data.MyTabella);
                    }

                    //aggiurna il valore ritornato al client
                    myTabella = await _myTabellaRepository.UpsertAsync(myTabella);
                    retVal.MyTabella = _mapper.Map<MyTabellaModel>(myTabella);
                }

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IMyTabellaService : IServiceBase
    {
        public Task<GenericResult<MyTabellaGetOutAllModel>> MyTabellaGetAll(GenericRequest<MyTabellaGetInAllModel> model, bool isSubProcess);
        public Task<GenericResult<MyTabellaGetOutModel>> MyTabellaGet(GenericRequest<MyTabellaGetInModel> model, bool isSubProcess);
        public Task<GenericResult<MyTabellaPutOutModel>> MyTabellaPut(GenericRequest<MyTabellaPutInModel> model, bool isSubProcess);
    }

```

## Punti d'attenzione
 1. Lo scambio dati con il client, vengono sempre utilizzati dei modelli (Model) specifici che stanno nella sottocartella \Models di ogni service.
 2. GenericResult e GenericRequest sono classi che avvolgono i dati in ingresso e in uscita per le chiamate ai servizi.
 3. Il nome del modelli per il trasferimento dei dati tra client e server non deve terminare con Dto ma con Models
 4. La firma dei metodi del ha sempre questo formato : 
    ```linguaggio
    public virtual async Task<GenericResult<Par_CausaliOutModel>> GetAll(GenericRequest<Par_CausaliInModel> model, bool isSubProcess)
    ```





## Models

Ecco un esempio di come definire i modelli per una tabella denominata MyTabella.

```linguaggio
public class MyTabellaModel
{
    public int Id { get; set; }
    public int IdAz_Anagrafica { get; set; }
    public string? Codice { get; set; }
    public string? Descrizione { get; set; }
}

public class MyTabellaGetInAllModel
{

}
public class MyTabellaGetOutAllModel : ModelResult
{
    public List<MyTabellaModel> MyTabella { get; set; } = new List<MyTabellaModel>();
    public MyTabellaGetOutAllModel()
    {

    }
}

public class MyTabellaGetInModel
{
    public int Id { get; set; } = 0;
}
public class MyTabellaGetOutModel : ModelResult
{
    public MyTabellaModel MyTabella { get; set; } = new MyTabellaModel();
}
public class MyTabellaPutInModel : ModelResult
{
    public MyTabellaModel MyTabella { get; set; } = new MyTabellaModel();
}
public class MyTabellaPutOutModel : ModelResult
{
    public MyTabellaModel MyTabella { get; set; } = new MyTabellaModel();
}

```

## Punti d'attenzione
 1. Nei models le variabili che rappresentano le liste, non vanno messe al plurale
 2. La classe che definisce il model di OUT della cancellazione deve essere come quella di OUD della Put


- [Home](./CONTRIBUTING.md)