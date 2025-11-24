
# Server.Data

Questo documento fornisce linee guida per contribuire ai dati di nvxapp. Seguire queste istruzioni per garantire che i vostri contributi siano conformi agli standard del progetto.



La solution Server , definische gli end point che il client chiamerà e utilizzara la logica di buisness esposta dai Services


Le classi che definiscono i controller per il database derivano da NvxControllerBase e in genere contengono un riferimento a una Service per poter utilizzare della logica di buisness o semplicemente per poter leggere e scrivere dati

I metodi esposti saranno tutti delle POST


<u>Attenzione !!! La logica di buisness non deve essere mai inserita nei controller ma sempre nei service dedicati.</u>

```linguaggio
[ApiController]
[Route("api/[controller]")]
public class MyTabellaController : NvxControllerBase
{
    private readonly IMyTabellaService _MyTabellaService;

    public MyTabellaController(
        IHttpContextAccessor httpContextAccessor,
        IMyTabellaService myTabellaService
    ) : base(httpContextAccessor)
    {
        _MyTabellaService = myTabellaService;
    }

    [Authorize]
    [HttpPost]
    [Route("GetAll")]
    public async Task<GenericResult<MyTabella_GetAllOutModel>> GetAll(GenericRequest<MyTabella_GetAllInModel> inModel)
    {
        var res = await _MyTabellaService.GetAll(inModel, false);
        return res;
    }

    [Authorize]
    [HttpPost]
    [Route("MyTabellaGet")]
    public async Task<GenericResult<MyTabella_GetOutModel>> MyTabellaGet(GenericRequest<MyTabella_GetInModel> inModel)
    {
        var res = await _MyTabellaService.MyTabellaGet(inModel, false);
        return res;
    }

    [Authorize]
    [HttpPost]
    [Route("MyTabellaPut")]
    public async Task<GenericResult<MyTabella_PutOutModel>> MyTabellaPut(GenericRequest<MyTabella_PutInModel> inModel)
    {
        var res = await _MyTabellaService.MyTabellaPut(inModel, false);
        return res;
    }

    [Authorize]
    [HttpPost]
    [Route("MyTabellaDelete")]
    public async Task<GenericResult<MyTabella_DeleteOutModel>> MyTabellaDelete(GenericRequest<MyTabella_DeleteInModel> inModel)
    {
        var res = await _MyTabellaService.MyTabellaDelete(inModel, false);
        return res;
    }
}
```

### Folder Structure

I controller vengono posizionate nella sottocartella `Controllers\Infrastructure` se espongono metodi comuni o nella sottocartella `Controllers\GestionePresenze`  se sono dedicate a un argomento specifico es GestionePresenze (comunque in fase di generazione , tenere sempre presente il branch corrente)

- [Home](./CONTRIBUTING.md)