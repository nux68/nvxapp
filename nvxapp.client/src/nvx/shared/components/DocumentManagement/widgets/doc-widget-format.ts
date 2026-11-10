// Formattazione comune dei widget del documentale

/** byte in forma leggibile (B, KB, MB, GB, TB) con al massimo un decimale */
export function formatBytes(bytes: number | null | undefined): string {
  const b = Math.max(0, bytes ?? 0);
  const unita = ['B', 'KB', 'MB', 'GB', 'TB'];
  let v = b;
  let i = 0;
  while (v >= 1024 && i < unita.length - 1) {
    v /= 1024;
    i++;
  }
  return v.toLocaleString(undefined, { maximumFractionDigits: i === 0 ? 0 : 1 }) + ' ' + unita[i];
}

/** quota in MB che il server usa per "illimitato" (int.MaxValue) */
export const QUOTA_ILLIMITATA_MB = 2147483647;
