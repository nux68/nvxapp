## ATTENZIONE !!! 
Si dialoga in italiano

La solution si porta avanti su branch differenti che rappresentano i gruppi "logici" dell'applicativo

1) "Funzioni base" comuni in tutto l'applicativo, viene sviluppata in Infrastructure_Dev e mergiata in Infrastructure
2) "Gestione Presenze"                            viene sviluppata in AttendanceTracking_Dev , riceve i merge da Infrastructure e viene mergiata in AttendanceTracking 
3) "CRM"                                          viene sviluppata in CRM_Dev                , riceve i merge da Infrastructure e viene mergiata in CRM  
4) "Documentale"                                  viene sviluppata in DocumentManagement_Dev , riceve i merge da Infrastructure e viene mergiata in DocumentManagement  
5) "Risorse Umane"                                viene sviluppata in HumanResources_Dev     , riceve i merge da Infrastructure e viene mergiata in HumanResources
6) "Compliance"                                   viene sviluppata in Compliance_Dev         , riceve i merge da Infrastructure e viene mergiata in Compliance
7) "Risorse aziendali"                            viene sviluppata in Assets_Dev             , riceve i merge da Infrastructure e viene mergiata in Assets
8) "All Function"                                 viene sviluppata in AllFunction_Dev        , riceve i merge da,Infrastructure(per primo) a seguire AttendanceTracking,CRM,DocumentManagement,HumanResources,Compliance,Assets  e mergiata in AllFunction

"Amministrazione" (Administration_Dev / Administration) e' prevista piu' avanti: vedi nvxapp.server/docs/ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md


## ATTENZIONE !!! 
I merge vanno sempe da "Funzioni base" verso gli altri branch a mai al contrario 



## ATTENZIONE !!! 
le mod. vanno fatte su un worktree dedicato, e va chiesto se procedere alla catena dei merge (se i merge hanno esito positivo, cancellare il worktree) 
non effettuare mai commit verso origin, tutte le mod. fatte devono rimenere locali
   
   
## ATTENZIONE !!! 
Con "Applicativo completo" si intende il merge  di Infrastructure,AttendanceTracking,CRM,DocumentManagement,HumanResources,Compliance,Assets nel branch AllFunction_Dev e AllFunction
quando richiesto, produci quasta sequenza di merge
   

Se richiedo modifiche che sono da fare su un brach differente da quello corrente, ci si sposte perchè devono essere fatte nel brach di pertinenza

Quando viene richiesta un analisi, in fondo al documento va prodotta la checklist dei punti da svolgere e man mano che i lavori vengono svolti, va aggiornata la checklist
I documenti di analisi avranno un prefisso del tipo ANALISI_XXX_YYY
dove XXX è il nome dell applicativo e YYY il nome dell analisi