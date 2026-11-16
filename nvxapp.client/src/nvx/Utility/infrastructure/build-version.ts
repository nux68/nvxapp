/*
 Versione della build del client, da aggiungere come ?v=<versione> agli URL dei file che mantengono lo stesso nome
 a ogni pubblicazione (es. le traduzioni in assets/i18n: vedi i18n.providers.ts). Con un URL che cambia a ogni
 build il browser non usa mai una copia vecchia, e il server puo' dare a quei file la cache lunga (Program.cs).

 NVX_BUILD_VERSION e' sostituito da esbuild con l'opzione "define" della build, passata da riga di comando con un
 valore diverso a ogni pubblicazione (data e ora):
   npx ng build --define "NVX_BUILD_VERSION='20261116153000'"
 lo fanno il target BuildAngular di nvxapp.server.csproj e il Dockerfile.
 In sviluppo (ng serve, ng build senza --define) non e' definito: la versione e' vuota e gli URL restano senza ?v=.
*/
declare const NVX_BUILD_VERSION: string | undefined;

export const BUILD_VERSION: string = typeof NVX_BUILD_VERSION === 'string' ? NVX_BUILD_VERSION : '';

/** Aggiunge ?v=<versione della build> a un URL (lo lascia com'e' se la versione non c'e'). */
export function withBuildVersion(url: string): string {
  if (!BUILD_VERSION) return url;
  return `${url}${url.includes('?') ? '&' : '?'}v=${encodeURIComponent(BUILD_VERSION)}`;
}
