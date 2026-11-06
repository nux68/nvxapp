## ATTENZIONE !!! 

Tenere presente che è attiva la gestione della lingua


ServiceBase rappresenta il punto di partenza della logica bisness, quando si crea un nuovo servizio devivare da li, prendendo spunto dei vari services della soluzione



## ATTENZIONE !!!

Si accede al database SOLO mediante i repository (nvxapp.server.data/Repositories): i servizi non iniettano mai un DbContext.
 - un repository per entita' (Repositories/Public, Repositories/Tenant/Common, Repositories/Tenant/<Applicativo>)
 - tabelle comuni (schema Common): scritture con i repository di Repositories/Tenant/Common; letture nelle query di un applicativo
   con i repository di sola lettura sul contesto dell'applicativo (<Prefisso>_<Entita>ReadRepository), perche' una query sta su un solo contesto
 - SQL atomico, operazioni massive o salvataggi in transazione: metodi del repository (o ExecuteUpdate/ExecuteDelete sull'IQueryable di FindAll)
 - fuori da una richiesta (hosted service, contesti per schema dalla factory): repository dallo scope o new XRepository(db, serviceProvider, httpContextAccessor)
Regole complete: nvxapp.server/docs/CONTRIBUTING_nvxapp_data.md, sezione "REGOLA BASE".
